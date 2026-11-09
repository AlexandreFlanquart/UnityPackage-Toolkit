using UnityEngine;
using UnityEngine.Audio;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Project-wide audio configuration: mixer, default output groups, default bus volumes and shared UI assets.
    /// Loaded by <see cref="AudioManager"/> from <c>Resources/AudioConfig</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "MyUnityPackage/Audio/AudioConfig")]
    public class AudioConfigSO : ScriptableObject
    {
        [Header("Mixer")]
        [Tooltip("Project AudioMixer. It must expose three float parameters named exactly \"Music\", \"Voice\" and \"SFX\".")]
        [SerializeField] private AudioMixer mixer;

        [Header("Default output groups")]
        [Tooltip("Group used by MusicManager. Drives the \"Music\" volume slider.")]
        [SerializeField] private AudioMixerGroup musicGroup;
        [Tooltip("Group used by VoiceManager. Drives the \"Voice\" volume slider.")]
        [SerializeField] private AudioMixerGroup voiceGroup;
        [Tooltip("Group used by SFXManager when a sound has no output set. Child of the SFX bus.")]
        [SerializeField] private AudioMixerGroup sfxGroup;
        [Tooltip("Group used for UI sounds (SFXManager.PlayUI, SoundSO.PlayUI) when a sound has no output set. Child of the SFX bus.")]
        [SerializeField] private AudioMixerGroup uiGroup;
        [Tooltip("Group used for spatial sounds placed in the world (SFXManager.Play with a position, RandomIntervalSound). Child of the SFX bus.")]
        [SerializeField] private AudioMixerGroup worldGroup;

        [Header("Default volumes")]
        [Tooltip("Music bus volume applied at startup, before any player setting is restored.")]
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
        [Tooltip("Voice bus volume applied at startup, before any player setting is restored.")]
        [SerializeField, Range(0f, 1f)] private float defaultVoiceVolume = 0.8f;
        [Tooltip("SFX bus volume applied at startup, before any player setting is restored.")]
        [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.8f;

        [Header("UI")]
        [Tooltip("Mute button sprite shown by AudioUpdater while a bus is muted.")]
        [SerializeField] private Sprite mutedSprite;
        [Tooltip("Mute button sprite shown by AudioUpdater while a bus is audible.")]
        [SerializeField] private Sprite unmutedSprite;

        public AudioMixer Mixer => mixer;
        public AudioMixerGroup MusicGroup => musicGroup;
        public AudioMixerGroup VoiceGroup => voiceGroup;
        public AudioMixerGroup SFXGroup => sfxGroup;
        public AudioMixerGroup UIGroup => uiGroup;
        public AudioMixerGroup WorldGroup => worldGroup;
        public Sprite MutedSprite => mutedSprite;
        public Sprite UnmutedSprite => unmutedSprite;

        /// <summary>Returns the startup volume [0, 1] of a bus.</summary>
        public float GetDefaultVolume(AudioBus bus)
        {
            switch (bus)
            {
                case AudioBus.Music: return defaultMusicVolume;
                case AudioBus.Voice: return defaultVoiceVolume;
                default:             return defaultSfxVolume;
            }
        }
    }
}
