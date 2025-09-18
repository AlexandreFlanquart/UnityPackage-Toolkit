using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public sealed class FloatEvent : UnityEvent<float> {}

public enum TimerModeType
{
    Countdown = 0,
    Stopwatch = 1
}

[DisallowMultipleComponent]
public class TimerHandler : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] [Min(0f)] private float duration = 60f;
    [SerializeField] private TimerModeType modeType = TimerModeType.Countdown;
    [SerializeField] private bool playOnStart = false;

    [Header("UI Binding")]
    [SerializeField] private Text timeLabel;
    [SerializeField] private Slider progressSlider;

    [Header("Events")]
    [SerializeField] private UnityEvent onTimerCompleted;
    [SerializeField] private FloatEvent onTimerUpdated;

    [SerializeField] [FormerlySerializedAs("countDown")] [HideInInspector] private bool legacyCountDown = true;
    [SerializeField] [HideInInspector] private bool legacyMigrated = false;

    private ITimerMode mode;
    private float currentTime;
    private bool isRunning;

    public bool IsRunning => isRunning;
    public float CurrentTime => currentTime;
    public float RemainingTime => mode != null ? mode.Remaining(currentTime, duration) : Mathf.Max(0f, duration - currentTime);
    public float NormalizedTime => mode != null ? mode.Normalized(currentTime, duration) : 0f;

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
        BuildMode();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            currentTime = mode != null ? mode.ClampAtEnd(mode.InitialTime(duration), duration) : Mathf.Clamp(currentTime, 0f, duration);
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

        float delta = Time.deltaTime;
        currentTime = mode.Advance(currentTime, delta, duration);

        if (mode.ShouldStop(currentTime, duration))
        {
            currentTime = mode.ClampAtEnd(currentTime, duration);
            isRunning = false;
            UpdateVisuals();
            onTimerCompleted?.Invoke();
            return;
        }

        UpdateVisuals();
    }

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
    }

    public void Pause() => isRunning = false;

    public void TogglePlay()
    {
        if (isRunning) Pause();
        else Play();
    }

    public void ResetTimer(bool startImmediately = false)
    {
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

    public void SetDuration(float seconds, bool restart = true)
    {
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

    public void SetMode(TimerModeType newMode, bool restart = true)
    {
        if (modeType == newMode && mode != null) return;

        modeType = newMode;
        legacyMigrated = true;
        legacyCountDown = modeType == TimerModeType.Countdown;
        BuildMode();

        if (restart)
        {
            ResetTimer(isRunning);
        }
        else
        {
            currentTime = mode != null ? mode.ClampAtEnd(currentTime, duration) : currentTime;
            UpdateVisuals();
        }
    }

    private void BuildMode()
    {
        mode = modeType switch
        {
            TimerModeType.Stopwatch => new StopwatchMode(),
            _ => new CountdownMode()
        };
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
            timeLabel.text = FormatTime(Mathf.Max(0f, currentTime));
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
            return Mathf.Max(0f, currentTime);
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
    }
}
