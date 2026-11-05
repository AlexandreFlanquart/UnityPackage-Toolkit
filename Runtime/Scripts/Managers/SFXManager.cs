using UnityEngine;
using UnityEngine.Audio;

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
    /// Pool-based player for every one-shot sound: UI clicks, 2D effects and 3D sounds at a world position.
    /// Several sounds overlap freely; each one gets its own source, so pitch randomisation never leaks to another sound.
    /// Respects per-<see cref="SoundSO"/> cooldowns, instance limits and priority.
    /// </summary>
    [DisallowMultipleComponent]
    public class SFXManager : MonoBehaviour
    {
        [Tooltip("Number of sounds that can play at the same time.")]
        [SerializeField, Min(1)] private int poolSize = 16;
        [Tooltip("Behaviour when every source is busy: drop the new sound, or interrupt the oldest one if the new sound's priority is equal or higher.")]
        [SerializeField] private SFXStealStrategy stealStrategy = SFXStealStrategy.StealOldest;

        [Header("3D sounds (Play with a position)")]
        [Tooltip("Distance in world units under which a 3D sound plays at full volume.")]
        [SerializeField, Min(0f)] private float spatialMinDistance = 1f;
        [Tooltip("Distance in world units beyond which a 3D sound is silent (linear rolloff).")]
        [SerializeField, Min(0f)] private float spatialMaxDistance = 15f;

        private class PooledSource
        {
            public AudioSource source;
            public SoundSO sound;
            public int generation;
            public float startTime;
        }

        private PooledSource[] _pool;

        private void Awake()
        {
            EnsurePool();
            ServiceLocator.AddService<SFXManager>(gameObject, true);
            MUPLogger.Info("SFXManager initialized.");
        }

        // Also called from Play: a script's Awake can reach this manager before its own Awake
        // (ServiceLocator falls back to a scene search).
        private void EnsurePool()
        {
            if (_pool != null) return;

            _pool = new PooledSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"SFXSource_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.dopplerLevel = 0f;
                src.rolloffMode = AudioRolloffMode.Linear;
                _pool[i] = new PooledSource { source = src };
            }
        }

        // The mixer ignores SetFloat until the scene has started: push the bus volumes now.
        private void Start()
        {
            AudioManager.ApplyAll();
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<SFXManager>(this);

            if (_pool == null) return;

            // Release sounds still tracked as playing, otherwise their maxInstances counters stay
            // incremented forever (ScriptableObject state outlives this scene).
            for (int i = 0; i < _pool.Length; i++)
            {
                ReleaseSound(i);
            }
        }

        // Releases the instance slot of sounds that ended on their own.
        private void Update()
        {
            for (int i = 0; i < _pool.Length; i++)
            {
                if (_pool[i].sound != null && !AudioManager.IsBusy(_pool[i].source) && !AudioListener.pause)
                {
                    ReleaseSound(i);
                }
            }
        }

        #region Play
        /// <summary>Plays a sound in 2D. Output: the sound's group, or the SFX group of the AudioConfig.</summary>
        public SoundHandle Play(SoundSO sound) => PlaySound(sound, null, DefaultGroup(false));

        /// <summary>Plays a sound in 3D at a world position. Output: the sound's group, or the World group of the AudioConfig.</summary>
        public SoundHandle Play(SoundSO sound, Vector3 position) => PlaySound(sound, position, DefaultGroup(true));

        /// <summary>Plays a UI sound in 2D. Output: the sound's group, or the UI group of the AudioConfig.</summary>
        public SoundHandle PlayUI(SoundSO sound)
        {
            var config = AudioManager.Config;
            return PlaySound(sound, null, config != null ? config.UIGroup : null);
        }

        /// <summary>
        /// Plays a raw clip in 2D, without SoundSO rules (no randomisation, cooldown or instance limit).
        /// Output: <paramref name="output"/>, or the SFX group of the AudioConfig.
        /// </summary>
        public SoundHandle Play(AudioClip clip, float volume = 1f, AudioMixerGroup output = null)
        {
            if (clip == null) return default;

            int index = GetFreeIndex(128);
            if (index < 0) return default;

            return StartSource(index, clip, output != null ? output : DefaultGroup(false), volume, 1f, 128, null, null);
        }

        private SoundHandle PlaySound(SoundSO sound, Vector3? position, AudioMixerGroup fallbackOutput)
        {
            if (sound == null || !sound.CanPlay()) return default;

            var clip = sound.GetClip();
            if (clip == null) return default;

            int index = GetFreeIndex(sound.Priority);
            if (index < 0) return default;

            var output = sound.Output != null ? sound.Output : fallbackOutput;
            var handle = StartSource(index, clip, output, sound.GetVolume(), sound.GetPitch(), sound.Priority, position, sound);
            sound.OnPlay();
            return handle;
        }

        private SoundHandle StartSource(int index, AudioClip clip, AudioMixerGroup output, float volume, float pitch,
                                        int priority, Vector3? position, SoundSO sound)
        {
            ReleaseSound(index);

            var pooled = _pool[index];
            var source = pooled.source;
            source.outputAudioMixerGroup = output;
            source.volume = volume;
            source.pitch = pitch;
            source.priority = priority;
            source.spatialBlend = position.HasValue ? 1f : 0f;
            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.minDistance = spatialMinDistance;
                source.maxDistance = spatialMaxDistance;
            }
            source.clip = clip;
            source.Play();

            pooled.sound = sound;
            pooled.generation++;
            pooled.startTime = Time.realtimeSinceStartup;
            return new SoundHandle(this, index, pooled.generation);
        }

        private AudioMixerGroup DefaultGroup(bool spatial)
        {
            var config = AudioManager.Config;
            if (config == null) return null;
            return spatial ? config.WorldGroup : config.SFXGroup;
        }
        #endregion

        #region Handles
        internal bool IsPlaying(SoundHandle handle)
        {
            var pooled = GetPooled(handle);
            return pooled != null && AudioManager.IsBusy(pooled.source);
        }

        internal void Stop(SoundHandle handle)
        {
            var pooled = GetPooled(handle);
            if (pooled == null) return;

            pooled.source.Stop();
            ReleaseSound(handle.SourceIndex);
        }

        // Null when the handle is empty or its source has since been reused by another sound.
        private PooledSource GetPooled(SoundHandle handle)
        {
            if (_pool == null || handle.SourceIndex < 0 || handle.SourceIndex >= _pool.Length) return null;
            var pooled = _pool[handle.SourceIndex];
            return pooled.generation == handle.Generation && handle.Generation > 0 ? pooled : null;
        }
        #endregion

        private void ReleaseSound(int index)
        {
            var pooled = _pool[index];
            if (pooled.sound == null) return;
            pooled.sound.OnStop();
            pooled.sound = null;
        }

        private int GetFreeIndex(int requestPriority)
        {
            EnsurePool();
            int oldest = -1;
            float oldestTime = float.MaxValue;

            for (int i = 0; i < _pool.Length; i++)
            {
                // While AudioListener.pause is on, every source reports not playing: only reuse released ones.
                bool busy = AudioManager.IsBusy(_pool[i].source) || (AudioListener.pause && _pool[i].sound != null);
                if (!busy) return i;

                if (_pool[i].startTime < oldestTime)
                {
                    oldestTime = _pool[i].startTime;
                    oldest = i;
                }
            }

            if (stealStrategy == SFXStealStrategy.DropNew)
            {
                MUPLogger.Warning("SFXManager: pool full, dropping new sound (DropNew).");
                return -1;
            }

            // Only steal if the incoming request has equal or higher priority (lower number = higher priority)
            if (oldest >= 0 && requestPriority <= _pool[oldest].source.priority)
            {
                MUPLogger.Warning("SFXManager: pool full, stealing oldest source (StealOldest).");
                return oldest;
            }

            MUPLogger.Warning("SFXManager: pool full and request priority too low, dropping.");
            return -1;
        }
    }
}
