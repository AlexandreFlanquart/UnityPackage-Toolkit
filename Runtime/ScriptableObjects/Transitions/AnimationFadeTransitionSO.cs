using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    [CreateAssetMenu(fileName = "AnimationFadeTransitionSO", menuName = "ScriptableObjects/AnimationFadeTransitionSO")]
    public class AnimationFadeTransitionSO : AnimationTransitionSO
    {
        public override Task PlayTransition(GameObject target)
        {
            Debug.Log("Play animation TransitionTestSO");
            // Appeler la méthode de base si nécessaire
            base.PlayTransition(target);
            Debug.Log("TransitionTestSO finished");
            // Ajouter une logique personnalisée
            // Par exemple, attendre que l'animation soit terminée
            return Task.CompletedTask;
        }
    }
}
