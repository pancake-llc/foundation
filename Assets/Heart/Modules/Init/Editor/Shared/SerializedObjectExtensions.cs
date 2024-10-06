using System;
using UnityEditor;

namespace Sisus.Shared.EditorOnly
{
    /// <summary>
    /// Extension methods for <see cref="UnityEditor.SerializedObject"/>.
    /// </summary>
    public static class SerializedObjectExtensions
    {
        private static readonly string[] scriptField = { "m_Script" };

        public static bool IsValid(this SerializedObject serializedObject) => serializedObject.m_NativeObjectPtr != IntPtr.Zero;

        public static void DrawPropertiesWithoutScriptField(this SerializedObject serializedObject) => Editor.DrawPropertiesExcluding(serializedObject, scriptField);
    }
}