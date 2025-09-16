using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public static class UIManager
    {
        // Stores the mapping between UI component types and their instances (e.g., Canvas, custom UI scripts)
        private static Dictionary<Type, UnityEngine.Object> canvasUI = new Dictionary<Type, UnityEngine.Object>(); // store type and component
        // Stores all loaded transitions by name for quick access
        private static Dictionary<string, TransitionSO> transitions = new Dictionary<string, TransitionSO>();

        // Static constructor: called once when the class is first accessed
        static UIManager()
        {
            // Initialize transitions dictionary
            transitions = new Dictionary<string, TransitionSO>();

            // Load all TransitionSO assets from Resources/Transitions folder
            TransitionSO[] loadedTransitions = Resources.LoadAll<TransitionSO>("Transitions");

            // Add each transition to the dictionary using its transitionName as the key
            foreach (TransitionSO transition in loadedTransitions)
            {
                if (transition != null)
                {
                    transitions[transition.transitionName] = transition;
                }
            }
        }

        // Register a UI component of type T from the given GameObject
        public static void AddCanvasUI<T>(GameObject go) where T : Object
        {
            //Init the dictionary
            if (canvasUI == null)
                canvasUI = new Dictionary<Type, UnityEngine.Object>();

            try
            {
                //Check if the key exist in the dictionary
                if (canvasUI.ContainsKey(typeof(T)))
                {
                    T cui = (T)canvasUI[typeof(T)];
                    if (cui == null) //The key exist but reference object doesn't exist anymore
                    {
                        canvasUI.Remove(typeof(T)); //Remove this key from the dictonary
                        canvasUI.Add(typeof(T), (UnityEngine.Object)go.GetComponent<T>());
                    }
                }
                else
                {
                    canvasUI.Add(typeof(T), (UnityEngine.Object)go.GetComponent<T>());
                }
            }
            catch
            {
                throw new System.NotImplementedException("The requested service is already referenced");
            }
        }

        // Retrieve a registered UI component of type T
        public static T GetCanvasUI<T>() where T : Object
        {
            //Init the dictionary
            if (canvasUI == null)
                canvasUI = new Dictionary<Type, UnityEngine.Object>();

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
                    MUPLogger.LogMessageWarningEditor("Can't find requested canvas UI");
                    return null;
                }
            }
            catch
            {
                throw new System.NotImplementedException("Can't find requested canvas UI");
            }
        }

        // Play a transition animation on the given canvas using a TransitionSO
        private static void PlayTransition(GameObject canvas, TransitionSO transition)
        {
            MUPLogger.LogMessage("PlayTransition : " + transition.transitionName + " at canvas : " + canvas.name);
            // Play the transition
            transition.PlayTransition(canvas);
        }

        // Play a transition by its name on the given canvas
        public static void PlayTransitionByName(GameObject canvas, string transitionName)
        {
            if (transitions.TryGetValue(transitionName, out TransitionSO transition))
            {
                PlayTransition(canvas, transition);
            }
            else
            {
                MUPLogger.LogMessageWarningEditor("Transition not found: " + transitionName + " check if this transition is in resource folder");
            }
        }
        // Play a transition by triggering an Animator parameter on the given canvas
        public static void PlayTransitionByTrigger(GameObject canvas, Animator animator, string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
            else
            {
                MUPLogger.LogMessageWarningEditor("Animator not found: " + canvas.name);
            }
        }
       
    }
}
