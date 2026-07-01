## [1.0.6] - 2026-07-01
### Added
- Add `MusicManager`: crossfaded background music (two A/B sources)
- Add `SFXManager`: pooled SFX playback with cooldown, max-instance, and priority-based voice stealing
- Add `UIAudioManager`: dedicated UI click/hover channel using `PlayOneShot`
- Add `AmbienceManager`: up to 3 independent fading ambience loops
- Add `AudioFocusHandler`: pauses the `AudioListener` on application focus/pause loss
- Add `AudioCueSO`: reusable sound definition (clip variants, volume/pitch randomisation, cooldown, instance limit)
- Add `AudioExampleScene` sample demonstrating Music, SFX, UI click, and Voice playback end-to-end
- Add `Ambience` and `UI` entries to `AudioManager.AudioType`
- Add optional I2 Localization support for `VoiceManager` (guarded by `I2LOC_PRESENT`, auto-detected via `versionDefines`)
- Add `IMUPObjectPool` interface and `MUP_ObjectPoolLocator.Exists<T>()`
- Add `MUP_ObjectPool<T>.IsInitialized` / `CountActive` / `CountInactive` / `CountAll`, and a `collectionCheck` parameter on `Initialize()` (default `true`, catches double-`Release()` bugs)
- Add `TimerHandler.Duration` / `TimerHandler.Mode` public getters and a `useUnscaledTime` option (keeps counting while `Time.timeScale` is 0)
- Add `CanvasHelper.IsVisible` / `UI_Base.IsVisible`

### Changed
- `UIButtonSound` now plays through `UIAudioManager` via an `AudioCueSO` (`clickCue`), falling back to the legacy `AudioPlaybackService` (`clickClip`) when no cue is assigned.
  ⚠️ The old `clickSound` field was renamed/split — re-assign `clickCue`/`clickClip` on any existing button after upgrading.
- `VoiceManager` now assigns its `AudioSource` to the `Voice` AudioMixer group automatically when none is set
- `VoiceManager.PlayVoices` always routes through the internal queue (even a single key), so a call made while a sequence is already playing gets appended instead of hijacking the shared `AudioSource`
- `MUP_ObjectPool` is now generic (`MUP_ObjectPool<T> where T : Component`) instead of hardcoded to `GameObject`. ⚠️ Breaking: `Get()`/`Release()` are strongly typed, and `OnGetPoolAction`/`OnReturnPoolAction`/`OnDestroyPoolAction` are now `Action<T>` (they receive the instance instead of firing a bare signal).
- `ServiceLocator.AddService<T>` now throws `InvalidOperationException` when `replaceExisting` is left `false` (the default) and a live service of that type is already registered. ⚠️ Breaking: previously logged a warning and silently overwrote it anyway — pass `replaceExisting: true` for the standard self-registering pattern.
- `TimerHandler.timeLabel` is now `TextMeshProUGUI` instead of legacy `UnityEngine.UI.Text`. ⚠️ Breaking: re-assign the label reference on existing `TimerHandler` components after upgrading.

### Fixed
- `AudioManager.EnsureInitialized` no longer partially re-initializes when only some settings are null
- `AudioManager.LoadAndApplyAudioSettingsSO` now also loads `AudioSettingsSO` assets for the `Ambience`/`UI` channels (previously only Music/SFX/Voice, despite both channels existing on `AudioType`)
- `TimerHandler`: `CountdownMode.ClampAtEnd` now clamps the upper bound too (previously only floored at 0) — shrinking `duration` while running now correctly shortens the remaining time instead of leaving the real countdown to run out at its old value
- `TimerHandler`: `SetMode(_, restart: false)`, and switching the mode dropdown live in the Inspector during Play Mode, now correctly convert elapsed time between Countdown/Stopwatch semantics instead of misinterpreting the raw value
- `MUP_ObjectPool`: `maxSizeActive = 0` (documented as "no limit") previously always rejected `Get()` due to an off-by-one guard (`>= 0` instead of `> 0`)
- `MUP_ObjectPoolLocator.Clear()` now clears each registered pool's idle instances instead of only dropping the dictionary reference
- `ServiceLocator.FindService<T>` no longer duplicates the cache/staleness check already performed by `GetService<T>`
- `UI_Base`: the `CanvasHelper` reference is now fetched in `Awake()` instead of `Start()`, removing a `NullReferenceException` window if `Show()`/`Hide()` was called too early
- `CanvasHelper.Awake()` now always applies its initial state explicitly, instead of only when `hideOnStart` was true (could otherwise leave `Canvas`/`CanvasGroup` out of sync)
- `SceneLoader`: `OnLoadingEvent` was cleared after the first load, breaking reuse of the component for subsequent load/unload cycles
- `SceneLoader`: added a null-check on the `SceneManager.LoadSceneAsync` result (previously crashed if a scene wasn't added to Build Settings)
- `SceneLoader`: scene unload is now awaited before the load phase starts, instead of firing both uncoordinated
- `AudioUpdater` now unsubscribes its slider/button listeners in `OnDestroy()`
- `ProjectVersionDisplay` was declared in the global namespace instead of `MyUnityPackage.Toolkit`

## [1.0.5] - 2025-11-17
### Fixed
- Fix objectpool parameters & check null reference

### Update
- Update docs

### Removed
- Animation System resource folder

## [1.0.4] - 2025-11-06
### Added
- Add MUP_ObjectPool Component
- Add MUP_ObjectPoolLocator Component

### Removed
- Animation System(move to another package)

## [1.0.3] - 2025-09-18
### Refactor
- Clear project & add docs
- Changed Logger

### Fixed
- ServiceLocator
- Sound Bug

## [1.0.3] - 2025-07-11
### Fixed
- Fixed Samples

[1.0.6]: https://github.com/AlexandreFlanquart/UnityPackage-Toolkit/releases/tag/1.0.6
[1.0.5]: https://github.com/AlexandreFlanquart/UnityPackage-Toolkit/releases/tag/1.0.5
[1.0.4]: https://github.com/AlexandreFlanquart/UnityPackage-Toolkit/releases/tag/1.0.4
[1.0.3]: https://github.com/AlexandreFlanquart/UnityPackage-Toolkit/releases/tag/1.0.3
[1.0.2]: https://github.com/AlexandreFlanquart/UnityPackage-Toolkit/releases/tag/1.0.2
