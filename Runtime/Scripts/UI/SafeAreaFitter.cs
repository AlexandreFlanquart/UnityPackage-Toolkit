using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Adjusts this RectTransform to match the device's safe area: the zone not covered by a
    /// camera notch/punch-hole, rounded corners, the status bar, or the system gesture bar
    /// (<see cref="Screen.safeArea"/>, handled natively by iOS/Android and exposed by Unity).
    ///
    /// USAGE: attach to a full-stretch root panel, directly under the Canvas. All interactive
    /// UI content should be a child of this panel (not a direct child of the Canvas) to
    /// automatically inherit the safe area.
    ///
    /// In landscape, the notch/punch-hole never lands on the top/bottom edge — only on the LEFT
    /// or RIGHT edge depending on the device's rotation direction (LandscapeLeft vs LandscapeRight),
    /// hence a dynamic fitter rather than a fixed one-sided margin.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector2Int lastScreenSize;
        private ScreenOrientation lastOrientation;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Screen.safeArea raises no event: compare every frame (negligible cost, one Rect
            // + two ints) and only recompute when resolution or orientation actually changed
            // (rotation, Android split-screen...).
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height ||
                lastOrientation != Screen.orientation)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // Screen pixels -> normalized anchors (0-1), same origin (bottom-left) as Screen.safeArea.
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;

            MUPLogger.LogDebug($"SafeAreaFitter: applied safeArea={safeArea} on '{name}' (screen={Screen.width}x{Screen.height}, orientation={Screen.orientation}).", this);
        }
    }
}
