using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Shows/hides a Canvas without deactivating its GameObject, keeping <see cref="Canvas.enabled"/>
    /// and the sibling <see cref="CanvasGroup"/>'s interactivity/raycast state in sync.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasHelper : MonoBehaviour
    {
        [Tooltip("Applied explicitly on Awake so Canvas/CanvasGroup start in sync, regardless of the values authored in the Inspector.")]
        [SerializeField] private bool hideOnStart = true;
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        /// <summary>The managed <see cref="Canvas"/> component.</summary>
        public Canvas Canvas
        {
            get
            {
                if (canvas == null) canvas = GetComponent<Canvas>();
                return canvas;
            }
        }
        /// <summary>The managed <see cref="CanvasGroup"/> component.</summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }
        /// <summary>Whether the canvas is currently shown (enabled, interactable, and raycastable).</summary>
        public bool IsVisible => Canvas.enabled;

        private void Awake()
        {
            // Apply explicitly (not just when hideOnStart is true) so Canvas.enabled and CanvasGroup
            // never start out of sync with each other, even if left inconsistent in the Inspector/prefab.
            Show(!hideOnStart);
        }

        /// <summary>Shows the canvas: enables it and makes it interactable/raycastable.</summary>
        public void Show()
        {
            Show(true);
        }

        /// <summary>Hides the canvas: disables it and makes it non-interactable/non-raycastable.</summary>
        public void Hide()
        {
            Show(false);
        }

        private void Show(bool pShow)
        {
            MUPLogger.Info($"CanvasHelper: {(pShow ? "show" : "hide")} '{name}'.", this);

            Canvas.enabled = pShow;
            CanvasGroup.interactable = pShow;
            CanvasGroup.blocksRaycasts = pShow;
        }
    }
}
