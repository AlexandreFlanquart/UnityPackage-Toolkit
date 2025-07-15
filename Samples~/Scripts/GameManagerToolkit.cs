using UnityEngine;
using MyUnityPackage.Toolkit;

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

            AudioUpdater[] audioUpdatersList = FindObjectsByType<AudioUpdater>(FindObjectsInactive.Include ,FindObjectsSortMode.None);
            foreach (AudioUpdater audioUpdater in audioUpdatersList)
            {
                audioUpdater.Initialize(); 
            }
                
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
