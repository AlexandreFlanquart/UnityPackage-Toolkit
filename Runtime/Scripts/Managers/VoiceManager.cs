using System;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays one voice at a time (instructions, dialogue lines). A new voice always interrupts the current one.
    /// <see cref="VoiceLineSO"/> picks the clip matching the current I2 Localization language.
    /// </summary>
    [DisallowMultipleComponent]
    public class VoiceManager : MonoBehaviour
    {
        [Tooltip("Optional. Created automatically when empty and routed to the Voice group of the AudioConfig.")]
        [SerializeField] private AudioSource audioSource;

        private bool _isPlaying;
        private bool _sourceReady;
        private int _playId;

        /// <summary>Raised when the current voice reaches its end. Not raised when it is interrupted or stopped.</summary>
        public event Action VoiceEnded;

        /// <summary>Clip currently loaded on the voice source, or null.</summary>
        public AudioClip CurrentClip => audioSource != null ? audioSource.clip : null;

        /// <summary>True while a voice is playing.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>Incremented on every <see cref="Play(AudioClip)"/>: identifies one playback of a voice.</summary>
        public int PlayId => _playId;

        private void Awake()
        {
            EnsureSource();
            ServiceLocator.AddService<VoiceManager>(gameObject, true);
            MUPLogger.Info("VoiceManager initialized.");
        }

        // Also called from Play: a VoiceAgent enabled before this Awake can already reach the manager
        // (ServiceLocator falls back to a scene search).
        private void EnsureSource()
        {
            if (_sourceReady) return;

            if (audioSource == null)
            {
                var go = new GameObject("VoiceSource");
                go.transform.SetParent(transform);
                audioSource = go.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            if (audioSource.outputAudioMixerGroup == null && AudioManager.Config != null)
            {
                audioSource.outputAudioMixerGroup = AudioManager.Config.VoiceGroup;
            }
            _sourceReady = true;
        }

        // The mixer ignores SetFloat until the scene has started: push the bus volumes now.
        private void Start()
        {
            AudioManager.ApplyAll();
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<VoiceManager>(this);
        }

        // AudioListener.pause (app in background) also reports isPlaying = false: not an end of voice.
        private void Update()
        {
            if (_isPlaying && !AudioManager.IsBusy(audioSource) && !AudioListener.pause)
            {
                _isPlaying = false;
                VoiceEnded?.Invoke();
            }
        }

        /// <summary>Plays the line in the current language, interrupting any voice in progress.</summary>
        public void Play(VoiceLineSO voiceLine)
        {
            if (voiceLine == null)
            {
                MUPLogger.Warning("VoiceManager.Play called with a null VoiceLineSO.");
                return;
            }
            Play(voiceLine.GetClip());
        }

        /// <summary>Plays a clip, interrupting any voice in progress. Playing the current clip again restarts it.</summary>
        public void Play(AudioClip clip)
        {
            if (clip == null)
            {
                MUPLogger.Warning("VoiceManager.Play called with a null clip.");
                return;
            }

            EnsureSource();
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            _isPlaying = true;
            _playId++;
            MUPLogger.Info($"VoiceManager: playing '{clip.name}'.", this);
        }

        /// <summary>Stops the current voice.</summary>
        public void Stop()
        {
            if (audioSource != null) audioSource.Stop();
            _isPlaying = false;
        }
    }
}
