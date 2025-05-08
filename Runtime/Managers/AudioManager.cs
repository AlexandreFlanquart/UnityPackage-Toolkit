using UnityEngine;
using UnityEngine.Audio;

namespace Prismify.Toolkit
{
    /// <summary>
    /// Static manager for audio settings including volume and mute states for different audio groups (Music, SFX, Voice)
    /// </summary>
    public static class AudioManager
    {
        private static AudioMixer _audioMixer;
        private static AudioMixer AudioMixer
        {
            get
            {
                if (_audioMixer == null)
                {
                    _audioMixer = Resources.Load<AudioMixer>("AudioMixer");
                    if (_audioMixer == null)
                    {
                        Debug.LogError("AudioMixer not found in Resources folder. Please ensure it exists at 'Resources/AudioMixer'");
                    }
                }
                return _audioMixer;
            }
        }
        
        // Exposed parameters in the AudioMixer
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SFXVolume";
        private const string VOICE_VOLUME_PARAM = "VoiceVolume";
        
        // Default values
        private const float DEFAULT_VOLUME = 0.8f;
        private const float MIN_VOLUME = 0.0001f; // -80dB
        private const float MAX_VOLUME = 1f; // 0dB

        #region Initialization
        /// <summary>
        /// Initializes the AudioManager and sets default volumes
        /// </summary>
        public static void Initialize()
        {
            SetMusicVolume(DEFAULT_VOLUME);
            SetSFXVolume(DEFAULT_VOLUME);
            SetVoiceVolume(DEFAULT_VOLUME);
        }
        #endregion

        #region Volume Control Methods
        /// <summary>
        /// Sets the volume for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        /// <param name="volume">Volume value between 0 and 1</param>
        private static void SetVolume(string parameter, float volume)
        {
            if (AudioMixer == null) return;
            
            // Convert linear volume (0-1) to dB (-80 to 0)
            float dB = volume <= MIN_VOLUME ? -80f : Mathf.Log10(volume) * 20f;
            AudioMixer.SetFloat(parameter, dB);
        }

        /// <summary>
        /// Gets the current volume for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        /// <returns>Volume value between 0 and 1</returns>
        private static float GetVolume(string parameter)
        {
            if (AudioMixer == null) return 0f;
            
            float dB;
            AudioMixer.GetFloat(parameter, out dB);
            return dB <= -80f ? 0f : Mathf.Pow(10f, dB / 20f);
        }
        #endregion

        #region Public Volume Methods
        // Music Volume
        public static void SetMusicVolume(float volume) => SetVolume(MUSIC_VOLUME_PARAM, volume);
        public static float GetMusicVolume() => GetVolume(MUSIC_VOLUME_PARAM);

        // SFX Volume
        public static void SetSFXVolume(float volume) => SetVolume(SFX_VOLUME_PARAM, volume);
        public static float GetSFXVolume() => GetVolume(SFX_VOLUME_PARAM);

        // Voice Volume
        public static void SetVoiceVolume(float volume) => SetVolume(VOICE_VOLUME_PARAM, volume);
        public static float GetVoiceVolume() => GetVolume(VOICE_VOLUME_PARAM);
        #endregion

        #region Mute Control Methods
        /// <summary>
        /// Toggles mute state for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        private static void ToggleMute(string parameter)
        {
            if (AudioMixer == null) return;
            
            float currentVolume;
            AudioMixer.GetFloat(parameter, out currentVolume);
            
            // If already muted (-80dB), restore to default volume
            if (currentVolume <= -80f)
            {
                SetVolume(parameter, DEFAULT_VOLUME);
            }
            else
            {
                SetVolume(parameter, MIN_VOLUME);
            }
        }

        // Public Mute Methods
        public static void ToggleMusicMute() => ToggleMute(MUSIC_VOLUME_PARAM);
        public static void ToggleSFXMute() => ToggleMute(SFX_VOLUME_PARAM);
        public static void ToggleVoiceMute() => ToggleMute(VOICE_VOLUME_PARAM);
        #endregion
    }
} 