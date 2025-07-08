using UnityEngine;
using UnityEngine.UI;


namespace MyUnityPackage.Toolkit
{
    public class UI_Menu : UI_Base
    {   
        [SerializeField] private Button buttonPlay;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            UIManager.AddCanvasUI<UI_Menu>(gameObject);
            buttonPlay.onClick.AddListener(OnButtonPlayClick);
        }

        private void OnButtonPlayClick()
        {
            Debug.Log("OnButtonPlayClick");
            UIManager.PlayTransitionByName(UIManager.GetCanvasUI<UI_Menu>().gameObject, "FadeInOut");
            //UIManager.PlayTransitionByName(UIManager.GetCanvasUI<UI_Game>().gameObject, "FadeIn");
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
