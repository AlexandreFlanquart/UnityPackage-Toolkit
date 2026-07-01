using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// <see cref="UnityEvent{T}"/> variant that forwards a float payload (the timer's current value).
    /// </summary>
    [Serializable]
    public sealed class FloatEvent : UnityEvent<float> {}

    /// <summary>
    /// Counting direction for <see cref="TimerHandler"/>.
    /// </summary>
    public enum TimerModeType
    {
        /// <summary>Decreases from <c>duration</c> down to 0.</summary>
        Countdown = 0,
        /// <summary>Increases from 0 up to <c>duration</c>.</summary>
        Stopwatch = 1
    }

    /// <summary>
    /// Drives a countdown or stopwatch timer, optionally updating a bound <see cref="TextMeshProUGUI"/> label
    /// and <see cref="Slider"/>, and firing <see cref="UnityEvent"/> callbacks on update/completion.
    /// </summary>
    [DisallowMultipleComponent]
    public class TimerHandler : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] [Min(0f)] private float duration = 60f;
        [SerializeField] private TimerModeType modeType = TimerModeType.Countdown;
        [SerializeField] private bool playOnStart = false;
        [Tooltip("When true, the timer advances using Time.unscaledDeltaTime and keeps running while Time.timeScale is 0 (e.g. pause menus).")]
        [SerializeField] private bool useUnscaledTime = false;

        [Header("UI Binding")]
        [SerializeField] private TextMeshProUGUI timeLabel;
        [SerializeField] private Slider progressSlider;

        [Header("Events")]
        [SerializeField] private UnityEvent onTimerCompleted;
        [SerializeField] private FloatEvent onTimerUpdated;

        [SerializeField] [FormerlySerializedAs("countDown")] [HideInInspector] private bool legacyCountDown = true;
        [SerializeField] [HideInInspector] private bool legacyMigrated = false;

        private static readonly ITimerMode CountdownInstance = new CountdownMode();
        private static readonly ITimerMode StopwatchInstance = new StopwatchMode();

        private ITimerMode mode;
        private float currentTime;
        private bool isRunning;

        /// <summary>Whether the timer is currently counting down/up.</summary>
        public bool IsRunning => isRunning;
        /// <summary>Raw internal time value — remaining time in Countdown mode, elapsed time in Stopwatch mode.</summary>
        public float CurrentTime => currentTime;
        /// <summary>Time left before completion, in seconds, for either mode.</summary>
        public float RemainingTime => mode != null ? mode.Remaining(currentTime, duration) : Mathf.Max(0f, duration - currentTime);
        /// <summary>Progress in the [0, 1] range (0 = just started, 1 = completed).</summary>
        public float NormalizedTime => mode != null ? mode.Normalized(currentTime, duration) : 0f;
        /// <summary>The configured total duration, in seconds. Use <see cref="SetDuration"/> to change it.</summary>
        public float Duration => duration;
        /// <summary>The current counting direction. Use <see cref="SetMode"/> to change it.</summary>
        public TimerModeType Mode => modeType;

        private void Awake()
        {
            BuildMode();
            ConfigureSlider();
            ResetTimer(false);
        }

        private void OnValidate()
        {
            if (!legacyMigrated)
            {
                modeType = legacyCountDown ? TimerModeType.Countdown : TimerModeType.Stopwatch;
                legacyMigrated = true;
            }

            duration = Mathf.Max(0f, duration);

#if UNITY_EDITOR
            // Dragging the mode dropdown in the Inspector while in Play Mode bypasses SetMode() —
            // reproduce the same elapsed-time conversion here so remaining/elapsed stays meaningful.
            bool modeChangedLive = Application.isPlaying && mode != null && (mode is StopwatchMode) != (modeType == TimerModeType.Stopwatch);
            if (modeChangedLive)
            {
                ApplyModeChange(modeType, preserveElapsed: true);
                UpdateVisuals();
                return;
            }
#endif

            BuildMode();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                currentTime = mode.ClampAtEnd(mode.InitialTime(duration), duration);
                UpdateVisuals();
            }
#endif
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        private void Update()
        {
            if (!isRunning || mode == null) return;

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            currentTime = mode.Advance(currentTime, delta, duration);

            if (mode.ShouldStop(currentTime, duration))
            {
                currentTime = mode.ClampAtEnd(currentTime, duration);
                isRunning = false;
                UpdateVisuals();
                MUPLogger.Info("TimerHandler: completed.", this);
                onTimerCompleted?.Invoke();
                return;
            }

            UpdateVisuals();
        }

        /// <summary>Starts or resumes the timer. No-op if already running, or if it had already reached its end (auto-resets first).</summary>
        public void Play()
        {
            if (mode == null) BuildMode();
            if (mode == null) return;
            if (isRunning) return;

            if (mode.ShouldStop(currentTime, duration))
            {
                ResetTimer(false);
                if (mode.ShouldStop(currentTime, duration))
                {
                    return;
                }
            }

            isRunning = true;
            MUPLogger.Info("TimerHandler: play.", this);
        }

        /// <summary>Pauses the timer in place; <see cref="Play"/> resumes from the same value.</summary>
        public void Pause()
        {
            isRunning = false;
            MUPLogger.Info("TimerHandler: pause.", this);
        }

        /// <summary>Toggles between <see cref="Play"/> and <see cref="Pause"/>.</summary>
        public void TogglePlay()
        {
            if (isRunning) Pause();
            else Play();
        }

        /// <summary>Resets the timer to its starting value for the current mode.</summary>
        /// <param name="startImmediately">If true, the timer starts running right away.</param>
        public void ResetTimer(bool startImmediately = false)
        {
            MUPLogger.Info($"TimerHandler: reset (startImmediately={startImmediately}).", this);

            if (mode == null) BuildMode();
            if (mode == null)
            {
                currentTime = 0f;
                isRunning = startImmediately;
                UpdateVisuals();
                return;
            }

            currentTime = mode.InitialTime(duration);
            currentTime = mode.ClampAtEnd(currentTime, duration);
            isRunning = startImmediately;
            UpdateVisuals();
        }

        /// <summary>Changes the timer's duration.</summary>
        /// <param name="seconds">New duration in seconds (clamped to 0 or above).</param>
        /// <param name="restart">If true, resets the timer; otherwise clamps the current value to the new duration.</param>
        public void SetDuration(float seconds, bool restart = true)
        {
            MUPLogger.Info($"TimerHandler: set duration to {seconds}s (restart={restart}).", this);

            duration = Mathf.Max(0f, seconds);
            ConfigureSlider();

            if (restart)
            {
                ResetTimer(isRunning);
            }
            else
            {
                currentTime = mode != null ? mode.ClampAtEnd(currentTime, duration) : Mathf.Clamp(currentTime, 0f, duration);
                UpdateVisuals();
            }
        }

        /// <summary>Switches between <see cref="TimerModeType.Countdown"/> and <see cref="TimerModeType.Stopwatch"/>.</summary>
        /// <param name="newMode">The mode to switch to.</param>
        /// <param name="restart">If true, resets the timer. Otherwise, the elapsed time so far is preserved and re-expressed in the new mode.</param>
        public void SetMode(TimerModeType newMode, bool restart = true)
        {
            if (modeType == newMode && mode != null) return;

            ApplyModeChange(newMode, preserveElapsed: !restart);

            MUPLogger.Info($"TimerHandler: set mode to {modeType} (restart={restart}).", this);

            if (restart)
            {
                ResetTimer(isRunning);
            }
            else
            {
                UpdateVisuals();
            }
        }

        /// <summary>
        /// Rebuilds <see cref="mode"/> for <paramref name="newMode"/>. When <paramref name="preserveElapsed"/> is true,
        /// the mode-agnostic elapsed time (<c>duration - remaining</c>) is computed from the old mode first, then
        /// re-expressed via the new mode's <see cref="ITimerMode.FromElapsed"/> — this is what keeps "15s elapsed"
        /// meaningful whether it's read back as "15s remaining" or "15s in" after switching modes.
        /// </summary>
        private void ApplyModeChange(TimerModeType newMode, bool preserveElapsed)
        {
            float elapsed = preserveElapsed && mode != null ? duration - mode.Remaining(currentTime, duration) : 0f;

            modeType = newMode;
            legacyMigrated = true;
            legacyCountDown = modeType == TimerModeType.Countdown;
            BuildMode();

            if (preserveElapsed)
            {
                currentTime = mode.FromElapsed(elapsed, duration);
            }
        }

        private void BuildMode()
        {
            mode = modeType == TimerModeType.Stopwatch ? StopwatchInstance : CountdownInstance;
        }

        private void ConfigureSlider()
        {
            if (!progressSlider) return;

            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.wholeNumbers = false;
            UpdateSlider();
        }

        private void UpdateVisuals()
        {
            if (timeLabel)
            {
                timeLabel.text = FormatTime(currentTime);
            }

            UpdateSlider();
            onTimerUpdated?.Invoke(currentTime);
        }

        private void UpdateSlider()
        {
            if (!progressSlider) return;

            progressSlider.value = NormalizedTime;
        }

        private static string FormatTime(float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));

            if (timeSpan.TotalHours >= 1d)
            {
                return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }

            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        private interface ITimerMode
        {
            float InitialTime(float duration);
            float Advance(float currentTime, float deltaTime, float duration);
            bool ShouldStop(float currentTime, float duration);
            float ClampAtEnd(float currentTime, float duration);
            float Normalized(float currentTime, float duration);
            float Remaining(float currentTime, float duration);
            /// <summary>Converts a mode-agnostic elapsed time into this mode's internal <c>currentTime</c> representation.</summary>
            float FromElapsed(float elapsedTime, float duration);
        }

        private sealed class CountdownMode : ITimerMode
        {
            public float InitialTime(float duration) => Mathf.Max(0f, duration);

            public float Advance(float currentTime, float deltaTime, float duration)
            {
                return currentTime - deltaTime;
            }

            public bool ShouldStop(float currentTime, float duration)
            {
                return currentTime <= 0f;
            }

            public float ClampAtEnd(float currentTime, float duration)
            {
                return Mathf.Clamp(currentTime, 0f, duration);
            }

            public float Normalized(float currentTime, float duration)
            {
                if (duration <= Mathf.Epsilon) return 0f;
                float clamped = Mathf.Clamp(currentTime, 0f, duration);
                return Mathf.Clamp01((duration - clamped) / duration);
            }

            public float Remaining(float currentTime, float duration)
            {
                return Mathf.Clamp(currentTime, 0f, duration);
            }

            public float FromElapsed(float elapsedTime, float duration) => ClampAtEnd(duration - elapsedTime, duration);
        }

        private sealed class StopwatchMode : ITimerMode
        {
            public float InitialTime(float duration) => 0f;

            public float Advance(float currentTime, float deltaTime, float duration)
            {
                return currentTime + deltaTime;
            }

            public bool ShouldStop(float currentTime, float duration)
            {
                return duration > 0f && currentTime >= duration;
            }

            public float ClampAtEnd(float currentTime, float duration)
            {
                return duration > 0f ? Mathf.Min(currentTime, duration) : Mathf.Max(0f, currentTime);
            }

            public float Normalized(float currentTime, float duration)
            {
                if (duration <= Mathf.Epsilon) return 0f;
                return Mathf.Clamp01(currentTime / duration);
            }

            public float Remaining(float currentTime, float duration)
            {
                if (duration <= Mathf.Epsilon) return 0f;
                return Mathf.Max(0f, duration - Mathf.Clamp(currentTime, 0f, duration));
            }

            public float FromElapsed(float elapsedTime, float duration) => ClampAtEnd(elapsedTime, duration);
        }
    }
}
