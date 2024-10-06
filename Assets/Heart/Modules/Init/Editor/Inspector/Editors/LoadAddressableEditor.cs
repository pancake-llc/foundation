#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.ValueProviders;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	[CustomEditor(typeof(LoadAddressable))]
	internal sealed class LoadAddressableEditor : ValueProviderDrawer
	{
		public override void Draw([AllowNull] GUIContent label, [AllowNull] SerializedProperty anyProperty, [AllowNull] Type valueType)
		{
			SerializedProperty property = serializedObject.GetIterator();
			property.NextVisible(true);
			while(property.NextVisible(false))
			{
				var guiColorWas = GUI.color;
				NullGuardResult nullGuardResult = target is INullGuard nullGuard ? nullGuard.EvaluateNullGuard(null) : NullGuardResult.Passed;

				if(label is null)
				{
					if(InitializerEditorUtility.TryGetTintForNullGuardResult(nullGuardResult, out Color setGuiColor))
					{
						GUI.color = setGuiColor;
					}

					EditorGUILayout.PropertyField(property, true);
				}
				else
				{
					GUILayout.BeginHorizontal();
					var rect = EditorGUILayout.GetControlRect();
					EditorGUI.BeginProperty(rect, label, property);
					rect = EditorGUI.PrefixLabel(rect, label);

					if(InitializerEditorUtility.TryGetTintForNullGuardResult(nullGuardResult, out Color setGuiColor))
					{
						GUI.color = setGuiColor;
					}

					EditorGUI.PropertyField(rect, property, GUIContent.none, true);
					EditorGUI.EndProperty();
					GUILayout.EndHorizontal();
				}

				GUI.color = guiColorWas;
			}
		}
	}
}
#endif