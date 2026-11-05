using System;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// The three volume buses exposed to the player. Each one matches an AudioMixer exposed parameter of the same name.
    /// Values are serialized (AudioUpdater): do not reorder.
    /// </summary>
    public enum AudioBus
    {
        Music = 0,
        SFX = 1,
        Voice = 2
    }

    /// <summary>
    /// Static manager for the volume and mute state of the three <see cref="AudioBus"/> buses.
    /// Reads its setup from the <see cref="AudioConfigSO"/> at <c>Resources/AudioConfig</c>.
    /// Volumes are not persisted here: listen to <see cref="VolumeChanged"/> and restore them with <see cref="SetVolume"/>.
    /// </summary>
    public static class AudioManager
    {
        public const string ConfigResourcePath = "AudioConfig";
        public const float MuteThreshold = 0.0001f; // -80dB
        private const float MutedDecibels = -80f;
        // Restored by ToggleMute when a bus starts muted and has no audible volume to come back to.
        private const float FallbackUnmuteVolume = 1f;

        private class BusState
        {
            public float volume;
            public float volumeBeforeMute;
        }

        private static AudioConfigSO _config;
        private static bool _configLoadAttempted;
        private static BusState[] _buses;
        private static bool _missingParameterReported;

        /// <summary>Raised whenever a bus volume changes (slider, mute toggle or code), with the new linear volume [0, 1].</summary>
        public static event Action<AudioBus, float> VolumeChanged;

        /// <summary>Project audio configuration, loaded once from <c>Resources/AudioConfig</c>. Null if missing.</summary>
        public static AudioConfigSO Config
        {
            get
            {
                // Loaded and reported once: a missing config must not log on every sound played.
                if (_config == null && !_configLoadAttempted)
                {
                    _configLoadAttempted = true;
                    _config = Resources.Load<AudioConfigSO>(ConfigResourcePath);
                    if (_config == null)
                    {
                        MUPLogger.Error($"AudioConfigSO not found. Create one at 'Resources/{ConfigResourcePath}'.");
                    }
                    else if (_config.Mixer == null)
                    {
                        MUPLogger.Warning($"AudioConfigSO '{_config.name}' has no mixer: volume sliders will have no effect.", _config);
                    }
                }
                return _config;
            }
        }

        #region Initialization
        // Static state survives Play mode when domain reload is disabled: reset it explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _config = null;
            _configLoadAttempted = false;
            _buses = null;
            _missingParameterReported = false;
            VolumeChanged = null;
        }

        private static void EnsureInitialized()
        {
            if (_buses != null) return;

            var busCount = Enum.GetValues(typeof(AudioBus)).Length;
            _buses = new BusState[busCount];
            for (int i = 0; i < busCount; i++)
            {
                float volume = Config != null ? Config.GetDefaultVolume((AudioBus)i) : 1f;
                _buses[i] = new BusState { volume = volume, volumeBeforeMute = volume > MuteThreshold ? volume : FallbackUnmuteVolume };
            }
            MUPLogger.Info("AudioManager initialized.");
        }

        /// <summary>
        /// Pushes every bus volume to the AudioMixer again. Called by the players in <c>Start</c>:
        /// <c>AudioMixer.SetFloat</c> calls made before the first <c>Start</c> of a scene are ignored by the mixer.
        /// </summary>
        public static void ApplyAll()
        {
            EnsureInitialized();
            for (int i = 0; i < _buses.Length; i++)
            {
                Apply((AudioBus)i);
            }
        }

        // Null for a value outside the enum, e.g. an AudioUpdater still serialized with the removed Ambience (3) or UI (4).
        private static BusState GetState(AudioBus bus)
        {
            EnsureInitialized();
            if ((uint)bus < (uint)_buses.Length) return _buses[(int)bus];

            MUPLogger.Warning($"AudioManager: unknown bus value {(int)bus}. Valid buses: Music, SFX, Voice.");
            return null;
        }
        #endregion

        #region Volume
        /// <summary>Sets the linear volume [0, 1] of a bus. 0 counts as muted.</summary>
        /// <param name="notify">False to skip <see cref="VolumeChanged"/>, e.g. when restoring saved volumes.</param>
        public static void SetVolume(AudioBus bus, float volume, bool notify = true)
        {
            var state = GetState(bus);
            if (state == null) return;

            volume = Mathf.Clamp01(volume);
            if (volume > MuteThreshold)
            {
                state.volumeBeforeMute = volume;
            }
            state.volume = volume;

            Apply(bus);
            if (notify) VolumeChanged?.Invoke(bus, volume);
        }

        /// <summary>Returns the linear volume [0, 1] of a bus.</summary>
        public static float GetVolume(AudioBus bus)
        {
            var state = GetState(bus);
            return state != null ? state.volume : 0f;
        }

        private static void Apply(AudioBus bus)
        {
            if (Config == null || Config.Mixer == null) return;

            float volume = GetState(bus).volume;
            float decibels = volume <= MuteThreshold ? MutedDecibels : Mathf.Log10(volume) * 20f;
            if (!Config.Mixer.SetFloat(bus.ToString(), decibels) && !_missingParameterReported)
            {
                // Reported once: a slider drag would otherwise log on every frame.
                _missingParameterReported = true;
                MUPLogger.Warning($"AudioManager: exposed parameter '{bus}' not found on mixer '{Config.Mixer.name}'.");
            }
        }
        #endregion

        /// <summary>
        /// True while the source plays, or while its clip is still loading after <c>Play()</c>
        /// (clips without Preload Audio Data can report <c>isPlaying = false</c> for a few frames).
        /// </summary>
        internal static bool IsBusy(AudioSource source)
        {
            if (source.isPlaying) return true;
            return source.clip != null && source.clip.loadState == AudioDataLoadState.Loading;
        }

        #region Mute
        /// <summary>True when the bus volume is at 0.</summary>
        public static bool IsMuted(AudioBus bus) => GetVolume(bus) <= MuteThreshold;

        /// <summary>Mutes the bus, or restores the volume it had before being muted.</summary>
        public static void ToggleMute(AudioBus bus)
        {
            var state = GetState(bus);
            if (state == null) return;
            SetVolume(bus, IsMuted(bus) ? state.volumeBeforeMute : 0f);
        }

        /// <summary>Returns the muted or unmuted sprite of the AudioConfig matching the bus state, or null.</summary>
        public static Sprite GetMuteSprite(AudioBus bus)
        {
            if (Config == null) return null;
            return IsMuted(bus) ? Config.MutedSprite : Config.UnmutedSprite;
        }
        #endregion
    }
}
