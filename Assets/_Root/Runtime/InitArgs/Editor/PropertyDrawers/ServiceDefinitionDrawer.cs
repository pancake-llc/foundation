using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pancake.Init.Internal;
using Pancake.Init.Serialization;
using UnityEditor;
using UnityEngine;
using static Pancake.NullExtensions;

namespace Pancake.Editor.Init
{
	/// <summary>
	/// Custom property drawer for <see cref="Any{T}"/>
	/// that allows assigning any value to the property.
	/// </summary>
	[CustomPropertyDrawer(typeof(ServiceDefinition), true)]
	internal class ServiceDefinitionDrawer : PropertyDrawer
	{
		private class State
        {
			public SerializedProperty service;
			public SerializedProperty definingType;
			public TypeDropdownButton definingTypeButton;
		}

		private const float asLabelWidth = 25f;

		private static readonly string definingTypeTooltip = "The defining type for the service, which can be used to retrieve the service.\n\nThis must be an interface that {0} implements, a base type that the it derives from, or its exact type.";
		private static readonly GUIContent asLabel = new GUIContent(" as ");
		private static readonly GUIContent noneLabel = new GUIContent("None");
		private readonly Dictionary<int, State> states = new Dictionary<int, State>();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			int index = property.GetArrayElementIndex();
			if(index < 0)
            {
				index = 0;
			}

			if(!states.TryGetValue(index, out State state))
			{
				state = new State();
				Setup(property, state);
				states.Add(index, state);
			}
			else if(state.service == null || state.service.serializedObject != property.serializedObject)
			{
				Setup(property, state);
			}

			var serviceRect = position;
			bool hasValue = state.service.objectReferenceValue != null;
			float controlWidth = hasValue ? (position.width - asLabelWidth) * 0.5f : position.width;
			serviceRect.width = controlWidth;

			EditorGUI.BeginChangeCheck();

			if(!hasValue)
            {
				GUI.color = Color.red;
			}

			EditorGUI.PropertyField(serviceRect, state.service, GUIContent.none);

			GUI.color = Color.white;

			if(EditorGUI.EndChangeCheck())
			{
				property.serializedObject.ApplyModifiedProperties();

				if(state.service.objectReferenceValue is GameObject gameObject)
				{
					var components = gameObject.GetComponents<Component>();

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("GameObject"), false, () =>
					{
						Undo.RecordObjects(state.service.serializedObject.targetObjects, "Set Service");
						state.service.objectReferenceValue = gameObject;
						state.service.serializedObject.ApplyModifiedProperties();
						states.Remove(index);
					});

					HashSet<string> addedItems = new HashSet<string>() { "GameObject" };

                    for(int i = 0, count = components.Length; i < count; i++)
                    {
						var component = components[i];
						if(component == null)
                        {
							continue;
                        }

						string name = ObjectNames.NicifyVariableName(component.GetType().Name);
						if(!addedItems.Add(name))
                        {
							int nth = 2;
							string uniqueName;
							do
							{
								uniqueName = name + " (" + nth + ")";
								nth++;
							}
							while(!addedItems.Add(uniqueName));
							name = uniqueName;
						}

						menu.AddItem(new GUIContent(name), false, () =>
						{
							Undo.RecordObjects(state.service.serializedObject.targetObjects, "Set Service");
							state.service.objectReferenceValue = component;
							state.service.serializedObject.ApplyModifiedProperties();
							states.Remove(index);
						});
					}

					var dropdownRect = GUILayoutUtility.GetLastRect();
					dropdownRect.x += 100f;
					dropdownRect.y += 105f;
					menu.DropDown(dropdownRect);
				}
				else
                {
					states.Remove(index);
				}
			}

			if(!hasValue)
            {
				return;
            }

			var asLabelRect = serviceRect;
			asLabelRect.x += serviceRect.width;
			asLabelRect.width = asLabelWidth;
			GUI.Label(asLabelRect, asLabel);

			var popupRect = asLabelRect;
			popupRect.x += asLabelWidth;
			popupRect.width = controlWidth;

			bool showMixedValueWas = EditorGUI.showMixedValue;
			if(state.definingType.hasMultipleDifferentValues)
			{
				EditorGUI.showMixedValue = true;
			}

			state.definingTypeButton.Draw(popupRect);

			EditorGUI.showMixedValue = showMixedValueWas;
		}

		private void Setup(SerializedProperty property, State state)
        {
            state.service = property.FindPropertyRelative(nameof(state.service));
            state.definingType = property.FindPropertyRelative(nameof(state.definingType));

			RebuildDefiningTypeButton(property, state);
		}

		private void RebuildDefiningTypeButton(SerializedProperty property, State state)
		{
			var serviceValue = state.service.objectReferenceValue;
			var typeOptions = GetTypeOptions(serviceValue);
			Type definingType = (state.definingType.GetValue() as _Type)?.Value;
			var definingTypeLabel = new GUIContent(TypeUtility.ToString(definingType), string.Format(definingTypeTooltip, serviceValue is null ? "the service class" : TypeUtility.ToString(serviceValue.GetType())));
			state.definingTypeButton = new TypeDropdownButton(GUIContent.none, definingTypeLabel, typeOptions, Enumerable.Repeat(definingType, 1), (setType) =>
			{
				Undo.RecordObjects(property.serializedObject.targetObjects, "Set Defining Type");
				state.definingType.SetValue(new _Type(setType));
				property.serializedObject.ApplyModifiedProperties();
				RebuildDefiningTypeButton(property, state);
				GUI.changed = true;
			}, true, false, "Service Defining Type");
		}

		private static IEnumerable<Type> GetTypeOptions(object serviceValue)
        {
			if(serviceValue is null)
            {
				return new Type[] { null };
			}

			var typeOptions = Enumerable.Empty<Type>();
			ConcatTypeOptions(ref typeOptions, serviceValue);
			var distinct = typeOptions.Distinct();

			if(CountExceeds(distinct, 30))
            {
				distinct = distinct.OrderBy(t => t.Name);
			}

			var withNull = distinct.Prepend(null);
			return withNull;
		}

		private static void ConcatTypeOptions(ref IEnumerable<Type> typeOptions, object serviceValue)
        {
            if(serviceValue is null)
            {
                return;
            }

			var serviceType = serviceValue.GetType();
			if(!(serviceValue is IValueProvider valueProvider))
            {
				ConcatAllTypeOptions(ref typeOptions, serviceType);
				return;
            }

			object wrappedValue = valueProvider.Value;
			if(wrappedValue != Null)
			{
				var wrappedType = wrappedValue.GetType();
				ConcatTypeOptions(ref typeOptions, serviceType, wrappedType);
				return;
			}

			foreach(var interfaceType in serviceType.GetInterfaces())
			{
				if(!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != typeof(IValueProvider<>))
				{
					continue;
				}

				var wrappedType = interfaceType.GetGenericArguments()[0];
				if(!TypeUtility.IsNullOrBaseType(wrappedType))
				{
					ConcatTypeOptions(ref typeOptions, serviceType, wrappedType);
					return;
				}
			}

			ConcatAllTypeOptions(ref typeOptions, serviceType);
		}

		private static void ConcatTypeOptions(ref IEnumerable<Type> typeOptions, [NotNull] Type serviceType, [CanBeNull] Type wrappedType)
        {
			if(TypeUtility.IsNullOrBaseType(wrappedType))
			{
				ConcatAllTypeOptions(ref typeOptions, serviceType);
				return;
			}

			if(wrappedType.IsInterface)
			{
				ConcatTypeAndBaseTypes(ref typeOptions, serviceType);
				ConcatDerivedTypes(ref typeOptions, serviceType);
				ConcatTypeAndBaseTypes(ref typeOptions, wrappedType);
				ConcatInterfaces(ref typeOptions, serviceType);
				return;
			}
			
			typeOptions.Append(wrappedType);
			typeOptions.Append(serviceType);
			ConcatBaseTypes(ref typeOptions, wrappedType);
			ConcatBaseTypes(ref typeOptions, serviceType);
			ConcatDerivedTypes(ref typeOptions, wrappedType);
			ConcatDerivedTypes(ref typeOptions, serviceType);
			ConcatInterfaces(ref typeOptions, wrappedType);
			ConcatInterfaces(ref typeOptions, serviceType);
		}

		private static void ConcatAllTypeOptions(ref IEnumerable<Type> typeOptions, Type serviceType)
        {
			ConcatTypeAndBaseTypes(ref typeOptions, serviceType);
			ConcatInterfaces(ref typeOptions, serviceType);
		}

		private static void ConcatTypeAndBaseTypes(ref IEnumerable<Type> typeOptions, Type type)
		{
			for(var t = type; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				typeOptions = typeOptions.Append(t);
			}
		}

		private static void ConcatBaseTypes(ref IEnumerable<Type> typeOptions, Type type)
		{
			for(var t = type.BaseType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				typeOptions = typeOptions.Append(t);
			}
		}

		private static void ConcatDerivedTypes(ref IEnumerable<Type> typeOptions, Type type)
        {
			typeOptions = typeOptions.Concat(TypeCache.GetTypesDerivedFrom(type));
		}

		private static void ConcatInterfaces(ref IEnumerable<Type> typeOptions, Type type)
		{
			typeOptions = typeOptions.Concat(type.GetInterfaces().Where(ShouldNotIgnoreInterface));
		}

		private static bool ShouldNotIgnoreInterface(Type type) => !ShouldIgnoreInterface(type);

		private static bool ShouldIgnoreInterface(Type type) => type == typeof(ISerializationCallbackReceiver)
			|| type == typeof(IOneArgument) || type == typeof(ITwoArguments) || type == typeof(IThreeArguments) || type == typeof(IFourArguments) || type == typeof(IFiveArguments)
			|| (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() is Type typeDefinition &&
			(typeDefinition == typeof(IFirstArgument<>) || typeDefinition == typeof(ISecondArgument<>) || typeDefinition == typeof(IThirdArgument<>) || typeDefinition == typeof(IFourthArgument<>) || typeDefinition == typeof(IFifthArgument<>)));

		private static bool CountExceeds(IEnumerable<Type> typeOptions, int count)
        {
			int counter = 0;
			using(var enumerator = typeOptions.GetEnumerator())
			{
				while(enumerator.MoveNext())
				{
					counter++;
					if(counter > count)
					{
						return true;
					}
				}
			}

			return false;
        }

		private static GUIContent GetLabel(Type type)
        {
			if(type is null)
            {
				return noneLabel;
            }

			string text = TypeUtility.ToString(type, '\0');

			string tooltip = TypeUtility.ToString(type, '.');

			return new GUIContent(text, tooltip);
        }
    }
}