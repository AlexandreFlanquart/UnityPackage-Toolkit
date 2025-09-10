using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    public class MUPLogger
    {

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogMessageEditor(string message)
        {
            Debug.Log(message);
        }
        
        public static void LogMessage(string message)
        {
            Debug.Log(message);
        }
    
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogMessageWarningEditor(string message)
        {
            Debug.LogWarning(message);
        }

        public static void LogMessageError(string message)
        {
            Debug.LogError(message);
        }
    }
}
