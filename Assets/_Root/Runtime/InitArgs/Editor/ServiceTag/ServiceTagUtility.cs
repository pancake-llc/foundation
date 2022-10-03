using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pancake.Init.Internal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	internal static class ServiceTagUtility
	{
		internal static Component openSelectTagsMenuFor;
		private static readonly List<ServiceTag> serviceTags = new List<ServiceTag>();
		private static readonly HashSet<Type> currentDefiningTypes = new HashSet<Type>();
		private static readonly HashSet<Type> definingTypeOptions = new HashSet<Type>();
		private static readonly GUIContent serviceLabel = new GUIContent("Service", "An instance of this service will be automatically provided during initialization.");

		internal static Rect GetTagRect(Component component, Rect headerRect, GUIContent label, GUIStyle style)
        {
			var componentTitle = new GUIContent(ObjectNames.GetInspectorTitle(component));
			float componentTitleEndX = 54f + EditorStyles.largeLabel.CalcSize(componentTitle).x + 10f;
			float availableSpace = Screen.width - componentTitleEndX - 69f;
			float labelWidth = style.CalcSize(label).x;
			if(labelWidth > availableSpace)
			{
				labelWidth = availableSpace;
			}
			const float MinWidth = 18f;
			if(labelWidth < MinWidth)
			{
				labelWidth = MinWidth;
			}

			var labelRect = headerRect;
			labelRect.x = Screen.width - 69f - labelWidth;
			labelRect.y += 3f;

			// Fixes Transform header label rect position.
			// For some reason the Transform header rect starts
			// lower and is shorter than all other headers.
			if(labelRect.height < 22f)
            {
                labelRect.y -= 22f - 15f;
            }

            labelRect.height = 20f;
			labelRect.width = labelWidth;
			return labelRect;
        }

		internal static void Draw(Rect controlRect, Action onClicked)
		{
			controlRect.width = Styles.ServiceTag.CalcSize(serviceLabel).x;
			controlRect.y += 2f;
			controlRect.height -= 2f;
			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if(GUI.Button(controlRect, serviceLabel, Styles.ServiceTag))
			{
				GUI.changed = true;
				onClicked?.Invoke();
				GUIUtility.ExitGUI();
			}
			EditorGUI.indentLevel = indentLevelWas;
		}

		internal static void Ping(Type serviceDefiningType, Component component)
		{
			try
			{
				var instance = ServiceUtility.GetService(component, serviceDefiningType) as Object;
				if(instance != null)
				{
					EditorGUIUtility.PingObject(instance);
					GUIUtility.ExitGUI();
				}
			}
			catch(TargetInvocationException)
			{

			}

			var classWithAttribute = ServiceInjector.GetClassWithServiceAttribute(serviceDefiningType);
			var script = Find.Script(classWithAttribute != null ? classWithAttribute : serviceDefiningType);
			if(script == null && classWithAttribute != null)
			{
				script = Find.Script(serviceDefiningType);
			}

			if(script != null)
			{
				EditorGUIUtility.PingObject(script);
				GUIUtility.ExitGUI();
			}
		}

		internal static void SelectAllReferencesInScene(Component service)
		{
			Selection.objects = GetServiceDefiningTypes(service).SelectMany(t => FindAllReferences(t)).ToArray();
		}

		internal static void PingDefiningObject(Component service)
		{
			// Ping services component that defines the service, if any...
			var services = Find.All<Services>().FirstOrDefault(s => s.providesServices.Any(i => AreEqual(i.service, service)));
			if(services != null)
			{
				EditorGUIUtility.PingObject(services);
				return;
			}

			// Ping MonoScript that contains the ServiceAttribute, if found...
			var classType = service.GetType();
			if(HasServiceAttribute(classType))
			{
				var scriptWithServiceAttribute = Find.Script(classType);
				if(scriptWithServiceAttribute != null)
				{
					EditorGUIUtility.PingObject(scriptWithServiceAttribute);
					return;
				}
			}

			if(service is IValueProvider valueProvider && valueProvider.Value is object providedValue && HasServiceAttribute(providedValue.GetType()))
			{
				var scriptWithServiceAttribute = Find.Script(providedValue.GetType());
				if(scriptWithServiceAttribute != null)
				{
					EditorGUIUtility.PingObject(scriptWithServiceAttribute);
				}
			}			
			
		}

		private static bool HasServiceAttribute(Type type) => type.GetCustomAttributes<ServiceAttribute>().Any();

		internal static bool CanAddServiceTag(Component component)
		{
			if(HasServiceAttribute(component.GetType()))
			{
				return false;
			}

			return !Find.All<Services>().Any(s => s.enabled && s.providesServices.Any(i => AreEqual(i.service, component)));
		}

		internal static IEnumerable<ServiceTag> GetServiceTags(Component component)
		{
			component.GetComponents(serviceTags);

			foreach(var tag in serviceTags)
			{
				if(tag.Service == component)
				{
					yield return tag;
				}
			}

			serviceTags.Clear();
		}

		internal static bool HasServiceTag(Component component)
		{
			component.GetComponents(serviceTags);

			foreach(var tag in serviceTags)
			{
				if(tag.Service == component)
				{
					serviceTags.Clear();
					return true;
				}
			}

			serviceTags.Clear();
			return false;
		}

		internal static void OpenToClientsMenu(Component component, Rect tagRect)
		{
			var serviceTags = GetServiceTags(component).ToArray();
			
			// If has no ServiceTag component then ping defining object.
			if(serviceTags.Length == 0)
			{
				PingDefiningObject(component);
				return;
			}

			GUI.changed = true;

			var names = Enum.GetNames(typeof(Clients));
			int selectedIndex = serviceTags[0].ToClients;

			names[0] = "In GameObject";
			int count = names.Length;
			for(int i = 1; i < count; i++)
			{
				names[i] = ObjectNames.NicifyVariableName(names[i]);
			}

			void OnItemSelected(object value)
			{
				Undo.RecordObjects(serviceTags, "Set Service Clients");
				int toClients = (int)value;
				foreach(var serviceTag in serviceTags)
				{
					serviceTag.ToClients = toClients;
				}
			}

			var enumValues = Enum.GetValues(typeof(Clients));
			var values = new object[count];
			for(int i = 0; i < count; i++)
			{
				values[i] = (int)(Clients)enumValues.GetValue(i);
			}

			string selectedValueName = names[selectedIndex];

			DropdownWindow.Show(tagRect, names, values, Enumerable.Repeat(selectedValueName, 1), OnItemSelected, "Availability");
		}

		internal static void OpenContextMenu(Component component, Rect tagRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Find Clients In Scenes"), false, () => SelectAllReferencesInScene(component));

			if(HasServiceTag(component))
			{
				menu.AddItem(new GUIContent("Set Service Types..."), false, () => openSelectTagsMenuFor = component);

				var tagRectScreenSpace = tagRect;
				tagRectScreenSpace.y += GUIUtility.GUIToScreenPoint(Vector2.zero).y;
				if(EditorWindow.mouseOverWindow != null)
				{
					tagRectScreenSpace.y -= EditorWindow.mouseOverWindow.position.y;
				}
				menu.AddItem(new GUIContent("Set Availability..."), false, () => OpenToClientsMenu(component, tagRectScreenSpace));
			}
			else
			{
				menu.AddItem(new GUIContent("Find Defining Object"), false, () => PingDefiningObject(component));
			}

			menu.DropDown(tagRect);
		}
		

		internal static void OpenSelectTagsMenu(Component component, Rect tagRect)
		{
			if(!CanAddServiceTag(component))
			{
				return;
			}

			GUI.changed = true;

			void OnTypeSelected(Type selectedType)
			{
				if(ServiceTag.Remove(component, selectedType))
				{
					return;
				}

				ServiceTag.Add(component, selectedType);
			}

			var typeOptions = GetAllDefiningTypeOptions(component);
			var selectedTypes = GetServiceDefiningTypes(component);
			TypeDropdownWindow.Show(tagRect, typeOptions, false, selectedTypes, OnTypeSelected, "Service Types");
		}

		private static IEnumerable<Type> GetAllDefiningTypeOptions(Component component)
		{
			definingTypeOptions.Clear();

			GetAllDefiningTypeOptions(component.GetType(), definingTypeOptions);

			// We want to display the types and interfaces of the target of wrappers and initializers as well.
			if(component is IValueProvider valueProvider && valueProvider.Value is object providedValue)
			{
				GetAllDefiningTypeOptions(providedValue.GetType(), definingTypeOptions);
			}

			return definingTypeOptions;
		}

		private static void GetAllDefiningTypeOptions(Type classType, HashSet<Type> definingTypeOptions)
		{
			definingTypeOptions.Add(classType);

			foreach(var t in classType.GetInterfaces())
			{
				definingTypeOptions.Add(t);
			}

			for(var t = classType.BaseType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				definingTypeOptions.Add(t);
			}
		}

		internal static IEnumerable<Type> GetServiceDefiningTypes(Component component)
		{
			currentDefiningTypes.Clear();

			GetServiceDefiningTypes(component, component.GetType(), currentDefiningTypes);

			// We want to display the service tag on wrappers and initializers as well when their target is a service.
			if(component is IValueProvider valueProvider && valueProvider.Value is object providedValue)
			{
				GetServiceDefiningTypes(component, providedValue.GetType(), currentDefiningTypes);
			}

			return currentDefiningTypes;
		}

		private static void GetServiceDefiningTypes(Component component, Type classType, HashSet<Type> currentDefiningTypes)
		{
			foreach(var serviceAttribute in classType.GetCustomAttributes<ServiceAttribute>())
			{
				currentDefiningTypes.Add(serviceAttribute.definingType != null ? serviceAttribute.definingType : classType);
			}

			for(var t = classType; !TypeUtility.IsNullOrBaseType(t); t = t.BaseType)
			{
				if(!Services.InfosByDefiningType.TryGetValue(t, out var services))
				{
					continue;
				}

				foreach(var service in services)
				{
					if(AreEqual(service.service, component))
					{
						currentDefiningTypes.Add(t);
						break;
					}
				}
			}

			foreach(var t in classType.GetInterfaces())
			{
				if(!Services.InfosByDefiningType.TryGetValue(t, out var services))
				{
					continue;
				}

				foreach(var service in services)
				{
					if(AreEqual(service.service, component))
					{
						currentDefiningTypes.Add(t);
						break;
					}
				}
			}
		}

		private static bool AreEqual(object service, Component component)
		{
			if(service is Component componentService && componentService == component)
			{
				return true;
			}

			if(component is IValueProvider valueProvider && valueProvider.Value == service)
			{
				return true;
			}

			return false;
		}

		private static IEnumerable<GameObject> FindAllReferences(Type serviceType)
		{
			List<Component> components = new List<Component>();
			for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
			{
				var scene = SceneManager.GetSceneAt(s);
				var rootGameObjects = scene.GetRootGameObjects();
				for(int r = rootGameObjects.Length - 1; r >= 0; r--)
				{
					foreach(var reference in FindAllReferences(rootGameObjects[r].transform, components, serviceType))
					{
						yield return reference;
					}
				}
			}
		}

		private static IEnumerable<GameObject> FindAllReferences(Transform transform, List<Component> components, Type serviceType)
		{
			transform.GetComponents(components);

			// Skip component at index 0 which is most likely a Transform.
			for(int c = components.Count - 1; c >= 1; c--)
			{
				var component = components[c];
				if(component == null)
				{
					continue;
				}

				var componentType = component.GetType();

				if(component is IOneArgument)
				{
					if(componentType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<>)).FirstOrDefault() is Type initializableType)
					{
						var argTypes = initializableType.GetGenericArguments();
						if(argTypes[0] == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
				}
				else if(component is ITwoArguments)
				{
					if(componentType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,>)).FirstOrDefault() is Type initializableType)
					{
						var argTypes = initializableType.GetGenericArguments();
						if(argTypes[0] == serviceType || argTypes[1] == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
				}
				else if(component is IThreeArguments)
				{
					if(componentType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,>)).FirstOrDefault() is Type initializableType)
					{
						var argTypes = initializableType.GetGenericArguments();
						if(argTypes[0] == serviceType || argTypes[1] == serviceType || argTypes[2] == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
				}
				else if(component is IFourArguments)
				{
					if(componentType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,,>)).FirstOrDefault() is Type initializableType)
					{
						var argTypes = initializableType.GetGenericArguments();
						if(argTypes[0] == serviceType || argTypes[1] == serviceType || argTypes[2] == serviceType || argTypes[3] == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
				}
				else if(component is IFiveArguments)
				{
					if(componentType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,,,>)).FirstOrDefault() is Type initializableType)
					{
						var argTypes = initializableType.GetGenericArguments();
						if(argTypes[0] == serviceType || argTypes[1] == serviceType || argTypes[2] == serviceType || argTypes[3] == serviceType || argTypes[4] == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
				}

				var serializedObject = new SerializedObject(component);
				var property = serializedObject.GetIterator();
				string serviceTypeName = serviceType.Name;
				string serviceTypeNameAlt = string.Concat("PPtr<", serviceTypeName, ">");
				
				if(property.NextVisible(true))
				{
					do
					{
						if(string.Equals(property.type, "Any`1") && property.GetValue() is object any)
						{
							Type anyValueType = any.GetType().GetGenericArguments()[0];
							if(anyValueType == serviceType)
							{
								yield return component.gameObject;
								continue;
							}
						}
						else if(string.Equals(property.type, serviceTypeName) || string.Equals(property.type, serviceTypeNameAlt) && property.GetType() == serviceType)
						{
							yield return component.gameObject;
							continue;
						}
					}
					// Checking only surface level fields, not nested fields, for performance reasons.
					while(property.NextVisible(false));
				}
			}

			components.Clear();

			for(int i = transform.childCount - 1; i >= 0; i--)
			{
				foreach(var reference in FindAllReferences(transform.GetChild(i), components, serviceType))
				{
					yield return reference;
				}
			}
		}
	}
}