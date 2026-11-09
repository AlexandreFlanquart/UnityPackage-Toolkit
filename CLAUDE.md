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
├── Managers/        AudioManager*, MusicManager, SFXManager, VoiceManager,
│                    AudioFocusHandler
├── Audio/           AudioUpdater, VoiceAgent, RandomIntervalSound, OneShotSound, SoundHandle
├── UI/              UIButtonSound, CanvasHelper, UI_Base
├── ObjectPool/      MUP_ObjectPool, MUP_ObjectPoolLocator
├── Localization/    LanguageSwitcher (#if I2LOC_PRESENT)
├── Utils/           MUPLogger*, TimerHandler, SceneLoader, ProjectVersionDisplay
└── ScriptableObjects/
    ├── AudioConfig/    AudioConfigSO (mixer, default groups and volumes)
    └── Sound/          SoundSO (sound asset: variants, randomisation, limits), VoiceLineSO (localized voice line)
Samples~/Scripts/    Usage examples — do not treat as authoritative architecture
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

Full reference: `Docs/AudioSystem.md`.

- **Config**: one `AudioConfigSO` at `Resources/AudioConfig` (`AudioManager.ConfigResourcePath`) references the mixer, the default output groups (Music, Voice, SFX, UI, World), the default bus volumes, the default click sound and the mute sprites. No string lookup of mixer groups.
- **Volumes**: `AudioManager` (static) drives three `AudioBus` values — `Music = 0`, `SFX = 1`, `Voice = 2` (serialized, do not reorder) — each matching a mixer exposed parameter **named exactly** after it. It does not persist volumes: consumers save from `AudioManager.VolumeChanged` and restore with `SetVolume`.
- **Players** (ServiceLocator, one GameObject):

| Player | Rule | Key API |
|---|---|---|
| `MusicManager` | N simultaneous 2D loops (music and ambience), keyed by `AudioClip` | `Play(clip, volume, fade)`, `SetVolume`, `Stop`, `StopAll`, `IsPlaying` |
| `VoiceManager` | One voice at a time, a new one interrupts | `Play(VoiceLineSO)`, `Play(clip)`, `Stop`, `VoiceEnded` |
| `SFXManager` | Pooled one-shots: 2D, UI, 3D at a position | `Play(sound)`, `Play(sound, position)`, `PlayUI(sound)`, `Play(clip)` → `SoundHandle` |

- **Components**: `VoiceAgent` (voice on enable / hover / click), `UIButtonSound` (its `SoundSO` or the config default click), `RandomIntervalSound` (random variant at random intervals on its own `AudioSource`), `OneShotSound` (random variant once, on `Play()`), `AudioUpdater` (bus slider + mute), `AudioFocusHandler`.
- **World loops need no script**: a plain looping `AudioSource` routed to the World group.
- `SoundSO.CanPlay()` / `OnPlay()` / `OnStop()` are called by `SFXManager` only. Naming: "sound" = a `SoundSO` asset; "clip" = a raw `AudioClip` (music, voices).
- `SoundHandle` uses a per-source generation counter: a stale handle never stops a sound that reused its source. `VoiceAgent` stops only its own playback through `VoiceManager.PlayId`.
- "Is this source still playing" goes through `AudioManager.IsBusy` (also true while a non-preloaded clip loads after `Play()`).

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
- `UIButtonSound` — plays its `clickSound` through `SFXManager.PlayUI`, or `AudioConfigSO.DefaultClickSound` when empty. A button without it is silent.

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
