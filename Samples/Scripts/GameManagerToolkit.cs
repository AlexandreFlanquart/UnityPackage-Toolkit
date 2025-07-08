using UnityEngine;
using MyUnityPackage.Toolkit;

namespace MyUnityPackage.Toolkit
{
    public class GameManagerToolkit : MonoBehaviour
    {
        private void Awake()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            AudioManager.Initialize();
        }

        public void StartGame()
        {
            Debug.Log("StartGame");
        }

        public void EndGame()
        {
            Debug.Log("EndGame");   
        }
    }
}
