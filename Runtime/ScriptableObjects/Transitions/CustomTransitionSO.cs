using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    public abstract class CustomTransitionSO : TransitionSO
    {
        // Cette classe ne contient pas d'animationClip ni d'animatorController
        // car elle est destinée aux transitions personnalisées

        public override async Task PlayTransition(GameObject target)
        {
            // Les classes dérivées doivent implémenter leur propre logique de transition
            await OnCustomTransition(target);
        }

        protected abstract Task OnCustomTransition(GameObject target);
    }
} 