using System;
using System.Reflection;
using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Extension methods for <see cref="UnityEditor.SerializedObject"/>.
	/// </summary>
	internal static class SerializedObjectExtensions
	{
		private static readonly FieldInfo getNativeObjectPtrField;

		static SerializedObjectExtensions() => getNativeObjectPtrField = typeof(SerializedObject).GetField("m_NativeObjectPtr", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		public static bool IsValid(this SerializedObject serializedObject) => (IntPtr)getNativeObjectPtrField.GetValue(serializedObject) != IntPtr.Zero;
	}
}
