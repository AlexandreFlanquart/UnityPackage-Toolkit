using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public class GameManagerToolkit : MonoBehaviour
    {
        private void Start()
        {
            var musicManager = ServiceLocator.GetService<MusicManager>();
            if (musicManager != null)
            {
                musicManager.Play(Resources.Load<AudioClip>("Music"));
            }
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
