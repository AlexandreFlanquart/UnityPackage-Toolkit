# Documentation du Système Audio

## Vue d'ensemble

Le système audio de UnityPackage-Toolkit fournit une architecture modulaire et extensible pour la gestion de l'audio dans Unity. Il utilise le pattern Service Locator et l'injection de dépendances pour une meilleure testabilité et flexibilité.

## Architecture

### Diagramme de l'architecture

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

## Composants Principaux

### 1. AudioManager (Classe Statique)

**Fichier :** [`Runtime/Scripts/Managers/AudioManager.cs`](../Runtime/Scripts/Managers/AudioManager.cs)

Le `AudioManager` est le cœur du système audio. Il gère les paramètres audio pour trois types d'audio : Music, SFX et Voice.

#### Fonctionnalités principales :
- **Gestion des volumes** : Contrôle du volume pour chaque type d'audio
- **Gestion du mute** : Activation/désactivation du son
- **Conversion dB** : Conversion entre valeurs linéaires (0-1) et décibels (-80 à 0)
- **Initialisation automatique** : Initialisation paresseuse des paramètres

#### Méthodes publiques principales :
- `Initialize()` : Initialise le système avec les volumes par défaut (0.8f)
- `SetVolume(AudioType, float)` : Définit le volume pour un type d'audio
- `GetVolume(AudioType)` : Récupère le volume actuel
- `ToggleMute(AudioType)` : Active/désactive le mute
- `GetAudioSettingsFromAudioType(AudioType)` : Récupère les paramètres complets

### 2. IAudioService (Interface)

**Fichier :** [`Runtime/Scripts/Audio/IAudioService.cs`](../Runtime/Scripts/Audio/IAudioService.cs)

Interface définissant le contrat pour les services audio, permettant l'injection de dépendances et la testabilité.

#### Méthodes définies :
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

### 3. AudioManagerService (Implémentation)

**Fichier :** [`Runtime/Scripts/Audio/AudioManagerService.cs`](../Runtime/Scripts/Audio/AudioManagerService.cs)

Implémentation par défaut de `IAudioService` qui délègue toutes les opérations au `AudioManager` statique.

```csharp
public class AudioManagerService : IAudioService
{
    public void SetVolume(AudioManager.AudioType audioType, float volume)
        => AudioManager.SetVolume(audioType, volume);
    // ... autres méthodes
}
```

### 4. AudioPlaybackService (Service de lecture)

**Fichier :** [`Runtime/Scripts/Audio/AudioPlaybackService.cs`](../Runtime/Scripts/Audio/AudioPlaybackService.cs)

Service MonoBehaviour responsable de la lecture des musiques, effets sonores et voix. 

#### Fonctionnalités principales :
- **Lecture par canal** : une source audio dédiée pour la musique, les SFX et les voix.
- **Compatibilité AudioMixer** : affecte automatiquement le groupe du mixer correspondant (Music/SFX/Voice) si présent.
- **Chargement via Resources** : possibilité de charger et jouer un clip directement depuis le dossier `Resources`.
- **Boucle ou lecture ponctuelle** : configuration fine via les paramètres `loop` et `volume`.
- **Arrêt global** : méthodes pour stopper un canal spécifique ou l'ensemble des lectures.

#### Méthodes principales :
- `PlayClip(AudioClip, AudioType, bool loop = false, float volume = 1f)` : joue un clip sur le canal souhaité.
- `PlayFromResources(string path, AudioType, bool loop = false, float volume = 1f)` : charge un clip depuis `Resources` et le joue immédiatement.
- Helpers `PlayMusic`, `PlaySFX` et `PlayVoice` : raccourcis pour les canaux les plus courants.
- `Stop(AudioType)` / `StopAll()` : arrête une lecture sur un canal ou tous les canaux.

> ⚠️ **Astuce :** ajoutez ce composant à un GameObject de votre scène (ex: `AudioSystem`) pour exposer le service via le `ServiceLocator`.
Vous pouvez ensuite le récupérer dans vos scripts avec `ServiceLocator.GetService<AudioPlaybackService>()`.

### 5. AudioUpdater (Composant UI)

**Fichier :** [`Runtime/Scripts/Audio/AudioUpdater.cs`](../Runtime/Scripts/Audio/AudioUpdater.cs)

Composant MonoBehaviour qui gère l'interface utilisateur pour le contrôle audio.

#### Fonctionnalités :
- **Slider de volume** : Contrôle visuel du volume
- **Bouton mute** : Activation/désactivation du son
- **Affichage du volume** : Texte montrant le pourcentage
- **Images de mute** : Sprites pour l'état mute/unmute
- **Injection de service** : Possibilité d'injecter un service personnalisé

#### Configuration requise :

**Variables OBLIGATOIRES :**
- `audioType` : Type d'audio à contrôler (Music/SFX/Voice)
- `audioSettingsSO` : Configuration des paramètres (ScriptableObject)

**Variables OPTIONNELLES :**
- `slider` : Composant UI Slider pour le contrôle du volume
- `muteButton` : Bouton pour activer/désactiver le mute
- `volumeText` : Texte affichant le pourcentage de volume
- `muteText` : Texte du bouton mute ("Mute"/"Unmute")
- `isMuted` : État initial du mute (booléen)

> ⚠️ **Important :** Les variables obligatoires doivent être assignées dans l'inspecteur, sinon le composant ne fonctionnera pas correctement. Les variables optionnelles sont vérifiées avec des `null checks` dans le code.

#### Méthodes principales :
- `Initialize()` : Initialise le composant et configure les listeners
- `InjectAudioService(IAudioService)` : Injection de dépendance
- `InitVolumeUpdater()` : Initialise l'UI avec les valeurs par défaut

### 6. AudioSettingsSO (ScriptableObject)

**Fichier :** [`Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs`](../Runtime/Scripts/ScriptableObjects/AudioSettings/AudioSettingsSO.cs)

Configuration des paramètres audio via un ScriptableObject.

```csharp
[CreateAssetMenu(fileName = "AudioSettingsSO", menuName = "ScriptableObjects/AudioSettingsSO")]
public class AudioSettingsSO : ScriptableObject
{
    [Range(0, 1)]
    public float defaultVolume;    // Volume par défaut
    public Sprite mutedImage;      // Image pour l'état mute
    public Sprite unmutedImage;    // Image pour l'état unmute
}
```

### 7. VoiceAgent (Composant Audio)

**Fichier :** [`Runtime/Scripts/Audio/VoiceAgent.cs`](../Runtime/Scripts/Audio/VoiceAgent.cs)

Composant spécialisé pour la gestion des voix et dialogues.

#### Fonctionnalités :
- **Lecture automatique** : Lecture au survol ou à l'activation
- **Gestion des délais** : Délai avant la lecture
- **Arrêt automatique** : Arrêt lors de la désactivation
- **Intégration ServiceLocator** : Utilise le VoiceManager via ServiceLocator

## Configuration et Utilisation

### 1. Configuration de l'AudioMixer

1. Créer un AudioMixer dans Unity
2. Exposer les paramètres suivants :
   - `Music` (float)
   - `SFX` (float) 
   - `Voice` (float)
3. Placer l'AudioMixer dans `Resources/AudioMixer`

### 2. Création d'un AudioSettingsSO

1. Clic droit dans le Project
2. Create → ScriptableObjects → AudioSettingsSO
3. Configurer le volume par défaut et les sprites

### 3. Configuration de l'UI Audio

1. Créer un GameObject avec le script `AudioUpdater`
2. Assigner les composants UI :
   - Slider pour le volume
   - Button pour le mute
   - TextMeshPro pour l'affichage
3. Assigner l'AudioSettingsSO
4. Sélectionner le type d'audio (Music/SFX/Voice)

