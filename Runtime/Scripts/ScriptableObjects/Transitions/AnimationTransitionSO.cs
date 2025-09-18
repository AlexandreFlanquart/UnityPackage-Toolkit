using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Base class for all animation transitions.
    /// </summary>
    public abstract class AnimationTransitionSO : TransitionSO
    {
        [SerializeField] protected AnimationClip _animationClip;
        [SerializeField] protected RuntimeAnimatorController _animatorController;

        public AnimationClip animationClip => _animationClip;
        public RuntimeAnimatorController animatorController => _animatorController;

        public override Task PlayTransition(GameObject target)
        {
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }
            // Create a OverrideController based on actual controller 
            AnimatorOverrideController overrideController = new AnimatorOverrideController(_animatorController);

            // Create copy of animations
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            // Replace the target clip
            overrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[0].Key, _animationClip);

            // Apply overrides
            overrideController.ApplyOverrides(overrides);

            // Apply new controller in Animator
            animator.runtimeAnimatorController = overrideController;
            animator.Play("Default", 0, 0f);

            return Task.CompletedTask;
        }
    }
} 