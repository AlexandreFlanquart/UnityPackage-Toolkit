using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays any number of 2D music loops at the same time (music, ambience beds), each with its own volume.
    /// A track is identified by its <see cref="AudioClip"/>. All tracks go through the Music group of the AudioConfig.
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicManager : MonoBehaviour
    {
        [Tooltip("Fade duration in seconds used when a call passes no explicit duration.")]
        [FormerlySerializedAs("defaultCrossfadeDuration")]
        [SerializeField, Min(0f)] private float defaultFadeDuration = 1f;

        private class Track
        {
            public AudioSource source;
            public Coroutine fade;
        }

        private readonly List<Track> _tracks = new();

        private void Awake()
        {
            ServiceLocator.AddService<MusicManager>(gameObject, true);
            MUPLogger.Info("MusicManager initialized.");
        }

        // The mixer ignores SetFloat until the scene has started: push the bus volumes now.
        private void Start()
        {
            AudioManager.ApplyAll();
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<MusicManager>(this);
        }

        /// <summary>
        /// Starts a looping track and fades it to <paramref name="volume"/>.
        /// If the clip is already playing, only its volume changes (the track does not restart).
        /// </summary>
        /// <param name="volume">Track volume [0, 1], multiplied by the Music bus volume.</param>
        /// <param name="fadeDuration">Seconds. Negative = default fade duration.</param>
        public void Play(AudioClip clip, float volume = 1f, float fadeDuration = -1f)
        {
            if (clip == null)
            {
                MUPLogger.Warning("MusicManager.Play called with a null clip.");
                return;
            }

            var track = FindTrack(clip);
            if (track == null)
            {
                track = GetFreeTrack();
                track.source.clip = clip;
                track.source.volume = 0f;
                track.source.Play();
                MUPLogger.Info($"MusicManager: playing '{clip.name}'.");
            }
            else if (!AudioManager.IsBusy(track.source) && !AudioListener.pause)
            {
                // Source stopped without being released (e.g. this object was disabled during a fade).
                track.source.Play();
            }

            FadeTo(track, volume, fadeDuration, false);
        }

        /// <summary>Fades a playing track to <paramref name="volume"/> [0, 1] without stopping it.</summary>
        public void SetVolume(AudioClip clip, float volume, float fadeDuration = -1f)
        {
            var track = FindTrack(clip);
            if (track == null)
            {
                MUPLogger.Warning($"MusicManager.SetVolume: '{(clip != null ? clip.name : "null")}' is not playing.");
                return;
            }

            FadeTo(track, volume, fadeDuration, false);
        }

        /// <summary>Fades a track out, then stops it.</summary>
        public void Stop(AudioClip clip, float fadeDuration = -1f)
        {
            var track = FindTrack(clip);
            if (track == null) return;

            FadeTo(track, 0f, fadeDuration, true);
            MUPLogger.Info($"MusicManager: stopping '{clip.name}'.");
        }

        /// <summary>Fades every track out, then stops them.</summary>
        public void StopAll(float fadeDuration = -1f)
        {
            foreach (var track in _tracks)
            {
                if (track.source.clip != null)
                {
                    FadeTo(track, 0f, fadeDuration, true);
                }
            }
        }

        /// <summary>True while the clip is playing, including during its fade-out.</summary>
        public bool IsPlaying(AudioClip clip) => FindTrack(clip) != null;

        private Track FindTrack(AudioClip clip)
        {
            if (clip == null) return null;
            foreach (var track in _tracks)
            {
                if (track.source.clip == clip) return track;
            }
            return null;
        }

        // Reuses a stopped source, or creates a new one: the number of simultaneous tracks is not capped.
        private Track GetFreeTrack()
        {
            foreach (var track in _tracks)
            {
                if (track.source.clip == null) return track;
            }

            var go = new GameObject($"MusicSource_{_tracks.Count}");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = AudioManager.Config != null ? AudioManager.Config.MusicGroup : null;

            var newTrack = new Track { source = source };
            _tracks.Add(newTrack);
            return newTrack;
        }

        private void FadeTo(Track track, float volume, float fadeDuration, bool stopAtEnd)
        {
            if (track.fade != null)
            {
                StopCoroutine(track.fade);
                track.fade = null;
            }
            if (fadeDuration < 0f) fadeDuration = defaultFadeDuration;
            volume = Mathf.Clamp01(volume);

            // No fade: apply at once. A slider calls SetVolume every frame and must not start a coroutine each time.
            if (fadeDuration <= 0f)
            {
                track.source.volume = volume;
                if (stopAtEnd) Release(track);
                return;
            }
            track.fade = StartCoroutine(FadeRoutine(track, volume, fadeDuration, stopAtEnd));
        }

        private static void Release(Track track)
        {
            track.source.Stop();
            track.source.clip = null;
        }

        // Unscaled time: fades keep running while the game is paused (Time.timeScale = 0).
        private IEnumerator FadeRoutine(Track track, float targetVolume, float duration, bool stopAtEnd)
        {
            var source = track.source;
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
            if (stopAtEnd) Release(track);
            track.fade = null;
        }
    }
}
