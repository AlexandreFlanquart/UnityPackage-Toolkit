using UnityEngine;

namespace MyUnityPackage.Toolkit{

    public class GameManager : MonoBehaviour
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
            Debug.Log("StartGame");
        }

        public void EndGame()
        {
            Debug.Log("EndGame");   
        }


    }
}
