# Audio System Documentation

## Overview

The audio system covers four kinds of sound, each with one dedicated player:

| Kind | Player | Rule |
|---|---|---|
| 2D music and ambience loops | `MusicManager` | Any number at once, each with its own volume |
| Voice (instructions, dialogue) | `VoiceManager` + `VoiceAgent` | One at a time — a new voice interrupts the current one |
| One-shots: UI clicks, 2D effects, 3D effects at a position | `SFXManager`, or `SoundSO.PlayUI()` from any UI event | Overlap freely (pool) |
| Sounds placed in the world | a plain `AudioSource` (loops), `RandomIntervalSound` (from time to time) or `OneShotSound` (once, on an event) | 3D, attenuated by distance to the `AudioListener` |

The player controls three volume buses with `AudioManager` / `AudioUpdater`:

| Bus | Covers |
|---|---|
| `Music` | Music and ambience loops |
| `Voice` | Every voice |
| `SFX` | UI, world sounds and game effects |

## Setup

### 1. AudioMixer

```
Master
├── Music   (exposed parameter "Music")
├── Voice   (exposed parameter "Voice")
└── SFX     (exposed parameter "SFX")
    ├── UI
    └── World
```

1. Create an AudioMixer with the groups above. Children of `SFX` are only used to balance UI and world sounds against each other.
2. Expose the **volume** of `Music`, `Voice` and `SFX` as float parameters named exactly `Music`, `Voice`, `SFX` (they match the `AudioBus` values).
3. The mixer can live anywhere: it is referenced by the `AudioConfigSO`.

Optional, no code: voice ducking — an **Audio Mixer Send** from `Voice` into a **Duck Volume** effect on `Music`.

### 2. AudioConfigSO

**Create → MyUnityPackage → Audio → AudioConfig**, saved at **`Resources/AudioConfig`** (`AudioManager.ConfigResourcePath`).

| Field | Role |
|---|---|
| `mixer` | The project AudioMixer |
| `musicGroup`, `voiceGroup`, `sfxGroup`, `uiGroup`, `worldGroup` | Default output of each player when a sound has no output set |
| `defaultMusicVolume`, `defaultVoiceVolume`, `defaultSfxVolume` | Bus volumes at startup |
| `mutedSprite`, `unmutedSprite` | Mute button images used by `AudioUpdater` |

### 3. Audio system GameObject

Add `MusicManager`, `VoiceManager`, `SFXManager` and `AudioFocusHandler` to one GameObject. Each manager registers itself in `ServiceLocator` on `Awake`:

```csharp
var music = ServiceLocator.GetService<MusicManager>();
```

### 4. Listener (2D games)

Spatial sounds attenuate with the 3D distance to the `AudioListener`. In a 2D game, put the listener **in the gameplay plane** (e.g. on the player at z = 0), not on a camera at z = -10: otherwise every world sound starts 10 units away.

## Components

### AudioManager (static) — volumes

**File:** [`Runtime/Scripts/Managers/AudioManager.cs`](../Runtime/Scripts/Managers/AudioManager.cs)

- `SetVolume(AudioBus, float, bool notify = true)` / `GetVolume(AudioBus)`: linear volume [0, 1], converted to dB (`20·log10`, floor at -80 dB). Pass `notify: false` to restore saved volumes without raising `VolumeChanged`
- `ToggleMute(AudioBus)` / `IsMuted(AudioBus)`: muting sets the bus to 0 and restores the previous volume on unmute
- `GetMuteSprite(AudioBus)`
- `Config`: the loaded `AudioConfigSO`
- `VolumeChanged` (`Action<AudioBus, float>`): raised on every change — **persistence is not done here**: save from this event and restore with `SetVolume(bus, value, notify: false)` at startup. It fires on every frame of a slider drag: debounce the save (e.g. save on panel close or after a short delay). Subscribe from `Awake`/`Start` or later: the event is cleared when Play mode starts
- `ApplyAll()`: pushes the volumes to the mixer again. Called by `MusicManager`, `VoiceManager` and `SFXManager` in `Start`, because `AudioMixer.SetFloat` calls made before the scene starts are ignored (verified in Play mode: a value set in `AfterSceneLoad` is lost)

`AudioBus` values are serialized (`Music = 0`, `SFX = 1`, `Voice = 2`): do not reorder.

### MusicManager — 2D loops

**File:** [`Runtime/Scripts/Managers/MusicManager.cs`](../Runtime/Scripts/Managers/MusicManager.cs)

A track is identified by its `AudioClip`. Sources are created on demand (no cap) and routed to the Music group.

```csharp
[SerializeField] private AudioClip _forestMusic;
[SerializeField] private AudioClip _riverAmbience;

music.Play(_forestMusic);              // fade in to volume 1
music.Play(_riverAmbience, 0.5f);      // plays on top, at half volume
music.SetVolume(_riverAmbience, 1f);   // player action: louder river, music unchanged
music.Stop(_forestMusic, 2f);          // 2 s fade-out
music.StopAll();
```

- `Play(clip, volume = 1, fadeDuration = -1)`: if the clip already plays, only its volume changes (no restart)
- `SetVolume(clip, volume, fadeDuration = -1)`, `Stop(clip, fadeDuration = -1)`, `StopAll(fadeDuration = -1)`, `IsPlaying(clip)`
- Negative `fadeDuration` = `defaultFadeDuration`; `0` applies the volume at once (use it for a slider dragged every frame). Fades use unscaled time (they run while the game is paused)
- `IsPlaying` stays true during a fade-out; `Play` or `SetVolume` on a track that is fading out cancels the stop and keeps it playing

### VoiceManager — one voice at a time

**File:** [`Runtime/Scripts/Managers/VoiceManager.cs`](../Runtime/Scripts/Managers/VoiceManager.cs)

- `Play(VoiceLineSO)` / `Play(AudioClip)`: interrupts the current voice; playing the same clip again restarts it
- `Stop()`, `IsPlaying`, `CurrentClip`
- `VoiceEnded`: raised when a voice reaches its end — **not** when it is interrupted by another voice or stopped. Code waiting for a voice to end (tutorial step) must also handle the interruption, e.g. with a timeout
- `PlayId`: incremented on each `Play`, identifies one playback
- A voice keeps its language once started: switching language mid-line does not change it

### VoiceLineSO — one line, several languages

**File:** [`Runtime/Scripts/ScriptableObjects/Sound/VoiceLineSO.cs`](../Runtime/Scripts/ScriptableObjects/Sound/VoiceLineSO.cs)

**Create → MyUnityPackage → Audio → VoiceLine.** One `{ languageCode, clip }` entry per language. With I2 Localization (`I2LOC_PRESENT`), `GetClip()` returns the entry matching `LocalizationManager.CurrentLanguageCode` (`"fr"` also matches `"fr-FR"`); otherwise, or when the language is missing (warning), the first entry.

### VoiceAgent — voices without code

**File:** [`Runtime/Scripts/Audio/VoiceAgent.cs`](../Runtime/Scripts/Audio/VoiceAgent.cs)

Put it on a UI element, assign a `VoiceLineSO`, tick one or more triggers:

| Option | Effect |
|---|---|
| `playOnEnable` | Plays when the object becomes active (panel opening) |
| `playOnHover` | Plays when the pointer enters — on a touch screen, on touch |
| `playOnClick` | Plays on click; clicking again restarts the line |
| `stopOnPointerExit` | Off by default: the line plays to the end |
| `stopOnDisable` | On by default: closing the panel stops its voice |
| `delay` | Seconds before the voice starts |

Hover and click need a raycast target (`Image`, text…) on the same GameObject. `Play()` / `Stop()` are public for `UnityEvent`s (e.g. a dedicated "listen again" button). `Stop()` only stops the playback this agent started (`VoiceManager.PlayId`), even if another agent plays the same line. `delay` counts in real time: it also elapses while `Time.timeScale` is 0.

### SFXManager — one-shots

**File:** [`Runtime/Scripts/Managers/SFXManager.cs`](../Runtime/Scripts/Managers/SFXManager.cs)

Pool of `poolSize` sources (default 16); each sound gets its own source, so pitch randomisation never leaks to another sound.

| Call | Output when the sound has none |
|---|---|
| `Play(sound)` | SFX group, 2D |
| `Play(sound, position)` | World group, 3D at `position`, linear rolloff from `spatialMinDistance` (full volume) to `spatialMaxDistance` (silent), set on the `SFXManager` |
| `PlayUI(sound)` | UI group, 2D |
| `Play(clip, volume = 1, output = null)` | `output` or SFX group, 2D, no `SoundSO` rules |

Each call returns a `SoundHandle` (`IsPlaying`, `Stop()`). A handle stays safe after its sound ended: once the source is reused, it reports not playing and `Stop()` does nothing. The default handle means the request was rejected (cooldown, instance limit, full pool).

`stealStrategy`: `StealOldest` (default) interrupts the oldest sound if the new one has equal or higher priority; `DropNew` drops it.

### SoundSO — sound asset

**File:** [`Runtime/Scripts/ScriptableObjects/Sound/SoundSO.cs`](../Runtime/Scripts/ScriptableObjects/Sound/SoundSO.cs)

**Create → MyUnityPackage → Audio → Sound.** Used by `SFXManager`, `RandomIntervalSound` and `OneShotSound`, or played directly with `PlayUI()` (music uses plain clips).

- `clips`: variants, never the same twice in a row
- `output`: mixer group; empty = the player's default (table above)
- `baseVolume` × `volumeVariation`, `pitchRange`: randomised on each play
- `cooldown`, `maxInstances`, `priority`: concurrency rules applied by `SFXManager`

`CanPlay()` / `OnPlay()` / `OnStop()` are called by `SFXManager` — do not call them manually.

### UI sounds — `SoundSO.PlayUI()`

Every UI sound goes through the same gesture, whatever the element: in its event (`Button > On Click`, `Toggle > On Value Changed`, `Slider > On Value Changed`, an `Event Trigger` entry), add a call, drag the `SoundSO` asset as the target and pick `SoundSO > PlayUI ()`. The sound plays through `SFXManager.PlayUI` on the UI group, with the asset's variants and randomisation, and nothing needs to be placed in the screen (an asset can be referenced from any prefab). An element without such a call makes no sound: there is no global click. To change the click of every button at once, change the clips of the shared click asset.

### World sounds

- **Continuous loop** (campfire, river): add an `AudioSource` on the object — clip, *Loop*, *Play On Awake*, *Spatial Blend* = 1, *Doppler Level* = 0, output = World group, *Linear* or *Custom* rolloff with min/max distance in world units. No script needed. A Unity **Preset** set as default for `AudioSource` (Preset Manager) applies these settings to every new source.
- **From time to time** (bird): same `AudioSource` without *Loop*, plus `RandomIntervalSound`.
- **Once, on an event** (a lock being opened): same `AudioSource` without *Loop* nor *Play On Awake*, plus `OneShotSound`; call `Play()` from a `UnityEvent` or from code. A single clip needs no script: call `AudioSource.Play()` directly.

### RandomIntervalSound

**File:** [`Runtime/Scripts/Audio/RandomIntervalSound.cs`](../Runtime/Scripts/Audio/RandomIntervalSound.cs)

Plays a random variant of its `sound` on the `AudioSource` of the same object, waits a random `minDelay`–`maxDelay` seconds after the sound ends, and starts again. The first sound also waits one delay. The delay uses game time: the birds go quiet while `Time.timeScale` is 0. 3D settings come from the `AudioSource`; the sound's volume/pitch randomisation applies, its cooldown and instance limit do not. `playOnEnable` (default on) or `StartPlaying()` / `StopPlaying()`; stops on disable.

### OneShotSound

**File:** [`Runtime/Scripts/Audio/OneShotSound.cs`](../Runtime/Scripts/Audio/OneShotSound.cs)

`Play()` plays one random variant of its `sound` on the `AudioSource` of the same object, with `PlayOneShot`: calls close together overlap instead of cutting each other. Meant for a `UnityEvent` (e.g. an interaction effect) or code. 3D settings come from the `AudioSource`, routed to the World group when its output is empty; the sound's volume/pitch randomisation applies, its cooldown and instance limit do not.

### AudioUpdater — settings UI

**File:** [`Runtime/Scripts/Audio/AudioUpdater.cs`](../Runtime/Scripts/Audio/AudioUpdater.cs)

One per bus: `bus`, `slider` (required), optional `volumeText`, `muteButton` (image switched with the config's mute sprites), `muteText`. `Refresh()` re-reads the state from `AudioManager`.

### AudioFocusHandler

**File:** [`Runtime/Scripts/Managers/AudioFocusHandler.cs`](../Runtime/Scripts/Managers/AudioFocusHandler.cs)

Pauses the `AudioListener` when the application loses focus or is suspended (`OnApplicationPause` on mobile, `OnApplicationFocus` on desktop).

## Import settings (mobile)

Unity's defaults (Decompress On Load, Vorbis 100 %, source sample rate, stereo, no preload) are the worst case on mobile: every loaded clip sits uncompressed in RAM and loads on first play, blocking the main thread. Recommended per kind — apply them with **AudioImporter Presets** filtered by folder in the Preset Manager, so new sounds get them automatically:

| Kind | Load Type | Format | Sample rate | Force To Mono | Preload / Load In Background |
|---|---|---|---|---|---|
| Music and ambience loops | Streaming | Vorbis 60 % | Preserve | no | Preload off / Background **on** |
| Voices | Compressed In Memory | Vorbis 70 % | 44 100 Hz | yes | Preload **on** |
| UI clicks and short effects | Decompress On Load | ADPCM | 44 100 Hz | yes | Preload **on** |
| Instrument notes (low latency) | Decompress On Load | ADPCM (Vorbis 70 % in memory if ADPCM hiss is audible) | 44 100 Hz | as needed | Preload **on** |
| World sounds (3D) | Compressed In Memory | Vorbis 60 % | 44 100 Hz | **yes** (required for correct 3D panning) | Preload **on** |

Mobile devices output at 48 kHz: sources at 96 or 192 kHz only waste memory. Presets apply on import: re-apply them once to existing clips (Preset > Apply, then reimport). The sample's sounds use these settings.

## Notes

- **Scene changes**: the managers are not `DontDestroyOnLoad`. Loading another scene cuts music and voices without fade (and without `VoiceEnded`). If you make them persistent, keep **one** instance per type: a second manager registered in `ServiceLocator` replaces the first, which keeps playing out of reach.
- **Import settings**: keep *Preload Audio Data* on for voices and short sounds. Without it, playback starts a few frames late (the players wait for the clip to load instead of treating it as finished).
- **`Time.timeScale = 0`**: music fades, one-shots and voices keep playing, `VoiceAgent.delay` still elapses; `RandomIntervalSound` waits until the game resumes.

- **No `AudioConfigSO`**: the error is logged once; sounds then play without mixer group and the sliders have no effect.
- **`AudioListener.pause`** (set by `AudioFocusHandler` in background) pauses every source, UI included. A game pause menu that should keep UI clicks must not use it.
- **`VoiceAgent`**: on a touch screen, `playOnHover` fires on touch and `playOnClick` on release — tick only one of them.
- **`AudioUpdater`** shows "Mute" / "Unmute" as plain text: leave `muteText` empty in a localized UI and rely on the mute sprites.
- **An `AudioSource` placed in a scene or prefab with an empty `Output`** bypasses the mixer: no bus slider or mute reaches it. Route it to a group of your project's mixer (World for map sounds).
- **Importing the sample** adds `Resources/AudioConfig` to your project: if you already have one, `Resources.Load` becomes ambiguous — delete one of them.

## Sample

`AudioExampleScene` shows every case: two music loops with their own volume sliders, the three `VoiceAgent` triggers, overlapping UI clicks, buttons and a toggle calling `SoundSO.PlayUI` from their events, a 2D sound played by code, a looping fire, a random bird and a `OneShotSound` played on click around a movable listener, a 3D sound played by code at the fire's position (`SFXManager.Play(sound, position)`), and the three bus sliders. Its sounds are generated placeholders.

No sample asset references the sample mixer: the `SoundSO` outputs and the scene's `AudioSource` outputs are empty, so everything follows the `AudioConfig` that `Resources.Load` finds — the project's own one when the sample is imported next to it. `RandomIntervalSound` routes its source to the World group by itself; `AudioExampleController` does it for the looping fire.
