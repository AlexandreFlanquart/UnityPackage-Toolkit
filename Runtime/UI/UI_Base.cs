using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    [RequireComponent(typeof(CanvasHelper))]
    public abstract class UI_Base : MonoBehaviour
    {
        [SerializeField] private CanvasHelper canvasHelper = default;

        public virtual void Show()
        {
            canvasHelper.Show();
        }

        public virtual void Hide()
        {
            canvasHelper.Hide();
        }
    }
}