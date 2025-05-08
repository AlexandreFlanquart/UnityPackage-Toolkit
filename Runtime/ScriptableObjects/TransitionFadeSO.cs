using UnityEngine;
using System.Threading.Tasks;

namespace Prismify.Toolkit
{
    [CreateAssetMenu(fileName = "FadeTransitionSO", menuName = "ScriptableObjects/FadeTransitionSO")]
    public class FadeTransitionSO : CustomTransitionSO
    {
        public float fadeDuration = 1f;

        protected override async Task OnCustomTransition(GameObject target)
        {
            Debug.Log("Play custom FadeTransitionSO");
            // Exemple d'une transition de fade personnalisée
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
