#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using System;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.ValueProviders;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
	[CustomEditor(typeof(LoadAddressable))]
	public sealed class LoadAddressableEditor : ValueProviderDrawer
	{
		public override void Draw([AllowNull] GUIContent label, [AllowNull] SerializedProperty anyProperty, [AllowNull] Type valueType)
		{
			SerializedProperty property = serializedObject.GetIterator();
			property.NextVisible(true);
			while(property.NextVisible(false))
			{
				var guiColorWas = GUI.color;
				NullGuardResult nullGuardResult = target is INullGuard nullGuard ? nullGuard.EvaluateNullGuard(null) : NullGuardResult.Passed;
				bool tintRed = nullGuardResult != NullGuardResult.Passed;

				if(label is null)
				{
					if(tintRed)
					{
						GUI.color = Color.red;
					}

					EditorGUILayout.PropertyField(property, true);
				}
				else
				{
					GUILayout.BeginHorizontal();
					var rect = EditorGUILayout.GetControlRect();
					EditorGUI.BeginProperty(rect, label, property);
					rect = EditorGUI.PrefixLabel(rect, label);

					if(tintRed)
					{
						GUI.color = Color.red;
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