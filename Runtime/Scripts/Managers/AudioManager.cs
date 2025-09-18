using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

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
        public class AudioSetting
        {
            public string AUDIO_NAME;
            public float defaultVolume;
            public float currentVolume;
            public float beforeMutedVolume;
            public bool isMuted;
            public AudioSettingsSO settingsSO; // Référence au SO
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
                        MUPLogger.Error("AudioMixer not found in Resources folder. Please ensure it exists at 'Resources/AudioMixer'");
                    }
                }
                return _audioMixer;
            }
        }

        private static AudioSetting musicSetting;
        private static AudioSetting sfxSetting;
        private static AudioSetting voiceSetting;
        private static Dictionary<AudioType, AudioSetting> audioSettings;

        private static void EnsureInitialized()
        {
            if (audioSettings == null || musicSetting == null || sfxSetting == null || voiceSetting == null)
            {
                Initialize();
            }
        }

        // Default values
        private const float MIN_VOLUME = 0.0001f; // -80dB
        private const float MAX_VOLUME = 1f; // 0dB

        public static AudioSetting GetAudioSettingsFromAudioType(AudioType audioType)
        {
            EnsureInitialized();
            return audioSettings[audioType];
        }

        #region Initialization
        /// <summary>
        /// Initializes the AudioManager and sets default volumes
        /// </summary>
        public static void Initialize()
        {
            musicSetting = new AudioSetting { AUDIO_NAME = AudioType.Music.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            sfxSetting = new AudioSetting { AUDIO_NAME = AudioType.SFX.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            voiceSetting = new AudioSetting { AUDIO_NAME = AudioType.Voice.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            audioSettings = new Dictionary<AudioType, AudioSetting>
            {
                { AudioType.Music, musicSetting },
                { AudioType.SFX, sfxSetting },
                { AudioType.Voice, voiceSetting }
            };
            LoadAndApplyAudioSettingsSO();
        }

        private static void LoadAndApplyAudioSettingsSO()
        {
            MUPLogger.Info("LoadAndApplyAudioSettingsSO");
            var musicSO = Resources.Load<AudioSettingsSO>("AudioSettings/MusicSettingsSO");
            var sfxSO = Resources.Load<AudioSettingsSO>("AudioSettings/SFXSettingsSO");
            var voiceSO = Resources.Load<AudioSettingsSO>("AudioSettings/VoiceSettingsSO");

            if (musicSO != null)
            {
                MUPLogger.Info("MusicSettingsSO loaded");
                musicSetting.settingsSO = musicSO;
                SetVolume(AudioType.Music, musicSO.defaultVolume);
            }
            if (sfxSO != null)
            {
                sfxSetting.settingsSO = sfxSO;
                SetVolume(AudioType.SFX, sfxSO.defaultVolume);
            }
            if (voiceSO != null)
            {
                voiceSetting.settingsSO = voiceSO;
                SetVolume(AudioType.Voice, voiceSO.defaultVolume);
            }
        }

        // Méthodes d'accès aux SO
        public static AudioSettingsSO GetAudioSettingsSO(AudioType audioType)
        {
            return GetAudioSettingsFromAudioType(audioType).settingsSO;
        }
        #endregion

        #region Volume Control Methods

        public static void InitVolume(AudioType audioType, float volume)
        {
            EnsureInitialized();
            InitVolume(GetAudioSettingsFromAudioType(audioType), volume);
        }

        private static void InitVolume(AudioSetting audioSetting, float volume)
        {
            MUPLogger.Info("Init volume " + audioSetting.AUDIO_NAME);
            audioSetting.currentVolume = volume;
            audioSetting.defaultVolume = volume;
        }

        /// <summary>
        /// Sets the volume for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        /// <param name="volume">Volume value between 0 and 1</param>
        private static void SetVolume(AudioSetting audioSetting, float volume)
        {
            MUPLogger.Info("Set volume " + audioSetting.AUDIO_NAME + " to " + volume);
            if (AudioMixer == null) return;
            if(audioSetting.currentVolume <= 0f && volume >= 0)
                audioSetting.isMuted = false;
            else if(audioSetting.currentVolume >= 0f && volume <= 0)
                audioSetting.isMuted = true;

            // Convert linear volume (0-1) to dB (-80 to 0)
            float dB = volume <= MIN_VOLUME ? -80f : Mathf.Log10(volume) * 20f;
            AudioMixer.SetFloat(audioSetting.AUDIO_NAME, dB);
            
            audioSetting.currentVolume = volume;
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
        public static void SetMusicVolume(float volume)
        {
            EnsureInitialized();
            SetVolume(musicSetting, volume);
        }
        public static float GetMusicVolume()
        {
            EnsureInitialized();
            return GetVolume(musicSetting.AUDIO_NAME);
        }

        // SFX Volume
        public static void SetSFXVolume(float volume)
        {
            EnsureInitialized();
            SetVolume(sfxSetting, volume);
        }
        public static float GetSFXVolume()
        {
            EnsureInitialized();
            return GetVolume(sfxSetting.AUDIO_NAME);
        }

        // Voice Volume
        public static void SetVoiceVolume(float volume)
        {
            EnsureInitialized();
            SetVolume(voiceSetting, volume);
        }
        public static float GetVoiceVolume()
        {
            EnsureInitialized();
            return GetVolume(voiceSetting.AUDIO_NAME);
        }

        public static void SetVolume(AudioType audioType, float volume)
        {
            EnsureInitialized();
            SetVolume(GetAudioSettingsFromAudioType(audioType), volume);
        }
        public static float GetVolume(AudioType audioType)
        {
            EnsureInitialized();
            return GetVolume(audioType.ToString());
        }
        #endregion

        #region Mute Control Methods
        /// <summary>
        /// Toggles mute state for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        private static void ToggleMute(AudioSetting audioSetting)
        {
            if (AudioMixer == null) return;
            //MUPLogger.Info("audioType: " + audioSetting.currentVolume);
            float currentVolume;
            AudioMixer.GetFloat(audioSetting.AUDIO_NAME, out currentVolume);

            // If already muted (-80dB), restore to default volume
            if (currentVolume <= -80f)
            {
                SetVolume(audioSetting, audioSetting.beforeMutedVolume);
                audioSetting.isMuted = false;
            }
            else
            {
                audioSetting.beforeMutedVolume = audioSetting.currentVolume;
                SetVolume(audioSetting, MIN_VOLUME);
                audioSetting.isMuted = true;
            }
        }

        public static void ToggleMute(AudioType audioType)
        {
            EnsureInitialized();
            ToggleMute(GetAudioSettingsFromAudioType(audioType));
        }

        // Retourne la sprite mute/unmute selon l'état courant
        public static Sprite GetMuteSprite(AudioType audioType)
        {
            var setting = GetAudioSettingsFromAudioType(audioType);
            if (setting.settingsSO == null) return null;
            return setting.isMuted ? setting.settingsSO.mutedImage : setting.settingsSO.unmutedImage;
        }
        #endregion
    }
}