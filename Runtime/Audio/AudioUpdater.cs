using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    public class AudioUpdater : MonoBehaviour
    {
        [SerializeField] private AudioManager.AudioType audioType;
        [SerializeField] private TextMeshProUGUI volumeText;
        [SerializeField] private Button muteButton;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI muteText;
        [SerializeField] private bool isMuted;

        [SerializeField] private AudioSettingsSO audioSettingsSO;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(OnVolumeChanged);
                slider.minValue = 0f;
                slider.maxValue = 1f;
            }
            if (isMuted)
            {
                AudioManager.ToggleMute(audioType);
            }

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(OnMuteClicked);
            }

            //audioSettingsSO = Resources.Load<AudioSettingsSO>("AudioSettings");

            //AudioManager.InitVolume(audioType,audioSettingsSO.defaultVolume);

        }
        public void InitVolumeUpdater()
        {
            AudioManager.InitVolume(audioType, audioSettingsSO.defaultVolume);
            slider.value = audioSettingsSO.defaultVolume;
            AudioManager.SetVolume(audioType, audioSettingsSO.defaultVolume);
            UpdateVolumeText(volumeText, audioSettingsSO.defaultVolume);
            UpdateMuteText(muteText, audioSettingsSO.defaultVolume);
        }
        private void OnVolumeChanged(float value)
        {
            AudioManager.SetVolume(audioType, value);
            UpdateVolumeText(volumeText, value);
        }


        private void OnMuteClicked()
        {
            AudioManager.ToggleMute(audioType);
            if (slider != null)
            {
                slider.value = AudioManager.GetVolume(audioType);
                UpdateVolumeText(volumeText, slider.value);
            }
            UpdateMuteText(muteText, slider.value);
            UpdateMuteImage();
        }
        private void UpdateVolumeText(TextMeshProUGUI text, float value)
        {
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }

        private void UpdateMuteText(TextMeshProUGUI text, float value)
        {
            if (text != null)
            {
                text.text = value <= 0.0001f ? "Unmute" : "Mute";
            }
        }
        private void UpdateMuteImage()
        {
            Debug.Log("UpdateMuteImage");
            if (isMuted)
            {
                muteButton.image.sprite = audioSettingsSO.unmutedImage;
            }
            else
            {
                muteButton.image.sprite = audioSettingsSO.mutedImage;
            }
            isMuted = !isMuted;
        }
    }
}