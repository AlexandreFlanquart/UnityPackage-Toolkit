using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Custom fade transition by script.
    /// </summary>
    [CreateAssetMenu(fileName = "FadeTransitionSO", menuName = "ScriptableObjects/CustomFadeTransitionSO")]
    public class CustomFadeTransitionSO : CustomTransitionSO
    {
        public float fadeDuration = 1f;

        protected override async Task OnCustomTransition(GameObject target)
        {
            MUPLogger.Info("Play custom FadeTransitionSO");
            // Example of a custom fade transition
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            // Fade out
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            // Fade in
            elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }
        }
    }
}
