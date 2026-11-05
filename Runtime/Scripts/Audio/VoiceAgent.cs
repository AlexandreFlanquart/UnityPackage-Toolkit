using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Plays a <see cref="VoiceLineSO"/> through <see cref="VoiceManager"/> without code:
    /// when the object is shown, when it is hovered (touched, on mobile) and/or when it is clicked.
    /// Hover and click need a UI raycast target (Image, Text…) on this GameObject.
    /// </summary>
    public class VoiceAgent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("Line to play.")]
        [SerializeField] private VoiceLineSO voiceLine;
        [Tooltip("Seconds to wait before the voice starts.")]
        [SerializeField, Min(0f)] private float delay = 0f;

        [Header("Triggers")]
        [Tooltip("Play when this object becomes active (e.g. when its panel opens).")]
        [FormerlySerializedAs("onEnable")]
        [SerializeField] private bool playOnEnable = false;
        [Tooltip("Play when the pointer enters this object. On a touch screen this fires on touch: do not combine with playOnClick, the line would start twice.")]
        [FormerlySerializedAs("activeOnHover")]
        [SerializeField] private bool playOnHover = false;
        [Tooltip("Play when this object is clicked. Clicking again while it speaks restarts the line.")]
        [SerializeField] private bool playOnClick = false;

        [Header("Stop")]
        [Tooltip("Stop the voice when the pointer leaves this object. Off: the line plays to the end.")]
        [SerializeField] private bool stopOnPointerExit = false;
        [Tooltip("Stop the voice when this object is disabled (e.g. when its panel closes).")]
        [SerializeField] private bool stopOnDisable = true;

        private int _playedId = -1;
        private Coroutine _delayedPlay;

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        private void OnDisable()
        {
            if (stopOnDisable)
            {
                Stop();
            }
            else if (_delayedPlay != null)
            {
                // Disabling the component alone does not stop its coroutines.
                StopCoroutine(_delayedPlay);
                _delayedPlay = null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (playOnHover) Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (playOnHover && stopOnPointerExit) Stop();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (playOnClick) Play();
        }

        /// <summary>Plays the line after <see cref="delay"/>. Can be wired to any UnityEvent (e.g. a Button's OnClick).</summary>
        public void Play()
        {
            if (voiceLine == null) return;

            if (_delayedPlay != null) StopCoroutine(_delayedPlay);

            // A coroutine cannot start on an inactive object: play at once in that case.
            if (delay > 0f && isActiveAndEnabled) _delayedPlay = StartCoroutine(PlayDelayed());
            else PlayNow();
        }

        /// <summary>Stops the voice, only if it is still the one started by this agent.</summary>
        public void Stop()
        {
            if (_delayedPlay != null)
            {
                StopCoroutine(_delayedPlay);
                _delayedPlay = null;
            }

            if (_playedId < 0) return;

            // Compares the playback id, not the clip: two agents can share the same line.
            var voiceManager = ServiceLocator.GetService<VoiceManager>();
            if (voiceManager != null && voiceManager.IsPlaying && voiceManager.PlayId == _playedId)
            {
                voiceManager.Stop();
            }
            _playedId = -1;
        }

        /// <summary>Replaces the line played by this agent.</summary>
        public void SetVoiceLine(VoiceLineSO line)
        {
            voiceLine = line;
        }

        private IEnumerator PlayDelayed()
        {
            // Realtime: the voice still starts when the game is paused (Time.timeScale = 0).
            yield return new WaitForSecondsRealtime(delay);
            _delayedPlay = null;
            PlayNow();
        }

        private void PlayNow()
        {
            var voiceManager = ServiceLocator.GetService<VoiceManager>();
            if (voiceManager == null)
            {
                MUPLogger.Error("VoiceAgent: VoiceManager not found in ServiceLocator.", this);
                return;
            }

            if (voiceLine == null) return;

            // PlayId only changes if the line really started (it can have no clip for this language).
            int previousId = voiceManager.PlayId;
            voiceManager.Play(voiceLine);
            _playedId = voiceManager.PlayId != previousId ? voiceManager.PlayId : -1;
        }
    }
}
