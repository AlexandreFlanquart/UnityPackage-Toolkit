using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Drives a volume slider and mute button UI for a single <see cref="AudioBus"/>.
    /// </summary>
    public class AudioUpdater : MonoBehaviour
    {
        [Tooltip("Bus controlled by this slider: Music (music and ambience), Voice, or SFX (UI, world and game sounds).")]
        [FormerlySerializedAs("audioType")]
        [SerializeField] private AudioBus bus;
        [Tooltip("Optional. Shows the volume as a percentage.")]
        [SerializeField] private TextMeshProUGUI volumeText;
        [Tooltip("Optional. Toggles mute; its image switches between the muted/unmuted sprites of the AudioConfig.")]
        [SerializeField] private Button muteButton;
        [Tooltip("Volume slider, driven in [0, 1].")]
        [SerializeField] private Slider slider;
        [Tooltip("Optional. Shows \"Mute\" or \"Unmute\".")]
        [SerializeField] private TextMeshProUGUI muteText;

        private void Awake()
        {
            if (slider != null)
            {
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.onValueChanged.AddListener(OnVolumeChanged);
            }
            else
            {
                MUPLogger.Error($"AudioUpdater on '{name}' is missing a Slider reference.");
            }

            if (muteButton != null)
            {
                muteButton.onClick.AddListener(OnMuteClicked);
            }
        }

        // Volumes can change while this UI is hidden (save restored, another slider on the same bus).
        private void OnEnable()
        {
            AudioManager.VolumeChanged += OnBusVolumeChanged;
            Refresh();
        }

        private void OnDisable()
        {
            AudioManager.VolumeChanged -= OnBusVolumeChanged;
        }

        private void OnDestroy()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(OnVolumeChanged);
            }
            if (muteButton != null)
            {
                muteButton.onClick.RemoveListener(OnMuteClicked);
            }
        }

        /// <summary>Refreshes the slider, texts and mute image from the current <see cref="AudioManager"/> state.</summary>
        public void Refresh()
        {
            float volume = AudioManager.GetVolume(bus);
            if (slider != null)
            {
                slider.SetValueWithoutNotify(volume);
            }
            UpdateLabels(volume);
        }

        // The UI refreshes through OnBusVolumeChanged, raised by SetVolume and ToggleMute.
        private void OnVolumeChanged(float value)
        {
            AudioManager.SetVolume(bus, value);
        }

        private void OnMuteClicked()
        {
            AudioManager.ToggleMute(bus);
            MUPLogger.Info($"{bus} isMuted: {AudioManager.IsMuted(bus)}");
        }

        private void OnBusVolumeChanged(AudioBus changedBus, float volume)
        {
            if (changedBus == bus) Refresh();
        }

        private void UpdateLabels(float volume)
        {
            if (volumeText != null)
            {
                volumeText.text = $"{Mathf.RoundToInt(volume * 100)}%";
            }
            if (muteText != null)
            {
                muteText.text = volume <= AudioManager.MuteThreshold ? "Unmute" : "Mute";
            }
            UpdateMuteImage();
        }

        private void UpdateMuteImage()
        {
            if (muteButton == null || muteButton.image == null) return;

            // Without sprites in the AudioConfig the button keeps its own image (a disabled image would block clicks).
            var sprite = AudioManager.GetMuteSprite(bus);
            if (sprite != null)
            {
                muteButton.image.sprite = sprite;
            }
        }
    }
}
