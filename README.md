# UnityPackage-Toolkit

## Description
**UnityPackage-Toolkit** is a comprehensive Unity package that provides a set of essential tools to quickly start a new project. It includes an audio system (simultaneous music loops, voices, UI sounds, spatial world sounds, three volume buses), UI management, object pooling, and much more.

This toolkit is designed for developers who want a solid, modular foundation for their Unity projects, with ready-to-use, optimized features.

## Key Features

Detailed documentation for each system is available in the `Docs/` folder:

- [Docs/AudioSystem.md](Docs/AudioSystem.md): `AudioConfigSO` and AudioMixer setup, `AudioManager` (Music / Voice / SFX bus volumes), `MusicManager` (simultaneous loops), `VoiceManager` / `VoiceAgent` (one voice at a time, localized `VoiceLineSO`), `SFXManager` (UI, 2D and 3D one-shots), `RandomIntervalSound` and world sounds, `SoundSO` sound assets.
- [Docs/UISystem.md](Docs/UISystem.md): UI canvas management helpers, base classes, and safe-area handling.
- [Docs/ServiceLocator.md](Docs/ServiceLocator.md): ServiceLocator pattern, creation, and lifecycle of services.
- [Docs/ObjectPool.md](Docs/ObjectPool.md): object pooling and a pool locator helper.
- [Docs/MUPLogger.md](Docs/MUPLogger.md): logger setup, log level management, and Unity integrations.

## Installation

### 1. Open the Package Manager
1. In Unity, go to the **top menu**
2. Click **Window > Package Manager**
3. The **Package Manager** window will open

### 2. Add the Git package
1. In the **Package Manager**, click the **+** button (top-left corner)
2. Select **"Add package from git URL..."**
3. Enter the Git repository URL:
   ```
   https://github.com/AlexandreFlanquart/UnityPackage-Toolkit.git
   ```
4. Click **"Add"**; Unity will download and install the package

### 3. Install a specific version (optional)
To install a specific version, **append the tag** to the end of the URL:
```
https://github.com/AlexandreFlanquart/UnityPackage-Toolkit.git#v1.0.2
```

## Changelog
To see the latest package updates, check out [CHANGELOG.md](CHANGELOG.md)!

## Examples
The package includes complete examples in the `Samples~` folder (import them from **Package Manager > UnityPackage-Toolkit > Samples**):
- `AudioExampleScene`: two music loops with their own volume, the three voice triggers, overlapping UI clicks, a looping fire and a random bird around a movable listener, and the three bus sliders
- `ObjectPoolScene`: object pooling in action
- UI prefabs and demo scripts

## Troubleshooting
If you encounter an issue, report it to the development team.

## Attribution
- **Images**:
  - PixelPerfect: https://www.flaticon.com/free-icon/volume_727269
  - Mayor Icons: https://www.flaticon.com/free-icon/volume-mute_4546899
