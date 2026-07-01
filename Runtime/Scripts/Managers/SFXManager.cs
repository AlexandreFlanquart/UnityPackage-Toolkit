using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>What happens when the SFX pool is full and a new sound is requested.</summary>
    public enum SFXStealStrategy
    {
        /// <summary>The new request is silently dropped.</summary>
        DropNew,
        /// <summary>The oldest playing source is interrupted and reused.</summary>
        StealOldest
    }

    /// <summary>
    /// Pool-based SFX manager. Supports 2D and fire-and-forget 3D sounds.
    /// Respects per-<see cref="AudioCueSO"/> cooldowns, instance limits, and priority.
    /// </summary>
    [DisallowMultipleComponent]
    public class SFXManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int poolSize = 16;
        [SerializeField] private SFXStealStrategy stealStrategy = SFXStealStrategy.StealOldest;

        private AudioSource[] _pool;

        // Maps each source to the cue currently using it + its release coroutine.
        // Used to properly clean up when a source is stolen.
        private readonly Dictionary<AudioSource, (AudioCueSO cue, Coroutine coroutine, float startTime)> _sourceMap = new();

        private void Awake()
        {
            _pool = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"SFXSource_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                _pool[i] = src;
            }
            ServiceLocator.AddService<SFXManager>(gameObject, true);
            MUPLogger.Info("SFXManager initialized.");
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<SFXManager>(this);

            // Release any cues still tracked as playing, otherwise their maxInstances/cooldown
            // counters stay incremented forever (ScriptableObject state outlives this scene).
            foreach (var entry in _sourceMap.Values)
            {
                entry.cue?.OnStop();
            }
            _sourceMap.Clear();
        }

        /// <summary>
        /// Plays an <see cref="AudioCueSO"/>. Applies randomised volume/pitch, respects cooldown and instance limit.
        /// </summary>
        /// <param name="cue">The sound definition to play.</param>
        /// <param name="position">World position for 3D sound. Null for 2D.</param>
        /// <returns>The <see cref="AudioSource"/> used, or <c>null</c> if the request was rejected.</returns>
        public AudioSource Play(AudioCueSO cue, Vector3? position = null)
        {
            if (cue == null || !cue.CanPlay()) return null;

            var clip = cue.GetClip();
            if (clip == null) return null;

            var source = GetFreeSource(cue.Priority);
            if (source == null) return null;

            AssignAndPlay(source, cue, clip, position);
            return source;
        }

        /// <summary>
        /// Plays a raw clip bypassing <see cref="AudioCueSO"/> rules. Useful for legacy callers.
        /// </summary>
        public AudioSource PlayRaw(AudioClip clip, AudioManager.AudioType audioType,
                                   float volume = 1f, float pitch = 1f, Vector3? position = null)
        {
            if (clip == null) return null;

            var source = GetFreeSource(128);
            if (source == null) return null;

            source.outputAudioMixerGroup = AudioManager.GetAudioMixerGroup(audioType);
            source.volume  = volume;
            source.pitch   = pitch;
            source.priority = 128;
            Configure3D(source, position);
            source.clip = clip;
            source.Play();

            // No cue tracking for raw playback
            _sourceMap[source] = (null, null, Time.realtimeSinceStartup);
            return source;
        }

        private void AssignAndPlay(AudioSource source, AudioCueSO cue, AudioClip clip, Vector3? position)
        {
            // If this source was already in use, cleanly stop the previous cue
            ReleasePreviousCue(source);

            source.outputAudioMixerGroup = AudioManager.GetAudioMixerGroup(cue.AudioType);
            source.volume  = cue.GetVolume();
            source.pitch   = cue.GetPitch();
            source.priority = cue.Priority;
            Configure3D(source, position);
            source.clip = clip;
            source.Play();

            cue.OnPlay();
            var coroutine = StartCoroutine(ReleaseWhenDone(cue, source));
            _sourceMap[source] = (cue, coroutine, Time.realtimeSinceStartup);
        }

        /// <summary>
        /// Stops the coroutine tracking a stolen source and notifies the old cue that its instance ended.
        /// </summary>
        private void ReleasePreviousCue(AudioSource source)
        {
            if (!_sourceMap.TryGetValue(source, out var entry)) return;
            if (entry.coroutine != null) StopCoroutine(entry.coroutine);
            entry.cue?.OnStop();
            _sourceMap.Remove(source);
        }

        private void Configure3D(AudioSource source, Vector3? position)
        {
            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.spatialBlend = 1f;
            }
            else
            {
                source.spatialBlend = 0f;
            }
        }

        private AudioSource GetFreeSource(int requestPriority)
        {
            AudioSource oldest = null;
            float oldestTime = float.MaxValue;

            foreach (var src in _pool)
            {
                if (!src.isPlaying) return src;

                if (_sourceMap.TryGetValue(src, out var entry) && entry.startTime < oldestTime)
                {
                    oldestTime = entry.startTime;
                    oldest = src;
                }
            }

            if (stealStrategy == SFXStealStrategy.DropNew)
            {
                MUPLogger.Warning("SFXManager: pool full, dropping new sound (DropNew).");
                return null;
            }

            // Only steal if the incoming request has equal or higher priority (lower number = higher priority)
            if (oldest != null && requestPriority <= oldest.priority)
            {
                MUPLogger.Warning("SFXManager: pool full, stealing oldest source (StealOldest).");
                return oldest;
            }

            MUPLogger.Warning("SFXManager: pool full and request priority too low, dropping.");
            return null;
        }

        private IEnumerator ReleaseWhenDone(AudioCueSO cue, AudioSource source)
        {
            yield return new WaitUntil(() => source == null || !source.isPlaying);
            cue.OnStop();
            _sourceMap.Remove(source);
        }
    }
}
