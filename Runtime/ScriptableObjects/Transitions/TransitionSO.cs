using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    [CreateAssetMenu(fileName = "TransitionSO", menuName = "ScriptableObjects/TransitionSO")]
    public abstract class TransitionSO : ScriptableObject
    {
        [SerializeField] protected string _transitionName;

        public string transitionName => _transitionName;

        public abstract Task PlayTransition(GameObject target);
    }
}
