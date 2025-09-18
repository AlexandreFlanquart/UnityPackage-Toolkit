using System.Threading.Tasks;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Custom animation fade transition.
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationFadeTransitionSO", menuName = "ScriptableObjects/AnimationFadeTransitionSO")]
    public class AnimationFadeTransitionSO : AnimationTransitionSO
    {
        public override Task PlayTransition(GameObject target)
        {
            MUPLogger.Info("Play animation TransitionTestSO");
            base.PlayTransition(target);
            MUPLogger.Info("TransitionTestSO finished");
            // Add custom logic
            // For example, wait for the animation to finish
            return Task.CompletedTask;
        }
    }
}
