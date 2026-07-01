using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Pauses the Unity <see cref="AudioListener"/> when the application loses focus or is suspended.
    /// On mobile (Android/iOS), <c>OnApplicationPause</c> is authoritative.
    /// On desktop, <c>OnApplicationFocus</c> is used instead.
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioFocusHandler : MonoBehaviour
    {
        [Tooltip("When true, all audio is paused while the app is in the background or out of focus.")]
        [SerializeField] private bool pauseOnFocusLoss = true;

        private void Awake()
        {
            ServiceLocator.AddService<AudioFocusHandler>(gameObject, true);
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<AudioFocusHandler>(this);
        }

        // Called on all platforms when the app is suspended/resumed (reliable on Android & iOS)
        private void OnApplicationPause(bool paused)
        {
            if (!pauseOnFocusLoss) return;
            SetAudioPaused(paused);
        }

        // Called on desktop when the window gains/loses focus; ignored on mobile to avoid double-firing
        private void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_ANDROID && !UNITY_IOS
            if (!pauseOnFocusLoss) return;
            SetAudioPaused(!hasFocus);
#endif
        }

        private void SetAudioPaused(bool paused)
        {
            AudioListener.pause = paused;
            MUPLogger.Info($"AudioFocusHandler: AudioListener.pause = {paused}.");
        }
    }
}
