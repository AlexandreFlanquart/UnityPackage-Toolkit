using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Sample controller wiring the AudioExampleScene UI to the Toolkit audio managers.
    /// Voices (VoiceAgent), UI sounds (SoundSO.PlayUI from the button and toggle events) and world sounds
    /// (AudioSource, RandomIntervalSound, OneShotSound) need no code: only music, the SFX played by code, the voice panel,
    /// the listener position and the fire's output are driven here.
    /// </summary>
    public class AudioExampleController : MonoBehaviour
    {
        [Header("Music")]
        [Tooltip("First music loop.")]
        [SerializeField] private AudioClip musicA;
        [Tooltip("Second music loop, played on top of the first one.")]
        [SerializeField] private AudioClip musicB;

        [Header("SFX")]
        [Tooltip("Sound played by PlaySFX (2D) and PlaySFXAtFire (3D at the fire's position).")]
        [FormerlySerializedAs("sfxCue")]
        [SerializeField] private SoundSO sfxSound;

        [Header("Voice")]
        [Tooltip("Panel holding a VoiceAgent set to play on enable.")]
        [SerializeField] private GameObject voicePanel;

        [Header("Spatial")]
        [Tooltip("Object holding the scene's AudioListener, moved by SetListenerX.")]
        [SerializeField] private Transform listener;
        [Tooltip("Looping AudioSource placed in the scene, with no output so it follows the project's AudioConfig.")]
        [SerializeField] private AudioSource fire;

        private float _musicAVolume = 1f;
        private float _musicBVolume = 1f;

        private MusicManager Music => ServiceLocator.GetService<MusicManager>();

        // A sample cannot reference the mixer of the project importing it: route the plain AudioSource
        // to the World group of whichever AudioConfig is loaded (RandomIntervalSound does it by itself).
        private void Start()
        {
            if (fire != null && fire.outputAudioMixerGroup == null && AudioManager.Config != null)
            {
                fire.outputAudioMixerGroup = AudioManager.Config.WorldGroup;
            }
        }

        public void PlayMusicA() => Music.Play(musicA, _musicAVolume);
        public void PlayMusicB() => Music.Play(musicB, _musicBVolume);

        // Slider callbacks: a player action changing the volume of one track only.
        public void SetMusicAVolume(float volume)
        {
            _musicAVolume = volume;
            if (Music.IsPlaying(musicA)) Music.SetVolume(musicA, volume, 0f);
        }

        public void SetMusicBVolume(float volume)
        {
            _musicBVolume = volume;
            if (Music.IsPlaying(musicB)) Music.SetVolume(musicB, volume, 0f);
        }

        public void StopAllMusic() => Music.StopAll();

        public void PlaySFX()
        {
            var sfx = ServiceLocator.GetService<SFXManager>();
            if (sfx != null) sfx.Play(sfxSound);
        }

        // 3D one-shot from code at a world position: heard from the fire's side.
        public void PlaySFXAtFire()
        {
            var sfx = ServiceLocator.GetService<SFXManager>();
            if (sfx != null && fire != null) sfx.Play(sfxSound, fire.transform.position);
        }

        public void ToggleVoicePanel()
        {
            if (voicePanel != null) voicePanel.SetActive(!voicePanel.activeSelf);
        }

        // Slider callback: moves the listener left/right between the fire and the bird.
        public void SetListenerX(float x)
        {
            if (listener == null) return;
            var position = listener.position;
            position.x = x;
            listener.position = position;
        }
    }
}
