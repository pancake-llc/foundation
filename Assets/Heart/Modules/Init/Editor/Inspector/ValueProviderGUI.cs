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
				OnClicked();
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

			void OnClicked()
			{
				if(clearValueButtonRect.Contains(Event.current.mousePosition))
				{
					onDiscardButtonPressed.Invoke();
					return;
				}
				
				var targetObject = anyProperty.serializedObject.targetObject;
				var scriptableObject = editor.target as ScriptableObject;

				if(Event.current.button is 1)
				{
					var menu = new GenericMenu();

					menu.AddItem(new("Edit Script"), false, () =>
					{
						if(Find.Script(editor.target.GetType(), out MonoScript script))
						{
							AssetDatabase.OpenAsset(script);
						}
					});

					var valueProviderIsAsset = scriptableObject && AssetDatabase.Contains(scriptableObject);
					if(valueProviderIsAsset)
					{
						menu.AddItem(new("Ping Asset"), false, () =>
						{
							EditorGUIUtility.PingObject(scriptableObject);
						});

						if(AssetDatabase.IsSubAsset(scriptableObject) && PrefabUtility.IsPartOfPrefabInstance(targetObject))
						{
							menu.AddItem(new("Embed Into Prefab Instance"), false, () =>
							{
								if(EditorUtility.DisplayDialog("Convert Into Embedded Value Provider?", $"Do you want to convert the value provider that is currently a sub-asset of the prefab '{AssetDatabase.GetAssetPath(scriptableObject)}' into an value provider embedded in this prefab instance?\n\nChoosing 'Convert' will cause the sub-asset to be deleted from the prefab asset, and moved to into be part of this particular instance of the prefab.", "Convert", "Cancel"))
								{
									var embeddedValueProvider = Object.Instantiate(scriptableObject);
									
									var prefabPath = AssetDatabase.GetAssetPath(scriptableObject);
									Undo.RegisterImporterUndo(prefabPath, "Embed Into Prefab Instance");
									Undo.RegisterFullObjectHierarchyUndo(targetObject, "Embed Into Prefab Instance");
									AssetDatabase.RemoveObjectFromAsset(scriptableObject);
									EditorUtility.SetDirty(targetObject);
									EditorUtility.SetDirty(scriptableObject);
									var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
									if(prefab)
									{
										EditorUtility.SetDirty(prefab);
									}

									referenceProperty.objectReferenceValue = embeddedValueProvider;
									referenceProperty.serializedObject.ApplyModifiedProperties();

									Debug.Log($"Converted the value provider {scriptableObject.GetType().Name} from being a sub-asset of the prefab '{prefabPath}' into being embedded in the prefab instance '{targetObject}'.", targetObject);
								}
							});
						}
					}
					else
					{
						// Allow converting embedded value providers on prefab instances into sub-assets of the prefab asset
						if(PrefabUtility.IsPartOfPrefabInstance(targetObject)
						   && scriptableObject)
						{
							menu.AddItem(new("Make Sub-Asset of Prefab"), false, () =>
							{
								var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetObject);
								if(string.IsNullOrEmpty(scriptableObject.name))
								{
									scriptableObject.name = valueProviderLabel.text;
								}

								Undo.RegisterImporterUndo(prefabPath, "Make Sub-Asset of Prefab");
								Undo.RegisterFullObjectHierarchyUndo(targetObject, "Make Sub-Asset of Prefab");
								AssetDatabase.AddObjectToAsset(scriptableObject, prefabPath);
								EditorUtility.SetDirty(targetObject);
								EditorUtility.SetDirty(scriptableObject);
								PrefabUtility.ApplyPropertyOverride(anyProperty, prefabPath, InteractionMode.UserAction);

								Debug.Log($"Converted embedded value provider {scriptableObject.GetType().Name} into a sub-asset of the prefab asset '{prefabPath}'.", AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
							});
						}
					}

					menu.ShowAsContext();
				}
				
				if(editor.target is IValueProvider valueProvider)
				{
					var value = valueProvider.Value as Object;
					if(value)
					{
						if(value is Component component)
						{
							EditorGUIUtility.PingObject(component.gameObject);
							return;
						}
						
						EditorGUIUtility.PingObject(value);
						return;
					}

					if(scriptableObject)
					{
						EditorGUIUtility.PingObject(scriptableObject);
						return;
					}
					
					Debug.Log($"{valueProvider.GetType().Name} could not locate value of type {TypeUtility.ToString(valueType)} at this time.", referenceProperty.serializedObject.targetObject);
					return;
				}
				
				if(editor.target is IValueByTypeProvider valueByTypeProvider && valueType != null)
				{
					var args = new object[] { referenceProperty.serializedObject.targetObject, null };
					bool found = (bool)valueByTypeProvider.GetType()
						.GetMethod(nameof(IValueByTypeProvider.TryGetFor))
						.MakeGenericMethod(valueType)
						.Invoke(valueByTypeProvider, args);

					if(found && args[1] is Object value && value)
					{
						if(value is Component component)
						{
							EditorGUIUtility.PingObject(component.gameObject);
							return;
						}
						
						EditorGUIUtility.PingObject(value);
						return;
					}
					
					if(found && args[1] is IEnumerable<Object> enumerable)
					{
						switch(enumerable.Count())
						{
							case 0:
								break;
							case 1:
								EditorGUIUtility.PingObject(enumerable.First());
								return;
							default:
								if(enumerable.First() is Component)
								{
									Selection.objects = enumerable.Select(x => (x as Component)?.gameObject).ToArray();
									return;
								}

								Selection.objects = enumerable.ToArray();
								return;
						}
					}
						
					Debug.Log($"{valueByTypeProvider.GetType().Name} could not locate value of type {TypeUtility.ToString(valueType)} at this time.", referenceProperty.serializedObject.targetObject);
				}
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