## [Unreleased]

### Added
- Sample: `AudioExampleScene` also shows `OneShotSound` (random variant on click), a 3D sound played by code at a position, and a toggle playing a clip through `AudioSource.PlayOneShot` from its event

## [1.0.10] - 2026-11-09

### Added
- Add `OneShotSound`: plays a random variant of a `SoundSO` once on its own `AudioSource` when `Play()` is called from a UnityEvent or code (e.g. a lock opened by the player)

## [1.0.9] - 2026-11-05

### Fixed
- Sample: the UI click sounds and the scene's fire and bird were hard-wired to the sample mixer, so once the sample was imported into a project with its own `AudioConfig`, the bus sliders and mute did not reach them. Their outputs are now empty and follow the loaded `AudioConfig`

## [1.0.8] - 2026-11-05
Audio system rework. ⚠️ Breaking: see *Migration* below.

### Added
- Add `AudioConfigSO` (`Resources/AudioConfig`): references the AudioMixer, the default output groups (Music, Voice, SFX, UI, World), the default bus volumes, the default click sound and the mute sprites
- Add `AudioBus` enum (`Music`, `SFX`, `Voice`), `AudioManager.VolumeChanged` event and `SetVolume(bus, volume, notify: false)`, so volumes can be saved and restored outside the toolkit
- Add `VoiceLineSO`: one voice line with a clip per language, resolved from the current I2 Localization language (`I2LOC_PRESENT`)
- Add `RandomIntervalSound`: plays a random variant of a `SoundSO` on its own `AudioSource` at random intervals (e.g. a bird singing from time to time)
- Add `SoundHandle`, returned by every `SFXManager.Play*` call (`IsPlaying`, `Stop()`), safe after its pooled source is reused
- Add `SFXManager.PlayUI(sound)`; `SFXManager.Play(sound, position)` now routes to the World group by default and uses a linear rolloff between `spatialMinDistance` and `spatialMaxDistance`
- Add `VoiceAgent.playOnClick` and `stopOnPointerExit`, and public `Play()` / `Stop()` for UnityEvents (`onEnable` / `activeOnHover` are renamed `playOnEnable` / `playOnHover`)
- Add `VoiceManager.PlayId`, so a `VoiceAgent` only stops the playback it started
- Rebuild `AudioExampleScene` to show every audio kind, with generated placeholder sounds

### Changed
- `MusicManager` plays any number of simultaneous loops keyed by `AudioClip` (`Play(clip, volume, fade)`, `SetVolume`, `Stop`, `StopAll`, `IsPlaying`) instead of one crossfaded track. Fades use unscaled time
- `VoiceManager` plays one voice at a time and a new voice interrupts the current one (`Play(VoiceLineSO)`, `Play(AudioClip)`, `Stop()`, `VoiceEnded`). Key-based loading from `Resources`/`StreamingAssets`, streaming and the queue are removed
- `SFXManager` now also plays UI sounds; each sound has its own pooled source, which fixes the pitch of overlapping UI clicks
- `AudioManager` drives three buses instead of five channels, logs a warning when an exposed parameter is missing, and the players re-apply the bus volumes in `Start` (`AudioMixer.SetFloat` calls made before the scene starts are ignored)
- `AudioCueSO` renamed `SoundSO` (menu **MyUnityPackage > Audio > Sound**, script GUID kept: existing assets stay valid). Its `audioType` is replaced by `output` (an `AudioMixerGroup`; empty = the player's default group)
- `UIButtonSound` plays through `SFXManager.PlayUI` and falls back to `AudioConfigSO.DefaultClickSound`; its field `clickCue` is renamed `clickSound` (value kept)
- `AudioUpdater.audioType` renamed `bus` (`AudioBus`), serialized values preserved; it now follows `VolumeChanged`, so several sliders on the same bus stay in sync, and the mute button keeps its own image when the config has no mute sprites
- Players treat a clip that is still loading after `Play()` as playing (clips without *Preload Audio Data*)

### Removed
- `AudioPlaybackService`, `AmbienceManager` (use `MusicManager`), `UIAudioManager` (use `SFXManager.PlayUI`), `IAudioService`, `AudioManagerService`, `AudioSettingsSO` (use `AudioConfigSO`), `UIButtonSound.clickClip`
- `MusicManager.Play(clip, crossfadeDuration)` → `Play(clip, volume, fadeDuration)` ⚠️ an old `Play(clip, 2f)` still compiles and now means volume 2 (clamped to 1); `Stop(fadeDuration)` → `Stop(clip, fadeDuration)` or `StopAll(fadeDuration)`
- `SFXManager.Play` returns a `SoundHandle` instead of an `AudioSource`; `Play(AudioCueSO, Vector3?)` is split into `Play(sound)` / `Play(sound, position)`; `PlayRaw` → `Play(clip, volume, output)`
- `VoiceManager.PlayVoices`, `ConfigVoiceElement`, `Mute`, `GlobalVolume`, `IsAudioStreaming`, `GetCurrentVoiceDuration`; `StopVoice` → `Stop`
- `VoiceAgent.Initialize`, `SetKeys`, `SetDelay`, `PlayVoice`, `StopVoice` (use `SetVoiceLine`, `Play`, `Stop`)
- `AudioManager.Initialize`, `InitVolume`, `SetMusicVolume` / `SetSFXVolume` / `SetVoiceVolume` and their getters, `GetAudioSettingsFromAudioType`, `AudioSetting`; `UIAudioManager.PlayClick`

### Migration
1. Mixer: keep groups `Music`, `Voice`, `SFX` (add children `SFX/UI`, `SFX/World`) and expose parameters `Music`, `Voice`, `SFX`. `Ambience` and `UI` exposed parameters are no longer used.
2. Create `Resources/AudioConfig` and fill it; delete `Resources/AudioSettings/*SettingsSO` assets.
3. Replace `AudioPlaybackService` with `MusicManager` / `SFXManager` / `VoiceManager`: `PlayMusic(clip)` → `MusicManager.Play(clip)`, `PlaySFX(clip)` → `SFXManager.Play(clip)` (keep the returned `SoundHandle` to stop it).
4. `AmbienceManager.SetLayer(i, clip, volume)` → `MusicManager.Play(clip, volume)`; `UIAudioManager.Play(AudioCueSO)` → `SFXManager.PlayUI(SoundSO)`.
5. `AudioManager.AudioType` → `AudioBus`; `AudioManager.GetAudioMixerGroup(type)` → `AudioManager.Config.MusicGroup` / `VoiceGroup` / `SFXGroup` / `UIGroup` / `WorldGroup`.
6. In code, replace `AudioCueSO` with `SoundSO`. Set `SoundSO.output` where a sound relied on `audioType` (UI sounds: the UI group), and assign `VoiceAgent.voiceLine` (keys are gone).
7. `UIButtonSound.clickClip` is gone with no fallback: those buttons now play the config's default click — assign a `SoundSO` to keep a specific sound. Buttons still serialized with the pre-1.0.6 `clickSound` **AudioClip** show a type mismatch: assign a `SoundSO` or leave empty.
8. `AudioUpdater` components set to the removed `Ambience` (3) or `UI` (4) channels now log a warning and do nothing: set them to `Music` or `SFX`.
9. `VoiceAgent.activeOnHover` defaulted to `true`; its successor `playOnHover` defaults to `false` (existing values are kept, new components are silent on hover).

## [1.0.7] - 2026-08-25
### Added
- Add `SafeAreaFitter`: fits a RectTransform to `Screen.safeArea` (notch/punch-hole, rounded corners, status/gesture bars), recomputing only when resolution or orientation actually changes

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
