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

### Changed
- `UIButtonSound` now plays through `UIAudioManager` via an `AudioCueSO` (`clickCue`), falling back to the legacy `AudioPlaybackService` (`clickClip`) when no cue is assigned.
  ⚠️ The old `clickSound` field was renamed/split — re-assign `clickCue`/`clickClip` on any existing button after upgrading.
- `VoiceManager` now assigns its `AudioSource` to the `Voice` AudioMixer group automatically when none is set

### Fixed
- `AudioManager.EnsureInitialized` no longer partially re-initializes when only some settings are null

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
