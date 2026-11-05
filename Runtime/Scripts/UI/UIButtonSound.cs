using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays a sound through <see cref="SFXManager"/> when the attached <see cref="Button"/> is clicked.
    /// Uses <see cref="clickSound"/>, or the default click of the <see cref="AudioConfigSO"/> when empty.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [Tooltip("Sound of this button. Leave empty to play the default click of the AudioConfig.")]
        [FormerlySerializedAs("clickCue")]
        [SerializeField] private SoundSO clickSound;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            var sound = clickSound;
            if (sound == null && AudioManager.Config != null) sound = AudioManager.Config.DefaultClickSound;
            if (sound == null) return;

            var sfx = ServiceLocator.GetService<SFXManager>();
            if (sfx != null) sfx.PlayUI(sound);
        }
    }
}
