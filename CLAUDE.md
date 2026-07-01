# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Package identity

- **UPM name:** `com.myunitypackage.toolkit`
- **Namespace:** `MyUnityPackage.Toolkit`
- **Assembly:** `MyUnityPackage.Toolkit` (`Runtime/MyUnityPackage.Toolkit.asmdef`)
- **Min Unity version:** 6000.0
- **No external mandatory dependencies** — I2 Localization is optional (see below)

## Project layout

```
Runtime/Scripts/
├── Static/          ServiceLocator
├── Managers/        AudioManager*, MusicManager, AmbienceManager, SFXManager,
│                    UIAudioManager, AudioFocusHandler, VoiceManager
├── Audio/           AudioPlaybackService (legacy), AudioUpdater, IAudioService,
│                    AudioManagerService, VoiceAgent
├── UI/              UIButtonSound, CanvasHelper, UI_Base
├── ObjectPool/      MUP_ObjectPool, MUP_ObjectPoolLocator
├── Localization/    LanguageSwitcher (#if I2LOC_PRESENT)
├── Utils/           MUPLogger*, TimerHandler, SceneLoader, ProjectVersionDisplay
└── ScriptableObjects/
    ├── AudioSettings/  AudioSettingsSO (per-channel config)
    └── AudioCue/       AudioCueSO (sound definition)
Samples/Scripts/     Usage examples — do not treat as authoritative architecture
Docs/                Human-readable docs per system
```

`*` = static class

## Core patterns

### ServiceLocator
Every MonoBehaviour service registers itself in `Awake` and unregisters in `OnDestroy`:
```csharp
void Awake() => ServiceLocator.AddService<MyService>(gameObject, replaceExisting: true);
void OnDestroy() => ServiceLocator.RemoveService<MyService>(this);
```
`AddService` throws `InvalidOperationException` if a live service of that type is already registered and `replaceExisting` is left `false` (the default) — pass `true` for the standard self-registering pattern above.
Call `ServiceLocator.Clear()` on scene unload to prevent stale references — this is **not** done automatically.

### Logging
Always use `MUPLogger` instead of `Debug.Log`:
```csharp
MUPLogger.Info("message");
MUPLogger.Error("message", this);          // clickable in console
MUPLogger.Info("debug", editorOnly: true); // stripped from builds
```

### Optional I2 Localization
The define `I2LOC_PRESENT` is set automatically via `versionDefines` in the asmdef when the `I2.Loc` assembly is present in the project. Guard all I2 code with `#if I2LOC_PRESENT`. Do **not** add `I2.Loc` to `references` in the asmdef.

## Audio system architecture

The audio system has two layers:
1. **Volume/mute**: `AudioManager` (static) controls Unity AudioMixer exposed parameters.
2. **Playback**: dedicated MonoBehaviour managers, each accessed via `ServiceLocator`.

### AudioManager (static)
Manages `AudioType` enum: `Music | SFX | Voice | Ambience | UI`.  
Maps each type to an AudioMixer exposed float parameter **named exactly** after the enum value string (`"Music"`, `"SFX"`, etc.).  
The mixer asset must live at `Resources/AudioMixer`.  
Per-channel `AudioSettingsSO` assets auto-load from `Resources/AudioSettings/{Music|SFX|Voice|Ambience|UI}SettingsSO` — a missing SO just keeps that channel's hardcoded default volume.

### Playback managers (ServiceLocator)

| Manager | Sources | Use for |
|---|---|---|
| `MusicManager` | 2 (A/B crossfade) | Background music, smooth transitions |
| `AmbienceManager` | 3 independent loops | Environmental layers (wind, rain, crowd) |
| `SFXManager` | Pool of 16 | One-shot game SFX, 2D or fire-and-forget 3D |
| `UIAudioManager` | 1 (`PlayOneShot`) | UI clicks, hover — stacks without interruption |
| `VoiceManager` | 1 (queue) | Dialogue, narration, streaming, I2LOC language paths |
| `AudioPlaybackService` | 3 (Music/SFX/Voice) | **Legacy** — kept for backward compat |

### AudioCueSO
The central sound definition. Assign to a field; pass to the appropriate manager:
```csharp
[SerializeField] private AudioCueSO _hitCue;
_sfxManager.Play(_hitCue, transform.position); // 3D
_uiAudio.Play(_clickCue);                       // UI
```
`AudioCueSO` handles: clip anti-repeat, volume/pitch randomisation, cooldown, maxInstances.  
`CanPlay()` / `OnPlay()` / `OnStop()` are called internally by managers — do not call them manually.

### AudioMixer setup (required in each Unity project)
1. Create an `AudioMixer` with groups: `Music`, `Ambience`, `SFX` (children: `SFX/World`, `SFX/UI`), `Voice`.
2. Expose float parameters named `Music`, `SFX`, `Voice`, `Ambience`, `UI`.
3. Place the asset at `Resources/AudioMixer`.

For ducking (voice over music), wire an **Audio Mixer Send** from the Voice group into a **Duck Volume** effect on the Music and Ambience groups — no code required.

## Object pool system

`MUP_ObjectPool<T>` is generic over any `Component` type — use a custom script (`Bullet`) or, for a prefab
with no distinguishing script, `Transform` (every GameObject has one).

```csharp
// Create
var pool = new MUP_ObjectPool<Bullet>();
pool.Initialize(baseSizeQueue: 10, maxSizeQueue: 20, maxSizeActive: 20, parent.transform, prefab.GetComponent<Bullet>());
MUP_ObjectPoolLocator.Add<Bullet>(pool);

// Use
Bullet bullet = MUP_ObjectPoolLocator.Get<Bullet>().Get(); // null if maxSizeActive is reached
MUP_ObjectPoolLocator.Get<Bullet>().Release(bullet); // never Destroy()
```

`baseSizeQueue` is only a capacity hint — instances are created lazily on first `Get()`, not pre-warmed by
`Initialize()`. `maxSizeActive` of 0 or less means no cap. `OnGetPoolAction`/`OnReturnPoolAction`/`OnDestroyPoolAction`
(`Action<T>`) receive the instance so pooled objects can reset their own state (position, rotation, particles)
when reused — do this on get, not in `Awake`/`OnEnable` (pooled instances only run those once).

## UI system

- `UI_Base` — abstract base for all canvases; exposes `Show()` / `Hide()` / `IsVisible` via `CanvasHelper` (fetched in `Awake`, not `Start`).
- `CanvasHelper` — requires `Canvas` + `CanvasGroup` on the same GameObject; toggles both `enabled` and `interactable`/`blocksRaycasts` together, always explicitly on `Awake` (not just when `hideOnStart` is true) so the two never start out of sync.
- `UIButtonSound` — assign a `AudioCueSO` (`clickCue`) for sounds; falls back to a raw `AudioClip` via `AudioPlaybackService` if no cue is set.

## Timer system

`TimerHandler` supports two modes set via `modeType`:
- `Countdown` — decreases from `duration` to 0.
- `Stopwatch` — increases from 0 to `duration`.

Bind a `TextMeshProUGUI` and/or a `Slider` in the Inspector; the component drives them automatically. Enable `useUnscaledTime` if the timer must keep running while `Time.timeScale` is 0 (e.g. pause menus).

Read `Duration` / `Mode` (public getters); use `SetDuration` / `SetMode` to change them. Both support `restart: false` to keep the timer running — elapsed time is preserved and correctly re-expressed across the mode switch (`ClampAtEnd` bounds `currentTime` to `[0, duration]` in both modes).

## Code conventions

- `PascalCase` for classes and methods; `camelCase` for locals; `_camelCase` for private fields.
- Expose Unity fields with `[SerializeField] private` — not `public`.
- `Awake` for self-initialisation; `Start` for cross-component references.
- New MonoBehaviour methods that touch the network or filesystem must be `async`/`await` with `try/catch`.
- Surround editor-only code with `#if UNITY_EDITOR`.
- Always log entry points of new public methods so behaviour can be verified at runtime.
