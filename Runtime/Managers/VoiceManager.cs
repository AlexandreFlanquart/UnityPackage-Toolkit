using System.Collections;
using System.Collections.Generic;
#if I2LOC_PRESENT
using I2.Loc;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Prismify.Toolkit
{
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
            audioSource.volume = globalVolume;
        }

        void OnDestroy()
        {
            ClearCache();
        }

        public void ConfigVoiceElement(GameObject pElement, float pDelay, params string[] pKeys)
        {
            VoiceAgent agent = pElement.GetComponent<VoiceAgent>();
            if (!agent)
            {
                agent = pElement.AddComponent<VoiceAgent>();
            }
            agent.Initialize(pDelay, pKeys);
        }

        public void ConfigVoiceElement(GameObject pElement, params string[] pKeys)
        {
            ConfigVoiceElement(pElement, 0, pKeys);
        }

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
                #if !UNITY_EDITOR
                    StartCoroutine(AudioStreaming(audioUrl));
                #endif
                return;
            }
            else
            {
                if (clipCache.TryGetValue(pKey, out AudioClip cachedClip))
                {
                    PlayClip(cachedClip);
                    return;
                }

                string bundleName = $"{pKey}";
                string path = $"Assets/Voices/{pKey}";
                #if UNITY_EDITOR
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                #elif UNITY_WEBGL
                    AudioClip clip = ServiceLocator.GetService<AssetBundleManager>().GetAsset<AudioClip>(bundleName, path);
                #endif
                
                if (clip != null)
                {
                    clipCache[pKey] = clip;
                }
                PlayClip(clip);
            }   
        }

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

        public bool Mute()
        {
            audioSource.mute = !audioSource.mute;
            return audioSource.mute;
        }

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

        public float GetCurrentVoiceDuration()
        {
            return audioSource.clip != null ? audioSource.clip.length : 0f;
        }

        private void ClearCache()
        {
            foreach (var clip in clipCache.Values)
            {
                if (clip != null)
                {
                    clip.UnloadAudioData();
                }
            }
            clipCache.Clear();
        }

        private IEnumerator AudioStreaming(string audioUrl)
        {
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG);
            DownloadHandlerAudioClip dHA = new DownloadHandlerAudioClip(string.Empty, AudioType.MPEG);
            dHA.streamAudio = true;
            www.downloadHandler = dHA;
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return new WaitForSeconds(.1f);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"VoiceManager: Failed to load audio clip from {audioUrl}. Error: {www.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                PlayClip(clip);
            }
        }
    }
}
