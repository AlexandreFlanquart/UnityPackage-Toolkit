using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using UnityEngine;
using System.Threading.Tasks;

namespace MyUnityPackage.Toolkit
{
    public static class UIManager
    { 
        private static Dictionary<object, object> canvasUI = new Dictionary<object, object>(); // store type and component
        private static Dictionary<string, TransitionSO> transitions = new Dictionary<string, TransitionSO>();
        
        static UIManager()
        {
            Debug.Log("UIManager initialized");
            
            // Initialize transitions dictionary
            transitions = new Dictionary<string, TransitionSO>();
            
            // Load all TransitionSO from Resources/Transitions folder
            TransitionSO[] loadedTransitions = Resources.LoadAll<TransitionSO>("Transitions");
            
            // Add each transition to the dictionary
            foreach (TransitionSO transition in loadedTransitions)
            {
                if (transition != null)
                {
                    transitions[transition.transitionName] = transition;
                    Debug.Log($"Loaded transition: {transition.transitionName}");
                }
            }
        }

        public static void AddCanvasUI<T>(GameObject go) where T : Object
        {
            //Init the dictionary
            if (canvasUI == null)
                canvasUI = new Dictionary<object, object>();

            try
            {
                //Check if the key exist in the dictionary
                if (canvasUI.ContainsKey(typeof(T)))
                {
                    T cui = (T)canvasUI[typeof(T)];
                    if (cui == null) //The key exist but reference object doesn't exist anymore
                    {
                        canvasUI.Remove(typeof(T)); //Remove this key from the dictonary
                        canvasUI.Add(typeof(T), go.GetComponent<T>());
                    }
                }
                else
                {
                    canvasUI.Add(typeof(T), go.GetComponent<T>());
                }
            }
            catch
            {
                throw new System.NotImplementedException("The requested service is already referenced");
            }
        }

        public static T GetCanvasUI<T>() where T : Object
        {
            //Init the dictionary
            if (canvasUI == null)
                canvasUI = new Dictionary<object, object>();

            try
            {
                //Check if the key exist in the dictionary
                if (canvasUI.ContainsKey(typeof(T)))
                {
                    T cui = (T)canvasUI[typeof(T)];
                    if (cui != null) //If Key exist and the object it reference to still exist
                    {
                        return cui;
                    }
                    else //The key exist but reference object doesn't exist anymore
                    {
                        canvasUI.Remove(typeof(T)); //Remove this key from the dictonary
                        return null;
                    }
                }
                else
                {
                    Debug.LogWarning("Can't find requested canvas UI");
                    return null;
                }
            }
            catch
            {
                throw new System.NotImplementedException("Can't find requested canvas UI");
            }
        }

        private static void PlayTransition(GameObject canvas, TransitionSO transition)
        {
            Debug.Log("PlayTransition : " + transition.transitionName + " at canvas : " + canvas.name);
            // Play the transition
            transition.PlayTransition(canvas);
        }

        public static void PlayTransitionByName(GameObject canvas, string transitionName)
        {
            if (transitions.TryGetValue(transitionName, out TransitionSO transition))
            {
                PlayTransition(canvas, transition);
            }
            else
            {
                Debug.LogWarning("Transition not found: " + transitionName);
            }
        }
    }
    
}
