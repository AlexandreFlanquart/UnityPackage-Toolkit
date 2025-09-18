using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public class GameManagerToolkit : MonoBehaviour
    {
        private void Start()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            AudioManager.Initialize();
        }

        public void StartGame()
        {
            MUPLogger.Info("StartGame");
        }

        public void EndGame()
        {
            MUPLogger.Info("EndGame");   
        }
    }
}
