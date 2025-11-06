using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    [RequireComponent(typeof(CanvasHelper))]
    public abstract class UI_Base : MonoBehaviour
    {
        private CanvasHelper canvasHelper = default;

        protected virtual void Start()
        {
            Debug.Log("UI_Base Start");
            canvasHelper = GetComponent<CanvasHelper>();
        }
        
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