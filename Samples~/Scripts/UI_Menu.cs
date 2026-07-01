using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;


namespace MyUnityPackage.Toolkit
{
    public class UI_Menu : UI_Base
    {
        [SerializeField] private Button buttonPlay;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        { 
            base.Start();
            buttonPlay.onClick.AddListener(OnButtonPlayClick);
        }

        private void OnButtonPlayClick()
        {
            MUPLogger.Info("OnButtonPlayClick");
            Hide();
            ServiceLocator.GetService<UI_Game>().Show();
        }
    }
}
