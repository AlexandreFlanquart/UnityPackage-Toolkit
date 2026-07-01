using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Loads/unloads a set of scenes additively. Reusable — call <see cref="LoadSceneList(string[], string[])"/>
    /// as many times as needed; <see cref="OnLoadingEvent"/> fires each time a requested load list finishes.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string[] sceneToLoad;
        /// <summary>Raised each time a requested scene load list finishes loading.</summary>
        public event Action OnLoadingEvent;

        private IEnumerator Start()
        {
            yield return LoadSceneListRoutine(sceneToLoad);
        }

        private IEnumerator LoadSceneListRoutine(string[] list)
        {
            foreach (var sceneName in list)
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                if (asyncOp == null)
                {
                    MUPLogger.Error($"SceneLoader: LoadSceneAsync returned null for '{sceneName}' — is it added to Build Settings?");
                    continue;
                }

                while (!asyncOp.isDone)
                {
                    yield return null;
                }
            }

            MUPLogger.Info($"SceneLoader: finished loading {list.Length} scene(s).");
            OnLoadingEvent?.Invoke();
        }

        private IEnumerator LoadAndUnloadRoutine(string[] loadList, string[] unloadList)
        {
            // Unload first and wait for completion so a scene being loaded never overlaps with one still unloading.
            foreach (var sceneName in unloadList)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
                if (unloadOp != null)
                {
                    yield return unloadOp;
                }
            }

            yield return LoadSceneListRoutine(loadList);
        }

        /// <summary>Unloads <paramref name="sceneToUnLoadList"/>, then loads <paramref name="sceneToLoadList"/> additively. Safe to call repeatedly.</summary>
        public void LoadSceneList(string[] sceneToLoadList, string[] sceneToUnLoadList)
        {
            MUPLogger.Info($"SceneLoader: unloading {sceneToUnLoadList.Length}, loading {sceneToLoadList.Length} scene(s).");
            StartCoroutine(LoadAndUnloadRoutine(sceneToLoadList, sceneToUnLoadList));
        }
    }
}
