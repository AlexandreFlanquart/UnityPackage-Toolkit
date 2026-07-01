using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays a sound when the attached <see cref="Button"/> is clicked.
    /// Assign <see cref="clickCue"/> (preferred) or fall back to a raw <see cref="clickClip"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [Tooltip("Preferred: AudioCueSO with clip variants, cooldown, and volume randomisation.")]
        [SerializeField] private AudioCueSO clickCue;
        [Tooltip("Legacy fallback used when no AudioCueSO is assigned.")]
        [SerializeField] private AudioClip clickClip;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            // Prefer UIAudioManager + cue
            if (clickCue != null)
            {
                var uiAudio = ServiceLocator.GetService<UIAudioManager>();
                if (uiAudio != null)
                {
                    uiAudio.Play(clickCue);
                    return;
                }
            }

            // Legacy path: raw clip through AudioPlaybackService
            if (clickClip != null)
            {
                var legacy = ServiceLocator.GetService<AudioPlaybackService>();
                legacy?.PlaySFX(clickClip);
            }
        }
    }
}
