using UnityEngine;
using UnityEngine.UI;

namespace MyUnityPackage.Toolkit
{
    public class UI_Game : UI_Base
    {
        [SerializeField] private Button buttonBack;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            buttonBack.onClick.AddListener(OnButtonBackClick);
        }

        private void OnButtonBackClick()
        {
            MUPLogger.Info("OnButtonBackClick");
            Hide();
            ServiceLocator.GetService<UI_Menu>().Show();
        }
    }
}
