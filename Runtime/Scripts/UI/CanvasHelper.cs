using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasHelper : MonoBehaviour
    {
        [SerializeField] private bool hideOnStart = true;
        private Canvas canvas;
        private CanvasGroup canvasGroup;

        public Canvas Canvas
        {
            get
            {
                if (canvas == null) canvas = GetComponent<Canvas>();
                return canvas;
            }
        }
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            if (hideOnStart) Show(false);
        }

        public void Show()
        {
            Show(true);
        }

        public void Hide()
        {
            Show(false);
        }

        private void Show(bool pShow)
        {
            Canvas.enabled = pShow;
            CanvasGroup.interactable = pShow;
            CanvasGroup.blocksRaycasts = pShow;
        }
    }
}
