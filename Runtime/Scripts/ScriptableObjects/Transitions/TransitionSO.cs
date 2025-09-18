using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    /// <summary>
    /// Base class for all transition scripts.
    /// </summary>
    public abstract class TransitionSO : ScriptableObject
    {
        [SerializeField] private string _transitionName;
        public string transitionName => _transitionName;
        public abstract Task PlayTransition(GameObject target);
    }
}
