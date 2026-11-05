using UnityEngine;
using UnityEngine.Audio;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Reusable sound asset: clip variants, randomisation, concurrency rules and mixer output.
    /// Played through <see cref="SFXManager"/> (UI, 2D, 3D one-shots) or <see cref="RandomIntervalSound"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundSO", menuName = "MyUnityPackage/Audio/Sound")]
    public class SoundSO : ScriptableObject
    {
        [Header("Clips")]
        [Tooltip("Variants of the sound. One is picked at random on each play, never the same twice in a row.")]
        [SerializeField] private AudioClip[] clips;

        [Header("Mixer")]
        [Tooltip("Mixer group to play through. Leave empty to use the player's default (SFX, UI or World group of the AudioConfig).")]
        [SerializeField] private AudioMixerGroup output;

        [Header("Volume")]
        [Tooltip("Volume of the sound before randomisation.")]
        [SerializeField, Range(0f, 1f)] private float baseVolume = 1f;
        [Tooltip("Random multiplier applied to the base volume on each play (min, max).")]
        [SerializeField] private Vector2 volumeVariation = new Vector2(0.9f, 1f);

        [Header("Pitch")]
        [Tooltip("Random pitch applied on each play (min, max). 1 = original pitch.")]
        [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

        [Header("Concurrency")]
        [Tooltip("Minimum seconds between two plays of this sound. 0 = no limit.")]
        [SerializeField, Min(0f)] private float cooldown = 0f;
        [Tooltip("Maximum simultaneous instances. Excess requests are dropped.")]
        [SerializeField, Min(1)] private int maxInstances = 8;
        [Tooltip("Unity priority 0 (highest) – 256 (lowest). Used when the SFX pool is full.")]
        [SerializeField, Range(0, 256)] private int priority = 128;

        // Runtime state, reset in OnEnable (asset load, or entering Play mode with domain reload)
        private int _lastClipIndex = -1;
        private float _lastPlayTime = float.NegativeInfinity;
        private int _activeInstances;

        public AudioMixerGroup Output => output;
        public int Priority => priority;
        public int MaxInstances => maxInstances;

        private void OnValidate()
        {
            if (volumeVariation.y < volumeVariation.x) volumeVariation.y = volumeVariation.x;
            if (pitchRange.x <= 0f) pitchRange.x = 0.01f;
            if (pitchRange.y < pitchRange.x) pitchRange.y = pitchRange.x;
        }

        private void OnEnable()
        {
            _lastClipIndex = -1;
            _lastPlayTime = float.NegativeInfinity;
            _activeInstances = 0;
        }

        /// <summary>Returns true when the sound is allowed to play (cooldown elapsed and instance limit not reached).</summary>
        public bool CanPlay()
        {
            if (_activeInstances >= maxInstances) return false;
            if (cooldown > 0f && Time.realtimeSinceStartup - _lastPlayTime < cooldown) return false;
            return true;
        }

        /// <summary>
        /// Returns a clip from the pool, avoiding immediate repeats when more than one clip is available.
        /// </summary>
        public AudioClip GetClip()
        {
            if (clips == null || clips.Length == 0) return null;
            if (clips.Length == 1) return clips[0];

            int index;
            if (_lastClipIndex < 0)
            {
                // First call: any clip is valid
                index = Random.Range(0, clips.Length);
            }
            else
            {
                // Pick from (Length-1) choices then shift past last index — O(1), guaranteed no repeat
                index = Random.Range(0, clips.Length - 1);
                if (index >= _lastClipIndex) index++;
            }

            _lastClipIndex = index;
            return clips[index];
        }

        /// <summary>Returns a randomised volume in [baseVolume * min, baseVolume * max].</summary>
        public float GetVolume() => baseVolume * Random.Range(volumeVariation.x, volumeVariation.y);

        /// <summary>Returns a randomised pitch in [pitchRange.x, pitchRange.y].</summary>
        public float GetPitch() => Random.Range(pitchRange.x, pitchRange.y);

        /// <summary>Called by the manager when playback starts. Tracks cooldown and instance count.</summary>
        public void OnPlay()
        {
            _lastPlayTime = Time.realtimeSinceStartup;
            _activeInstances++;
        }

        /// <summary>Called by the manager when playback ends. Decrements instance count.</summary>
        public void OnStop()
        {
            _activeInstances = Mathf.Max(0, _activeInstances - 1);
        }
    }
}
