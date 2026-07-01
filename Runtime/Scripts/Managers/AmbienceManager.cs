using System.Collections;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Manages up to <see cref="MaxLayers"/> independent ambient sound loops (e.g. wind, rain, crowd).
    /// Each layer fades in/out independently so they can be blended dynamically by gameplay.
    /// </summary>
    [DisallowMultipleComponent]
    public class AmbienceManager : MonoBehaviour
    {
        public const int MaxLayers = 3;

        private AudioSource[] _sources;
        private Coroutine[] _fadeCoroutines;

        private void Awake()
        {
            _sources = new AudioSource[MaxLayers];
            _fadeCoroutines = new Coroutine[MaxLayers];

            for (int i = 0; i < MaxLayers; i++)
            {
                var go = new GameObject($"AmbienceSource_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = true;
                src.volume = 0f;
                src.spatialBlend = 0f;
                src.outputAudioMixerGroup = AudioManager.GetAudioMixerGroup(AudioManager.AudioType.Ambience);
                _sources[i] = src;
            }

            ServiceLocator.AddService<AmbienceManager>(gameObject, true);
            MUPLogger.Info("AmbienceManager initialized.");
        }

        private void OnDestroy()
        {
            ServiceLocator.RemoveService<AmbienceManager>(this);
        }

        /// <summary>
        /// Sets or replaces the clip on a layer. Pass <c>null</c> to fade out and clear the layer.
        /// </summary>
        /// <param name="layerIndex">Layer index [0, MaxLayers - 1].</param>
        /// <param name="clip">Clip to play, or null to stop.</param>
        /// <param name="targetVolume">Target volume [0, 1].</param>
        /// <param name="fadeDuration">Fade duration in seconds.</param>
        public void SetLayer(int layerIndex, AudioClip clip, float targetVolume = 1f, float fadeDuration = 1f)
        {
            if (!IsValidLayer(layerIndex)) return;

            CancelFade(layerIndex);

            if (clip == null)
            {
                _fadeCoroutines[layerIndex] = StartCoroutine(FadeOutRoutine(layerIndex, fadeDuration));
                return;
            }

            var src = _sources[layerIndex];
            if (src.clip == clip && src.isPlaying)
            {
                // Same clip already playing — just adjust volume
                _fadeCoroutines[layerIndex] = StartCoroutine(FadeToVolumeRoutine(layerIndex, targetVolume, fadeDuration));
                return;
            }

            src.clip = clip;
            src.volume = 0f;
            src.Play();
            _fadeCoroutines[layerIndex] = StartCoroutine(FadeToVolumeRoutine(layerIndex, targetVolume, fadeDuration));
            MUPLogger.Info($"AmbienceManager: layer {layerIndex} → '{clip.name}'.");
        }

        /// <summary>Fades a layer's volume to <paramref name="targetVolume"/> without changing the clip.</summary>
        public void SetLayerVolume(int layerIndex, float targetVolume, float fadeDuration = 0.5f)
        {
            if (!IsValidLayer(layerIndex)) return;
            CancelFade(layerIndex);
            _fadeCoroutines[layerIndex] = StartCoroutine(FadeToVolumeRoutine(layerIndex, targetVolume, fadeDuration));
        }

        /// <summary>Fades out and stops all layers.</summary>
        public void StopAll(float fadeDuration = 1f)
        {
            for (int i = 0; i < MaxLayers; i++)
            {
                CancelFade(i);
                _fadeCoroutines[i] = StartCoroutine(FadeOutRoutine(i, fadeDuration));
            }
        }

        private bool IsValidLayer(int index)
        {
            if (index >= 0 && index < MaxLayers) return true;
            MUPLogger.Warning($"AmbienceManager: layer index {index} is out of range [0, {MaxLayers - 1}].");
            return false;
        }

        private void CancelFade(int layerIndex)
        {
            if (_fadeCoroutines[layerIndex] != null)
            {
                StopCoroutine(_fadeCoroutines[layerIndex]);
                _fadeCoroutines[layerIndex] = null;
            }
        }

        private IEnumerator FadeToVolumeRoutine(int layerIndex, float targetVolume, float duration)
        {
            var src = _sources[layerIndex];
            float startVolume = src.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                src.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            src.volume = targetVolume;
            _fadeCoroutines[layerIndex] = null;
        }

        private IEnumerator FadeOutRoutine(int layerIndex, float duration)
        {
            var src = _sources[layerIndex];
            float startVolume = src.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                src.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            src.Stop();
            src.clip = null;
            _fadeCoroutines[layerIndex] = null;
        }
    }
}
