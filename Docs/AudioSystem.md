# Audio System Documentation

## Overview

The UnityPackage-Toolkit audio system provides a modular, extensible architecture for managing audio in Unity. It uses the Service Locator pattern and dependency injection to improve testability and flexibility.

The system has two layers:
1. **Volume/mute** — `AudioManager` (static) controls the Unity AudioMixer's exposed parameters.
2. **Playback** — dedicated `MonoBehaviour` managers, one per channel, each registered via `ServiceLocator`.

## Architecture

### Architecture diagram

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   AudioUpdater  │────│  IAudioService   │────│ AudioManager    │
│   (UI Layer)    │    │   (Interface)    │    │ (Core Logic)    │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│ AudioSettingsSO │    │AudioManagerService│    │   AudioMixer    │
│ (Configuration) │    │ (Implementation) │    │  (Unity Audio)  │
└─────────────────┘    └──────────────────┘    └─────────────────┘

┌───────────────┐  ┌─────────────┐  ┌────────────────┐  ┌────────────────┐  ┌──────────────┐
│ MusicManager  │  │ SFXManager  │  │ UIAudioManager │  │ AmbienceManager│  │ VoiceManager │
│ (crossfade)   │  │ (pool)      │  │ (PlayOneShot)  │  │ (3 loops)      │  │ (queue)      │
└───────┬───────┘  └──────┬──────┘  └────────┬───────┘  └────────┬───────┘  └──────┬───────┘
        └─────────────────┴──────────────────┴───────────────────┴─────────────────┘
                                              │
                                       AudioCueSO (sound definition)
```

### Playback managers (ServiceLocator)

| Manager | Sources | Use for |
|---|---|---|
| `MusicManager` | 2 (A/B crossfade) | Background music, smooth transitions |
| `AmbienceManager` | 3 independent loops | Environmental layers (wind, rain, crowd) |
| `SFXManager` | Pool of 16 | One-shot game SFX, 2D or fire-and-forget 3D |
| `UIAudioManager` | 1 (`PlayOneShot`) | UI clicks, hover — stacks without interruption |
| `VoiceManager` | 1 (queue) | Dialogue, narration, streaming, I2LOC language paths |
| `AudioPlaybackService` | 3 (Music/SFX/Voice) | **Legacy** — kept for backward compatibility |

## Main Components

### 1. AudioManager (static class)

**File:** [`Runtime/Scripts/Managers/AudioManager.cs`](../Runtime/Scripts/Managers/AudioManager.cs)

`AudioManager` is the core of the audio system. It manages volume/mute settings for five audio channels: `Music`, `SFX`, `Voice`, `Ambience`, and `UI`.

#### Key features:
- **Volume management**: control volume per audio type
- **Mute management**: enable/disable sound
- **dB conversion**: convert between linear values (0–1) and decibels (-80 to 0)
- **Lazy initialization**: initializes settings on demand
- **Mixer group lookup**: `GetAudioMixerGroup(AudioType)` resolves the matching `AudioMixerGroup` for playback managers to route their `AudioSource`s

#### Main public methods:
- `Initialize()`: initializes the system with default volumes (0.8f for Music/SFX/Voice/UI, 0.7f for Ambience)
- `SetVolume(AudioType, float)` / `GetVolume(AudioType)`: set/get the volume for a channel
- `ToggleMute(AudioType)`: toggles mute
- `GetAudioSettingsFromAudioType(AudioType)`: retrieves the full settings object
- `GetAudioMixerGroup(AudioType)`: resolves the `AudioMixerGroup` matching a channel name

### 2. MusicManager

**File:** [`Runtime/Scripts/Managers/MusicManager.cs`](../Runtime/Scripts/Managers/MusicManager.cs)

Crossfades background music between two internal `AudioSource`s (A/B) so the outgoing track fades out while the incoming one fades in.

- `Play(AudioClip clip, float crossfadeDuration = -1f)` / `Play(AudioCueSO cue, float crossfadeDuration = -1f)`
- `Stop(float fadeDuration = 1f)`

### 3. SFXManager

**File:** [`Runtime/Scripts/Managers/SFXManager.cs`](../Runtime/Scripts/Managers/SFXManager.cs)

Pool-based manager (default 16 `AudioSource`s) for one-shot SFX, 2D or 3D. Respects each `AudioCueSO`'s cooldown, max-instance limit, and priority.

- `Play(AudioCueSO cue, Vector3? position = null)`: plays a cue, 3D if a position is given
- `PlayRaw(AudioClip clip, AudioType audioType, float volume = 1f, float pitch = 1f, Vector3? position = null)`: bypasses cue rules
- `SFXStealStrategy`: `DropNew` or `StealOldest` — behaviour when the pool is full

### 4. UIAudioManager

**File:** [`Runtime/Scripts/Managers/UIAudioManager.cs`](../Runtime/Scripts/Managers/UIAudioManager.cs)

Dedicated channel for UI sounds (clicks, hovers). Uses `AudioSource.PlayOneShot` so rapid clicks stack without interrupting each other.

- `Play(AudioCueSO cue)`: plays a UI cue
- `PlayClick()`: plays the Inspector-assigned `defaultClickCue`
- `PlayRaw(AudioClip clip, float volume = 1f)`: raw clip, no cooldown/instance tracking

Consumed by [`UIButtonSound`](#8-uibuttonsound) so buttons get sound with no extra wiring.

### 5. AmbienceManager

**File:** [`Runtime/Scripts/Managers/AmbienceManager.cs`](../Runtime/Scripts/Managers/AmbienceManager.cs)

Manages up to `MaxLayers` (3) independent looping ambience layers, each fading in/out on its own so they can be blended dynamically by gameplay (e.g. indoor/outdoor, weather).

- `SetLayer(int layerIndex, AudioClip clip, float targetVolume = 1f, float fadeDuration = 1f)`: pass `null` clip to fade out and clear the layer
- `SetLayerVolume(int layerIndex, float targetVolume, float fadeDuration = 0.5f)`
- `StopAll(float fadeDuration = 1f)`

### 6. VoiceManager

**File:** [`Runtime/Scripts/Managers/VoiceManager.cs`](../Runtime/Scripts/Managers/VoiceManager.cs)

Handles playback, streaming, and queuing of voice/dialogue clips. Supports local (`Resources`/`Assets`) and streamed (`StreamingAssets`) audio, caches loaded clips, and integrates with I2 Localization when present (`I2LOC_PRESENT`, auto-detected via the asmdef's `versionDefines`).

- `PlayVoices(float delay, params string[] keys)`: always routed through the internal queue (even a single key), so a call made while another sequence is still playing gets appended instead of hijacking the shared `AudioSource`
- `ConfigVoiceElement(GameObject, float delay, params string[] keys)`: adds/configures a [`VoiceAgent`](#9-voiceagent) on a GameObject
- `StopVoice()` / `Mute()` / `GetCurrentVoiceDuration()`
- Clip resolution order: `Assets/Voices/{lang}/{key}` (editor only) → `Resources/Voices/{lang}/{key}` → `Resources/Voices/{key}` → `Resources/{key}`

### 7. AudioCueSO (ScriptableObject)

**File:** [`Runtime/Scripts/ScriptableObjects/AudioCue/AudioCueSO.cs`](../Runtime/Scripts/ScriptableObjects/AudioCue/AudioCueSO.cs)

The central sound definition consumed by `MusicManager`, `SFXManager`, and `UIAudioManager`. Create via **Assets > Create > MyUnityPackage > Audio > AudioCue**.

- **Clips**: one or more variants; `GetClip()` avoids repeating the same clip twice in a row
- **Volume/Pitch randomisation**: `baseVolume` × `volumeVariation` range, `pitchRange`
- **Concurrency**: `cooldown` (min seconds between plays), `maxInstances`, `priority` (used for voice-stealing in `SFXManager`)

`CanPlay()` / `OnPlay()` / `OnStop()` are called internally by the managers — do not call them manually.

### 8. UIButtonSound

**File:** [`Runtime/Scripts/UI/UIButtonSound.cs`](../Runtime/Scripts/UI/UIButtonSound.cs)

Plays a sound when the attached `Button` is clicked. Assign `clickCue` (`AudioCueSO`, preferred — routes through `UIAudioManager`) or `clickClip` (legacy raw clip, routes through `AudioPlaybackService`) as a fallback.

> ⚠️ Upgrading from an older version: the previous `clickSound` field was replaced by `clickCue`/`clickClip` — re-assign it on existing buttons.

### 9. VoiceAgent

**File:** [`Runtime/Scripts/Audio/VoiceAgent.cs`](../Runtime/Scripts/Audio/VoiceAgent.cs)

Specialized component for triggering dialogue on a GameObject — on enable, on hover, or manually.

- **Automatic playback**: on enable and/or on pointer hover
- **Delay management**: delay before playback
- **Automatic stop**: stops when disabled
- **ServiceLocator integration**: uses `VoiceManager`

### 10. AudioFocusHandler

**File:** [`Runtime/Scripts/Managers/AudioFocusHandler.cs`](../Runtime/Scripts/Managers/AudioFocusHandler.cs)

Pauses the Unity `AudioListener` when the application loses focus or is suspended (`OnApplicationPause` on mobile, `OnApplicationFocus` on desktop). Add once to the audio system GameObject.

### 11. IAudioService / AudioManagerService

**Files:** [`Runtime/Scripts/Audio/IAudioService.cs`](../Runtime/Scripts/Audio/IAudioService.cs), [`Runtime/Scripts/Audio/AudioManagerService.cs`](../Runtime/Scripts/Audio/AudioManagerService.cs)

Interface + default implementation exposing `AudioManager`'s volume/mute API for dependency injection and testability (consumed by `AudioUpdater`).

### 12. AudioPlaybackService (legacy)

**File:** [`Runtime/Scripts/Audio/AudioPlaybackService.cs`](../Runtime/Scripts/Audio/AudioPlaybackService.cs)

MonoBehaviour service responsible for playing music, SFX, and voice through 3 fixed `AudioSource`s. Kept for backward compatibility — prefer `MusicManager` / `SFXManager` / `UIAudioManager` / `VoiceManager` for new code.

- `PlayClip(AudioClip, AudioType, bool loop = false, float volume = 1f)` / `PlayFromResources(string path, AudioType, bool loop = false, float volume = 1f)`
- Helpers `PlayMusic`, `PlaySFX`, `PlayVoice`
- `Stop(AudioType)` / `StopAll()`

### 13. AudioUpdater (UI component)

**File:** [`Runtime/Scripts/Audio/AudioUpdater.cs`](../Runtime/Scripts/Audio/AudioUpdater.cs)

MonoBehaviour component that manages the UI for audio control (volume slider, mute button, percentage text, mute sprites). Settings are fetched dynamically from `AudioManager` via `IAudioService.GetAudioSettingsFromAudioType(audioType)` — there is no `AudioSettingsSO` field to assign directly on this component.

**Required:** `audioType` (logs an error and skips slider setup if `slider` is left unassigned). **Optional** (null-checked): `muteButton`, `volumeText`, `muteText`, `isMuted`.

#### Main methods:
- `InjectAudioService(IAudioService)`: dependency injection, replacing the default `AudioManagerService`
- `InitVolumeUpdater()`: refreshes the UI from the current audio service state
- Unsubscribes its slider/button listeners in `OnDestroy()`

### 14. AudioSettingsSO (ScriptableObject)

**File:** [`Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs`](../Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs)

Per-channel audio settings configured via a ScriptableObject.

```csharp
[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "ScriptableObjects/AudioSettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0, 1)]
    public float defaultVolume;    // Default volume
    public Sprite mutedImage;      // Image for muted state
    public Sprite unmutedImage;    // Image for unmuted state
}
```

## Setup and Usage

### 1. AudioMixer setup

1. Create an AudioMixer in Unity
2. Create groups: `Music`, `Ambience`, `SFX` (optionally with children `SFX/World`, `SFX/UI`), `Voice`
3. Expose float parameters named exactly `Music`, `SFX`, `Voice`, `Ambience`, `UI` (these names are matched against `AudioManager.AudioType`)
4. Place the AudioMixer asset at `Resources/AudioMixer`
5. Optional — voice ducking: wire an **Audio Mixer Send** from the Voice group into a **Duck Volume** effect on the Music and Ambience groups (no code required)

### 2. Create an AudioSettingsSO

1. Right-click in the Project window
2. Create → ScriptableObjects → AudioSettingsSO
3. Configure the default volume and sprites
4. Place it at `Resources/AudioSettings/{Music|SFX|Voice|Ambience|UI}SettingsSO` so `AudioManager` auto-loads it on `Initialize()` — a missing SO for a channel just keeps that channel's hardcoded default volume, no error

### 3. Create an AudioCueSO

1. Right-click in the Project window
2. Create → MyUnityPackage → Audio → AudioCue
3. Assign one or more clips, pick the `AudioType`, and tune volume/pitch variation, cooldown, and max instances

### 4. Audio UI setup

1. Create a GameObject with the `AudioUpdater` script
2. Assign the UI components:
   - Slider for volume (required — logs an error if left empty)
   - Button for mute (optional)
   - TextMeshPro for volume/mute text display (optional)
3. Select the audio type (Music/SFX/Voice/Ambience/UI)

### 5. Playback managers setup

Add `MusicManager`, `SFXManager`, `UIAudioManager`, `AmbienceManager`, `VoiceManager` (as needed) and `AudioFocusHandler` to a single persistent `AudioSystem` GameObject. Each manager registers itself in `ServiceLocator` on `Awake`, so it can be retrieved anywhere with `ServiceLocator.GetService<T>()`. See the `AudioExampleScene` sample for a complete working setup.
