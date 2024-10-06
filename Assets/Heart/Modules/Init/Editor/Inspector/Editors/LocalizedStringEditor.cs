#if UNITY_LOCALIZATION
using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	[CustomEditor(typeof(LocalizedString))]
	internal sealed class LocalizedStringEditor : ValueProviderDrawer
	{
		public override void Draw([AllowNull] GUIContent label, [AllowNull] SerializedProperty anyProperty, [AllowNull] Type valueType)
		{
			bool hierarchyModeWas = EditorGUIUtility.hierarchyMode;
			EditorGUIUtility.hierarchyMode = true;

			var localizedStringProperty = serializedObject.FindProperty("value");
			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			NullGuardResult nullGuardResult = target is INullGuard nullGuard ? nullGuard.EvaluateNullGuard(null) : NullGuardResult.Passed;
			// Don't tint the whole UI if it's expanded, as that would be irritating to use, hindering readability.
			if(InitializerEditorUtility.TryGetTintForNullGuardResult(nullGuardResult, out Color setGuiColor) && !localizedStringProperty.isExpanded)
			{
				var rect = EditorGUILayout.GetControlRect(true);
				EditorGUI.PrefixLabel(rect, label);
				EditorGUI.BeginProperty(rect, label, localizedStringProperty);
				var guiColorWas = GUI.color;
				GUI.color = setGuiColor;
				EditorGUI.PropertyField(rect, localizedStringProperty, GUIContent.none, false);
				GUI.color = guiColorWas;
				EditorGUI.EndProperty();
			}
			else
			{
				EditorGUILayout.PropertyField(localizedStringProperty, label, true);
			}

			EditorGUI.indentLevel = indentLevelWas;

			EditorGUIUtility.hierarchyMode = hierarchyModeWas;
		}
	}
}
#endif