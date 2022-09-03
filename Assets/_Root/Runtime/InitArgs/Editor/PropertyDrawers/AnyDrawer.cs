using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Init;
using Pancake.Init.Internal;
using Pancake.Init.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Editor.Init
{
	/// <summary>
	/// Custom property drawer for <see cref="Any{T}"/>
	/// that allows assigning any value to the property.
	/// </summary>
	[CustomPropertyDrawer(typeof(Any<>), true)]
    public sealed class AnyDrawer : PropertyDrawer
    {
		private class State
        {
			public bool drawObjectField;
			public SerializedProperty referenceProperty;
			public SerializedProperty valueProperty;
			public TypeDropdownButton typeDropdownButton;
			public object valueLastFrame;
		}

		private static readonly GUIContent valueText = new GUIContent("Value");
		private static readonly GUIContent blankLabel = new GUIContent(" ");
		private static readonly GUIContent serviceLabel = new GUIContent("Service", "An Instance of this service will be automatically provided during initialization.");
		private const float minDropdownWidth = 63f;
		private const float objectTextWidth = 63f;
		private const float controlOffset = 3f;

		private Type anyType;
		private Type valueType;
		private Type objectFieldType;
		private IEqualityComparer equalityComparer;

        private bool argumentIsService;
        private bool canBeUnityObject;
		private bool canBeNonUnityObject;

		private float height = EditorGUIUtility.singleLineHeight;
		private readonly Dictionary<Type, bool> isAssignableCache = new Dictionary<Type, bool>();

		private readonly Dictionary<int, State> states = new Dictionary<int, State>();
		private CrossSceneReferenceDrawer crossSceneReferenceDrawer;

		public bool? DraggedObjectIsAssignable => DragAndDrop.objectReferences.Length > 0 && TryGetAssignableTypeFromDraggedObject(DragAndDrop.objectReferences[0], out _);

		private bool ValueHasChanged(State state)
		{
			if(state.valueProperty is null || state.referenceProperty is null)
			{
				state.valueLastFrame = null;
				return false;
			}

			var objectReferenceValue = state.referenceProperty.objectReferenceValue;
			if(objectReferenceValue != null)
			{
				if(objectReferenceValue != state.valueLastFrame as Object)
				{
					state.valueLastFrame = objectReferenceValue;
					return true;
				}

				return false;
			}
			
			if(state.valueProperty.GetValue() is object value)
			{
				if(state.valueLastFrame is null || !valueType.IsInstanceOfType(state.valueLastFrame))
				{
					state.valueLastFrame = value;
					return true;
				}

				if(!equalityComparer.Equals(value, state.valueLastFrame))
				{
					state.valueLastFrame = value;
					return true;
				}

				return false;
			}
			
			if(state.valueLastFrame != null)
			{
				state.valueLastFrame = null;
				return true;
			}

			return false;
		}

		public override void OnGUI(Rect position, SerializedProperty anyProperty, GUIContent label)
        {
			int index = anyProperty.GetArrayElementIndex();
			if(index < 0)
			{
				index = 0;
			}

			if(!states.TryGetValue(index, out State state))
			{
				state = new State();
				Setup(anyProperty, state);
				states.Add(index, state);
			}
			else if(state.referenceProperty == null || (state.valueProperty != null && state.valueProperty.serializedObject != anyProperty.serializedObject) || ValueHasChanged(state))
			{
				Setup(anyProperty, state);
			}

			var referenceProperty = state.referenceProperty;

			bool dragging = DragAndDrop.objectReferences.Length > 0;
			bool? draggedObjectIsAssignable;
			if(dragging && TryGetAssignableTypeFromDraggedObject(DragAndDrop.objectReferences[0], out Type assignableType))
            {
				draggedObjectIsAssignable = true;
				objectFieldType = assignableType;
			}
			else
            {
				draggedObjectIsAssignable = dragging ? false : default(bool?);
				var objectReferenceValue = referenceProperty.objectReferenceValue;
				bool hasObjectReferenceValue = objectReferenceValue != null;
				var objectReferenceValueType = hasObjectReferenceValue ? objectReferenceValue.GetType() : null;

				if(typeof(Object).IsAssignableFrom(valueType) || valueType.IsInterface)
                {
					objectFieldType = !hasObjectReferenceValue || valueType.IsAssignableFrom(objectReferenceValueType) ? valueType : typeof(Object);
				}
				else
                {
					var valueProviderType = typeof(IValueProvider<>).MakeGenericType(valueType);
					objectFieldType = !hasObjectReferenceValue || valueProviderType.IsAssignableFrom(objectReferenceValueType) ? valueProviderType : typeof(Object);
				}
			}

			height = DrawValueField(position, anyProperty, state, label, valueType, objectFieldType, argumentIsService, canBeNonUnityObject, draggedObjectIsAssignable);

			if(GUI.changed)
			{
				anyProperty.serializedObject.ApplyModifiedProperties();
			}
        }

		private void OnDestroy()
		{
			if(crossSceneReferenceDrawer != null)
			{
				crossSceneReferenceDrawer.Dispose();
				crossSceneReferenceDrawer = null;
			}
		}

		internal static bool TryGetAssignableTypeFromDraggedObject(Object draggedObject, Type anyType, Type valueType, Dictionary<Type, bool> isAssignableCache, out Type assignableType)
        {
			if(draggedObject == null)
			{
				assignableType = null;
				return false;
			}

			if(draggedObject is GameObject gameObject && valueType != typeof(GameObject) && valueType != typeof(Object) && valueType != typeof(object))
            {
				foreach(var component in gameObject.GetComponents<Component>())
                {
					if(TryGetAssignableTypeFromDraggedObject(component, anyType, valueType, isAssignableCache, out assignableType))
                    {
						return true;
                    }
                }

				assignableType = null;
				return false;
            }

			var draggedType = draggedObject.GetType();
			if(isAssignableCache.TryGetValue(draggedType, out bool isAssignable))
			{
				assignableType = isAssignable ? draggedObject.GetType() : null;
				return isAssignable;
			}

			foreach(MethodInfo isCreatableFromMethod in anyType.GetMember(nameof(Any<object>.IsCreatableFrom), MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				isAssignable = (bool)isCreatableFromMethod.Invoke(null, new object[] { draggedObject });
				isAssignableCache.Add(draggedType, isAssignable);
				assignableType = isAssignable ? draggedObject.GetType() : null;
				return isAssignable;
			}

			isAssignableCache.Add(draggedType, false);
			assignableType = null;
			return false;
        }

		private bool TryGetAssignableTypeFromDraggedObject(Object draggedObject, out Type assignableType) => TryGetAssignableTypeFromDraggedObject(draggedObject, anyType, valueType, isAssignableCache, out assignableType);

		private void Setup(SerializedProperty anyProperty, State state)
        {
			state.referenceProperty = anyProperty.FindPropertyRelative("reference");
			state.valueProperty = anyProperty.FindPropertyRelative("value");
			bool valuePropertyIsNull = state.valueProperty == null;

			anyType = fieldInfo.FieldType;

			if(anyType.IsArray)
            {
				anyType = anyType.GetElementType();
			}
			else if(anyType is ICollection && anyType.IsGenericType)
            {
				anyType = anyType.GetGenericArguments()[0];
			}

			valueType = anyType.GetGenericArguments()[0];
			objectFieldType = typeof(Object);
			argumentIsService = ServiceUtility.ServiceExists(anyProperty.serializedObject.targetObject, valueType);
			equalityComparer = typeof(EqualityComparer<>).MakeGenericType(valueType).GetProperty(nameof(EqualityComparer<object>.Default), BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as IEqualityComparer;

			if(argumentIsService)
			{
				return;
			}

            canBeUnityObject = CanAssignUnityObjectToField(valueType);
			canBeNonUnityObject = !valuePropertyIsNull && CanAssignNonUnityObjectToField(valueType);

			state.drawObjectField = GetShouldDrawObjectField(state.referenceProperty);

			RebuildDefiningTypeButton(anyProperty, state, valuePropertyIsNull);
		}

		private void RebuildDefiningTypeButton(SerializedProperty property, State state, bool valuePropertyIsNull)
		{
			var value = valuePropertyIsNull ? null : state.valueProperty.GetValue();

			var typeOptions = GetTypeOptions(state.valueProperty, valueType);
			var prefixLabel = GUIContent.none;
			var instanceType = value != null ? value.GetType() : state.drawObjectField ? typeof(Object) : null;
			var buttonLabel = new GUIContent(ObjectNames.NicifyVariableName(TypeUtility.ToString(instanceType)));
			bool useGroups = ShouldUseGroups(valueType, typeOptions);
			IEnumerable<Type> selectedTypes = Enumerable.Repeat(instanceType, 1);
			state.typeDropdownButton = new TypeDropdownButton(prefixLabel, buttonLabel, typeOptions, selectedTypes, OnSelectedItemChanged, false, useGroups, "Select Type");

			void OnSelectedItemChanged(Type setType)
			{
				Undo.RecordObjects(property.serializedObject.targetObjects, "Set Defining Type");

				SetUserSelectedValueType(state, setType);

				property.serializedObject.ApplyModifiedProperties();
				RebuildDefiningTypeButton(property, state, valuePropertyIsNull);
				GUI.changed = true;
			}
		}

		private static bool ShouldUseGroups(Type valueType, IEnumerable<Type> typeOptions)
        {
			if(valueType == typeof(object) || valueType == typeof(Object))
            {
				return true;
            }

			var enumerator = typeOptions.GetEnumerator();
			int max = 20;
			int counter;
			for(counter = 0; counter < max; counter++)
            {
				if(!enumerator.MoveNext())
                {
					break;
                }
            }

			return counter >= max;
        }

		private IEnumerable<Type> GetTypeOptions(SerializedProperty valueProperty, Type valueType)
		{
			if(valueProperty == null || valueProperty.propertyType != SerializedPropertyType.ManagedReference)
			{
				return canBeUnityObject ? (new Type[] { valueType, typeof(Object) }) : (new Type[] { valueType });
			}

			IEnumerable<Type> typeOptions = TypeCache.GetTypesDerivedFrom(valueType).Where(t => !t.IsAbstract && !typeof(Object).IsAssignableFrom(t) && !t.IsGenericTypeDefinition && t.Name.IndexOf('=') == -1 && t.Name.IndexOf('<') == -1);

			typeOptions = typeOptions.OrderBy(t => t.Name);

			if(!valueType.IsAbstract && !typeof(Object).IsAssignableFrom(valueType) && !valueType.IsGenericTypeDefinition)
			{
				typeOptions = typeOptions.Prepend(valueType);
			}

			// TypeCache.GetTypesDerivedFrom only returns results within "all classes loaded in Unity domain assemblies"
			// which apparently doesn't include primitive types.
			if(valueType == typeof(object) || valueType == typeof(IConvertible) || valueType == typeof(IComparable))
			{
				typeOptions = typeOptions.Prepend(typeof(_Boolean));
				typeOptions = typeOptions.Prepend(typeof(_Integer));
				typeOptions = typeOptions.Prepend(typeof(_Float));
				typeOptions = typeOptions.Prepend(typeof(_Double));
				typeOptions = typeOptions.Prepend(typeof(_String));
			}
			else if(valueType.IsAssignableFrom(typeof(bool)))
			{
				typeOptions = typeOptions.Prepend(typeof(_Boolean));
			}
			else if(valueType.IsAssignableFrom(typeof(int)))
			{
				typeOptions = typeOptions.Prepend(typeof(_Integer));
			}
			else if(valueType.IsAssignableFrom(typeof(float)))
			{
				typeOptions = typeOptions.Prepend(typeof(_Float));
			}
			else if(valueType.IsAssignableFrom(typeof(double)))
			{
				typeOptions = typeOptions.Prepend(typeof(_Double));
			}
			else if(valueType.IsAssignableFrom(typeof(string)))
			{
				typeOptions = typeOptions.Prepend(typeof(_String));
			}

			typeOptions = typeOptions.Distinct();

			if(canBeUnityObject)
			{
				typeOptions = typeOptions.Prepend(typeof(Object));
			}

			typeOptions = typeOptions.Prepend(null);

			return typeOptions;
        }

		private bool GetShouldDrawObjectField(SerializedProperty referenceProperty) => canBeUnityObject && (!canBeNonUnityObject || referenceProperty.objectReferenceValue != null);

		private float DrawValueField(Rect position, SerializedProperty anyProperty, State state, GUIContent label, Type valueType, Type objectFieldType,
			bool isService, bool canBeNonUnityObject, bool? draggedObjectIsAssignable)
        {
			int indentLevelWas = EditorGUI.indentLevel;
			position.height = EditorGUIUtility.singleLineHeight;
			bool draggingAssignableObject = draggedObjectIsAssignable.HasValue && draggedObjectIsAssignable.Value;
			var referenceProperty = state.referenceProperty;
			var valueProperty = state.valueProperty;
			object managedValue = valueProperty == null || valueProperty.propertyType != SerializedPropertyType.ManagedReference ? null : valueProperty.GetValue();
			bool managedValueIsNull = managedValue is null;
			bool objectReferenceValueIsNull = referenceProperty.objectReferenceValue == null;
			var targetObject = anyProperty.serializedObject.targetObject;
			bool drawAsObjectField = !objectReferenceValueIsNull || (draggingAssignableObject && managedValueIsNull) || (!isService && state.drawObjectField);

			if(drawAsObjectField)
            {
				var controlRect = EditorGUI.PrefixLabel(position, label);
				bool drawTypeDropdown = canBeNonUnityObject && objectReferenceValueIsNull && (!draggingAssignableObject || !managedValueIsNull);

				if(drawTypeDropdown)
                {
					float totalWidth = controlRect.width;
					controlRect.width = objectTextWidth;
					DrawTypeDropdown(controlRect, state, true);
					controlRect.x += objectTextWidth + controlOffset;
					controlRect.width = totalWidth - objectTextWidth - controlOffset;
				}

				var referenceValue = referenceProperty.objectReferenceValue;
				if(referenceValue != null)
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

							if(crossSceneReferenceDrawer is null)
							{
								crossSceneReferenceDrawer = new CrossSceneReferenceDrawer(objectFieldType);
							}

							crossSceneReferenceDrawer.OnGUI(controlRect, referenceProperty, GUIContent.none);

							EditorGUI.indentLevel = indentLevelWas;

							anyProperty.serializedObject.ApplyModifiedProperties();
							return EditorGUI.GetPropertyHeight(valueProperty, label, true);
						}
					}
					else if(GetScene(referenceValue) is Scene referenceScene && referenceScene.IsValid() && referenceScene != GetScene(targetObject))
					{
						#if DEV_MODE
						Debug.Log($"Cross scene reference detected. {referenceValue.name} scene != {GetScene(targetObject)}");
						#endif

						var referenceGameObject = GetGameObject(referenceValue);
						bool isCrossSceneReferenceable = false;
						foreach(var refTag in referenceGameObject.GetComponents<RefTag>()) // Could reuse a list to avoid allocating so much
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

						referenceProperty.objectReferenceValue = Create.Instance<CrossSceneReference, GameObject, Object>(GetGameObject(targetObject), referenceValue);
					}
				}

				bool preventCrossSceneReferencesWas = EditorSceneManager.preventCrossSceneReferences;
				EditorSceneManager.preventCrossSceneReferences = false;

				EditorGUI.indentLevel = 0;
				EditorGUI.ObjectField(controlRect, referenceProperty, objectFieldType, GUIContent.none);
				EditorGUI.indentLevel = indentLevelWas;

				EditorSceneManager.preventCrossSceneReferences = preventCrossSceneReferencesWas;

				if(controlRect.Contains(Event.current.mousePosition))
				{
					DragAndDrop.visualMode = draggedObjectIsAssignable.HasValue && !draggedObjectIsAssignable.Value ? DragAndDropVisualMode.Rejected : DragAndDropVisualMode.Generic;
				}
				else if(draggedObjectIsAssignable.HasValue)
				{
					var tintColor = draggedObjectIsAssignable.Value ? new Color(0f, 1f, 0f, 0.05f) : new Color(1f, 0f, 0f, 0.05f);
					EditorGUI.DrawRect(controlRect, tintColor);
				}

				return EditorGUIUtility.singleLineHeight;
            }

			if(isService && managedValueIsNull)
            {
                var controlRect = EditorGUI.PrefixLabel(position, blankLabel);
				controlRect.width = Styles.ServiceTag.CalcSize(serviceLabel).x;
				GUI.Label(controlRect, serviceLabel, Styles.ServiceTag);

				ServiceTagUtility.Draw(controlRect, () => ServiceTagUtility.Ping(valueType, targetObject as Component));

                position.width -= controlRect.width;
				EditorGUI.indentLevel = 0;
				GUI.Label(position, label);
				EditorGUI.indentLevel = indentLevelWas;

				return EditorGUIUtility.singleLineHeight;
            }

            var remainingRect = EditorGUI.PrefixLabel(position, label);

			if(valueProperty == null)
            {
				return EditorGUIUtility.singleLineHeight;
			}

			if(valueProperty.propertyType != SerializedPropertyType.ManagedReference)
            {
				if(!canBeNonUnityObject)
				{
					EditorGUI.indentLevel = 0;
					EditorGUI.PropertyField(remainingRect, valueProperty, GUIContent.none, true);
					EditorGUI.indentLevel = indentLevelWas;
				}
				else
				{
					EditorGUI.indentLevel = 0;
					remainingRect = DrawTypeDropdown(remainingRect, state, false);
					EditorGUI.PropertyField(remainingRect, valueProperty, GUIContent.none, true);
					EditorGUI.indentLevel = indentLevelWas;
				}

				return EditorGUI.GetPropertyHeight(valueProperty, label, true);
            }

			if(valueType == typeof(object))
            {
				switch(valueProperty.type)
                {
					case "managedReference<Int32>":
						SetManagedValue(valueProperty, new _Integer() { value = (int)managedValue }, "Set Int Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Integer>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var intWas = ((_Integer)managedValue).value;
						var setInt = EditorGUI.IntField(remainingRect, GUIContent.none, intWas);
                        if(intWas != setInt)
                        {
                            SetManagedValue(valueProperty, new _Integer() { value = setInt }, "Set Int Value");
                        }
                        EditorGUI.EndProperty();
                        valueProperty.serializedObject.ApplyModifiedProperties();
                        return EditorGUIUtility.singleLineHeight;
					case "managedReference<Type>":
						SetManagedValue(valueProperty, new _Type((Type)managedValue, null), "Set Type Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
                    case "managedReference<Boolean>":
						SetManagedValue(valueProperty, new _Boolean() { value = (bool)managedValue }, "Set Boolean Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Boolean>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
                        var boolWas = ((_Boolean)managedValue).value;
						var setBool = EditorGUI.Toggle(remainingRect, GUIContent.none, boolWas);
                        if(boolWas != setBool)
                        {
                            SetManagedValue(valueProperty, new _Boolean() { value = setBool }, "Set Boolean Value");
                        }
                        EditorGUI.EndProperty();
                        valueProperty.serializedObject.ApplyModifiedProperties();
                        return EditorGUIUtility.singleLineHeight;
					case "managedReference<Single>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						SetManagedValue(valueProperty, new _Float() { value = (float)managedValue }, "Set Float Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Float>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
                        var floatWas = ((_Float)managedValue).value;
                        var setFloat = EditorGUI.FloatField(remainingRect, GUIContent.none, floatWas);
                        if(floatWas != setFloat)
                        {
                            SetManagedValue(valueProperty, new _Float() { value = setFloat }, "Set Float Value");
                        }
                        EditorGUI.EndProperty();
                        return EditorGUIUtility.singleLineHeight;
					case "managedReference<Double>":
						SetManagedValue(valueProperty, new _Double() { value = (double)managedValue }, "Set Double Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_Double>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var doubleWas = ((_Double)managedValue).value;
						var setDouble = EditorGUI.DoubleField(remainingRect, GUIContent.none, doubleWas);
						if(doubleWas != setDouble)
						{
							SetManagedValue(valueProperty, new _Double() { value = setDouble }, "Set Double Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<String>":
						SetManagedValue(valueProperty, new _String() { value = (string)managedValue }, "Set String Value");
						valueProperty.serializedObject.ApplyModifiedProperties();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<_String>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var stringWas = ((_String)managedValue).value;
						var setString = EditorGUI.TextField(remainingRect, GUIContent.none, stringWas);
						if(stringWas != setString)
						{
							SetManagedValue(valueProperty, new _String() { value = setString }, "Set String Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
                    case "managedReference<Color>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var colorWas = (Color)managedValue;
						var setColor = EditorGUI.ColorField(remainingRect, GUIContent.none, colorWas);
						if(colorWas != setColor)
						{
							SetManagedValue(valueProperty, setColor, "Set Color Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
                    case "managedReference<Vector2>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector2Was = (Vector2)managedValue;
						var setVector2 = EditorGUI.Vector2Field(remainingRect, GUIContent.none, vector2Was);
						if(vector2Was != setVector2)
						{
							SetManagedValue(valueProperty, setVector2, "Set Vector2 Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector3>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector3Was = (Vector3)managedValue;
						var setVector3 = EditorGUI.Vector3Field(remainingRect, GUIContent.none, vector3Was);
						if(vector3Was != setVector3)
						{
							SetManagedValue(valueProperty, setVector3, "Set Vector3 Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector4>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector4Was = (Vector4)managedValue;
						var setVector4 = EditorGUI.Vector4Field(remainingRect, GUIContent.none, vector4Was);
						if(vector4Was != setVector4)
						{
							SetManagedValue(valueProperty, setVector4, "Set Vector4 Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector2Int>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector2IntWas = (Vector2Int)managedValue;
						var setVector2Int = EditorGUI.Vector2IntField(remainingRect, GUIContent.none, vector2IntWas);
						if(vector2IntWas != setVector2Int)
						{
							SetManagedValue(valueProperty, setVector2Int, "Set Vector2Int Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
					case "managedReference<Vector3Int>":
						remainingRect = DrawTypeDropdown(remainingRect, state, false);
						EditorGUI.BeginProperty(remainingRect, label, valueProperty);
						var vector3IntWas = (Vector3Int)managedValue;
						var setVector3Int = EditorGUI.Vector3IntField(remainingRect, GUIContent.none, vector3IntWas);
						if(vector3IntWas != setVector3Int)
						{
							SetManagedValue(valueProperty, setVector3Int, "Set Vector3Int Value");
						}
						EditorGUI.EndProperty();
						return EditorGUIUtility.singleLineHeight;
                }
            }
			
			EditorGUI.indentLevel = 0;
			DrawTypeDropdown(remainingRect, state, true);
			EditorGUI.indentLevel = indentLevelWas;

			if(managedValueIsNull)
            {
				return EditorGUIUtility.singleLineHeight;
            }

            var assignedInstanceType = managedValue.GetType();
            if(assignedInstanceType is null)
            {
				return EditorGUIUtility.singleLineHeight;
            }

            if(!TypeUtility.IsSerializableByUnity(assignedInstanceType))
            {
				var boxPosition = position;
                boxPosition.y += position.height;
                EditorGUI.HelpBox(boxPosition, assignedInstanceType.Name + " is missing the [Serializable] attribute.", MessageType.Info);
                return EditorGUIUtility.singleLineHeight * 2f;
            }

            EditorGUI.indentLevel++;
            
			var valuePosition = position;
            valuePosition.y += position.height;
            EditorGUI.PropertyField(valuePosition, valueProperty, valueText, true);
			
			EditorGUI.indentLevel--;

			return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(valueProperty, valueText, true);
        }

        private static void SetManagedValue<T>(SerializedProperty valueProperty, T setValue, string undoText)
        {
			var targets = valueProperty.serializedObject.targetObjects;

			Undo.RecordObjects(targets, undoText);

			valueProperty.managedReferenceValue = setValue;

			foreach(var target in targets)
            {
                EditorUtility.SetDirty(target);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
			return height;
        }

        private static bool CanAssignUnityObjectToField(Type type)
		{
			if(!type.IsInterface)
			{
				// Always true because any Object could have a TypeConverter supporting the type.
				return true;
			}

			foreach(var derivedType in TypeCache.GetTypesDerivedFrom(type))
			{
				if(typeof(Object).IsAssignableFrom(derivedType) && !derivedType.IsAbstract)
				{
					return true;
				}
			}

			return false;
		}

		private static bool CanAssignNonUnityObjectToField(Type type)
        {
			if(typeof(Object).IsAssignableFrom(type))
			{
				return false;
			}

			if(!type.IsInterface)
			{
				return true;
			}

			foreach(var derivedType in TypeCache.GetTypesDerivedFrom(type))
			{
				if(!typeof(Object).IsAssignableFrom(derivedType) && !derivedType.IsAbstract)
				{
					return true;
				}
			}

			return false;
		}

		private static Rect DrawTypeDropdown(Rect rect, State state, bool fullWidth)
        {
			float totalWidth = rect.width;
			float width = rect.width;
			if(!fullWidth)
            {
				GUIContent buttonLabel = state.typeDropdownButton.buttonLabel;
				width = EditorStyles.popup.CalcSize(buttonLabel).x;
				if(width < minDropdownWidth)
				{
					width = minDropdownWidth;
				}
				rect.width = width;
			}
			
			bool showMixedValueWas = EditorGUI.showMixedValue;
			if(state.valueProperty != null && state.valueProperty.hasMultipleDifferentValues)
			{
				EditorGUI.showMixedValue = true;
			}

			// If GUI is tinted red by null argument guard, but value selected in the dropdown is not Null,
			// then we don't tint the type dropdown value red, as that would be misleading, but only the value that follows it.
			if(GUI.color == Color.red && !string.Equals(state.typeDropdownButton.buttonLabel.text, "Null"))
			{
				GUI.color = Color.white;
				state.typeDropdownButton.Draw(rect);
				GUI.color = Color.red;
			}
			else
			{
				state.typeDropdownButton.Draw(rect);
			}

			EditorGUI.showMixedValue = showMixedValueWas;

			var remainingRect = rect;
			remainingRect.x += width + controlOffset;
			remainingRect.width = totalWidth - width - controlOffset;

			return remainingRect;
        }

		private static void SetUserSelectedValueType(State state, Type type)
        {
			SerializedProperty valueProperty = state.valueProperty;

			if(valueProperty.propertyType != SerializedPropertyType.ManagedReference)
			{
				state.drawObjectField = type != null && typeof(Object).IsAssignableFrom(type);

				if(type == typeof(int))
				{
					valueProperty.intValue = 0;
				}
				else if(type == typeof(string))
				{
					valueProperty.stringValue = "";
				}
				else if(type == typeof(float))
				{
					valueProperty.floatValue = 0f;
				}
				else if(type == typeof(bool))
				{
					valueProperty.boolValue = false;
				}
				else if(type == typeof(double))
				{
					valueProperty.doubleValue = 0d;
				}
				else if(type == typeof(Vector2))
				{
					valueProperty.vector2Value = Vector2.zero;
				}
				else if(type == typeof(Vector3))
				{
					valueProperty.vector3Value = Vector3.zero;
				}
				else if(type == typeof(Vector2Int))
				{
					valueProperty.vector2IntValue = Vector2Int.zero;
				}
				else if(type is null)
				{
					state.referenceProperty.objectReferenceValue = null;
				}
			}
			else if(type is null)
            {
				valueProperty.managedReferenceValue = null;
				state.drawObjectField = false;
				state.referenceProperty.objectReferenceValue = null;
			}
			else if(typeof(Object).IsAssignableFrom(type))
            {
				state.drawObjectField = true;
				valueProperty.managedReferenceValue = null;
			}
			else if(typeof(Type).IsAssignableFrom(type))
            {
				state.drawObjectField = false;
				valueProperty.managedReferenceValue = new _Type(type, null);
			}
			else
            {
				state.drawObjectField = false;
				valueProperty.managedReferenceValue = InitializerEditorUtility.CreateInstance(type);
			}

			valueProperty.serializedObject.ApplyModifiedProperties();
		}

		private static Scene GetScene(Object target) => target is Component component && component != null ? component.gameObject.scene : target is GameObject gameObject && gameObject != null ? gameObject.scene : default;
		private static GameObject GetGameObject(Object target) => target is Component component && component != null ? component.gameObject : target as GameObject;
    }
}