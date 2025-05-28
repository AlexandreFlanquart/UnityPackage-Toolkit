using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    public abstract class TransitionSO : ScriptableObject
    {
        [SerializeField] protected string _transitionName;

        public string transitionName => _transitionName;

        public abstract Task PlayTransition(GameObject target);
    }
}
