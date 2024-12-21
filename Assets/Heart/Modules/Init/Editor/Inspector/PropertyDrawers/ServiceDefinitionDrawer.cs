using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sisus.Init.Internal;
using Sisus.Init.Serialization;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;
using static Sisus.NullExtensions;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Custom property drawer for <see cref="ServiceDefinition"/>.
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

		private const float AsLabelWidth = 25f;

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
			bool hasValue = state.serviceProperty.objectReferenceValue;
			float controlWidth = hasValue ? (position.width - AsLabelWidth) * 0.5f : position.width;
			serviceRect.width = controlWidth;

			EditorGUI.BeginChangeCheck();

			if(!hasValue && InitializerEditorUtility.TryGetTintForNullGuardResult(NullGuardResult.ValueMissing, out Color setGuiColor))
			{
				GUI.color = setGuiColor;
			}

			EditorGUI.PropertyField(serviceRect, state.serviceProperty, GUIContent.none);

			GUI.color = Color.white;

			if(EditorGUI.EndChangeCheck())
			{
				serviceDefinitionProperty.serializedObject.ApplyModifiedProperties();

				if(state.serviceProperty.objectReferenceValue is GameObject gameObject)
				{
					using var components = gameObject.GetComponentsNonAlloc<Component>();

					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("GameObject"), false, () =>
					{
						Undo.RecordObjects(state.serviceProperty.serializedObject.targetObjects, "Set Service");
						state.serviceProperty.objectReferenceValue = gameObject;
						state.definingTypeProperty.SetValue(new _Type(typeof(GameObject)));
						state.serviceProperty.serializedObject.ApplyModifiedProperties();
						states.Remove(serviceDefinitionProperty.propertyPath);
						serviceDefinitionProperty.serializedObject.Update();
					});

					var addedItems = new HashSet<string>() { "GameObject" };

					for(int i = 0, count = components.Count; i < count; i++)
					{
						var component = components[i];
						if(!component)
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
							state.definingTypeProperty.SetValue(new _Type(component.GetType()));
							states.Remove(serviceDefinitionProperty.propertyPath);
							serviceDefinitionProperty.serializedObject.Update();
						});
					}

					menu.DropDown(serviceRect);
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
			asLabelRect.width = AsLabelWidth;
			GUI.Label(asLabelRect, asLabel);

			var dropdownRect = asLabelRect;
			dropdownRect.x += AsLabelWidth;
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
				type => (type is null ? "Null" : TypeUtility.ToStringNicified(type), null)
			);
		}

		private static List<Type> GetTypeOptions(object serviceValue)
        {
			if(serviceValue is null)
            {
				return new(0);
			}

			var typeOptions = GetTypeBaseTypesInterfacesIncludingFromProvidedValues(serviceValue);

			if(typeOptions.Count > 30)
            {
				typeOptions.Sort((x, y) => x.Name.CompareTo(y.Name));
			}

			return typeOptions;
		}

		private static List<Type> GetTypeBaseTypesInterfacesIncludingFromProvidedValues([AllowNull] object serviceOrServiceProvider)
        {
			var providedValueTypes = new List<Type>(64);

			if(serviceOrServiceProvider is null)
			{
				return providedValueTypes;
			}

			var serviceOrServiceProviderType = serviceOrServiceProvider.GetType();

			if (serviceOrServiceProvider is IValueProvider valueProvider)
			{
				try
				{
					object wrappedValue = valueProvider.Value;
					if(wrappedValue != Null)
					{
						AddIfDoesNotContain(providedValueTypes, wrappedValue.GetType());
					}
				}
				catch
				#if DEV_MODE
				(Exception e) { Debug.LogWarning($"{serviceOrServiceProviderType} IValueProvider.Value threw {e}.");
				#else
				{
					#endif
				}

				foreach(var interfaceType in serviceOrServiceProviderType.GetInterfaces())
				{
					if(!interfaceType.IsGenericType
						|| interfaceType.GetGenericTypeDefinition() is not Type typeDefinition
						|| (typeDefinition != typeof(IValueProvider<>) && typeDefinition != typeof(IValueProviderAsync<>)))
					{
						continue;
					}

					var wrappedType = interfaceType.GetGenericArguments()[0];
					if(!TypeUtility.IsNullOrBaseType(wrappedType))
					{
						AddIfDoesNotContain(providedValueTypes, wrappedType);
					}
				}
			}
			else if (serviceOrServiceProvider is IValueByTypeProvider valueByTypeProvider)
			{
				var client = serviceOrServiceProvider as Component;
				foreach(var providedValueType in TypeUtility.GetAllTypesVisibleTo(serviceOrServiceProviderType, valueType => valueByTypeProvider.CanProvideValue(client, valueType), 256))
				{
					AddIfDoesNotContain(providedValueTypes, providedValueType);
				}
			}

			int capacity = 1 + providedValueTypes.Count;
			capacity += capacity;
			capacity += capacity;
			var results = new List<Type>(capacity);

			foreach(var providedValueType in providedValueTypes)
			{
				if(!providedValueType.IsInterface)
				{
					AddIfDoesNotContain(results, providedValueType);
				}
			}

			AddIfDoesNotContain(results, serviceOrServiceProviderType);

			foreach(var providedValueType in providedValueTypes)
			{
				if(!providedValueType.IsInterface)
				{
					AddBaseTypes(results, providedValueType);
				}
			}

			AddBaseTypes(results, serviceOrServiceProviderType);

			foreach(var providedValueType in providedValueTypes)
			{
				if(!providedValueType.IsInterface)
				{
					AddDerivedTypes(results, providedValueType);
				}
			}

			AddDerivedTypes(results, serviceOrServiceProviderType);

			foreach(var providedValueType in providedValueTypes)
			{
				if(!providedValueType.IsInterface)
				{
					AddInterfaces(results, providedValueType);
				}
			}

			AddInterfaces(results, serviceOrServiceProviderType);

			foreach(var providedValueType in providedValueTypes)
			{
				if(providedValueType.IsInterface)
				{
					AddIfDoesNotContain(results, providedValueType);
				}
			}

			foreach(var providedValueType in providedValueTypes)
			{
				if(providedValueType.IsInterface)
				{
					AddBaseTypes(results, providedValueType);
				}
			}

			return results;
		}

		private static void AddIfDoesNotContain(List<Type> addToList, Type wrappedValueType)
		{
			if(!addToList.Contains(wrappedValueType))
			{
				addToList.Add(wrappedValueType);
			}
		}

		private static void AddBaseTypes(List<Type> addToList, Type parentType)
		{
			if(parentType.IsValueType)
			{
				return;
			}

			for(var baseType = parentType.BaseType; !TypeUtility.IsNullOrBaseType(baseType); baseType = baseType.BaseType)
			{
				AddIfDoesNotContain(addToList, baseType);
			}
		}

		private static void AddDerivedTypes(List<Type> addToList, Type baseType)
		{
			foreach(var derivedType in TypeCache.GetTypesDerivedFrom(baseType))
			{
				AddIfDoesNotContain(addToList, derivedType);
			}
		}

		private static void AddInterfaces(List<Type> addToList, Type type)
		{
			addToList.AddRange(type.GetInterfaces().Where(ShouldNotIgnoreInterface));

			static bool ShouldNotIgnoreInterface(Type type) => !ShouldIgnoreInterface(type);

			static bool ShouldIgnoreInterface(Type type) => type == typeof(ISerializationCallbackReceiver)
			|| type == typeof(IOneArgument) || type == typeof(ITwoArguments) || type == typeof(IThreeArguments) || type == typeof(IFourArguments) || type == typeof(IFiveArguments) || type == typeof(ISixArguments)
			|| type == typeof(IWrapper)
			|| type == typeof(IComparable) || type == typeof(IFormattable) || type == typeof(IConvertible) || type == typeof(IDisposable)
			|| (type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() is Type typeDefinition
				&& (typeDefinition == typeof(IEquatable<>) || typeDefinition == typeof(IComparable<>)
				|| type == typeof(IWrapper<>)
				|| typeDefinition == typeof(IFirstArgument<>) || typeDefinition == typeof(ISecondArgument<>) || typeDefinition == typeof(IThirdArgument<>)
				|| typeDefinition == typeof(IFourthArgument<>) || typeDefinition == typeof(IFifthArgument<>) || typeDefinition == typeof(ISixthArgument<>)));
		}
    }
}