using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Base class for all custom (by script) transitions.
    /// </summary>
    public abstract class CustomTransitionSO : TransitionSO
    {
        public override async Task PlayTransition(GameObject target)
        {
            // Subclasses must implement their own transition logic
            await OnCustomTransition(target);
        }

        protected abstract Task OnCustomTransition(GameObject target);
    }
} 