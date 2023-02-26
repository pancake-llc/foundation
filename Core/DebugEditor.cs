namespace Pancake
{
    using System.Diagnostics;
    using Debug = UnityEngine.Debug;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Class to log only in Unity editor, double clicking console logs produced by this class still open the calling source file)
    /// NOTE: Implement your own version of this is supported. Just implement a class named "DebugEditor" and any method inside this class starting with "Log" will, when double clicked, open the file of the calling method. Use [Conditional] attributes to control when any of these methods should be included.
    /// </summary>
    public static class DebugEditor
    {
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context = null) { Debug.Log(message, context); }

        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context = null) { Debug.LogWarning(message, context); }

        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message, Object context = null) { Debug.LogError(message, context); }
    }
}