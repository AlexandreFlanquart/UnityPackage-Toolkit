# Système de Gestion UI

Documentation minimaliste pour le système de gestion de l'interface utilisateur avec support des transitions.

## 📋 Vue d'ensemble

Le système UI se compose de deux parties principales :
- **Gestion des Canvas** : [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs) et [`CanvasHelper.cs`](../Runtime/Scripts/UI/CanvasHelper.cs)
- **Système de Transitions** : ScriptableObjects pour les animations et transitions personnalisées

---

## 🎨 Gestion des Canvas

### [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs)
Classe abstraite de base pour tous les composants UI.

```csharp
public abstract class UI_Base : MonoBehaviour
{
    [SerializeField] private CanvasHelper canvasHelper = default;

    public virtual void Show()    // Affiche le canvas
    public virtual void Hide()    // Cache le canvas
}
```

**Utilisation :**
```csharp
public class MainMenuUI : UI_Base
{
    public void OnPlayButtonClicked()
    {
        Hide(); // Cache le menu principal
    }
}
```

### [`CanvasHelper.cs`](../Runtime/Scripts/UI/CanvasHelper.cs)
Gère l'affichage/masquage des Canvas avec support des interactions.

**Fonctionnalités :**
- Active/désactive le `Canvas`
- Contrôle `CanvasGroup.interactable` et `CanvasGroup.blocksRaycasts`
- Option `hideOnStart` pour masquer automatiquement au démarrage

## 🔄 Système de Transitions

### Architecture des Transitions

```
TransitionSO (Base)
├── CustomTransitionSO (Script personnalisé)
│   └── CustomFadeTransitionSO (Fade par script)
└── AnimationTransitionSO (Animation Unity)
    └── AnimationFadeTransitionSO (Fade par animation)
```

### [`TransitionSO.cs`](../Runtime/Scripts/ScriptableObjects/Transitions/TransitionSO.cs)
Classe de base pour toutes les transitions.

```csharp
public abstract class TransitionSO : ScriptableObject
{
    public string transitionName { get; private set; }
    
    public abstract Task PlayTransition(GameObject target);
}
```

### [`CustomTransitionSO.cs`](../Runtime/Scripts/ScriptableObjects/Transitions/CustomTransitionSO.cs)
Base pour les transitions personnalisées par script.

```csharp
public abstract class CustomTransitionSO : TransitionSO
{
    protected abstract Task OnCustomTransition(GameObject target);
}
```

### [`CustomFadeTransitionSO.cs`](../Runtime/Scripts/ScriptableObjects/Transitions/CustomFadeTransitionSO.cs)
Exemple de classe custom à creer pour faire une transition personnalisée (effet de fondu).

```csharp
[CreateAssetMenu(fileName = "FadeTransitionSO", menuName = "ScriptableObjects/CustomFadeTransitionSO")]
public class CustomFadeTransitionSO : CustomTransitionSO
{
    public float fadeDuration = 1f; // Durée du fondu
    
    protected override async Task OnCustomTransition(GameObject target)
    {
        // TO DO
    }
}
```

### [`AnimationTransitionSO.cs`](../Runtime/Scripts/ScriptableObjects/Transitions/AnimationTransitionSO.cs)
Base pour les transitions par animation Unity.

```csharp
public abstract class AnimationTransitionSO : TransitionSO
{
    [SerializeField] protected AnimationClip _animationClip;
    [SerializeField] protected RuntimeAnimatorController _animatorController;
}
```

**Fonctionnement :**
- Crée un `AnimatorOverrideController`
- Remplace l'animation par défaut
- Lance l'animation sur le GameObject cible

### [`AnimationFadeTransitionSO.cs`](../Runtime/Scripts/ScriptableObjects/Transitions/AnimationFadeTransitionSO.cs)
Exemple de classe custom à creer pour une transition par animation Unity (effet de fondu).

```csharp
[CreateAssetMenu(fileName = "AnimationFadeTransitionSO", menuName = "ScriptableObjects/AnimationFadeTransitionSO")]
public class AnimationFadeTransitionSO : AnimationTransitionSO
{
    public override Task PlayTransition(GameObject target)
    {
        MUPLogger.Info("Play animation TransitionTestSO");
        base.PlayTransition(target);
        MUPLogger.Info("TransitionTestSO finished");
        // Add custom logic
        // For example, wait for the animation to finish
        return Task.CompletedTask;
    }
}
```

---

## 🎮 [`UIManager.cs`](../Runtime/Scripts/Managers/UIManager.cs)

Gestionnaire statique pour l'UI et les transitions.

### Gestion des Canvas UI

```csharp
// Enregistrer un canvas UI
UIManager.AddCanvasUI<MainMenuUI>(gameObject);

// Récupérer un canvas UI
MainMenuUI mainMenu = UIManager.GetCanvasUI<MainMenuUI>();
```

### Gestion des Transitions

**Chargement automatique :**
- Toutes les transitions dans `Resources/Transitions/` sont chargées automatiquement
- Utilise le `transitionName` comme clé

**Utilisation :**

```csharp
// Par nom de transition
UIManager.PlayTransitionByName(canvas, "FadeTransition");

// Par trigger d'animator
UIManager.PlayTransitionByTrigger(canvas, animator, "FadeIn");
```

---

## 📁 Structure des Fichiers

```
Resources/
└── Transitions/
    ├── FadeTransition.asset (CustomFadeTransitionSO)
    ├── SlideTransition.asset (AnimationFadeTransitionSO)
```

---

## ⚙️ Configuration et Création

### 1. Créer un ScriptableObject de Transition

**Pour une transition personnalisée :**
1. Creez un Script scriptableObject qui hérite de AnimationTransitionSO ou CustomTransitionSO selon si vous utilisez un script ou une animation
2. Creez votre SO → `Create` → `ScriptableObjects` → `[votreNomDeScriptableObject]`
3. Placez-le dans `Resources/Transitions/`
4. Configurez le dans l'Inspector ci necessaire

### 2. Configuration du Canvas

**Setup minimal :**
1. Créez un GameObject avec un `Canvas`
2. Ajoutez le composant `CanvasHelper` (ajoute automatiquement `CanvasGroup`)
3. Créez un script qui hérite de `UI_Base`
4. Assignez le script au GameObject
5. Configurez `hideOnStart` dans `CanvasHelper` selon vos besoins
6. Enregistrez le canvas dans `UIManager` si nécessaire

```csharp
public class MainMenuUI : UI_Base
{
    void Start()
    {
        // Enregistrer ce canvas dans le UIManager
        UIManager.AddCanvasUI<MainMenuUI>(gameObject);
    }
}
```

