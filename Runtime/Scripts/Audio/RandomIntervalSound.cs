using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays a random variant of a <see cref="SoundSO"/> on this object's <see cref="AudioSource"/>,
    /// again and again with a random pause in between (e.g. a bird singing from time to time).
    /// 3D settings (distance, rolloff, spatial blend) come from the AudioSource itself; for a continuous loop,
    /// use a plain looping AudioSource instead.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RandomIntervalSound : MonoBehaviour
    {
        [Tooltip("Sound variants to pick from. Its volume and pitch randomisation apply; its cooldown and instance limit do not.")]
        [FormerlySerializedAs("cue")]
        [SerializeField] private SoundSO sound;
        [Tooltip("Shortest pause in seconds between the end of a sound and the start of the next one.")]
        [SerializeField, Min(0f)] private float minDelay = 3f;
        [Tooltip("Longest pause in seconds between the end of a sound and the start of the next one.")]
        [SerializeField, Min(0f)] private float maxDelay = 8f;
        [Tooltip("Start automatically when the object becomes active. Off: call StartPlaying() from code or a UnityEvent.")]
        [SerializeField] private bool playOnEnable = true;

        private AudioSource _source;
        private Coroutine _loop;

        private void Awake()
        {
            CacheSource();
        }

        // Also called from StopPlaying, which a UnityEvent can reach before Awake (inactive object).
        private void CacheSource()
        {
            if (_source != null) return;
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            if (_source.outputAudioMixerGroup == null && AudioManager.Config != null)
            {
                _source.outputAudioMixerGroup = AudioManager.Config.WorldGroup;
            }
        }

        private void OnEnable()
        {
            if (playOnEnable) StartPlaying();
        }

        private void OnDisable()
        {
            StopPlaying();
        }

        private void OnValidate()
        {
            if (maxDelay < minDelay) maxDelay = minDelay;
        }

        /// <summary>Starts the random loop. Does nothing if it already runs.</summary>
        public void StartPlaying()
        {
            if (_loop != null) return;
            if (sound == null)
            {
                MUPLogger.Warning($"RandomIntervalSound on '{name}' has no sound assigned.", this);
                return;
            }
            _loop = StartCoroutine(PlayLoop());
        }

        /// <summary>Stops the loop and the sound in progress.</summary>
        public void StopPlaying()
        {
            if (_loop != null)
            {
                StopCoroutine(_loop);
                _loop = null;
            }
            CacheSource();
            _source.Stop();
        }

        private IEnumerator PlayLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

                var clip = sound.GetClip();
                if (clip == null)
                {
                    MUPLogger.Warning($"RandomIntervalSound on '{name}': sound '{sound.name}' has no clip.", this);
                    _loop = null;
                    yield break;
                }

                if (sound.Output != null) _source.outputAudioMixerGroup = sound.Output;
                _source.clip = clip;
                _source.volume = sound.GetVolume();
                _source.pitch = sound.GetPitch();
                _source.Play();

                // AudioListener.pause (app in background) reports isPlaying = false: wait for the resume.
                yield return new WaitWhile(() => AudioManager.IsBusy(_source) || AudioListener.pause);
            }
        }
    }
}
