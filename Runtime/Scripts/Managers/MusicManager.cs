using System.Collections;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Manages background music with smooth crossfading between tracks.
    /// Uses two AudioSources (A/B) so the outgoing track fades out while the incoming one fades in.
    /// </summary>
    [DisallowMultipleComponent]
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private float defaultCrossfadeDuration = 1f;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _activeSource;
        private Coroutine _crossfadeCoroutine;

        private void Awake()
        {
            _sourceA = CreateSource("MusicSource_A");
            _sourceB = CreateSource("MusicSource_B");
            _activeSource = _sourceA;
            ServiceLocator.AddService<MusicManager>(gameObject, true);
            MUPLogger.Info("MusicManager initialized.");
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<MusicManager>(this);
        }

        private AudioSource CreateSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = AudioManager.GetAudioMixerGroup(AudioManager.AudioType.Music);
            return src;
        }

        /// <summary>Crossfades to a new clip. Ignores the call if the clip is already the active one.</summary>
        public void Play(AudioClip clip, float crossfadeDuration = -1f)
        {
            if (clip == null)
            {
                MUPLogger.Warning("MusicManager.Play called with a null clip.");
                return;
            }

            if (_activeSource.clip == clip && _activeSource.isPlaying)
            {
                MUPLogger.Info($"MusicManager: '{clip.name}' is already playing.");
                return;
            }

            if (crossfadeDuration < 0f) crossfadeDuration = defaultCrossfadeDuration;

            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            _crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(clip, crossfadeDuration));
            MUPLogger.Info($"MusicManager: crossfading to '{clip.name}' over {crossfadeDuration}s.");
        }

        /// <summary>Crossfades to the clip from an <see cref="AudioCueSO"/>.</summary>
        public void Play(AudioCueSO cue, float crossfadeDuration = -1f)
        {
            if (cue == null) return;
            Play(cue.GetClip(), crossfadeDuration);
        }

        /// <summary>Fades out the current track and stops playback.</summary>
        public void Stop(float fadeDuration = 1f)
        {
            if (_crossfadeCoroutine != null)
                StopCoroutine(_crossfadeCoroutine);

            // Fade out both sources: a crossfade may be in progress, leaving the incoming
            // source at a partial volume that would otherwise never be stopped.
            _crossfadeCoroutine = StartCoroutine(FadeOutBothRoutine(fadeDuration));
            MUPLogger.Info($"MusicManager: stopping over {fadeDuration}s.");
        }

        private IEnumerator CrossfadeRoutine(AudioClip clip, float duration)
        {
            AudioSource incoming = _activeSource == _sourceA ? _sourceB : _sourceA;
            AudioSource outgoing = _activeSource;

            float outgoingStart = outgoing.volume;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                incoming.volume = t;
                outgoing.volume = Mathf.Lerp(outgoingStart, 0f, t);
                yield return null;
            }

            incoming.volume = 1f;
            outgoing.Stop();
            outgoing.clip = null;

            _activeSource = incoming;
            _crossfadeCoroutine = null;
        }

        private IEnumerator FadeOutBothRoutine(float duration)
        {
            float startVolumeA = _sourceA.volume;
            float startVolumeB = _sourceB.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _sourceA.volume = Mathf.Lerp(startVolumeA, 0f, t);
                _sourceB.volume = Mathf.Lerp(startVolumeB, 0f, t);
                yield return null;
            }

            _sourceA.Stop();
            _sourceA.clip = null;
            _sourceB.Stop();
            _sourceB.clip = null;
            _crossfadeCoroutine = null;
        }
    }
}
