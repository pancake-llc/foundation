using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class ValueProviderGUI : IDisposable
	{
		private Editor editor;
		private readonly GUIContent prefixLabel;
		private readonly GUIContent valueProviderLabel;
		private readonly SerializedProperty anyProperty;
		private readonly SerializedProperty referenceProperty;
		private readonly Type valueType;
		private readonly bool isControlless;
		private readonly Action onDiscardButtonPressed;
		private static readonly GUILayoutOption[] discardButtonLayoutOptions = { GUILayout.Height(10f), GUILayout.Width(10f) };
		private readonly MethodInfo evaluateNullGuard;
		private readonly object[] evaluateNullGuardArgs;

		private static Color ObjectFieldBackgroundColor => InitializerGUI.NowDrawing is not null ? HelpBoxBackgroundColor : InspectorBackgroundColor;
		private static Color HelpBoxBackgroundColor => EditorGUIUtility.isProSkin ? new Color32(64, 64, 64, 255) : new Color32(208, 208, 208, 255);
		private static Color InspectorBackgroundColor => EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(200, 200, 200, 255);

		public ValueProviderGUI(Editor editor, GUIContent prefixLabel, SerializedProperty anyProperty, SerializedProperty referenceProperty, Type anyType, Type valueType, Action onDiscardButtonPressed)
		{
			this.editor = editor;
			this.prefixLabel = prefixLabel;
			this.anyProperty = anyProperty;
			this.referenceProperty = referenceProperty;
			this.valueType = valueType;
			this.onDiscardButtonPressed = onDiscardButtonPressed;

			evaluateNullGuard = anyType.GetMethod(nameof(Any<object>.EvaluateNullGuard), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			evaluateNullGuardArgs = new object[] { anyProperty.serializedObject.targetObject as Component, Context.MainThread };

			var valueProvider = editor.target;
			if(valueProvider?.GetType() is Type valueProviderType)
			{
				valueProviderLabel = new GUIContent("");

				if(valueProviderType.GetCustomAttribute<ValueProviderMenuAttribute>() is ValueProviderMenuAttribute attribute)
				{
					if(!string.IsNullOrEmpty(attribute.ItemName))
					{
						int lastDividerIndex = attribute.ItemName.LastIndexOf('/');
						if(lastDividerIndex != -1)
						{
							valueProviderLabel.text = attribute.ItemName.Substring(lastDividerIndex + 1);
						}
						else
						{
							valueProviderLabel.text = attribute.ItemName;
						}
					}
					else
					{
						valueProviderLabel.text = "";
					}
					
					string tooltip = attribute.Tooltip ?? "";
					valueProviderLabel.tooltip = tooltip;
					if(prefixLabel.text.Length > 0)
					{
						prefixLabel.tooltip = tooltip;
					}
				}
				
				if(valueProviderLabel.text.Length == 0)
				{ 
					valueProviderLabel.text = ObjectNames.NicifyVariableName(valueProviderType.Name);
				}
			}
			else
			{
				valueProviderLabel = GUIContent.none;
			}

			if(editor is ValueProviderDrawer || !CustomEditorUtility.IsDefaultOrOdinEditor(editor.GetType()))
			{
				isControlless = false;
			}
			else
			{
				var firstProperty = editor.serializedObject.GetIterator();
				firstProperty.NextVisible(true);
				isControlless = !firstProperty.NextVisible(false);
			}
		}

		public float OnInspectorGUI()
		{
			var startRect = GetLastRect();

			if(editor is ValueProviderDrawer customDrawer)
			{
				editor.serializedObject.Update();
				GUILayout.Space(-2f);
				GUILayout.BeginHorizontal();
				customDrawer.Draw(prefixLabel, anyProperty, valueType);
				GUILayout.Space(1f);
				bool discard = GUILayout.Button(GUIContent.none, EditorStyles.label, discardButtonLayoutOptions);
				var discardRect = GUILayoutUtility.GetLastRect();
				discardRect.x -= 3f;
				discardRect.y += 1f;
				discardRect.width = 15f;
				GUI.Label(discardRect, GUIContent.none, Styles.Discard);
				GUILayout.EndHorizontal();
				GUILayout.Space(1f);

				if(discard)
				{
					onDiscardButtonPressed();
				}
				else
				{
					editor.serializedObject.ApplyModifiedProperties();
				}

				return 0f;
			}

			if(isControlless)
			{
				editor.serializedObject.Update();
				GUILayout.Space(-5f);
				var fullRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);
				var remainingRect = EditorGUI.PrefixLabel(fullRect, prefixLabel);
				DrawTagGUI(remainingRect);

				// editor can be destroyed by DrawTagGUI if the discard button is pressed
				if(editor)
				{
					editor.serializedObject.ApplyModifiedProperties();
				}

				return 0f;
			}

			bool isExpanded = InternalEditorUtility.GetIsInspectorExpanded(editor);
			Rect foldoutRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.foldout);

			if(isExpanded)
			{
				GUILayout.Space(2f);
				EditorGUI.indentLevel++;
				editor.OnNestedInspectorGUI();
				EditorGUI.indentLevel--;
			}

			var valueProviderLabelRect = foldoutRect;
			valueProviderLabelRect.x += EditorGUIUtility.labelWidth;
			valueProviderLabelRect.width -= EditorGUIUtility.labelWidth;
			DrawTagGUI(valueProviderLabelRect);

			bool setExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, prefixLabel);
			if(isExpanded != setExpanded)
			{
				InternalEditorUtility.SetIsInspectorExpanded(editor, setExpanded);
			}

			return GetLastRect().yMax - startRect.y;
		}

		private static Rect GetLastRect()
		{
			GUILayout.Button(GUIContent.none, EditorStyles.label, GUILayout.Height(0f));
			return GUILayoutUtility.GetLastRect();
		}

		public void DrawTagGUI(Rect valueProviderLabelRect)
		{
			// If script reference field of the inlined Editor has been "hidden" by offsetting the whole
			// editor by one line, then cover up the script reference field by drawing a rectangle on top of it.
			var backgroundRect = valueProviderLabelRect;
			backgroundRect.x = 20f;
			backgroundRect.xMax = valueProviderLabelRect.xMax;
			backgroundRect.height += 1f;
			EditorGUI.DrawRect(backgroundRect, ObjectFieldBackgroundColor);

			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Center the tag vertically
			valueProviderLabelRect.y += 2;
			valueProviderLabelRect.height -= 4f;

			var valueProviderLabelClickableRect = valueProviderLabelRect;
			valueProviderLabelClickableRect.width -= EditorGUIUtility.singleLineHeight;

			float clearValueButtonWidth = EditorGUIUtility.singleLineHeight;
			valueProviderLabelRect.width = Mathf.Min(Styles.ValueProvider.CalcSize(valueProviderLabel).x + clearValueButtonWidth, valueProviderLabelRect.width);

			var clearValueButtonRect = valueProviderLabelRect;
			clearValueButtonRect.x += valueProviderLabelRect.width - clearValueButtonWidth;
			clearValueButtonRect.width = EditorGUIUtility.singleLineHeight;

			if(GUI.Button(valueProviderLabelClickableRect, GUIContent.none, EditorStyles.label))
			{
				if(clearValueButtonRect.Contains(Event.current.mousePosition))
				{
					onDiscardButtonPressed.Invoke();
				}
				else if(editor.target is IValueProvider valueProvider)
				{
					var value = valueProvider.Value as Object;
					if(value != null)
					{
						if(value is Component component)
						{
							EditorGUIUtility.PingObject(component.gameObject);
						}
						else
						{
							EditorGUIUtility.PingObject(value);
						}
					}
					else
					{
						Debug.Log($"{valueProvider.GetType().Name} could not locate value of type {TypeUtility.ToString(valueType)} at this time.", referenceProperty.serializedObject.targetObject);
					}
				}
				else if(editor.target is IValueByTypeProvider valueByTypeProvider && valueType != null)
				{
					var args = new object[] { referenceProperty.serializedObject.targetObject, null };
					bool found = (bool)valueByTypeProvider.GetType()
						.GetMethod(nameof(IValueByTypeProvider.TryGetFor))
						.MakeGenericMethod(valueType)
						.Invoke(valueByTypeProvider, args);

					if(found && args[1] is Object value && value != null)
					{
						if(value is Component component)
						{
							EditorGUIUtility.PingObject(component.gameObject);
						}
						else if(value is Object obj)
						{
							EditorGUIUtility.PingObject(value);
						}
						else if(value is IEnumerable<Object> enumerable)
						{
							switch(enumerable.Count())
							{
								case 0:
									break;
								case 1:
									EditorGUIUtility.PingObject(enumerable.First());
									break;
								default:
									if(enumerable.First() is Component)
									{
										Selection.objects = enumerable.Select(x => (x as Component)?.gameObject).ToArray();
									}
									else
									{
										Selection.objects = enumerable.ToArray();
									}
									break;
							}
						}
					}
					else
					{
						Debug.Log($"{valueByTypeProvider.GetType().Name} could not locate value of type {TypeUtility.ToString(valueType)} at this time.", referenceProperty.serializedObject.targetObject);
					}
				}
			}

			var nullGuardResult = (NullGuardResult)evaluateNullGuard.Invoke(anyProperty.GetValue(), evaluateNullGuardArgs);

			// Tint label green if value exists at this moment
			var backgroundColorWas = GUI.backgroundColor;
			var guiColorWas = GUI.color;
			if(nullGuardResult == NullGuardResult.Passed)
			{
				GUI.backgroundColor = new Color(1f, 1f, 0f);
			}
			else if(nullGuardResult != NullGuardResult.ValueProviderValueNullInEditMode)
			{
				GUI.backgroundColor = new Color(1f, 1f, 0.5f);
				GUI.color = new Color(1f, 0.15f, 0.15f);
			}

			string valueProviderTooltipWas = valueProviderLabel.tooltip;
			string nullGuardTooltip = nullGuardResult.GetTooltip();
			if(!string.IsNullOrEmpty(nullGuardTooltip))
			{
				valueProviderLabel.tooltip = valueProviderTooltipWas.Length == 0 ? nullGuardTooltip : valueProviderTooltipWas + "\n\n" + nullGuardTooltip;
			}

			GUI.Label(valueProviderLabelRect, valueProviderLabel, Styles.ValueProvider);

			valueProviderLabel.tooltip = valueProviderTooltipWas;

			GUI.backgroundColor = backgroundColorWas;
			GUI.color = guiColorWas;

			EditorGUI.indentLevel = indentLevelWas;

			if(GUI.Button(clearValueButtonRect, GUIContent.none, Styles.Discard))
			{
				onDiscardButtonPressed.Invoke();
			}
		}

		public void Dispose()
		{
			if(editor)
			{
				Object.DestroyImmediate(editor);
				editor = null;
			}
		}
	}
}