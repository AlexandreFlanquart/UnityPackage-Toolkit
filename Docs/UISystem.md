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



