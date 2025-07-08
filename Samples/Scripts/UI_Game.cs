using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    public class UI_Game : MonoBehaviour
    {
        [SerializeField] private Button buttonBack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            UIManager.AddCanvasUI<UI_Game>(gameObject);
            buttonBack.onClick.AddListener(OnButtonBackClick);
        }

        private void OnButtonBackClick()
        {
            Debug.Log("OnButtonBackClick");
            UIManager.PlayTransitionByName(UIManager.GetCanvasUI<UI_Game>().gameObject, "FadeOut");
            UIManager.PlayTransitionByName(UIManager.GetCanvasUI<UI_Menu>().gameObject, "FadeIn");
        }

    // Update is called once per frame
        void Update()
        {
            
        }
    }
}
