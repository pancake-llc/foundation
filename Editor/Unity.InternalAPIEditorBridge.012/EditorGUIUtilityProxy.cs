using UnityEditor;
using UnityEngine;

namespace UnityInternalBridge
{
    internal static class EditorGUIUtilityProxy
    {
        public static Texture2D GetHelpIcon(MessageType type)
        {
            return EditorGUIUtility.GetHelpIcon(type);
        }
    }
}