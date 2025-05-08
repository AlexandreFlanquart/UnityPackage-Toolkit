using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.Animations;
using System.Linq;

namespace Prismify.Toolkit
{
    public abstract class AnimationTransitionSO : TransitionSO
    {
        [SerializeField] protected AnimationClip _animationClip;
        [SerializeField] protected AnimatorController _animatorController;

        public AnimationClip animationClip => _animationClip;
        public AnimatorController animatorController => _animatorController;

        public override Task PlayTransition(GameObject target)
        {
            var animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.AddComponent<Animator>();
            }

            if (animatorController != null && animationClip != null)
            {
                if (!animatorController.animationClips.Contains(animationClip))
                {
                    animatorController.AddMotion(animationClip);
                }
                animator.runtimeAnimatorController = animatorController;
                animator.Play(animationClip.name);
            }

            return Task.CompletedTask;
        }
    }
} 