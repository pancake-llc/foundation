using UnityEditor;
using UnityEngine;

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

        [Conditional("UNITY_EDITOR")]
        public static void Toast(string message, float duration = 1f)
        {
#if UNITY_EDITOR
            foreach (SceneView scene in SceneView.sceneViews)
            {
                scene.ShowNotification(new GUIContent(message), duration);
                scene.Repaint();
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawCircle(Vector3 position, float radius, int segments, Color color, float duration = 0)
        {
            var angle = 0f;
            var lastPoint = Vector3.zero;
            var thisPoint = Vector3.zero;

            for (var i = 0; i < segments + 1; i++)
            {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

                if (i > 0)
                {
                    Debug.DrawLine(lastPoint + position, thisPoint + position, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }
    }
}