# 🛠️ UnityPackage-Toolkit

## 📋 Description
**UnityPackage-Toolkit** est un package Unity complet qui fournit un ensemble d'outils essentiels pour démarrer rapidement un nouveau projet. Il inclut des systèmes d'audio avancés, de gestion d'interface utilisateur, de transitions animées, et bien plus encore.

Ce toolkit est conçu pour les développeurs qui souhaitent avoir une base solide et modulaire pour leurs projets Unity, avec des fonctionnalités prêtes à l'emploi et optimisées.

## 🚀 Fonctionnalités Principales

### 🎵 **Système Audio**
- **AudioManager** : Gestion centralisée du volume et du mute (Music, SFX, Voice)
- **VoiceManager** : Système de voix avec streaming audio et mise en cache
- **AudioUpdater** : Mise à jour automatique des paramètres audio connectés à l'UI
- **Support AudioMixer** : Intégration complète avec les AudioMixer Unity

### 🖥️ **Gestion d'Interface Utilisateur**
- **UIManager** : Système de gestion centralisé des composants UI
- **UI_Base** : Classe de base pour tous les éléments d'interface
- **CanvasHelper** : Assistant pour la gestion des Canvas
- **Système de transitions** : Animations de transition personnalisables

### 🎬 **Système de Transitions**
- **TransitionSO** : ScriptableObjects pour définir des transitions
- **AnimationFadeTransitionSO** : Transitions de fondu avec animations
- **CustomTransitionSO** : Transitions personnalisées
- **Support Animator** : Intégration avec le système d'animation Unity

### 🌍 **Multilingue**
- **LanguageSwitcher** : Changement de langue en temps réel
- **Support I2Localization** : Intégration avec I2 Localization
- **Interface Toggle** : Contrôles UI pour le changement de langue

### 🏗️ **Architecture de Services**
- **ServiceLocator** : Pattern de localisation de services avec cache
- **Gestion automatique** : Détection et création automatique de services
- **Support inactif** : Localisation même des objets inactifs

### 🎮 **Chargement de Scènes**
- **SceneLoader** : Chargement asynchrone de scènes multiples
- **Gestion additive** : Support du chargement/déchargement de scènes
- **Événements** : Système d'événements pour le suivi du chargement

### 🛠️ **Utilitaires**
- **Logger** : Système de logging personnalisé et optimisé
- **ScriptableObjects** : Configuration via assets (AudioSettings, Transitions)
- **Gestion des ressources** : Chargement optimisé depuis Resources et StreamingAssets

## 📦 Installation

### 🔹 1. Ouvrir le Package Manager
1. Dans Unity, allez dans le **menu supérieur**
2. Cliquez sur **Window > Package Manager**
3. La fenêtre **Package Manager** s'ouvrira

### 🔹 2. Ajouter le Package Git
1. Dans le **Package Manager**, cliquez sur le bouton **➕** (coin supérieur gauche)
2. Sélectionnez **"Add package from git URL..."**
3. Entrez l'URL du repository Git : <br>
   ```
   https://github.com/AlexandreFlanquart/UnityPackage-Toolkit.git
   ```
4. Cliquez sur **"Add"**, Unity téléchargera et installera le package

### 🔹 3. Installer une Version Spécifique (Optionnel)
Pour installer une version spécifique, **ajoutez le tag** à la fin de l'URL : <br>
```
https://github.com/AlexandreFlanquart/UnityPackage-Toolkit.git#v1.0.2
```

## 📚 Utilisation Rapide

### ServiceLocator
```csharp
// Récupérer un service
var audioManager = ServiceLocator.GetService<AudioManager>();

// Ajouter un service
ServiceLocator.AddService<MyService>(gameObject);
```

### AudioManager
```csharp
// Contrôler le volume
AudioManager.SetMusicVolume(0.8f);
AudioManager.SetSFXVolume(0.6f);

// Basculer le mute
AudioManager.ToggleMute(AudioManager.AudioType.Music);
```

### UIManager
```csharp
// Enregistrer un composant UI
UIManager.AddCanvasUI<Canvas>(gameObject);

// Récupérer un composant UI
var canvas = UIManager.GetCanvasUI<Canvas>();

// Jouer une transition
UIManager.PlayTransitionByName(canvas, "FadeIn");
```

## 📋 Changelog
Pour voir les dernières mises à jour du package, consultez [ici](CHANGELOG.md) !

## 🎯 Exemples
Le package inclut des exemples complets dans le dossier `Samples~` :
- Prefabs d'interface utilisateur
- Scènes d'exemple
- Animations de transition
- Scripts de démonstration

## 🛠️ Dépannage
Si vous rencontrez un problème, signalez-le à l'équipe de développement.

## 📄 Attribution
- **Images** : 
  - PixelPerfect : https://www.flaticon.com/free-icon/volume_727269
  - Mayor Icons : https://www.flaticon.com/free-icon/volume-mute_4546899
