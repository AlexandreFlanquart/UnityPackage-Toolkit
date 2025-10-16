using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioClip clickSound;

        void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (clickSound == null) return;
            var audioService = ServiceLocator.GetService<AudioPlaybackService>();
            audioService.PlaySFX(clickSound);
        }
    }
}
