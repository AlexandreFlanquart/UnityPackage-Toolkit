using System.Threading.Tasks;
using UnityEngine;

namespace Prismify.Toolkit
{
    [CreateAssetMenu(fileName = "TransitionTestSO", menuName = "ScriptableObjects/TransitionTestSO")]
    public class TransitionTestSO : AnimationTransitionSO
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
