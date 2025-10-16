using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Service responsible for playing back audio clips (music, sound effects, voice).
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioPlaybackService : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        private readonly Dictionary<AudioManager.AudioType, AudioSource> sources = new();

        private void Awake()
        {
            sources.Clear();
            ConfigureSource(AudioManager.AudioType.Music, ref musicSource, "MusicSource");
            ConfigureSource(AudioManager.AudioType.SFX, ref sfxSource, "SFXSource");
            ConfigureSource(AudioManager.AudioType.Voice, ref voiceSource, "VoiceSource");

            ServiceLocator.AddService<AudioPlaybackService>(gameObject, true);
        }

        private void ConfigureSource(AudioManager.AudioType audioType, ref AudioSource source, string defaultName)
        {
            if (source == null)
            {
                source = FindExistingSource(defaultName);
            }

            if (source == null)
            {
                var child = new GameObject(defaultName);
                child.transform.SetParent(transform);
                source = child.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // 2D by default

            if (source.outputAudioMixerGroup == null)
            {
                var mixerGroup = AudioManager.GetAudioMixerGroup(audioType);
                if (mixerGroup != null)
                {
                    source.outputAudioMixerGroup = mixerGroup;
                }
            }

            sources[audioType] = source;
        }

        private AudioSource FindExistingSource(string name)
        {
            var child = transform.Find(name);
            if (child != null)
            {
                return child.GetComponent<AudioSource>();
            }

            return null;
        }

        private AudioSource GetSource(AudioManager.AudioType audioType)
        {
            if (sources.TryGetValue(audioType, out var source) && source != null)
            {
                return source;
            }

            MUPLogger.Error($"AudioPlaybackService: No AudioSource configured for channel {audioType}.");
            return null;
        }

        /// <inheritdoc />
        public AudioSource PlayClip(AudioClip clip, AudioManager.AudioType audioType, bool loop = false, float volume = 1f)
        {
            if (clip == null)
            {
                MUPLogger.Warning("AudioPlaybackService.PlayClip called with a null AudioClip.");
                return null;
            }

            var source = GetSource(audioType);
            if (source == null)
            {
                return null;
            }

            volume = Mathf.Clamp01(volume);

            if (source.isPlaying)
            {
                source.Stop();
            }

            source.loop = loop;
            source.volume = volume;
            source.clip = clip;
            source.Play();

            return source;
        }

        public AudioClip PlayFromResources(string resourcePath, AudioManager.AudioType audioType, bool loop = false, float volume = 1f)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                MUPLogger.Warning("AudioPlaybackService.PlayFromResources was called with an empty resource path.");
                return null;
            }

            var clip = Resources.Load<AudioClip>(resourcePath);
            if (clip == null)
            {
                MUPLogger.Error($"AudioPlaybackService: Unable to load AudioClip at Resources/{resourcePath}.");
                return null;
            }

            PlayClip(clip, audioType, loop, volume);
            return clip;
        }

        /// <summary>
        /// Helper that routes directly to the music channel.
        /// </summary>
        public AudioSource PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
            => PlayClip(clip, AudioManager.AudioType.Music, loop, volume);

        /// <summary>
        /// Helper that plays a non-looping sound effect.
        /// </summary>
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f)
            => PlayClip(clip, AudioManager.AudioType.SFX, false, volume);

        /// <summary>
        /// Helper that routes to the voice channel.
        /// </summary>
        public AudioSource PlayVoice(AudioClip clip, bool loop = false, float volume = 1f)
            => PlayClip(clip, AudioManager.AudioType.Voice, loop, volume);

        /// <inheritdoc />
        public void Stop(AudioManager.AudioType audioType)
        {
            var source = GetSource(audioType);
            if (source == null)
            {
                return;
            }

            source.Stop();
            if (audioType != AudioManager.AudioType.SFX)
            {
                source.clip = null;
            }
        }

        /// <inheritdoc />
        public void StopAll()
        {
            foreach (var pair in sources)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                pair.Value.Stop();
                if (pair.Key != AudioManager.AudioType.SFX)
                {
                    pair.Value.clip = null;
                }
            }
        }
    }
}
