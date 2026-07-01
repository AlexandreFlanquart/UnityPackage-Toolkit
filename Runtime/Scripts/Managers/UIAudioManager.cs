using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Dedicated manager for UI sounds (clicks, hovers, transitions).
    /// Uses <see cref="AudioSource.PlayOneShot"/> so rapid clicks stack cleanly without interrupting each other.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIAudioManager : MonoBehaviour
    {
        [Tooltip("Default click cue played by UIButtonSound when no per-button cue is assigned.")]
        [SerializeField] private AudioCueSO defaultClickCue;

        private AudioSource _source;

        // Tracks cues whose OnStop() hasn't fired yet, so OnDestroy can release them
        // and avoid leaking their maxInstances/cooldown counters across scenes.
        private readonly List<AudioCueSO> _pendingCues = new();

        private void Awake()
        {
            var go = new GameObject("UIAudioSource");
            go.transform.SetParent(transform);
            _source = go.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.outputAudioMixerGroup = AudioManager.GetAudioMixerGroup(AudioManager.AudioType.UI);

            ServiceLocator.AddService<UIAudioManager>(gameObject, true);
            MUPLogger.Info("UIAudioManager initialized.");
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<UIAudioManager>(this);

            foreach (var cue in _pendingCues)
            {
                cue?.OnStop();
            }
            _pendingCues.Clear();
        }

        /// <summary>Plays a UI cue. Concurrent calls stack via PlayOneShot — no interruption.</summary>
        public void Play(AudioCueSO cue)
        {
            if (cue == null) return;
            if (!cue.CanPlay()) return;

            var clip = cue.GetClip();
            if (clip == null) return;

            float pitch  = cue.GetPitch();
            float volume = cue.GetVolume();

            _source.pitch = pitch;
            _source.PlayOneShot(clip, volume);
            cue.OnPlay();
            _pendingCues.Add(cue);

            // Each call gets its own coroutine so concurrent sounds each release their instance slot correctly
            StartCoroutine(ReleaseAfterClip(cue, clip.length / Mathf.Abs(pitch)));
        }

        /// <summary>Plays the <see cref="defaultClickCue"/> assigned in the Inspector.</summary>
        public void PlayClick() => Play(defaultClickCue);

        /// <summary>Plays a raw <see cref="AudioClip"/> without a cue (no cooldown / instance tracking).</summary>
        public void PlayRaw(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            _source.PlayOneShot(clip, volume);
        }

        private IEnumerator ReleaseAfterClip(AudioCueSO cue, float duration)
        {
            yield return new WaitForSeconds(duration);
            _pendingCues.Remove(cue);
            cue.OnStop();
        }
    }
}
