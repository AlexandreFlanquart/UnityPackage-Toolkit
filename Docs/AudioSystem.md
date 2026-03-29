# Audio System Documentation

## Overview

The UnityPackage-Toolkit audio system provides a modular, extensible architecture for managing audio in Unity. It uses the Service Locator pattern and dependency injection to improve testability and flexibility.

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
```

## Main Components

### 1. AudioManager (static class)

**File:** [`Runtime/Scripts/Managers/AudioManager.cs`](../Runtime/Scripts/Managers/AudioManager.cs)

`AudioManager` is the core of the audio system. It manages settings for three audio types: Music, SFX, and Voice.

#### Key features:
- **Volume management**: control volume per audio type
- **Mute management**: enable/disable sound
- **dB conversion**: convert between linear values (0–1) and decibels (-80 to 0)
- **Lazy initialization**: initializes settings on demand

#### Main public methods:
- `Initialize()`: initializes the system with default volumes (0.8f)
- `SetVolume(AudioType, float)`: sets the volume for an audio type
- `GetVolume(AudioType)`: gets the current volume
- `ToggleMute(AudioType)`: toggles mute
- `GetAudioSettingsFromAudioType(AudioType)`: retrieves the full settings object

### 2. IAudioService (interface)

**File:** [`Runtime/Scripts/Audio/IAudioService.cs`](../Runtime/Scripts/Audio/IAudioService.cs)

Interface defining the contract for audio services, enabling dependency injection and testability.

#### Defined methods:
```csharp
public interface IAudioService
{
    void InitVolume(AudioManager.AudioType audioType, float volume);
    void SetVolume(AudioManager.AudioType audioType, float volume);
    float GetVolume(AudioManager.AudioType audioType);
    void ToggleMute(AudioManager.AudioType audioType);
    AudioManager.AudioSetting GetAudioSettingsFromAudioType(AudioManager.AudioType audioType);
}
```

### 3. AudioManagerService (implementation)

**File:** [`Runtime/Scripts/Audio/AudioManagerService.cs`](../Runtime/Scripts/Audio/AudioManagerService.cs)

Default implementation of `IAudioService` that delegates all operations to the static `AudioManager`.

```csharp
public class AudioManagerService : IAudioService
{
    public void SetVolume(AudioManager.AudioType audioType, float volume)
        => AudioManager.SetVolume(audioType, volume);
    // ... other methods
}
```

### 4. AudioPlaybackService (playback service)

**File:** [`Runtime/Scripts/Audio/AudioPlaybackService.cs`](../Runtime/Scripts/Audio/AudioPlaybackService.cs)

MonoBehaviour service responsible for playing music, sound effects, and voices.

#### Key features:
- **Per-channel playback**: dedicated audio source for music, SFX, and voice
- **AudioMixer compatibility**: automatically assigns the corresponding mixer group (Music/SFX/Voice) when available
- **Resources loading**: can load and play a clip directly from the `Resources` folder
- **Loop or one-shot**: fine control via `loop` and `volume` parameters
- **Global stop**: methods to stop a specific channel or all channels

#### Main methods:
- `PlayClip(AudioClip, AudioType, bool loop = false, float volume = 1f)`: plays a clip on the desired channel
- `PlayFromResources(string path, AudioType, bool loop = false, float volume = 1f)`: loads a clip from `Resources` and plays it immediately
- Helpers `PlayMusic`, `PlaySFX`, and `PlayVoice`: shortcuts for the most common channels
- `Stop(AudioType)` / `StopAll()`: stops playback on a channel or all channels

> ⚠️ **Tip:** add this component to a GameObject in your scene (e.g., `AudioSystem`) to expose the service via the `ServiceLocator`.
> You can then retrieve it in your scripts with `ServiceLocator.GetService<AudioPlaybackService>()`.

### 5. AudioUpdater (UI component)

**File:** [`Runtime/Scripts/Audio/AudioUpdater.cs`](../Runtime/Scripts/Audio/AudioUpdater.cs)

MonoBehaviour component that manages the UI for audio control.

#### Features:
- **Volume slider**: visual volume control
- **Mute button**: toggles sound on/off
- **Volume display**: text showing the percentage
- **Mute images**: sprites for mute/unmute states
- **Service injection**: ability to inject a custom service

#### Required setup:

**REQUIRED variables:**
- `audioType`: audio type to control (Music/SFX/Voice)
- `audioSettingsSO`: settings configuration (ScriptableObject)

**OPTIONAL variables:**
- `slider`: UI Slider component for volume control
- `muteButton`: button to toggle mute
- `volumeText`: text showing the volume percentage
- `muteText`: label for the mute button ("Mute"/"Unmute")
- `isMuted`: initial mute state (bool)

> ⚠️ **Important:** required variables must be assigned in the Inspector; otherwise the component will not work correctly. Optional variables are guarded with null checks in the code.

#### Main methods:
- `Initialize()`: initializes the component and configures listeners
- `InjectAudioService(IAudioService)`: dependency injection
- `InitVolumeUpdater()`: initializes the UI with default values

### 6. AudioSettingsSO (ScriptableObject)

**File:** [`Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs`](../Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs)

Audio settings configured via a ScriptableObject.

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

### 7. VoiceAgent (audio component)

**File:** [`Runtime/Scripts/Audio/VoiceAgent.cs`](../Runtime/Scripts/Audio/VoiceAgent.cs)

Specialized component for managing voice and dialogue.

#### Features:
- **Automatic playback**: plays on hover or activation
- **Delay management**: delay before playback
- **Automatic stop**: stops when disabled
- **ServiceLocator integration**: uses VoiceManager via ServiceLocator

## Setup and Usage

### 1. AudioMixer setup

1. Create an AudioMixer in Unity
2. Expose the following parameters:
   - `Music` (float)
   - `SFX` (float)
   - `Voice` (float)
3. Place the AudioMixer in `Resources/AudioMixer`

### 2. Create an AudioSettingsSO

1. Right-click in the Project window
2. Create → ScriptableObjects → AudioSettingsSO
3. Configure the default volume and sprites

### 3. Audio UI setup

1. Create a GameObject with the `AudioUpdater` script
2. Assign the UI components:
   - Slider for volume
   - Button for mute
   - TextMeshPro for display
3. Assign the AudioSettingsSO
4. Select the audio type (Music/SFX/Voice)

