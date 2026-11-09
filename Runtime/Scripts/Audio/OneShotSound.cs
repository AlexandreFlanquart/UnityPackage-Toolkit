using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays a random variant of a <see cref="SoundSO"/> once on this object's <see cref="AudioSource"/>,
    /// each time <see cref="Play"/> is called from a UnityEvent or from code (e.g. a lock being opened).
    /// 3D settings (distance, rolloff, spatial blend) come from the AudioSource itself.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class OneShotSound : MonoBehaviour
    {
        [Tooltip("Sound variants to pick from. Its volume and pitch randomisation apply; its cooldown and instance limit do not.")]
        [SerializeField] private SoundSO sound;

        private AudioSource _source;

        private void Awake()
        {
            CacheSource();
        }

        // Also called from Play, which a UnityEvent can reach before Awake (inactive object).
        private void CacheSource()
        {
            if (_source != null) return;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            if (_source.outputAudioMixerGroup == null && AudioManager.Config != null)
            {
                _source.outputAudioMixerGroup = AudioManager.Config.WorldGroup;
            }
        }

        /// <summary>Plays one variant. Calls close together overlap instead of cutting each other.</summary>
        public void Play()
        {
            var clip = sound != null ? sound.GetClip() : null;
            if (clip == null)
            {
                MUPLogger.Warning($"OneShotSound on '{name}' has no sound or no clip assigned.", this);
                return;
            }

            CacheSource();
            if (sound.Output != null) _source.outputAudioMixerGroup = sound.Output;
            _source.pitch = sound.GetPitch();
            _source.PlayOneShot(clip, sound.GetVolume());
        }
    }
}
