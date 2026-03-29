# UI Management System

Small set of helpers to make UI show/hide and canvas interaction consistent across a project.

## 📋 Overview

UI system:
- **Canvas management**: [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs) and [`CanvasHelper.cs`](../Runtime/Scripts/UI/CanvasHelper.cs)

---

## 🎨 Canvas Management

### [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs)
Abstract base class for all UI components.

```csharp
public abstract class UI_Base : MonoBehaviour
{
    private CanvasHelper canvasHelper = default;

    public virtual void Show()    // Shows the canvas
    public virtual void Hide()    // Hides the canvas
}
```

**Usage:**
```csharp
public class MainMenuUI : UI_Base
{
    public void OnPlayButtonClicked()
    {
        Hide(); // Hide the main menu
    }
}
```

### [`CanvasHelper.cs`](../Runtime/Scripts/UI/CanvasHelper.cs)
Handles showing/hiding canvases with interaction support.

**Features:**
- Enables/disables the `Canvas`
- Controls `CanvasGroup.interactable` and `CanvasGroup.blocksRaycasts`
- `hideOnStart` option to automatically hide at startup



