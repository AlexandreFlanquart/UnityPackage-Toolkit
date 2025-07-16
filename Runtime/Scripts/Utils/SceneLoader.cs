using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyUnityPackage.Toolkit
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string[] sceneToLoad;
        public event Action OnLoadingEvent;

        IEnumerator Start()
        {
            yield return LoadSceneList(sceneToLoad);
        }

        private IEnumerator LoadSceneList(string[] list)
        {
            foreach (var name in list)
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
                while (asyncOp.isDone == false)
                {
                    yield return null;
                }
            }
            OnLoadingEvent?.Invoke();
            OnLoadingEvent = null;
        }
        private void UnloadSceneList(string[] list)
        {
            foreach (var name in list)
            {
                SceneManager.UnloadSceneAsync(name);
            }
        }
        public void LoadSceneList(string[] sceneToLoadList, string[] sceneToUnLoadList)
        {
            UnloadSceneList(sceneToUnLoadList);
            StartCoroutine(LoadSceneList(sceneToLoadList));
        }

    }
}