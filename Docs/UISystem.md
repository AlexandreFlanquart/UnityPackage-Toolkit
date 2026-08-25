# UI Management System

Small set of helpers to make UI show/hide and canvas interaction consistent across a project.

## 📋 Overview

UI system:
- **Canvas management**: [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs) and [`CanvasHelper.cs`](../Runtime/Scripts/UI/CanvasHelper.cs)
- **Safe area handling**: [`SafeAreaFitter.cs`](../Runtime/Scripts/UI/SafeAreaFitter.cs)

---

## 🎨 Canvas Management

### [`UI_Base.cs`](../Runtime/Scripts/UI/UI_Base.cs)
Abstract base class for all UI components.

```csharp
public abstract class UI_Base : MonoBehaviour
{
    private CanvasHelper canvasHelper = default; // fetched in Awake()

    public bool IsVisible { get; }        // Current shown/hidden state
    public virtual void Show()            // Shows the canvas
    public virtual void Hide()            // Hides the canvas
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
- `hideOnStart` option to automatically hide at startup — applied explicitly on `Awake()` either way, so `Canvas`/`CanvasGroup` never start out of sync even if authored inconsistently in the Inspector
- `IsVisible` read-only property to query the current state

---

## 📱 Safe Area

### [`SafeAreaFitter.cs`](../Runtime/Scripts/UI/SafeAreaFitter.cs)
Fits a `RectTransform` to `Screen.safeArea` — keeps content clear of camera notches/punch-holes, rounded corners, and the status/gesture bars.

```csharp
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    // Recomputes only when Screen.width/height/orientation actually change
}
```

**Usage:** attach to a full-stretch panel directly under the Canvas, and parent all interactive UI content under it (not directly under the Canvas) so it inherits the safe area automatically. In landscape, a notch/punch-hole lands on the left or right edge depending on the device's rotation direction — the fitter recomputes dynamically rather than assuming a fixed side.


