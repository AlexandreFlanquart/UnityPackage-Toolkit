using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Abstract base for UI screens/panels. Wraps the required sibling <see cref="CanvasHelper"/> component.
    /// </summary>
    [RequireComponent(typeof(CanvasHelper))]
    public abstract class UI_Base : MonoBehaviour
    {
        private CanvasHelper canvasHelper = default;

        /// <summary>Whether this UI is currently shown.</summary>
        public bool IsVisible => canvasHelper.IsVisible;

        protected virtual void Awake()
        {
            canvasHelper = GetComponent<CanvasHelper>();
        }

        protected virtual void Start()
        {
        }

        /// <summary>Shows this UI.</summary>
        public virtual void Show()
        {
            canvasHelper.Show();
        }

        /// <summary>Hides this UI.</summary>
        public virtual void Hide()
        {
            canvasHelper.Hide();
        }
    }
}
