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
        [SerializeField] private AudioSettingsSO audioSettingsSO;

        // Reference to the audio service interface. Can be injected for testing or replaced with another implementation.
        private IAudioService audioService;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        //public void Start()
        public void Initialize()
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
            
            var audioSettings = AudioManager.GetAudioSettingsFromAudioType(audioType);
            if (audioSettings != null && isMuted != audioSettings.isMuted)
            {
                AudioManager.ToggleMute(audioType);
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
            AudioManager.InitVolume(audioType, audioSettingsSO.defaultVolume);
            slider.value = audioSettingsSO.defaultVolume;
            AudioManager.SetVolume(audioType, audioSettingsSO.defaultVolume);
            UpdateVolumeText(audioSettingsSO.defaultVolume);
            UpdateMuteText(audioSettingsSO.defaultVolume);
            isMuted = AudioManager.GetAudioSettingsFromAudioType(audioType).isMuted;
            UpdateMuteImage();
        }

        // Called when the volume slider value changes
        private void OnVolumeChanged(float value)
        {
            audioService.SetVolume(audioType, value);
            UpdateMuteImage();
            UpdateVolumeText(value);
        }

        // Called when the mute button is clicked
        private void OnMuteClicked()
        {
            Debug.Log("OnMuteClicked");
            AudioManager.ToggleMute(audioType);
            isMuted = AudioManager.GetAudioSettingsFromAudioType(audioType).isMuted;
            MUPLogger.Info(audioType.ToString() + " isMuted: " + isMuted.ToString());

            float currentVolume = AudioManager.GetVolume(audioType);
            if (slider != null)
            {
                slider.value = currentVolume;
                UpdateVolumeText(currentVolume);
            }
            UpdateMuteText(currentVolume);
            UpdateMuteImage();

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
                muteText.text = value <= 0.0001f ? "Unmute" : "Mute";
            }
        }

        // Updates the mute button image to reflect the current mute state
        private void UpdateMuteImage()
        {
            if (muteButton == null) return;

            if (AudioManager.GetAudioSettingsFromAudioType(audioType).isMuted)
                muteButton.image.sprite = audioSettingsSO.mutedImage;
            else
                muteButton.image.sprite = audioSettingsSO.unmutedImage;
        }
    }
}

