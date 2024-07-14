using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Internal;
using Sisus.Init.Serialization;
using UnityEditor;
using UnityEngine;
using static Sisus.NullExtensions;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Custom property drawer for <see cref="Any{T}"/>
	/// that allows assigning any value to the property.
	/// </summary>
	[CustomPropertyDrawer(typeof(ServiceDefinition), true)]
	internal class ServiceDefinitionDrawer : PropertyDrawer
	{
		private sealed class State
        {
			public SerializedProperty serviceProperty;
			public SerializedProperty definingTypeProperty;
			public TypeDropdownButton definingTypeButton;
		}

		private const float asLabelWidth = 25f;

		private static readonly string definingTypeTooltip = "The defining type for the service, which can be used to retrieve the service.\n\nThis must be an interface that {0} implements, a base type that the it derives from, or its exact type.";
		private static readonly GUIContent asLabel = new(" as ");
		private readonly Dictionary<string, State> states = new();

		public override void OnGUI(Rect position, SerializedProperty serviceDefinitionProperty, GUIContent label)
		{
			if(!states.TryGetValue(serviceDefinitionProperty.propertyPath, out State state))
			{
				state = new State();
				Setup(serviceDefinitionProperty, state);
				states.Add(serviceDefinitionProperty.propertyPath, state);
			}
			else if(state.serviceProperty == null || state.serviceProperty.serializedObject != serviceDefinitionProperty.serializedObject)
			{
				Setup(serviceDefinitionProperty, state);
			}

			var serviceRect = position;
			bool hasValue = state.serviceProperty.objectReferenceValue != null;
			float controlWidth = hasValue ? (position.width - asLabelWidth) * 0.5f : position.width;
			serviceRect.width = controlWidth;

			EditorGUI.BeginChangeCheck();

			if(!hasValue)
            {
				GUI.color = Color.red;
			}

			EditorGUI.PropertyField(serviceRect, state.serviceProperty, GUIContent.none);

			GUI.color = Color.white;

			if(EditorGUI.EndChangeCheck())
			{
				serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();

				if(state.serviceProperty.objectReferenceValue is GameObject gameObject)
				{
					var components = gameObject.GetComponents<Component>();

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("GameObject"), false, () =>
					{
						Undo.RecordObjects(state.serviceProperty.serializedObject.targetObjects, "Set Service");
						state.serviceProperty.objectReferenceValue = gameObject;
						state.serviceProperty.serializedObject.ApplyModifiedProperties();
						states.Remove(serviceDefinitionProperty.propertyPath);
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
							Undo.RecordObjects(state.serviceProperty.serializedObject.targetObjects, "Set Service");
							state.serviceProperty.objectReferenceValue = component;
							state.serviceProperty.serializedObject.ApplyModifiedProperties();
							states.Remove(serviceDefinitionProperty.propertyPath);
						});
					}

					var openMenuBelowRect = GUILayoutUtility.GetLastRect();
					openMenuBelowRect.x += 100f;
					openMenuBelowRect.y += 105f;
					menu.DropDown(openMenuBelowRect);
				}
				// If a user tries to register a class by dragging its script to the services component,
				// inform the user that this is not supported.
				if(state.serviceProperty.objectReferenceValue is MonoScript script)
				{
					if(script.GetClass() is Type scriptClassType)
					{
						if(scriptClassType == typeof(MonoBehaviour))
						{
							Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can attach the component to a GameObject, and then drag-and-drop the GameObject here instead.", script);
						}
						else if(scriptClassType == typeof(ScriptableObject))
						{
							Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can create an asset from it using the [CreateAssetMenu] attribute, and drag-and-drop the asset here instead.", script);
						}
						else
						{
							Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of type {scriptClassType.Name} as a service, you can create a component class that derives from Wrapper<{scriptClassType.Name}>, or a ScriptableObject class that implements IValueProvider<{scriptClassType.Name}>, and drag-and-drop an instance of that class here instead.", script);
						}
					}
					else
					{
						Debug.LogWarning($"You can not register a script asset as a service. If you want to register an instance of the type defined inside the script as a service, you can create a component class that derives from Wrapper<TService>, or a ScriptableObject class that implements IValueProvider<TService>, and drag-and-drop an instance of that class here instead.", script);
					}

					state.serviceProperty.objectReferenceValue = null;
					serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();
				}
				else
                {
					states.Remove(serviceDefinitionProperty.propertyPath);
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

			var dropdownRect = asLabelRect;
			dropdownRect.x += asLabelWidth;
			dropdownRect.width = controlWidth;

			bool showMixedValueWas = EditorGUI.showMixedValue;
			if(state.definingTypeProperty.hasMultipleDifferentValues)
			{
				EditorGUI.showMixedValue = true;
			}

			state.definingTypeButton.Draw(dropdownRect);

			EditorGUI.showMixedValue = showMixedValueWas;
		}

		private void Setup(SerializedProperty property, State state)
        {
            state.serviceProperty = property.FindPropertyRelative("service");
            state.definingTypeProperty = property.FindPropertyRelative("definingType");

			RebuildDefiningTypeButton(property, state);
		}

		private void RebuildDefiningTypeButton(SerializedProperty property, State state)
		{
			var serviceValue = state.serviceProperty.objectReferenceValue;
			var typeOptions = GetTypeOptions(serviceValue);
			Type definingType = (state.definingTypeProperty.GetValue() as _Type)?.Value;
			var definingTypeLabel = new GUIContent(TypeUtility.ToString(definingType), string.Format(definingTypeTooltip, serviceValue is null ? "the service class" : TypeUtility.ToString(serviceValue.GetType())));

			state.definingTypeButton = new TypeDropdownButton
			(
				GUIContent.none,
				definingTypeLabel,
				typeOptions,
				Enumerable.Repeat(definingType, 1),
				(setType) =>
				{
					Undo.RecordObjects(property.serializedObject.targetObjects, "Set Defining Type");
					state.definingTypeProperty.SetValue(new _Type(setType));
					property.serializedObject.ApplyModifiedProperties();
					RebuildDefiningTypeButton(property, state);
					GUI.changed = true;
				},
				"Service Defining Type",
				type => (TypeUtility.ToStringNicified(type), null)
			);
		}

		private static IEnumerable<Type> GetTypeOptions(object serviceValue)
        {
			if(serviceValue is null)
            {
				return new Type[0];
			}

			var typeOptions = Enumerable.Empty<Type>();
			ConcatTypeOptions(ref typeOptions, serviceValue);
			var distinct = typeOptions.Distinct();

			if(CountExceeds(distinct, 30))
            {
				distinct = distinct.OrderBy(t => t.Name);
			}

			return distinct;
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

		private static void ConcatTypeOptions(ref IEnumerable<Type> typeOptions, [DisallowNull] Type serviceType, [AllowNull] Type wrappedType)
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
			
			typeOptions = typeOptions.Append(wrappedType);
			typeOptions = typeOptions.Append(serviceType);
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
			if(type.IsValueType)
			{
				typeOptions = typeOptions.Append(type);
				return;
			}

			for(var t = type; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				typeOptions = typeOptions.Append(t);
			}
		}

		private static void ConcatBaseTypes(ref IEnumerable<Type> typeOptions, Type type)
		{
			if(type.IsValueType)
			{
				return;
			}

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
			|| type == typeof(IOneArgument) || type == typeof(ITwoArguments) || type == typeof(IThreeArguments) || type == typeof(IFourArguments) || type == typeof(IFiveArguments) || type == typeof(ISixArguments)
			|| type == typeof(IWrapper)
			|| type == typeof(IComparable) || type == typeof(IFormattable) || type == typeof(IConvertible) || type == typeof(IDisposable)
			|| (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() is Type typeDefinition
				&& (typeDefinition == typeof(IEquatable<>) || typeDefinition == typeof(IComparable<>)
				|| type == typeof(IWrapper<>)
				|| typeDefinition == typeof(IFirstArgument<>) || typeDefinition == typeof(ISecondArgument<>) || typeDefinition == typeof(IThirdArgument<>)
				|| typeDefinition == typeof(IFourthArgument<>) || typeDefinition == typeof(IFifthArgument<>) || typeDefinition == typeof(ISixthArgument<>)));

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
    }
}