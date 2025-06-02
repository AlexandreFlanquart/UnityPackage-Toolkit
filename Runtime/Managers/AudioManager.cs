using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;

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
        public class AudioSettings
        {
            public string AUDIO_NAME;
            public float defaultVolume;
            public float currentVolume;
            public float beforeMutedVolume;
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


        public static AudioSettings GetAudioSettingsFromAudioType(AudioType audioType)
        {
            switch (audioType)
            {
                case AudioType.Music:
                    return musicSettings;
                    break;
                case AudioType.SFX:
                    return sfxSettings;
                    break;
                case AudioType.Voice:
                    return voiceSettings;
                    break;
                default:
                    throw new System.Exception("AudioType not recognize !");

                    break;
            }
        }

        #region Initialization
        /// <summary>
        /// Initializes the AudioManager and sets default volumes
        /// </summary>
        public static void Initialize()
        {
            musicSettings = new AudioSettings { AUDIO_NAME = AudioType.Music.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            sfxSettings = new AudioSettings { AUDIO_NAME = AudioType.SFX.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };
            voiceSettings = new AudioSettings { AUDIO_NAME = AudioType.Voice.ToString(), defaultVolume = 0.8f, currentVolume = 0.8f, isMuted = false };

            Debug.Log("Initialize" + AudioType.Music.ToString());

            AudioUpdater[] onlyInactive = GameObject.FindObjectsOfType<AudioUpdater>(true).Where(sr => !sr.gameObject.activeInHierarchy).ToArray();
            foreach (AudioUpdater audio in onlyInactive)
            {
                Debug.Log("audio : " + audio.name);
                audio.InitVolumeUpdater();
            }
        }
        #endregion

        #region Volume Control Methods

        public static void InitVolume(AudioType audioType, float volume) => InitVolume(GetAudioSettingsFromAudioType(audioType), volume);
        private static void InitVolume(AudioSettings audioSettings, float volume)
        {
            Debug.Log("Init volume " + audioSettings.AUDIO_NAME);
            audioSettings.currentVolume = volume;
            audioSettings.defaultVolume = volume;
            Debug.Log("Volume change" + audioSettings.currentVolume);
        }


        /// <summary>
        /// Sets the volume for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        /// <param name="volume">Volume value between 0 and 1</param>
        private static void SetVolume(AudioSettings audioSettings, float volume)
        {
            if (AudioMixer == null) return;
            Debug.Log("Set volume : " + volume);
            Debug.Log("current  volume : " + audioSettings.currentVolume);
            // Convert linear volume (0-1) to dB (-80 to 0)
            float dB = volume <= MIN_VOLUME ? -80f : Mathf.Log10(volume) * 20f;
            AudioMixer.SetFloat(audioSettings.AUDIO_NAME, dB);

            audioSettings.currentVolume = volume;
            Debug.Log("current  volume 2: " + audioSettings.currentVolume);
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
        public static void SetMusicVolume(float volume) => SetVolume(musicSettings, volume);
        public static float GetMusicVolume() => GetVolume(musicSettings.AUDIO_NAME);

        // SFX Volume
        public static void SetSFXVolume(float volume) => SetVolume(sfxSettings, volume);
        public static float GetSFXVolume() => GetVolume(sfxSettings.AUDIO_NAME);

        // Voice Volume
        public static void SetVoiceVolume(float volume) => SetVolume(voiceSettings, volume);
        public static float GetVoiceVolume() => GetVolume(voiceSettings.AUDIO_NAME);

        public static void SetVolume(AudioType audioType, float volume) => SetVolume(GetAudioSettingsFromAudioType(audioType), volume);
        public static float GetVolume(AudioType audioType) => GetVolume(audioType.ToString());
        #endregion

        #region Mute Control Methods
        /// <summary>
        /// Toggles mute state for a specific audio group
        /// </summary>
        /// <param name="parameter">The exposed parameter name in the AudioMixer</param>
        private static void ToggleMute(AudioSettings audioSettings)
        {
            if (AudioMixer == null) return;
            Debug.Log("audioType: " + audioSettings.currentVolume);
            float currentVolume;
            AudioMixer.GetFloat(audioSettings.AUDIO_NAME, out currentVolume);

            Debug.Log("currentVolume: " + currentVolume);
            // If already muted (-80dB), restore to default volume
            if (currentVolume <= -80f)
            {

                SetVolume(audioSettings, audioSettings.beforeMutedVolume);
                Debug.Log("AudioType: " + audioSettings.currentVolume + "  currentVolume");
                Debug.Log("Mixer: " + currentVolume + "  currentVolume");
                Debug.Log("Je demute");
            }
            else
            {
                audioSettings.beforeMutedVolume = audioSettings.currentVolume;
                SetVolume(audioSettings, MIN_VOLUME);
                Debug.Log("AudioType: " + audioSettings.currentVolume + "  currentVolume");
                Debug.Log("Mixer: " + currentVolume + "  currentVolume");
                Debug.Log("Je mute");
            }
        }

        // Public Mute Methods
        public static void ToggleMusicMute() => ToggleMute(musicSettings);
        public static void ToggleSFXMute() => ToggleMute(sfxSettings);
        public static void ToggleVoiceMute() => ToggleMute(voiceSettings);

        public static void ToggleMute(AudioType audioType) => ToggleMute(GetAudioSettingsFromAudioType(audioType));

        #endregion
    }
}