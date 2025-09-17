using System;
using System.Text;
using UnityEngine;

namespace MyUnityPackage.Toolkit
{
    // Enhanced logger for Unity with prefix support and method overloads.
    // - Keep only: levels and global enable flag.
    // - Multiple entry points for different logging scenarios.
    // - Optional MinimumLevel filtering, editorOnly & timestamps.
    public static class MUPLogger
    {
        // Logging levels in increasing order of severity.
        public enum LogLevel
        {
            Debug = 0,
            Info = 1,
            Warning = 2,
            Error = 3,
            Exception = 4,
        }

        // Master switch: disable to mute all logs at runtime.
        public static bool Enabled = true;

        // Minimum level to output.
        public static LogLevel MinimumLevel = LogLevel.Info;

        // Global prefix for all logs
        public static string GlobalPrefix = "[MUP]";

        // Enable/disable timestamp in logs
        public static bool IncludeTimestamp = false;

        // Enable/disable editor-only logs in builds
        public static bool EditorOnlyLogsEnabled = true;

        #region Core Logging Methods

        /// <summary>
        /// Log a message with a given level. Optionally attach a Unity context object.
        /// This is the main logging method with full formatting support.
        /// </summary>
        /// <param name="level">Severity of the log.</param>
        /// <param name="message">Text to print. Provide a preformatted message.</param>
        /// <param name="context">Optional context object (makes the log clickable in Console).</param>
        /// <param name="editorOnly">If true, this log will only appear in the Unity Editor, not in builds.</param>
        public static void Log(LogLevel level, string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            if (!Enabled) return;                 // Fast exit if logging is globally disabled
            if (level < MinimumLevel) return;     // Filter out messages below the configured level
            
            // Skip editor-only logs in builds
            if (editorOnly && !IsEditor())
            {
                return;
            }

            string formattedMessage = FormatMessage(level, message);

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    if (context != null) UnityEngine.Debug.Log(formattedMessage, context); else UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    if (context != null) UnityEngine.Debug.LogWarning(formattedMessage, context); else UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Exception: // Simplified: treat Exception as an error message
                    if (context != null) UnityEngine.Debug.LogError(formattedMessage, context); else UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }

        #endregion

        #region Convenience Overloads

        /// <summary>
        /// Log a debug message with optional context.
        /// </summary>
        public static void LogDebug(string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Debug, message, context, editorOnly);
        }

        /// <summary>
        /// Log an info message with optional context.
        /// </summary>
        public static void Info(string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Info, message, context, editorOnly);
        }

        /// <summary>
        /// Log a warning message with optional context.
        /// </summary>
        public static void Warning(string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Warning, message, context, editorOnly);
        }

        /// <summary>
        /// Log an error message with optional context.
        /// </summary>
        public static void Error(string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Error, message, context, editorOnly);
        }

        /// <summary>
        /// Log an exception message with optional context.
        /// </summary>
        public static void Exception(string message, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Exception, message, context, editorOnly);
        }

        /// <summary>
        /// Log an exception with its details.
        /// </summary>
        public static void Exception(System.Exception ex, UnityEngine.Object context = null, bool editorOnly = false)
        {
            Log(LogLevel.Exception, $"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}", context, editorOnly);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Format a log message with prefix, timestamp, and level information.
        /// </summary>
        private static string FormatMessage(LogLevel level, string message)
        {
            var sb = new StringBuilder();

            // Add global prefix
            if (!string.IsNullOrEmpty(GlobalPrefix))
            {
                sb.Append(GlobalPrefix);
                sb.Append(" ");
            }

            // Add timestamp if enabled
            if (IncludeTimestamp)
            {
                sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] ");
            }

            // Add level indicator
            sb.Append($"[{level.ToString().ToUpper()}] ");

            // Add the actual message
            sb.Append(message);

            return sb.ToString();
        }

        /// <summary>
        /// Check if we're running in the Unity Editor.
        /// </summary>
        private static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        #endregion
    }
}
