using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    // This class is used to update the volume of one audio type (Music, SFX, Voice)
    // To add in a scene, add a GameObject with this script and configure the AudioSettingsSO
    public class AudioUpdater : MonoBehaviour
    {
        [SerializeField] private AudioManager.AudioType audioType;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private Button muteButton;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI muteText;
        [SerializeField] private bool isMuted;

        // Reference to the audio service interface. Can be injected for testing or replaced with another implementation.
        private IAudioService audioService;

        void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            // If no audio service has been injected, use the default implementation
            if (audioService == null)
                audioService = new AudioManagerService(); // default fallback

            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnVolumeChanged);
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
            else
            {
                MUPLogger.Error($"AudioUpdater on '{name}' is missing a Slider reference.");
            }

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(OnMuteClicked);
            }
            InitVolumeUpdater();
        }

        // Allows external injection of a custom audio service (e.g., for testing or alternative implementations)
        public void InjectAudioService(IAudioService service)
        {
            audioService = service;
        }

        // Initializes the volume UI and audio settings based on the provided AudioSettingsSO
        public void InitVolumeUpdater()
        {
            float volume = audioService.GetVolume(audioType);

            if (slider != null)
            {
                slider.value = volume;
            }

            UpdateVolumeText(volume);
            UpdateMuteText(volume);

            var settings = audioService.GetAudioSettingsFromAudioType(audioType);
            isMuted = settings != null && settings.isMuted;

            UpdateMuteImage(settings);
        }

        // Called when the volume slider value changes
        private void OnVolumeChanged(float value)
        {
            audioService.SetVolume(audioType, value);
            var settings = audioService.GetAudioSettingsFromAudioType(audioType);
            UpdateMuteImage(settings);
            UpdateVolumeText(value);
        }

        // Called when the mute button is clicked
        private void OnMuteClicked()
        {
            MUPLogger.Info("OnMuteClicked");
            audioService.ToggleMute(audioType);
            var settings = audioService.GetAudioSettingsFromAudioType(audioType);
            isMuted = settings != null && settings.isMuted;
            MUPLogger.Info(audioType.ToString() + " isMuted: " + isMuted.ToString());

            float currentVolume = audioService.GetVolume(audioType);
            if (slider != null)
            {
                slider.value = currentVolume;
                UpdateVolumeText(currentVolume);
            }
            UpdateMuteText(currentVolume);
            UpdateMuteImage(settings);

        }

        // Updates the volume text UI to reflect the current value
        private void UpdateVolumeText(float value)
        {
            if (volumeText != null)
            {
                volumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        // Updates the mute/unmute text based on the current volume
        private void UpdateMuteText(float value)
        {
            if (muteText != null)
            {
                muteText.text = value <= AudioManager.MuteThreshold ? "Unmute" : "Mute";
            }
        }

        // Updates the mute button image to reflect the current mute state
        private void UpdateMuteImage(AudioManager.AudioSetting settings)
        {
            if (muteButton == null || muteButton.image == null)
            {
                return;
            }

            var sprite = settings?.settingsSO == null
                ? null
                : (settings.isMuted ? settings.settingsSO.mutedImage : settings.settingsSO.unmutedImage);

            muteButton.image.enabled = sprite != null;
            if (sprite != null)
            {
                muteButton.image.sprite = sprite;
            }
        }
    }
}

