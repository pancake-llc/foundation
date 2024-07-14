using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Base class for custom editors for a scriptable object value providers that can be
	/// drawn inlined inside another editor with a prefix label - just like a property drawer.
	/// </summary>
	public abstract class ValueProviderDrawer : Editor
	{
		public abstract void Draw([AllowNull] GUIContent label, [AllowNull] SerializedProperty anyProperty, [AllowNull] Type valueType);

		public sealed override void OnInspectorGUI() => Draw(null, null, null);
	}
}