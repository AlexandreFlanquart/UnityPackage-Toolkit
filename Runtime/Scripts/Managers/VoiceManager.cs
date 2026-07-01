using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;
#if I2LOC_PRESENT
using I2.Loc;
#endif

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// VoiceManager handles the playback, streaming, and queuing of voice audio clips.
    /// It supports both local and streamed audio, manages a cache for performance,
    /// and provides global volume and mute controls.
    /// </summary>
    public class VoiceManager : MonoBehaviour
    {
        [SerializeField] private bool audioStreaming = false;
        [SerializeField] private AudioSource audioSource = default;
        [SerializeField] private float globalVolume = 1f;

        private bool noClip = false;
        private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private Queue<string> voiceQueue = new Queue<string>();
        private bool isProcessingQueue = false;

        public AudioClip CurrentClip { get => audioSource.clip; }
        public bool IsAudioStreaming { get => audioStreaming; }
        public float GlobalVolume 
        { 
            get => globalVolume;
            set 
            {
                globalVolume = Mathf.Clamp01(value);
                audioSource.volume = globalVolume;
            }
        }

        void Awake()
        {
            ServiceLocator.AddService<VoiceManager>(gameObject);
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            audioSource.volume = globalVolume;
#if I2LOC_PRESENT
            LocalizationManager.OnLocalizeEvent.AddListener(OnLanguageChanged);
#endif
        }

        void OnDestroy()
        {
            ServiceLocator.RemoveService<VoiceManager>(this);
#if I2LOC_PRESENT
            LocalizationManager.OnLocalizeEvent.RemoveListener(OnLanguageChanged);
#endif
            ClearCache();
        }

#if I2LOC_PRESENT
        private void OnLanguageChanged()
        {
            ClearCache();
            MUPLogger.Info("VoiceManager: language changed, cache cleared.");
        }
#endif

        // Configure a GameObject as a VoiceAgent with delay and keys
        public void ConfigVoiceElement(GameObject pElement, float pDelay, params string[] pKeys)
        {
            VoiceAgent agent = pElement.GetComponent<VoiceAgent>();
            if (!agent)
            {
                agent = pElement.AddComponent<VoiceAgent>();
            }
            agent.Initialize(pDelay, pKeys);
        }

        // Overload for ConfigVoiceElement without delay
        public void ConfigVoiceElement(GameObject pElement, params string[] pKeys)
        {
            ConfigVoiceElement(pElement, 0, pKeys);
        }

        // Play a sequence of voices, optionally with a delay between them
        public bool PlayVoices(float pDelay, params string[] pKeys)
        {
            if (audioSource.mute || pKeys == null || pKeys.Length == 0)
            {
                return false;
            }

            if (pKeys.Length > 1)
            {
                StartCoroutine(PlayVoicesQueued(pKeys, pDelay));
            }
            else
            {
                PlayVoice(pKeys[0]);
            }

            return true;
        }

        // Play a single voice by key (from cache, streaming, or loading from assets)
        private void PlayVoice(string pKey)
        {
            if (string.IsNullOrEmpty(pKey)) return;

            noClip = false;
      
            if(audioStreaming)
            {
                #if I2LOC_PRESENT
                    string audioUrl = Application.streamingAssetsPath + $"/Voices/{LocalizationManager.CurrentLanguageCode}/{pKey}.mp3";
                #else
                    string audioUrl = Application.streamingAssetsPath + $"/Voices/{pKey}.mp3";
                #endif
                StartCoroutine(AudioStreaming(audioUrl));
                return;
            }
            else
            {
#if I2LOC_PRESENT
                string cacheKey = $"{LocalizationManager.CurrentLanguageCode}_{pKey}";
#else
                string cacheKey = pKey;
#endif
                if (clipCache.TryGetValue(cacheKey, out AudioClip cachedClip))
                {
                    PlayClip(cachedClip);
                    return;
                }

                AudioClip clip = LoadAudioClip(pKey);

                if (clip != null)
                {
                    clipCache[cacheKey] = clip;
                }
                PlayClip(clip);
            }   
        }

        // Play the given AudioClip through the AudioSource
        private void PlayClip(AudioClip pClip)
        {
            if (pClip == null)
            {
                noClip = true;
                return;
            }

            if (audioSource.clip != null)
            {
                audioSource.clip.UnloadAudioData();
            }

            audioSource.clip = pClip;
            audioSource.Play();
        }

        // Coroutine to play a queue of voices with optional delay between each
        private IEnumerator PlayVoicesQueued(string[] pKeys, float pDelay)
        {
            if (isProcessingQueue)
            {
                foreach (var key in pKeys)
                {
                    voiceQueue.Enqueue(key);
                }
                yield break;
            }

            isProcessingQueue = true;

            // Ajouter les clés initiales à la file d'attente
            foreach (var key in pKeys)
            {
                voiceQueue.Enqueue(key);
            }

            // Traiter toutes les voix dans la file d'attente
            while (voiceQueue.Count > 0)
            {
                string nextKey = voiceQueue.Dequeue();
                PlayVoice(nextKey);
                
                // Attendre que la voix soit terminée
                while (!noClip && audioSource.isPlaying)
                {
                    yield return null;
                }

                yield return new WaitForEndOfFrame();
                
                // Ajouter le délai entre les voix si nécessaire
                if (pDelay > 0)
                {
                    yield return new WaitForSeconds(pDelay);
                }
            }

            isProcessingQueue = false;
        }

        // Toggle mute on the AudioSource and return the new mute state
        public bool Mute()
        {
            audioSource.mute = !audioSource.mute;
            return audioSource.mute;
        }

        // Stop all voice playback and clear the queue
        public void StopVoice()
        {
            StopAllCoroutines();
            voiceQueue.Clear();
            isProcessingQueue = false;
            audioSource.Stop();
            if (audioSource.clip != null)
            {
                audioSource.clip.UnloadAudioData();
                audioSource.clip = null;
            }
        }

        // Get the duration of the currently playing voice clip
        public float GetCurrentVoiceDuration()
        {
            return audioSource.clip != null ? audioSource.clip.length : 0f;
        }

        // Clear and unload all cached audio clips, preserving the currently playing clip
        private void ClearCache()
        {
            AudioClip currentClip = audioSource != null ? audioSource.clip : null;
            foreach (var clip in clipCache.Values)
            {
                if (clip != null && clip != currentClip)
                {
                    clip.UnloadAudioData();
                }
            }
            clipCache.Clear();
        }

        // Coroutine to stream audio from a URL and play it
        private IEnumerator AudioStreaming(string audioUrl)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG);

            if (www.downloadHandler is DownloadHandlerAudioClip downloadHandler)
            {
                downloadHandler.streamAudio = true;
            }
            else
            {
                var handler = new DownloadHandlerAudioClip(audioUrl, AudioType.MPEG)
                {
                    streamAudio = true
                };
                www.downloadHandler = handler;
            }

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                MUPLogger.Error($"VoiceManager: Failed to load audio clip from {audioUrl}. Error: {www.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                PlayClip(clip);
            }
        }

        private AudioClip LoadAudioClip(string pKey)
        {
            AudioClip clip = null;
#if UNITY_EDITOR
#if I2LOC_PRESENT
            string editorPath = $"Assets/Voices/{LocalizationManager.CurrentLanguageCode}/{pKey}";
#else
            string editorPath = $"Assets/Voices/{pKey}";
#endif
            clip = AssetDatabase.LoadAssetAtPath<AudioClip>(editorPath);
#endif
            if (clip == null)
            {
#if I2LOC_PRESENT
                clip = Resources.Load<AudioClip>($"Voices/{LocalizationManager.CurrentLanguageCode}/{pKey}");
                if (clip == null)
                    clip = Resources.Load<AudioClip>($"Voices/{pKey}") ?? Resources.Load<AudioClip>(pKey);
#else
                clip = Resources.Load<AudioClip>($"Voices/{pKey}") ?? Resources.Load<AudioClip>(pKey);
#endif
            }

            return clip;
        }
    }
}
