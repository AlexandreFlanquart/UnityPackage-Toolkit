using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Static manager for audio settings including volume and mute states for different audio groups (Music, SFX, Voice)
    /// </summary>
    public static class AudioManager
    {
        public enum AudioType
        {
            Music,
            SFX,
            Voice
        }
        public struct AudioSettings
        {
            public string AUDIO_NAME;
            public float defaultVolume;
            public float currentVolume;
            public bool isMuted;
        }

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

        private static AudioSettings musicSettings;
        private static AudioSettings sfxSettings;
        private static AudioSettings voiceSettings;

        // Default values
        private const float MIN_VOLUME = 0.0001f; // -80dB
        private const float MAX_VOLUME = 1f; // 0dB

        #region Initialization
        /// <summary>
        /// Initializes the AudioManager and sets default volumes
        /// </summary>
        public static void Initialize()
        {
            musicSettings = new AudioSettings { AUDIO_NAME = AudioType.Music.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            sfxSettings = new AudioSettings { AUDIO_NAME = AudioType.SFX.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            voiceSettings = new AudioSettings { AUDIO_NAME = AudioType.Voice.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };

            SetMusicVolume(musicSettings.defaultVolume);
            SetSFXVolume(sfxSettings.defaultVolume);
            SetVoiceVolume(voiceSettings.defaultVolume);
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
        public static void SetMusicVolume(float volume) => SetVolume(musicSettings.AUDIO_NAME, volume);
        public static float GetMusicVolume() => GetVolume(musicSettings.AUDIO_NAME);

        // SFX Volume
        public static void SetSFXVolume(float volume) => SetVolume(sfxSettings.AUDIO_NAME, volume);
        public static float GetSFXVolume() => GetVolume(sfxSettings.AUDIO_NAME);

        // Voice Volume
        public static void SetVoiceVolume(float volume) => SetVolume(voiceSettings.AUDIO_NAME, volume);
        public static float GetVoiceVolume() => GetVolume(voiceSettings.AUDIO_NAME);

        public static void SetVolume(AudioType audioType, float volume) => SetVolume(audioType.ToString(), volume);
        public static float GetVolume(AudioType audioType) => GetVolume(audioType.ToString());
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
                SetVolume(parameter, musicSettings.currentVolume);
            }
            else
            {
                SetVolume(parameter, MIN_VOLUME);
            }
        }

        // Public Mute Methods
        public static void ToggleMusicMute() => ToggleMute(musicSettings.AUDIO_NAME);
        public static void ToggleSFXMute() => ToggleMute(sfxSettings.AUDIO_NAME);
        public static void ToggleVoiceMute() => ToggleMute(voiceSettings.AUDIO_NAME);
        public static void ToggleMute(AudioType audioType) => ToggleMute(audioType.ToString());
        #endregion
    }
}