//#define DEBUG_ENABLED
#define DEBUG_CROSS_SCENE_REFERENCES

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Sisus.Init.Internal;
using Sisus.Init.Serialization;
using Sisus.Init.ValueProviders;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Sisus.NullExtensions;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Custom property drawer for <see cref="Any{T}"/> and <see cref="AnyGeneric{T}"/>,
	/// that allows assigning any value to the property.
	/// </summary>
	[CustomPropertyDrawer(typeof(IAny), useForChildren: true)]
	internal sealed class AnyPropertyDrawer : PropertyDrawer
	{
		public const string ServiceLabel = "Service";
		private const float DROPDOWN_BUTTON_WIDTH = DropdownButton.ADD_ICON_WIDTH;

		private static readonly GUILayoutOption[] oneGUILayoutOption = new GUILayoutOption[1];
		public static NullGuardResult CurrentlyDrawnItemNullGuardResult { get; private set; }

		/// <summary>
		/// Contains the <see cref="SerializedProperty"/> of the Any field that changed
		/// and the new type that the user selected.
		/// </summary>
		public static event Action<SerializedProperty, Type> UserSelectedTypeChanged;

		private const float controlOffset = 3f;
		private static readonly GUIContent valueText = new("Value");
		private static readonly Dictionary<Object, Dictionary<string, State>> statesByTarget = new();
		private float height = EditorGUIUtility.singleLineHeight;

		public static void OpenDropdown(Rect buttonPosition, SerializedProperty anyProperty)
		{
			var state = new State(anyProperty, (FieldInfo)anyProperty.GetMemberInfo());
			state.typeDropdownButton.OpenDropdown(buttonPosition);
		}

		public override void OnGUI(Rect position, SerializedProperty anyProperty, GUIContent label)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = onGUIMarker.Auto();
			#endif

			var targetObject = anyProperty.serializedObject.targetObject;
			if(!statesByTarget.TryGetValue(targetObject, out var states))
			{
				states = new();
				statesByTarget.Add(targetObject, states);
			}

			if(!states.TryGetValue(anyProperty.propertyPath, out State state))
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"states[{anyProperty.propertyPath}] = new State({TypeUtility.ToString(fieldInfo.ReflectedType)}.{fieldInfo.Name})");
				#endif

				state = new(anyProperty, fieldInfo);
				states.Add(anyProperty.propertyPath, state);
			}
			else if(!state.IsValid() || state.ValueHasChanged() || state.anyProperty.serializedObject != anyProperty.serializedObject)
			{
				#if DEV_MODE && DEBUG_ENABLED
				if(!state.IsValid()) { Debug.LogWarning($"State.IsValid:false with Event.current:{Event.current?.type.ToString() ?? "None"}"); }
				else if(state.anyProperty.serializedObject != anyProperty.serializedObject) { Debug.Log($"State.serializedObject has changed. Event.current:{Event.current?.type.ToString() ?? "None"}"); }
				else { Debug.Log($"State.ValueHasChanged:true with Event.current:{Event.current?.type.ToString() ?? "None"}"); }
				#endif

				state.Dispose();
				state = new(anyProperty, fieldInfo);
				states[anyProperty.propertyPath] = state;
				UserSelectedTypeChanged?.Invoke(anyProperty, state.valueType);
			}
			else
			{
				state.Update();
			}

			height = DrawValueField(position, anyProperty, state, state.Label);
			anyProperty.serializedObject.ApplyModifiedProperties();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;

		public static Type GetAnyTypeFromField(FieldInfo fieldInfo)
		{
			var fieldType = fieldInfo.FieldType;
			if(fieldType.IsArray)
			{
				return fieldType.GetElementType();
			}

			// List<Any<T>>, Queue<Any<T>> etc.
			if(fieldType is ICollection && fieldType.IsGenericType)
			{
				return fieldType.GetGenericArguments()[0];
			}

			return fieldType;
		}

		public static bool IsService(Object targetObject, Type valueType) => ServiceUtility.IsServiceDefiningType(valueType) || ServiceUtility.ExistsFor(targetObject, valueType);
		public static Type GetValueTypeFromAnyType(Type anyType) => anyType.GetGenericArguments()[0];
		private bool TryGetAssignableType(State state, Object draggedObject, out Type assignableType) => InitializerEditorUtility.TryGetAssignableType(draggedObject, state.referenceProperty.serializedObject.targetObject, state.anyType, state.valueType, out assignableType);

		private static int lastDraggedObjectCount = 0;

		private float DrawValueField(Rect position, SerializedProperty anyProperty, State state, GUIContent label)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = drawValueFieldMarker.Auto();
			#endif

			// Repaint whenever dragged object references change because
			// the controls can change in reaction to objects being dragged.
			if(lastDraggedObjectCount != DragAndDrop.objectReferences.Length)
			{
				GUI.changed = true;
				lastDraggedObjectCount = DragAndDrop.objectReferences.Length;
			}

			int indentLevelWas = EditorGUI.indentLevel;
			position.height = EditorGUIUtility.singleLineHeight;
			bool draggingAssignableObject = state.draggedObjectIsAssignable.HasValue && state.draggedObjectIsAssignable.Value;
			var referenceProperty = state.referenceProperty;
			var valueProperty = state.valueProperty;
			object managedValue = valueProperty is null || valueProperty.propertyType == SerializedPropertyType.ObjectReference ? null : valueProperty.GetValue();
			bool managedValueIsNull = managedValue is null;
			var referenceValue = referenceProperty.objectReferenceValue;
			bool objectReferenceValueIsNull = !referenceValue;
			var firstTargetObject = anyProperty.serializedObject.targetObject;
			bool drawAsObjectField = !objectReferenceValueIsNull || draggingAssignableObject || (!state.isService && state.drawObjectField);
			bool hasSerializedValue = !managedValueIsNull || !objectReferenceValueIsNull;
			var guiColorWas = GUI.color;
			GUI.color = Color.white;

			if(state.isService && !hasSerializedValue && !IsDraggingObjectReferenceThatIsAssignableToProperty(firstTargetObject, state.anyType, state.valueType))
			{
				bool clicked = EditorServiceTagUtility.Draw(position, label, anyProperty);
				EditorGUI.indentLevel = indentLevelWas;

				if(clicked)
				{
					LayoutUtility.ExitGUI();
				}

				return EditorGUIUtility.singleLineHeight;
			}

			if(state.valueProviderGUI is not null)
			{
				state.valueProviderGUI.OnInspectorGUI();
				CurrentlyDrawnItemNullGuardResult = NullGuardResult.Passed;
				return 0f; // GUI drawn using GUILayout does not count towards property height
			}

			if(state.propertyDrawerOverride is not null)
			{
				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				DrawUsingCustomPropertyDrawer(label, state);
				GUI.color = guiColorWas;
				return 0f; // GUI drawn using GUILayout does not count towards property height
			}

			#if ODIN_INSPECTOR
			if(state.odinDrawer is not null)
			{
				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				DrawUsingOdin(state);
				CurrentlyDrawnItemNullGuardResult = NullGuardResult.Passed;
				GUI.color = guiColorWas;
				return 0f;
			}
			#endif

			if(drawAsObjectField)
			{
				EditorGUI.BeginProperty(position, label, referenceProperty);
				var controlRect = EditorGUI.PrefixLabel(position, label);
				bool drawTypeDropdown = state.drawTypeDropdownButton && objectReferenceValueIsNull && (!draggingAssignableObject || !managedValueIsNull);
				if(drawTypeDropdown)
				{
					controlRect = DrawTypeDropdown(controlRect, state, false);
				}

				if(referenceValue)
				{
					if(referenceValue is CrossSceneReference crossSceneReference)
					{
						if(!crossSceneReference.isCrossScene)
						{
							referenceProperty.objectReferenceValue = crossSceneReference.Value;
						}
						else
						{
							EditorGUI.indentLevel = 0;

							state.crossSceneReferenceDrawer ??= new(state.valueType);

							if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
							{
								GUI.color = setGUIColor;
							}

							state.crossSceneReferenceDrawer.OnGUI(controlRect, referenceProperty, GUIContent.none);
							GUI.color = guiColorWas;

							EditorGUI.indentLevel = indentLevelWas;

							anyProperty.serializedObject.ApplyModifiedProperties();
							EditorGUI.EndProperty();
							return EditorGUI.GetPropertyHeight(valueProperty, label, true);
						}
					}
					else if(CrossSceneReference.Detect(firstTargetObject, referenceValue))
					{
						#if DEV_MODE && DEBUG_CROSS_SCENE_REFERENCES
						Debug.Log($"Cross scene reference detected.\nOwner {firstTargetObject.name} ({firstTargetObject.GetType().Name}) scene: {GetScene(firstTargetObject)}\nReference:{referenceValue.name} ({referenceValue.GetType().Name}) scene: {GetScene(referenceValue)}");
						#endif

						var referenceGameObject = GetGameObject(referenceValue);
						bool isCrossSceneReferenceable = false;
						foreach(var refTag in referenceGameObject.GetComponentsNonAlloc<RefTag>())
						{
							if(refTag.Target == referenceValue)
							{
								isCrossSceneReferenceable = true;
								break;
							}
						}

						if(!isCrossSceneReferenceable)
						{
							var referenceable = referenceGameObject.AddComponent<RefTag, Object>(referenceValue);
							Undo.RegisterCreatedObjectUndo(referenceable, "Create Cross-Scene Reference");
						}

						referenceProperty.objectReferenceValue = Create.Instance<CrossSceneReference, GameObject, Object>(GetGameObject(firstTargetObject), referenceValue);
						#if DEV_MODE
						referenceProperty.objectReferenceValue.name = $"CrossSceneReference {firstTargetObject.name} <- {referenceValue?.name ?? "null"} ({referenceProperty.objectReferenceValue.GetInstanceID()})";
						#endif
					}
				}

				bool preventCrossSceneReferencesWas = EditorSceneManager.preventCrossSceneReferences;
				EditorSceneManager.preventCrossSceneReferences = false;

				EditorGUI.indentLevel = 0;
				if(state.draggedObjectIsAssignable.HasValue && state.draggedObjectIsAssignable.Value
					&& CrossSceneReference.Detect(firstTargetObject, DragAndDrop.objectReferences[0]))
				{

					if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
					{
						GUI.color = setGUIColor;
					}

					CrossSceneReferenceGUI.OverrideObjectPicker(controlRect, state.referenceProperty, state.valueType);
					Object setReferenceValue = EditorGUI.ObjectField(controlRect, GUIContent.none, referenceValue, state.objectFieldConstraint, true);

					GUI.color = guiColorWas;

					if(setReferenceValue != referenceValue && setReferenceValue && TryGetAssignableType(state, setReferenceValue, out _))
					{
						referenceValue = setReferenceValue;
						#if DEV_MODE && DEBUG_CROSS_SCENE_REFERENCES
						Debug.Log($"Cross scene reference detected.\nOwner {firstTargetObject.name} ({firstTargetObject.GetType().Name}) scene: {GetScene(firstTargetObject)}\nReference:{referenceValue.name} ({referenceValue.GetType().Name}) scene: {GetScene(referenceValue)}");
						#endif

						var referenceGameObject = GetGameObject(referenceValue);
						bool isCrossSceneReferenceable = false;
						foreach(var refTag in referenceGameObject.GetComponentsNonAlloc<RefTag>())
						{
							if(refTag.Target == referenceValue)
							{
								isCrossSceneReferenceable = true;
								break;
							}
						}

						if(!isCrossSceneReferenceable)
						{
							var referenceable = referenceGameObject.AddComponent<RefTag, Object>(referenceValue);
							Undo.RegisterCreatedObjectUndo(referenceable, "Create Cross-Scene Reference");
						}

						var crossSceneReference = Create.Instance<CrossSceneReference, GameObject, Object>(GetGameObject(firstTargetObject), referenceValue);
						#if DEV_MODE
						Debug.Assert(crossSceneReference, "referenceProperty.objectReferenceValue null");
						Debug.Assert(firstTargetObject, "firstTargetObject null");
						Debug.Assert(referenceValue, "referenceValue null");
						crossSceneReference.name = $"CrossSceneReference {firstTargetObject.name} <- {referenceValue.name ?? "null"} ({crossSceneReference.GetInstanceID()})";
						#endif

						referenceProperty.objectReferenceValue = crossSceneReference;
						#if DEV_MODE
						Debug.Assert(referenceProperty.objectReferenceValue, "referenceProperty.objectReferenceValue null");
						#endif
					}
				}
				else
				{
					if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
					{
						GUI.color = setGUIColor;
					}

					DrawObjectField(controlRect, referenceValue, state);

					GUI.color = guiColorWas;
				}
				
				EditorGUI.indentLevel = indentLevelWas;

				EditorSceneManager.preventCrossSceneReferences = preventCrossSceneReferencesWas;

				if(controlRect.Contains(Event.current.mousePosition))
				{
					DragAndDrop.visualMode = state.draggedObjectIsAssignable.HasValue && !state.draggedObjectIsAssignable.Value ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Generic;
				}
				else if(state.draggedObjectIsAssignable.HasValue)
				{
					var tintColor = state.draggedObjectIsAssignable.Value ? new Color(0f, 1f, 0f, 0.05f) : new Color(1f, 0f, 0f, 0.05f);
					EditorGUI.DrawRect(controlRect, tintColor);
				}

				EditorGUI.EndProperty();
				return EditorGUIUtility.singleLineHeight;
			}

			bool drawDiscardButton = state.drawDiscardButton;
			if(drawDiscardButton)
			{
				var discardRect = position;
				discardRect.x = position.xMax - DiscardButton.Width;
				state.discardValueButton.Draw(discardRect);

				position.width -= DiscardButton.Width;
			}

			bool drawTypeDropdownButton = state.drawTypeDropdownButton;

			// Elements with foldouts are drawn better, when not drawing PrefixLabel manually,
			// so prefer that, if type dropdown does not need to be drawn.
			if(!drawTypeDropdownButton && !draggingAssignableObject && valueProperty?.propertyType is not SerializedPropertyType.ObjectReference)
			{
				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				EditorGUI.PropertyField(position, valueProperty, label, true);

				GUI.color = guiColorWas;

				return EditorGUI.GetPropertyHeight(valueProperty, label, true);
			}

			// Arrays and lists are drawn better, when not drawing PrefixLabel manually.
			if(valueProperty is not null && valueProperty.isArray
			   // SerializedProperty.isArray is true for string fields! Skip those.
			   && valueProperty.propertyType != SerializedPropertyType.String)
			{
				var dropdownRect = position;
				float labelWidth = EditorGUIUtility.labelWidth;
				dropdownRect.x += labelWidth;
				const float ARRAY_SIZE_CONTROL_WIDTH = 200f;
				dropdownRect.width -= labelWidth + ARRAY_SIZE_CONTROL_WIDTH;

				if(dropdownRect.width >= EditorGUIUtility.singleLineHeight)
				{
					DrawTypeDropdown(dropdownRect, state, true);
				}

				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				EditorGUI.PropertyField(position, valueProperty, label, true);

				GUI.color = guiColorWas;

				if(dropdownRect.width >= EditorGUIUtility.singleLineHeight)
				{
					DrawTypeDropdown(dropdownRect, state, true);
				}

				return EditorGUI.GetPropertyHeight(valueProperty, label, true);
			}

			float labelWidthWas = EditorGUIUtility.labelWidth;
			Rect remainingRect;
			if(draggingAssignableObject)
			{
				if(valueProperty.isExpanded && drawTypeDropdownButton)
				{
					EditorGUI.PrefixLabel(position, label);
					remainingRect = position;
				}
				else
				{
					remainingRect = EditorGUI.PrefixLabel(position, label);
				}

				label = GUIContent.none;

				Rect prefixRect = position;
				prefixRect.xMax = remainingRect.x;

				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				DrawObjectField(prefixRect, referenceValue, state);

				GUI.color = guiColorWas;
			}
			else if(drawTypeDropdownButton && !managedValueIsNull && valueProperty.HasFoldoutInInspector(managedValue.GetType()))
			{
				var dropdownRect = position;
				float labelWidth = EditorGUIUtility.labelWidth;
				dropdownRect.x += labelWidth;
				if(dropdownRect.width >= EditorGUIUtility.singleLineHeight)
				{
					DrawTypeDropdown(dropdownRect, state, false);
				}

				EditorGUIUtility.labelWidth += DROPDOWN_BUTTON_WIDTH;
				drawTypeDropdownButton = false;
				remainingRect = position;
			}
			else
			{
				remainingRect = label.text.Length == 0 ? position : EditorGUI.PrefixLabel(position, label);
				label = GUIContent.none;
			}

			if(valueProperty?.propertyType != SerializedPropertyType.ManagedReference)
			{
				EditorGUI.indentLevel = 0;
				if(drawTypeDropdownButton)
				{
					remainingRect = DrawTypeDropdown(remainingRect, state, false);
				}

				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				EditorGUI.PropertyField(remainingRect, valueProperty, label, true);

				GUI.color = guiColorWas;
				EditorGUI.indentLevel = indentLevelWas;
				EditorGUIUtility.labelWidth = labelWidthWas;
				return EditorGUI.GetPropertyHeight(valueProperty, label, true);
			}

			if(state.valueType == typeof(object))
			{
				switch(valueProperty.type)
				{
					case "managedReference<Int32>":
						SetManagedReference(valueProperty, new _Integer() { value = (int)managedValue }, "Set Int Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Integer>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var intWas = ((_Integer)managedValue).value;
						var setInt = EditorGUI.IntField(remainingRect, label, intWas);
						if(intWas != setInt)
						{
							SetManagedReference(valueProperty, new _Integer() { value = setInt }, "Set Int Value");
						}

						EditorGUI.EndProperty();
						valueProperty.serializedObject.ApplyModifiedProperties();

						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Type>":
						SetManagedReference(valueProperty, new _Type((Type)managedValue, null), "Set Type Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Boolean>":
						SetManagedReference(valueProperty, new _Boolean() { value = (bool)managedValue }, "Set Boolean Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Boolean>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var boolWas = ((_Boolean)managedValue).value;
						var setBool = EditorGUI.Toggle(remainingRect, label, boolWas);
						if(boolWas != setBool)
						{
							SetManagedReference(valueProperty, new _Boolean() { value = setBool }, "Set Boolean Value");
						}

						EditorGUI.EndProperty();
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Single>":
						SetManagedReference(valueProperty, new _Float() { value = (float)managedValue }, "Set Float Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Float>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var floatWas = ((_Float)managedValue).value;
						var setFloat = EditorGUI.FloatField(remainingRect, label, floatWas);
						if(floatWas != setFloat)
						{
							SetManagedReference(valueProperty, new _Float() { value = setFloat }, "Set Float Value");
						}

						EditorGUI.EndProperty();

						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Double>":
						SetManagedReference(valueProperty, new _Double() { value = (double)managedValue }, "Set Double Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Double>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var doubleWas = ((_Double)managedValue).value;
						var setDouble = EditorGUI.DoubleField(remainingRect, label, doubleWas);
						if(doubleWas != setDouble)
						{
							SetManagedReference(valueProperty, new _Double() { value = setDouble }, "Set Double Value");
						}

						EditorGUI.EndProperty();

						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<String>":
						SetManagedReference(valueProperty, new _String() { value = (string)managedValue }, "Set String Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_String>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var stringWas = ((_String)managedValue).value;
						var setString = EditorGUI.TextField(remainingRect, label, stringWas);
						if(stringWas != setString)
						{
							SetManagedReference(valueProperty, new _String() { value = setString }, "Set String Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Color>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var colorWas = (Color)managedValue;
						var setColor = EditorGUI.ColorField(remainingRect, label, colorWas);
						if(colorWas != setColor)
						{
							SetManagedReference(valueProperty, setColor, "Set Color Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector2>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector2Was = (Vector2)managedValue;
						var setVector2 = EditorGUI.Vector2Field(remainingRect, label, vector2Was);
						if(vector2Was != setVector2)
						{
							SetManagedReference(valueProperty, setVector2, "Set Vector2 Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector3>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector3Was = (Vector3)managedValue;
						var setVector3 = EditorGUI.Vector3Field(remainingRect, label, vector3Was);
						if(vector3Was != setVector3)
						{
							SetManagedReference(valueProperty, setVector3, "Set Vector3 Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector4>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector4Was = (Vector4)managedValue;
						var setVector4 = EditorGUI.Vector4Field(remainingRect, label, vector4Was);
						if(vector4Was != setVector4)
						{
							SetManagedReference(valueProperty, setVector4, "Set Vector4 Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector2Int>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector2IntWas = (Vector2Int)managedValue;
						var setVector2Int = EditorGUI.Vector2IntField(remainingRect, label, vector2IntWas);
						if(vector2IntWas != setVector2Int)
						{
							SetManagedReference(valueProperty, setVector2Int, "Set Vector2Int Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector3Int>":
						if(drawTypeDropdownButton)
						{
							remainingRect = DrawTypeDropdown(remainingRect, state, false);
						}

						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector3IntWas = (Vector3Int)managedValue;
						var setVector3Int = EditorGUI.Vector3IntField(remainingRect, label, vector3IntWas);
						if(vector3IntWas != setVector3Int)
						{
							SetManagedReference(valueProperty, setVector3Int, "Set Vector3Int Value");
						}

						EditorGUI.EndProperty();
						GUI.color = guiColorWas;
						EditorGUI.indentLevel = indentLevelWas;
						EditorGUIUtility.labelWidth = labelWidthWas;
						return EditorGUIUtility.singleLineHeight;
				}
			}

			if(drawTypeDropdownButton)
			{
				if(!state.valueHasChildProperties && label.text.Length > 0)
				{
					remainingRect = EditorGUI.PrefixLabel(remainingRect, label);
					label = GUIContent.none;
				}

				if(TryGetNullArgumentGuardBasedControlTint(out Color setGUIColor))
				{
					GUI.color = setGUIColor;
				}

				DrawTypeDropdown(remainingRect, state, true);

				GUI.color = guiColorWas;
			}

			if(managedValueIsNull)
			{
				GUI.color = guiColorWas;
				EditorGUI.indentLevel = indentLevelWas;
				EditorGUIUtility.labelWidth = labelWidthWas;
				return EditorGUIUtility.singleLineHeight;
			}

			var assignedInstanceType = managedValue.GetType();
			if(assignedInstanceType is null)
			{
				GUI.color = guiColorWas;
				EditorGUI.indentLevel = indentLevelWas;
				EditorGUIUtility.labelWidth = labelWidthWas;
				return EditorGUIUtility.singleLineHeight;
			}

			bool isSerializableByUnity = TypeUtility.IsSerializableByUnity(assignedInstanceType);

			if(!isSerializableByUnity)
			{
				var boxPosition = position;
				boxPosition.y += position.height;
				EditorGUI.HelpBox(boxPosition, assignedInstanceType.Name + " is missing the [Serializable] attribute.", MessageType.Info);
				GUI.color = guiColorWas;
				EditorGUI.indentLevel = indentLevelWas;
				EditorGUIUtility.labelWidth = labelWidthWas;
				return EditorGUIUtility.singleLineHeight * 2f;
			}

			if(state.valueHasChildProperties)
			{
				EditorGUI.PropertyField(position, valueProperty, label, true);
				GUI.color = guiColorWas;
				EditorGUI.indentLevel = indentLevelWas;
				EditorGUIUtility.labelWidth = labelWidthWas;
				return EditorGUI.GetPropertyHeight(valueProperty, valueText, true);
			}

			var valuePosition = position;
			valuePosition.y += position.height;

			EditorGUI.indentLevel++;
			EditorGUI.PropertyField(valuePosition, valueProperty, valueText, true);
			EditorGUI.indentLevel--;

			GUI.color = guiColorWas;
			EditorGUI.indentLevel = indentLevelWas;
			EditorGUIUtility.labelWidth = labelWidthWas;
			return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(valueProperty, valueText, true);
		}

		private static bool IsCrossSceneReference(Object owner, Object reference)
		{
			var referenceScene = GetScene(reference);
			return referenceScene.IsValid() && GetScene(owner) != referenceScene;
		}

		void DrawObjectField(Rect controlRect, Object referenceValue, State state)
		{
			CrossSceneReferenceGUI.OverrideObjectPicker(controlRect, state.referenceProperty, state.valueType);
			EditorGUI.ObjectField(controlRect, state.referenceProperty, state.objectFieldConstraint, GUIContent.none);

			Object newReferenceValue = state.referenceProperty.objectReferenceValue;
			if(newReferenceValue == referenceValue || !newReferenceValue)
			{
				return;
			}

			if(!state.valueType.IsInstanceOfType(newReferenceValue))
			{
				if(newReferenceValue is GameObject gameObject)
				{
					newReferenceValue = Find.In(gameObject, state.valueType) as Object;
					if(!newReferenceValue)
					{
						state.referenceProperty.objectReferenceValue = referenceValue;
						return;
					}

					state.referenceProperty.objectReferenceValue = newReferenceValue;
					if(newReferenceValue == referenceValue)
					{
						return;
					}
				}
				else
				{
					MethodInfo isCreatableFromMethod = state.anyType.GetMethod(nameof(Any<object>.IsCreatableFrom), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					bool isCreatableFrom = (bool)isCreatableFromMethod.Invoke(null, new object[] { newReferenceValue, state.referenceProperty.serializedObject.targetObject });
					if(!isCreatableFrom)
					{
						state.referenceProperty.objectReferenceValue = referenceValue;
						return;
					}
				}
			}

			if(!state.valueType.IsValueType && state.valueProperty?.GetValue() != Null)
			{
				state.valueProperty.SetValue(null);
				state.referenceProperty.serializedObject.ApplyModifiedProperties();
				state.Update(true);
				UserSelectedTypeChanged?.Invoke(state.anyProperty, null);
				GUI.changed = true;
				InspectorContents.Repaint();
				LayoutUtility.ExitGUI();
			}
		}

		static void DrawUsingCustomPropertyDrawer(GUIContent label, State state)
		{
			SerializedProperty valueProperty;
			if(typeof(Object).IsAssignableFrom(state.valueType))
			{
				valueProperty = state.anyProperty.FindPropertyRelative("reference");
			}
			else
			{
				valueProperty = state.anyProperty.FindPropertyRelative("value");
				valueProperty ??= state.anyProperty.FindPropertyRelative("reference");
			}

			float height = state.propertyDrawerOverride.GetPropertyHeight(valueProperty, label);
			if(height < 0f)
			{
				height = EditorGUIUtility.singleLineHeight;
			}

			oneGUILayoutOption[0] = GUILayout.Height(height);
			var rect = EditorGUILayout.GetControlRect(oneGUILayoutOption);
			state.anyProperty.serializedObject.Update();
			try
			{
				state.propertyDrawerOverride.OnGUI(rect, valueProperty, label);
			}
			finally
			{
				if(state.IsValid())
				{
					state.anyProperty.serializedObject.ApplyModifiedProperties();
				}
			}
		}

		#if ODIN_INSPECTOR
		static void DrawUsingOdin(State state)
		{
			state.odinDrawer.Tree.BeginDraw(true);
			state.odinDrawer.Draw();
			state.odinDrawer.Tree.EndDraw();
		}
		#endif

		private static bool IsDraggingObjectReferenceThatIsAssignableToProperty(Object targetObject, Type anyType, Type valueType)
		{
			if(DragAndDrop.objectReferences.Length == 0)
			{
				return false;
			}

			return InitializerEditorUtility.TryGetAssignableType(DragAndDrop.objectReferences[0], targetObject, anyType, valueType, out _);
		}

		private static void SetManagedReference<T>([DisallowNull] SerializedProperty valueProperty, T setValue, string undoText)
		{
			var targets = valueProperty.serializedObject.targetObjects;

			Undo.RecordObjects(targets, undoText);

			valueProperty.managedReferenceValue = setValue;

			foreach(var target in targets)
			{
				EditorUtility.SetDirty(target);
			}
		}

		/// <summary>
		/// Can an <see cref="Object"/> type value be assigned to a field of type <paramref name="valueType"/>?
		/// <para>
		/// Does not consider whether or not any value providers can be assigned to an Any field with the given value type.
		/// </para>
		/// </summary>
		/// <param name="valueType"></param>
		/// <returns> <see langword="true"/> if <paramref name="valueType"/> is <see cref="object"/>, <see cref="Object"/>
		/// or an interface type implemented by any non-abstact <see cref="Object"/>-derived class;
		/// otherwise, <see langword="false"/>. </returns>
		public static bool CanAssignUnityObjectToField([DisallowNull] Type valueType)
		{
			if(valueType == typeof(object) || typeof(Object).IsAssignableFrom(valueType))
			{
				return true;
			}

			if(valueType.IsInterface)
			{
				foreach(var derivedType in TypeCache.GetTypesDerivedFrom(valueType))
				{
					if(typeof(Object).IsAssignableFrom(derivedType) && !derivedType.IsAbstract)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool CanAssignNonUnityObjectToField([DisallowNull] Type valueType)
		{
			if(typeof(Object).IsAssignableFrom(valueType))
			{
				return false;
			}

			if(valueType.IsInterface)
			{
				foreach(var derivedType in TypeCache.GetTypesDerivedFrom(valueType))
				{
					if(!typeof(Object).IsAssignableFrom(derivedType) && !derivedType.IsAbstract)
					{
						return true;
					}
				}

				return false;
			}

			// Skip values that Unity can't serialize nor draw in the inspector
			if(typeof(Delegate).IsAssignableFrom(valueType)
			|| typeof(IDictionary).IsAssignableFrom(valueType)
			|| typeof(MemberInfo).IsAssignableFrom(valueType)
			|| valueType == typeof(DateTime)
			|| valueType == typeof(TimeSpan))
			{
				return false;
			}

			return true;
		}

		private static Rect DrawTypeDropdown(Rect rect, [DisallowNull] State state, bool fullWidth)
		{
			float totalWidth = rect.width;
			float width = rect.width;
			if(!fullWidth)
			{
				GUIContent buttonLabel = state.typeDropdownButton.buttonLabel;
				if(buttonLabel.text.Length > 0)
				{
					width = DropdownButton.TextStyle.CalcSize(buttonLabel).x + DROPDOWN_BUTTON_WIDTH + 3f;
				} 
				else
				{
					width = DROPDOWN_BUTTON_WIDTH;
				}

				rect.width = width;
			}
			
			bool showMixedValueWas = EditorGUI.showMixedValue;
			if(state.valueProperty is not null && state.valueProperty.hasMultipleDifferentValues)
			{
				EditorGUI.showMixedValue = true;
			}

			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// If GUI is tinted by null argument guard, but value selected in the dropdown is not Null,
			// then we don't tint the type dropdown value, as that would be misleading, but only the value that follows it.
			if(state.typeDropdownButton.buttonLabel.text.Length == 0 && (GUI.color == InitializerEditorUtility.nullGuardFailedColor || GUI.color == InitializerEditorUtility.nullGuardWarningColor))
			{
				var guiColorWas = GUI.color;
				GUI.color = Color.white;
				state.typeDropdownButton.Draw(rect);
				GUI.color = guiColorWas;
			}
			else
			{
				state.typeDropdownButton.Draw(rect);
			}

			EditorGUI.indentLevel = indentLevelWas;

			EditorGUI.showMixedValue = showMixedValueWas;

			var remainingRect = rect;
			remainingRect.x += width + controlOffset;
			remainingRect.width = totalWidth - width - controlOffset;

			return remainingRect;
		}

		private static Scene GetScene(Object target) => target is Component component && component ? component.gameObject.scene : target is GameObject gameObject && gameObject ? gameObject.scene : default;
		private static GameObject GetGameObject(Object target) => target is Component component && component ? component.gameObject : target as GameObject;

		public static void Dispose(SerializedObject initializerSerializedObject)
		{
			var targetObject = initializerSerializedObject.targetObject;
			if(!statesByTarget.TryGetValue(targetObject, out var states))
			{
				return;
			}

			foreach(var state in states.Values)
			{
				state.Dispose();
			}

			statesByTarget.Remove(targetObject);
		}

		public static void Dispose(SerializedProperty anyProperty)
		{
			var target = anyProperty.serializedObject.targetObject;
			if(target is not null && statesByTarget.TryGetValue(target, out var states) &&
				states.TryGetValue(anyProperty.propertyPath, out var state))
			{
				states.Remove(anyProperty.propertyPath);
				state.Dispose();
			}
		}

		public static void UpdateValueBasedState(SerializedProperty anyProperty)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = updateValueBasedStateMarker.Auto();
			#endif

			if(!anyProperty.serializedObject.targetObject)
			{
				return;
			}

			if(statesByTarget.TryGetValue(anyProperty.serializedObject.targetObject, out var states)
				&& states.TryGetValue(anyProperty.propertyPath, out var state))
			{
				state.UpdateValueBasedState();
			}
		}

		public static void DisposeAllStaticState()
		{
			#if DEV_MODE && DEBUG_ENABLED
			Debug.Log($"AnyPropertyDrawer.DisposeAllStaticState() with Event.current.type:{Event.current?.type.ToString() ?? "None"}");
			#endif

			foreach(var states in statesByTarget.Values.ToArray())
			{
				foreach(var state in states.Values)
				{
					state.Dispose();
				}
			}

			statesByTarget.Clear();
		}

		private sealed class State
		{
			public readonly Type anyType;
			public readonly Type valueType;
			public readonly IEqualityComparer equalityComparer;
			public readonly bool valueHasChildProperties;
			public readonly bool canBeUnityObject;
			public readonly bool canBeNonUnityObject;

			public Type objectFieldConstraint;
			public bool isService;
			public bool? draggedObjectIsAssignable;

			/// <summary>
			/// Should the <see cref="Any{T}"/> field be drawn as an Object reference field at this moment?
			/// <para>
			/// True if field currently holds an Object reference value, if only Object reference values can be assigned to the field,
			/// or if the user has selected "Object" from the type dropdown of an interface or object field.
			/// </para>
			/// </summary>
			public bool drawObjectField;
			public bool drawNullOption;
			public bool drawTypeDropdownButton;
			public bool drawDiscardButton;
			public SerializedProperty anyProperty;
			public SerializedProperty referenceProperty;
			public SerializedProperty valueProperty;
			public TypeDropdownButton typeDropdownButton;
			public DiscardButton discardValueButton;
			public object valueLastFrame;
			public readonly Attribute[] attributes;

			public ValueProviderGUI valueProviderGUI;
			public CrossSceneReferenceGUI crossSceneReferenceDrawer;
			public readonly PropertyDrawer propertyDrawerOverride;

			#if ODIN_INSPECTOR
			public readonly InspectorProperty odinDrawer;
			#endif

			public bool SkipWhenDrawing
			{
				get
				{
					#if ODIN_INSPECTOR
					if(odinDrawer?.GetActiveDrawerChain()?.Current?.SkipWhenDrawing ?? false)
					{
						return true;
					}
					#endif

					return false;
				}
			}

			private GUIContent label;

			public GUIContent Label
			{
				get => label;

				set
				{
					label = value;

					#if ODIN_INSPECTOR
					if(odinDrawer is not null)
					{
						odinDrawer.Label = value;
					}
					#endif
				}
			}

			public bool CanBeNull => !(InitParameterGUI.NowDrawing?.NullArgumentGuardActive ?? false);

			#if ODIN_INSPECTOR
			public PropertyTree OdinPropertyTree => InitializerGUI.NowDrawing?.OdinPropertyTree;
			#endif
			
			static State()
			{
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
				ObjectChangeEvents.changesPublished -= OnChangesPublished;
				ObjectChangeEvents.changesPublished += OnChangesPublished;

				static void OnChangesPublished(ref ObjectChangeEventStream stream) => DisposeAllStaticState();
				static void OnPlayModeStateChanged(PlayModeStateChange obj) => DisposeAllStaticState();
			}

			public State(SerializedProperty anyProperty, FieldInfo fieldInfo)
			{
				this.anyProperty = anyProperty;
				referenceProperty = anyProperty.FindPropertyRelative(nameof(Any<object>.reference));
				valueProperty = anyProperty.FindPropertyRelative(nameof(Any<object>.value));

				bool valuePropertyIsNull = valueProperty is null;
			
				anyType = GetAnyTypeFromField(fieldInfo);
				valueType = GetValueTypeFromAnyType(anyType);
				label = InitializerEditorUtility.GetLabel(anyProperty, valueType, fieldInfo);

				objectFieldConstraint = typeof(Object);
				isService = IsService(anyProperty.serializedObject.targetObject, valueType);
				
				equalityComparer = typeof(EqualityComparer<>).MakeGenericType(valueType).GetProperty(nameof(EqualityComparer<object>.Default), BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as IEqualityComparer;

				#if DEV_MODE && DEBUG_ENABLED
				if(valuePropertyIsNull) { Debug.Log($"{anyProperty.propertyPath} value SerializedProperty is null. Probably because Unity can't serialize field of type {valueType} with the SerializeReferenceAttribute."); }
				#endif

				valueHasChildProperties = !valuePropertyIsNull && valueProperty.hasChildren && valueProperty.propertyType != SerializedPropertyType.String;
				canBeUnityObject = CanAssignUnityObjectToField(valueType);
				canBeNonUnityObject = !valuePropertyIsNull && CanAssignNonUnityObjectToField(valueType);

				UpdateValueProviderEditor();

				attributes = InitParameterGUI.NowDrawing?.Attributes ?? Attribute.GetCustomAttributes(fieldInfo);

				if(valueProviderGUI is null)
				{
					InitializerEditorUtility.TryGetAttributeBasedPropertyDrawer(typeof(Object).IsAssignableFrom(valueType) ? referenceProperty : valueProperty, attributes, out propertyDrawerOverride);
				}

				#if ODIN_INSPECTOR
				odinDrawer = GetOdinInspectorProperty(label, anyProperty, valueType, OdinPropertyTree, attributes);
				#endif

				Update(true);

				#if ODIN_INSPECTOR
				[return: MaybeNull]
				static InspectorProperty GetOdinInspectorProperty(GUIContent label, [DisallowNull] SerializedProperty anyProperty, [DisallowNull] Type valueType, PropertyTree odinPropertyTree, [AllowNull] Attribute[] attributes)
				{
					if(attributes is null)
					{
						return null;
					}

					const int None = 0;
					const int Reference = 1;
					const int Value = 2;
					int drawAs = None;

					foreach(var attribute in attributes)
					{
						if(attribute.GetType().Namespace.StartsWith("Sirenix."))
						{
							drawAs = typeof(Object).IsAssignableFrom(valueType) || valueType.IsInterface ? Reference : Value;
							break;
						}
					}

					if(drawAs == None)
					{
						return null;
					}

					string propertyPath;
					if(drawAs == Reference)
					{
						propertyPath = anyProperty.propertyPath + "." + nameof(Any<object>.reference);
					}
					else
					{
						propertyPath = anyProperty.propertyPath + "." + nameof(Any<object>.value);
					}
				
					var result = odinPropertyTree.GetPropertyAtUnityPath(propertyPath);
					if(result is null)
					{
						#if DEV_MODE
						Debug.LogWarning($"Failed to get InspectorProperty from {odinPropertyTree.TargetType.Name} path {propertyPath}.");
						#endif
						return null;
					}

					result.Label = label;
					result.AnimateVisibility = false;
		  			return result;
				}
				#endif
			}

			public void Update(bool forceDeepUpdate = false)
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = updateMarker.Auto();
				#endif

				var nullGuardActive = InitParameterGUI.NowDrawing?.NullArgumentGuardActive ?? false;
				if(nullGuardActive && anyProperty.GetValue() is INullGuard nullGuard)
				{
					CurrentlyDrawnItemNullGuardResult = nullGuard.EvaluateNullGuard(anyProperty.serializedObject.targetObject as Component);
				}
				else
				{
					CurrentlyDrawnItemNullGuardResult = NullGuardResult.Passed;
				}

				// This can happen if Unity can't serialize the field with the SerializeReference attribute.
				var valuePropertyIsNull = valueProperty is null; 
				if(referenceProperty.objectReferenceValue is _Null nullObject && nullObject && !valuePropertyIsNull && valueProperty.propertyType == SerializedPropertyType.ManagedReference)
				{
					valueProperty.SetValue(null);
				}

				bool setIsService = IsService(anyProperty.serializedObject.targetObject, valueType);
				bool shouldRebuildButtons = forceDeepUpdate;

				if(setIsService != isService)
				{
					isService = setIsService;
					shouldRebuildButtons = true;
				}
				
				object managedValue = valuePropertyIsNull || valueProperty.propertyType == SerializedPropertyType.ObjectReference ? null : valueProperty.GetValue();
				bool managedValueIsNull = managedValue is null;
				drawObjectField = GetShouldDrawObjectField(managedValueIsNull);
				drawNullOption = !isService && !valueType.IsValueType && ((canBeUnityObject && canBeNonUnityObject) || TryGetValue(out _, out _) == -1);

				if(shouldRebuildButtons)
				{
					RebuildDefiningTypeAndDiscardButtons(out drawTypeDropdownButton, out drawDiscardButton);
					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"{valueType?.Name}.drawTypeDropdownButton:{drawTypeDropdownButton}, propertyType:{valueProperty?.propertyType.ToString() ?? "n/a"}, drawObjectField:{drawObjectField}");
					#endif
				}

				bool dragging = DragAndDrop.objectReferences.Length > 0;
				if(dragging && InitializerEditorUtility.TryGetAssignableType(DragAndDrop.objectReferences[0], referenceProperty.serializedObject.targetObject, anyType, valueType, out Type assignableType))
				{
					draggedObjectIsAssignable = true;
					objectFieldConstraint = assignableType;
				}
				else
				{
					draggedObjectIsAssignable = dragging ? false : default(bool?);
					var objectReferenceValue = referenceProperty.objectReferenceValue;
					bool hasObjectReferenceValue = objectReferenceValue;
					var objectReferenceValueType = hasObjectReferenceValue ? objectReferenceValue.GetType() : null;

					if(typeof(Object).IsAssignableFrom(valueType) || valueType.IsInterface)
					{
						objectFieldConstraint = !hasObjectReferenceValue || valueType.IsAssignableFrom(objectReferenceValueType) ? valueType : typeof(Object);
					}
					else
					{
						#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
						using var xx = valueProviderMakeGenericTypeMarker.Auto();
						#endif
						var valueProviderType = typeof(IValueProvider<>).MakeGenericType(valueType);
						objectFieldConstraint = !hasObjectReferenceValue || valueProviderType.IsAssignableFrom(objectReferenceValueType) ? valueProviderType : typeof(Object);
					}
				}

				valueLastFrame = TryGetValue(out object value, out Object reference) switch
				{
					0 => value,
					1 => reference,
					_ => null
				};

				bool GetShouldDrawObjectField(bool managedValueIsNull)
				{
					#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
					using var x = getShouldDrawObjectFieldMarker.Auto();
					#endif

					if(referenceProperty.objectReferenceValue)
					{
						return true;
					}

					if(isService)
					{
						return false;
					}

					if(!canBeNonUnityObject)
					{
						return true;
					}

					if(!managedValueIsNull)
					{
						return false;
					}

					if(valuePropertyIsNull || valueProperty.propertyType == SerializedPropertyType.ObjectReference)
					{
						return true;
					}

					return canBeUnityObject;
				}
			}

			/// <returns> <see langword="false"/> if menu has no items. </returns>
			private void RebuildDefiningTypeAndDiscardButtons(out bool drawTypeDropdownButton, out bool drawDiscardButton)
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = rebuildDefiningTypeAndDiscardButtonsMarker.Auto();
				#endif

				LayoutUtility.Repaint();
				var typeOptions = GetTypeOptions();
				var valueProviderOptions = GetAllInitArgMenuItemValueProviderTypes(valueType);
				bool hasMoreThanOneNonValueProviderOption = typeOptions.Skip(1).Any();

				if(valueProviderOptions.Any())
				{
					if(typeOptions.Any())
					{
						typeOptions = typeOptions.Append(typeof(Separator));
					}

					typeOptions = typeOptions.Concat(valueProviderOptions);
				}
				else if(!hasMoreThanOneNonValueProviderOption)
				{
					typeDropdownButton = null;
					drawTypeDropdownButton = false;
					drawDiscardButton = false;
					return;
				}

				var prefixLabel = GUIContent.none;
				var valuePropertyIsNull = valueProperty is null;
				var managedValue = valuePropertyIsNull ? null : valueProperty.GetValue();
				bool managedValueIsNull = managedValue is null;
				var instanceType = !managedValueIsNull ? managedValue.GetType()
									: typeof(Object).IsAssignableFrom(valueType) ? valueType
									: null;

				string buttonText = GetItemContent(instanceType).fullPath;
				int buttonTextLastPartStart = buttonText.LastIndexOf('/');
				if(buttonTextLastPartStart != -1)
				{
					buttonText = buttonText.Substring(buttonTextLastPartStart + 1);
				}

				IEnumerable<Type> selectedTypes = Enumerable.Repeat(instanceType, 1);
				const string menuTitle = "Select Value";

				if(!managedValueIsNull && (valueProperty.isExpanded || valueHasChildProperties || TypeUtility.IsSerializableByUnity(instanceType)))
				{
					typeDropdownButton = new(GUIContent.none, new GUIContent(buttonText), typeOptions, selectedTypes, OnSelectedItemChanged, menuTitle, GetItemContent);
					discardValueButton = null;
					drawTypeDropdownButton = true;
					drawDiscardButton = false;
					return;
				}

				if(!hasMoreThanOneNonValueProviderOption || (managedValueIsNull && drawObjectField))
				{
					typeDropdownButton = new(GUIContent.none, GUIContent.none, typeOptions, selectedTypes, OnSelectedItemChanged, menuTitle, GetItemContent);
					discardValueButton = null;
					drawTypeDropdownButton = true;
					drawDiscardButton = false;
					return;
				}

				if(managedValueIsNull)
				{
					typeDropdownButton = new TypeDropdownButton(prefixLabel, new GUIContent(buttonText), typeOptions, selectedTypes, OnSelectedItemChanged, menuTitle, GetItemContent);
					discardValueButton = null;
					drawTypeDropdownButton = true;
					drawDiscardButton = false;
					return;
				}
				
				drawTypeDropdownButton = false;
				drawDiscardButton = true;
				typeDropdownButton = null;
				discardValueButton = new DiscardButton($"Click to discard value of type {(TypeUtility.ToString(managedValue.GetType()))}.", () => OnSelectedItemChanged(null));

				void OnSelectedItemChanged(Type setType)
				{
					Undo.RecordObjects(anyProperty.serializedObject.targetObjects, "Set Defining Type");
					SetUserSelectedValueType(setType);
				}

				(string fullPath, Texture icon) GetItemContent(Type type)
				{
					#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
					using var x = getItemContentMarker.Auto();
					#endif

					const string noneLabel = "None";

					if(type is null)
					{
						var nullLabel = isService ? ServiceLabel : noneLabel;
						return (nullLabel, null);
					}

					if(type == typeof(_Null))
					{
						var nullLabel = noneLabel;
						return (nullLabel, null);
					}

					if(type == typeof(Object))
					{
						if(type == valueType)
						{
							return ("Object", null);
						}

						return ("Reference", null);
					}

					if(ValueProviderUtility.IsValueProvider(type)
					&& typeof(Object).IsAssignableFrom(type)
					&& type.GetCustomAttribute<ValueProviderMenuAttribute>() is { } attribute)
					{ 
						string itemName = !string.IsNullOrEmpty(attribute.ItemName)
										? attribute.ItemName
										: TypeUtility.ToStringNicified(type);

						return (itemName, null);
					}

					return (TypeUtility.ToStringNicified(type), null);
				}
			}

			private static IEnumerable<Type> GetAllInitArgMenuItemValueProviderTypes(Type valueType)
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = getAllInitArgMenuItemValueProviderTypesMarker.Auto();
				#endif

				return ValueProviderEditorUtility.GetAllValueProviderMenuItemTargetTypes()
													.Where(t => t.GetCustomAttribute<ValueProviderMenuAttribute>() is ValueProviderMenuAttribute attribute
													&& (attribute.IsAny.Length == 0 || Array.Exists(attribute.IsAny, t => t.IsAssignableFrom(valueType)))
													&& (attribute.NotAny.Length == 0 || !Array.Exists(attribute.NotAny, t => t.IsAssignableFrom(valueType)))
													&& MatchesAny(valueType, attribute.WhereAny)
													&& MatchesAll(valueType, attribute.WhereAll)
													&& (attribute.WhereNone == Is.Unconstrained || !MatchesAny(valueType, attribute.WhereNone)))
													.OrderBy(t => t.Name);

				static bool MatchesAny(Type valueType, Is whereAny)
				{
					if(whereAny == Is.Unconstrained)
					{
						return true;
					}

					if((whereAny.HasFlag(Is.Class)			&& valueType.IsClass) ||
						(whereAny.HasFlag(Is.ValueType)		&& valueType.IsValueType) ||
						(whereAny.HasFlag(Is.Concrete)		&& !valueType.IsAbstract) ||
						(whereAny.HasFlag(Is.Abstract)		&& valueType.IsAbstract) ||
						(whereAny.HasFlag(Is.BuiltIn)		&& (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(object))) ||
						(whereAny.HasFlag(Is.Interface)		&& valueType.IsInterface) ||
						(whereAny.HasFlag(Is.Component)		&& Find.typesToComponentTypes.ContainsKey(valueType)) ||
						(whereAny.HasFlag(Is.WrappedObject)	&& Find.typesToWrapperTypes.ContainsKey(valueType)) ||
						(whereAny.HasFlag(Is.SceneObject)	&& Find.typesToFindableTypes.ContainsKey(valueType) && (!typeof(Object).IsAssignableFrom(valueType) || typeof(Component).IsAssignableFrom(valueType) || valueType == typeof(GameObject))) ||
						(whereAny.HasFlag(Is.Asset)			&& Find.typesToFindableTypes.ContainsKey(valueType)) ||
						(whereAny.HasFlag(Is.Service)		&& ServiceUtility.IsServiceDefiningType(valueType)) ||
						(whereAny.HasFlag(Is.Collection)		&& TypeUtility.IsCommonCollectionType(valueType)))
					{
						return true;
					}

					return false;
				}

				static bool MatchesAll(Type valueType, Is whereAll)
				{
					if(whereAll == Is.Unconstrained)
					{
						return true;
					}

					if(whereAll.HasFlag(Is.Collection))
					{
						if(!typeof(IEnumerable).IsAssignableFrom(valueType))
						{
							return false;
						}

						var elementType = TypeUtility.GetCollectionElementType(valueType);
						if(elementType is null)
						{
							#if DEV_MODE
							Debug.LogWarning($"Failed to get collection element type from {valueType}.");
							#endif
							return false;
						}

						valueType = elementType;
					}

					if((!whereAll.HasFlag(Is.Class)			|| valueType.IsClass) &&
						(!whereAll.HasFlag(Is.ValueType)	|| valueType.IsValueType) &&
						(!whereAll.HasFlag(Is.Concrete)		|| !valueType.IsAbstract) &&
						(!whereAll.HasFlag(Is.Abstract)		|| valueType.IsAbstract) &&
						(!whereAll.HasFlag(Is.Interface)	|| valueType.IsInterface) &&
						(!whereAll.HasFlag(Is.Component)	|| Find.typesToComponentTypes.ContainsKey(valueType)) &&
						(!whereAll.HasFlag(Is.WrappedObject)|| Find.typesToWrapperTypes.ContainsKey(valueType)) &&
						(!whereAll.HasFlag(Is.SceneObject)	|| (Find.typesToFindableTypes.ContainsKey(valueType) && (!typeof(Object).IsAssignableFrom(valueType) || typeof(Component).IsAssignableFrom(valueType) || valueType == typeof(GameObject)))) &&
						(!whereAll.HasFlag(Is.Asset)		|| Find.typesToFindableTypes.ContainsKey(valueType)) &&
						(!whereAll.HasFlag(Is.Service)		|| ServiceUtility.IsServiceDefiningType(valueType)))
					{
						return true;
					}

					return false;
				}
			}

			private void SetUserSelectedValueType(Type setType)
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = setUserSelectedValueTypeMarker.Auto();
				#endif

				#if DEV_MODE
				Debug.Assert(setType is null || valueType.IsAssignableFrom(setType) || typeof(Object).IsAssignableFrom(setType));
				#endif
				
				var valuePropertyIsNull = valueProperty is null;
				
				if(setType is null)
				{
					if(!valuePropertyIsNull)
					{
						valueProperty.managedReferenceValue = null;
					}

					referenceProperty.objectReferenceValue = null;
				}
				else if(typeof(Object).IsAssignableFrom(setType))
				{
					if(!valuePropertyIsNull && valueProperty.propertyType == SerializedPropertyType.ManagedReference)
					{
						valueProperty.managedReferenceValue = null;
					}

					if(setType == typeof(Object))
					{
						if(isService)
						{
							var fakeNull = ScriptableObject.CreateInstance<_Null>();
							referenceProperty.objectReferenceValue = fakeNull;
						}
						else if(setType == typeof(Object) && valueType != typeof(Object))
						{
							referenceProperty.objectReferenceValue = null;
						}
					}
					else if(setType.GetCustomAttribute<ValueProviderMenuAttribute>() is not null
							&& typeof(ScriptableObject).IsAssignableFrom(setType))
					{
						// if exactly a single asset of the given type exists in the project,
						// and said asset has no serialized fields, then reuse that instance
						// across all clients (the flyweight pattern).
						bool useSingleSharedInstance = ValueProviderEditorUtility.TryGetSingleSharedInstanceSlow(setType, out var singleSharedInstance);

						#if DEV_MODE
						if(!useSingleSharedInstance) Debug.Log($"Creating new embedded instance of value provider {setType.Name}, because no shared single instance was found in the project.");
						#endif

						referenceProperty.objectReferenceValue = useSingleSharedInstance ? singleSharedInstance : ScriptableObject.CreateInstance(setType);
					}
				}
				else if(!valuePropertyIsNull && valueProperty.propertyType != SerializedPropertyType.ManagedReference)
				{
					if(setType == typeof(int))
					{
						valueProperty.intValue = 0;
					}
					else if(setType == typeof(string))
					{
						valueProperty.stringValue = "";
					}
					else if(setType == typeof(float))
					{
						valueProperty.floatValue = 0f;
					}
					else if(setType == typeof(bool))
					{
						valueProperty.boolValue = false;
					}
					else if(setType == typeof(double))
					{
						valueProperty.doubleValue = 0d;
					}
					else if(setType == typeof(Vector2))
					{
						valueProperty.vector2Value = Vector2.zero;
					}
					else if(setType == typeof(Vector3))
					{
						valueProperty.vector3Value = Vector3.zero;
					}
					else if(setType == typeof(Vector2Int))
					{
						valueProperty.vector2IntValue = Vector2Int.zero;
					}
					#if UNITY_LOCALIZATION
					else if(setType == typeof(LocalizedString))
					{
						var localizedString = ScriptableObject.CreateInstance<LocalizedString>();
						localizedString.value.TableReference = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.DefaultTable;
						referenceProperty.objectReferenceValue = localizedString;
					}
					#endif
				}
				else if(typeof(Type).IsAssignableFrom(setType))
				{
					if(!valuePropertyIsNull)
					{
						valueProperty.managedReferenceValue = new _Type(setType, null);
					}

					referenceProperty.objectReferenceValue = null;
				}
				else
				{
					if(!valuePropertyIsNull)
					{
						valueProperty.managedReferenceValue = InitializerEditorUtility.CreateInstance(setType);
					}

					referenceProperty.objectReferenceValue = null;
				}

				Update(true);
				anyProperty.serializedObject.ApplyModifiedProperties();
				UserSelectedTypeChanged?.Invoke(anyProperty, setType);
				GUI.changed = true;
				InspectorContents.Repaint();
			}

			/// <returns> -1 if no value, 0 if object value, 1 if Object reference.</returns>
			private int TryGetValue(out object value, out Object reference)
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = tryGetValueMarker.Auto();
				#endif

				reference = referenceProperty?.objectReferenceValue;
				if(reference)
				{
					value = null;
					return 1;
				}
			
				value = valueProperty?.GetValue();
				if(value is not null)
				{
					return 0;
				}

				return -1;
			}

			public bool ValueHasChanged() => TryGetValue(out object value, out Object reference) switch
			{
				0 => valueLastFrame is null || !valueType.IsInstanceOfType(valueLastFrame) || !equalityComparer.Equals(value, valueLastFrame),
				1 => valueLastFrame as Object != reference,
				_ => valueLastFrame is not null
			};

			public bool IsValid() => anyProperty.serializedObject.IsValid();

			private IEnumerable<Type> GetTypeOptions()
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = getTypeOptionsMarker.Auto();
				#endif

				if(valueProperty is null || valueProperty.propertyType != SerializedPropertyType.ManagedReference || valueType.IsPrimitive || valueType == typeof(string) || valueType.IsEnum)
				{
					return new[] { valueType };
				}

				IEnumerable<Type> typeOptions
					= TypeCache.GetTypesDerivedFrom(valueType)
					.Where(t => !t.IsAbstract && !typeof(Object).IsAssignableFrom(t) && !t.IsGenericTypeDefinition && t.Name.IndexOf('=') == -1 && t.Name.IndexOf('<') == -1)
					.OrderBy(t => t.Name);

				// TypeCache.GetTypesDerivedFrom apparently doesn't include primitive types, even for typeof(object), typeof(IConvertible) etc.
				// Also, we want these to be at the top, where they are easier to find.
				if(valueType == typeof(object) || valueType.IsInterface)
				{
					if(valueType.IsAssignableFrom(typeof(bool)))
					{
						typeOptions = typeOptions.Prepend(typeof(bool));
					}

					if(valueType.IsAssignableFrom(typeof(int)))
					{
						typeOptions = typeOptions.Prepend(typeof(int));
					}

					if(valueType.IsAssignableFrom(typeof(float)))
					{
						typeOptions = typeOptions.Prepend(typeof(float));
					}

					if(valueType.IsAssignableFrom(typeof(double)))
					{
						typeOptions = typeOptions.Prepend(typeof(double));
					}

					if(valueType.IsAssignableFrom(typeof(string)))
					{
						typeOptions = typeOptions.Prepend(typeof(string));
					}

					typeOptions = typeOptions.Distinct();
				}

				if(!valueType.IsAbstract
					&& !valueType.IsGenericTypeDefinition
					&& !typeof(Object).IsAssignableFrom(valueType)
					// in theory a valid option, but in practice it's highly unlikely anybody
					// will want to inject an instance of it, so I prefer to hide it in the menu,
					// to reduce clutter and avoid potential confusion with UnityEngine.Object.
					&& valueType != typeof(object))
				{
					if(typeOptions.Any())
					{
						typeOptions = typeOptions.Prepend(typeof(Separator));
					}

					typeOptions = typeOptions.Prepend(valueType);
				}

				if(isService || drawNullOption || canBeUnityObject)
				{
					if(typeOptions.Any())
					{
						typeOptions = typeOptions.Prepend(typeof(Separator));
					}

					typeOptions = typeOptions.Prepend(null);
				}

				return typeOptions;
			}

			public void UpdateValueBasedState()
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = updateValueBasedStateMarker.Auto();
				#endif


				UpdateValueProviderEditor();

				if(crossSceneReferenceDrawer is not null)
				{
					crossSceneReferenceDrawer.Dispose();
					crossSceneReferenceDrawer = null;
				}
			}

			private void UpdateValueProviderEditor()
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = updateValueProviderEditorMarker.Auto();
				#endif

				if(valueProviderGUI is not null)
				{
					valueProviderGUI.Dispose();
					valueProviderGUI = null;
				}

				if(referenceProperty.objectReferenceValue is ScriptableObject valueProvider && ShouldDrawEmbedded(valueProvider))
				{
					Editor valueProviderEditor = Editor.CreateEditor(valueProvider, null);
					if(valueProviderEditor)
					{
						valueProviderGUI = new(valueProviderEditor, label, anyProperty, referenceProperty, anyType:anyType, valueType:valueType, DiscardObjectReferenceValue);
					}
				}

				static bool ShouldDrawEmbedded(ScriptableObject valueProvider)
				{
					if(!valueProvider
					|| !ValueProviderUtility.IsValueProvider(valueProvider)
					|| valueProvider.GetType().GetCustomAttribute<ValueProviderMenuAttribute>() is null
					|| valueProvider is _Null)
					{
						return false;
					}

					return !AssetDatabase.Contains(valueProvider) // Draw embedded value providers inlined
						|| AssetDatabase.IsSubAsset(valueProvider) // Draw sub-assets inlined
						|| ValueProviderEditorUtility.IsSingleSharedInstanceSlow(valueProvider); // Draw stateless "singletons" inlined
				}
			}

			private void DiscardObjectReferenceValue()
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = discardObjectReferenceValueMarker.Auto();
				#endif

				if(valueProviderGUI is not null)
				{
					valueProviderGUI.Dispose();
					valueProviderGUI = null;
				}

				if(referenceProperty.objectReferenceValue is ScriptableObject valueProvider
				&& valueProvider
				&& string.IsNullOrEmpty(AssetDatabase.GetAssetPath(valueProvider)))
				{
					Undo.DestroyObjectImmediate(valueProvider);
				}

				referenceProperty.objectReferenceValue = null;
				referenceProperty.serializedObject.ApplyModifiedProperties();

				GUI.changed = true;
			}

			private void HandleOpeningCustomContextMenu(Rect rightClickableRect)
			{
				if(Event.current.type == EventType.MouseDown
				&& Event.current.button == 1
				&& rightClickableRect.Contains(Event.current.mousePosition)
				&& valueProviderGUI is not null)
				{
					OpenCustomContextMenu(rightClickableRect);
				}
			}

			private void OpenCustomContextMenu(Rect rect)
			{
				var menu = new GenericMenu();
			
				if(valueProperty?.GetValue() is { } value && !value.GetType().IsValueType)
				{
					menu.AddItem(new("Set Null"), false, ()=>
					{
						valueProperty.SetValue(null);
						referenceProperty.objectReferenceValue = null;
					});
				}
				else if(referenceProperty.objectReferenceValue)
				{
					menu.AddItem(new("Set Null"), false, ()=>referenceProperty.objectReferenceValue = null);
				}
				else if(isService)
				{
					menu.AddItem(new("Set Null"), false, ()=>referenceProperty.objectReferenceValue = ScriptableObject.CreateInstance<_Null>());
				}

				menu.AddItem(new("Copy Property Path"), false, ()=> GUIUtility.systemCopyBuffer = valueProperty.propertyPath.Substring(valueProperty.propertyPath.LastIndexOf('.')));

				menu.DropDown(rect);
			}

			public void Dispose()
			{
				#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
				using var x = disposeMarker.Auto();
				#endif

				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"AnyPropertyDrawer.State.Dispose() with Event.current.type:{Event.current?.type.ToString() ?? "None"}");
				#endif

				#if ODIN_INSPECTOR
				odinDrawer?.Dispose();
				#endif

				if(crossSceneReferenceDrawer is not null)
				{
					crossSceneReferenceDrawer.Dispose();
					crossSceneReferenceDrawer = null;
				}

				if(valueProviderGUI is not null)
				{
					valueProviderGUI.Dispose();
					valueProviderGUI = null;
				}
			}

			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			private static readonly ProfilerMarker updateMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(Update));
			private static readonly ProfilerMarker valueProviderMakeGenericTypeMarker = new(ProfilerCategory.Gui, "typeof(IValueProvider<>).MakeGenericType");
			private static readonly ProfilerMarker rebuildDefiningTypeAndDiscardButtonsMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(RebuildDefiningTypeAndDiscardButtons));
			private static readonly ProfilerMarker getAllInitArgMenuItemValueProviderTypesMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(GetAllInitArgMenuItemValueProviderTypes));
			private static readonly ProfilerMarker setUserSelectedValueTypeMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(SetUserSelectedValueType));
			private static readonly ProfilerMarker tryGetValueMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(TryGetValue));
			private static readonly ProfilerMarker getTypeOptionsMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(GetTypeOptions));
			private static readonly ProfilerMarker updateValueProviderEditorMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(UpdateValueProviderEditor));
			private static readonly ProfilerMarker discardObjectReferenceValueMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + "." + nameof(DiscardObjectReferenceValue));
			private static readonly ProfilerMarker getShouldDrawObjectFieldMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + ".Update.GetShouldDrawObjectField");
			private static readonly ProfilerMarker getItemContentMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(State) + ".Update.GetItemContent");
			#endif
		}

		public static bool TryGetNullArgumentGuardBasedControlTint(out Color color) => InitializerEditorUtility.TryGetTintForNullGuardResult(CurrentlyDrawnItemNullGuardResult, out color);

		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker updateValueBasedStateMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(UpdateValueBasedState));
		private static readonly ProfilerMarker drawValueFieldMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(DrawValueField));
		private static readonly ProfilerMarker onGUIMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(OnGUI));
		private static readonly ProfilerMarker disposeMarker = new(ProfilerCategory.Gui, nameof(AnyPropertyDrawer) + "." + nameof(Dispose));
		#endif
	}
}