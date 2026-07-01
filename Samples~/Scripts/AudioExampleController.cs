using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Sample controller wiring UI buttons to the Toolkit audio managers.
    /// Demonstrates Music (crossfade), SFX (pooled), Voice, and UI click channels.
    /// </summary>
    public class AudioExampleController : MonoBehaviour
    {
        [Header("Cues")]
        [SerializeField] private AudioCueSO menuMusicCue;
        [SerializeField] private AudioCueSO sfxCue;

        [Header("Voice")]
        [Tooltip("Resource key looked up under Resources/Voices/, falling back to the Resources root. Reuses the SFX sample clip for this demo.")]
        [SerializeField] private string voiceDemoKey = "SFX";

        private void Awake()
        {
            AudioManager.Initialize();
            MUPLogger.Info("AudioExampleController: AudioManager initialized.");
        }

        public void PlayMenuMusic()
        {
            MUPLogger.Info("AudioExampleController.PlayMenuMusic");
            ServiceLocator.GetService<MusicManager>()?.Play(menuMusicCue);
        }

        public void StopMusic()
        {
            MUPLogger.Info("AudioExampleController.StopMusic");
            ServiceLocator.GetService<MusicManager>()?.Stop();
        }

        public void PlaySFX()
        {
            MUPLogger.Info("AudioExampleController.PlaySFX");
            ServiceLocator.GetService<SFXManager>()?.Play(sfxCue);
        }

        public void PlayVoiceDemo()
        {
            MUPLogger.Info("AudioExampleController.PlayVoiceDemo");
            ServiceLocator.GetService<VoiceManager>()?.PlayVoices(0f, voiceDemoKey);
        }
    }
}
