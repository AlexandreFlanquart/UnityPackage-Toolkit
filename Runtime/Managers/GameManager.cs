using UnityEngine;

namespace Prismify.Toolkit{

    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            SetupGame();
        }

        private void SetupGame()
        {
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
