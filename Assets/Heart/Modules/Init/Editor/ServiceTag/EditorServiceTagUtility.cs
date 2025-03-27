//#define DEBUG_CLEAR_CACHE

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using static Sisus.Init.Internal.ServiceTagUtility;
using static Sisus.NullExtensions;

#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
using UnityEngine.AddressableAssets;
#endif

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class EditorServiceTagUtility
	{
		internal static Component openSelectTagsMenuFor;
		private static readonly GUIContent serviceLabel = new("Service", "A service of this type is available.\n\nIt can be acquired automatically during initialization.");
		private static readonly GUIContent blankLabel = new(" ");
		private static readonly HashSet<Type> definingTypesBuilder = new();
		private static readonly Dictionary<object, Type[]> objectDefiningTypesCache = new();

		static EditorServiceTagUtility()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
			ObjectChangeEvents.changesPublished -= OnObjectChangesPublished;
			ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
			Service.AnyChangedEditorOnly -= OnAnyServiceChanged;
			Service.AnyChangedEditorOnly += OnAnyServiceChanged;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			SceneManager.sceneUnloaded -= OnSceneUnloaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			EditorSceneManager.sceneClosed -= OnSceneClosed;
			EditorSceneManager.sceneClosed += OnSceneClosed;

			static void OnObjectChangesPublished(ref ObjectChangeEventStream stream) => ClearDefiningTypesCache();
			static void OnUndoRedoPerformed() => ClearDefiningTypesCache();

			static void OnSelectionChanged()
			{
				// Need to repaint editor header to update service tag position.
				RepaintAllServiceEditors();
			}

			static void OnAnyServiceChanged() => ClearDefiningTypesCache();
			static void OnPlayModeStateChanged(PlayModeStateChange mode) => ClearDefiningTypesCache();
			static void OnSceneUnloaded(Scene scene) => ClearDefiningTypesCache();
			static void OnSceneClosed(Scene scene) => ClearDefiningTypesCache();

			static void ClearDefiningTypesCache()
			{
				#if DEV_MODE && DEBUG_CLEAR_CACHE
				Debug.Log("ClearDefiningTypesCache");
				#endif
				EditorApplication.delayCall -= ClearDefiningTypesCacheImmediate;
				EditorApplication.delayCall += ClearDefiningTypesCacheImmediate;
			}

			#if DEV_MODE
			[MenuItem("DevMode/Clear Service Defining Types Cache")]
			#endif
			static void ClearDefiningTypesCacheImmediate()
			{
				var components = Selection.activeGameObject ? Selection.activeGameObject.GetComponentsNonAlloc<Component>() : ComponentCollection<Component>.Empty();
				RepaintEditorsWhenService(components);

				foreach(var definingTypes in objectDefiningTypesCache.Values)
				{
					if(definingTypes.Length > 0)
					{
						ArrayPool<Type>.Shared.Return(definingTypes, true);
					}
				}

				objectDefiningTypesCache.Clear();

				RepaintEditorsWhenService(components);
			}

			static void RepaintEditorsWhenService(ComponentCollection<Component> inspectedComponents)
			{
				foreach(var component in inspectedComponents)
				{
					if(component && GetServiceDefiningTypes(component).Length > 0)
					{
						// Need to repaint editor header to update service tag position.
						InspectorContents.RepaintEditorsWithTarget(component);
					}
				}
			}

			static void RepaintAllServiceEditors()
			{
				var gameObject = Selection.activeGameObject;
				if(!gameObject)
				{
					return;
				}

				foreach(var component in Selection.activeGameObject.GetComponentsNonAlloc<Component>())
				{
					// Skip missing components
					if(!component)
					{
						continue;
					}
					
					if(GetServiceDefiningTypes(component).Length > 0)
					{
						// Need to repaint editor header to update service tag position.
						InspectorContents.RepaintEditorsWithTarget(component);
					}
				}
			}
		}

		internal static Span<Type> GetServiceDefiningTypes([DisallowNull] object serviceOrServiceProvider)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = getServiceDefiningTypesMarker.Auto();
			#endif

			if(objectDefiningTypesCache.TryGetValue(serviceOrServiceProvider, out var cachedResults))
			{
				if(cachedResults.Length <= 1)
				{
					return cachedResults;
				}

				int nullIndex = Array.IndexOf(cachedResults, null, 1);
				return nullIndex is -1 ? cachedResults.AsSpan() : cachedResults.AsSpan(0, nullIndex);
			}

			Transform clientOrNull = Find.In<Transform>(serviceOrServiceProvider);
			AddServiceDefiningTypes(clientOrNull, serviceOrServiceProvider, definingTypesBuilder);

			if(serviceOrServiceProvider is IValueProvider valueProvider)
			{
				AddServiceProviderValueDefiningTypes(clientOrNull, valueProvider, definingTypesBuilder);
			}

			int count = definingTypesBuilder.Count;
			if(count == 0)
			{
				objectDefiningTypesCache.Add(serviceOrServiceProvider, Array.Empty<Type>());
				return Span<Type>.Empty;
			}

			var definingTypes = ArrayPool<Type>.Shared.Rent(definingTypesBuilder.Count);
			definingTypesBuilder.CopyTo(definingTypes);
			definingTypesBuilder.Clear();
			objectDefiningTypesCache.Add(serviceOrServiceProvider, definingTypes);
			return definingTypes.AsSpan(0, count);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void AddServiceDefiningTypes([AllowNull] Component clientOrNull, [DisallowNull] object service, [DisallowNull] HashSet<Type> definingTypes)
			{
				Type concreteType = service.GetType();

				for(var typeOrBaseType = concreteType; !TypeUtility.IsNullOrBaseType(typeOrBaseType); typeOrBaseType = typeOrBaseType.BaseType)
				{
					if(!definingTypes.Contains(typeOrBaseType) && (!clientOrNull ? ServiceUtility.IsServiceOrServiceProvider(typeOrBaseType, service) : ServiceUtility.IsServiceOrServiceProviderFor(clientOrNull, typeOrBaseType, service)))
					{
						definingTypes.Add(typeOrBaseType);
					}
				}

				var interfaces = concreteType.GetInterfaces();
				foreach(var interfaceType in interfaces)
				{
					if(definingTypes.Contains(interfaceType))
					{
						continue;
					}

					if(clientOrNull ? !ServiceUtility.IsServiceOrServiceProviderFor(clientOrNull, interfaceType, service) : !ServiceUtility.IsServiceOrServiceProvider(interfaceType, service))
					{
						continue;
					}

					definingTypes.Add(interfaceType);
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void AddServiceProviderValueDefiningTypes([AllowNull] Component client, [DisallowNull] IValueProvider serviceProvider, [DisallowNull] HashSet<Type> definingTypes)
			{
				Type concreteType;
				var interfaces = serviceProvider.GetType().GetInterfaces();
				HashSet<Type> providedValueTypes;
				object providedValue = serviceProvider.Value;
				bool hasValue = providedValue != Null;
				if(hasValue)
				{
					concreteType = providedValue.GetType();
					providedValueTypes = interfaces.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValueProvider<>)).Select(t => t.GetGenericArguments()[0]).Append(concreteType).ToHashSet();
				}
				else
				{
					providedValueTypes = interfaces.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IValueProvider<>)).Select(t => t.GetGenericArguments()[0]).ToHashSet();
				}

				foreach(var providedValueType in providedValueTypes)
				{
					foreach(var definingType in ServiceAttributeUtility.GetDefiningTypes(providedValueType))
					{
						definingTypes.Add(definingType);
					}
				}

				if(!hasValue)
				{
					return;
				}

				foreach(var providedValueType in providedValueTypes)
				{
					for(var typeOrBaseType = providedValueType; !TypeUtility.IsNullOrBaseType(typeOrBaseType); typeOrBaseType = typeOrBaseType.BaseType)
					{
						if(!definingTypes.Contains(typeOrBaseType) && ServiceUtility.IsServiceOrServiceProviderFor(client, typeOrBaseType, providedValue))
						{
							definingTypes.Add(typeOrBaseType);
						}
					}

					interfaces = providedValueType.GetInterfaces();
					foreach(var interfaceType in interfaces)
					{
						if(!definingTypes.Contains(interfaceType) && ServiceUtility.IsServiceOrServiceProviderFor(client, interfaceType, providedValue))
						{
							definingTypes.Add(interfaceType);
						}
					}
				}
			}
		}

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

		/// <param name="anyProperty"> SerializedProperty of <see cref="Any{T}"/> or some other type field. </param>
		internal static bool Draw(Rect position, GUIContent prefixLabel, SerializedProperty anyProperty, GUIContent serviceLabel = null, bool serviceExists = true)
		{
			var controlRect = EditorGUI.PrefixLabel(position, blankLabel);
			bool clicked = Draw(controlRect, anyProperty, serviceLabel, serviceExists);
			position.width -= controlRect.x - position.x;
			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			GUI.Label(position, prefixLabel);
			EditorGUI.indentLevel = indentLevelWas;
			return clicked;
		}

		/// <param name="anyProperty"> SerializedProperty of <see cref="Any{T}"/> or some other type field. </param>
		internal static bool Draw(Rect controlRect, SerializedProperty anyProperty = null, GUIContent label = null, bool serviceExists = true)
		{
			label ??= serviceLabel;
			float maxWidth = Styles.ServiceTag.CalcSize(label).x;
			if(controlRect.width > maxWidth)
			{
				controlRect.width = maxWidth;
			}

			controlRect.y += 2f;
			controlRect.height -= 2f;
			int indentLevelWas = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var backgroundColorWas = GUI.backgroundColor;
			if(serviceExists)
			{
				GUI.backgroundColor = new Color(1f, 1f, 0f);
			}

			bool clicked = GUI.Button(controlRect, label, Styles.ServiceTag);

			GUI.backgroundColor = backgroundColorWas;

			GUILayout.Space(2f);

			EditorGUI.indentLevel = indentLevelWas;

			if(!clicked)
			{
				return false;
			}

			GUI.changed = true;

			if(anyProperty is not null)
			{
				OnServiceTagClicked(controlRect, anyProperty);
			}

			return true;
		}

		internal static void PingServiceOfClient(Object client, Type serviceDefiningType)
		{
			try
			{
				if(ServiceUtility.TryGetFor(client, serviceDefiningType, out var service) && service is Object unityObjectService && unityObjectService)
				{
					EditorGUIUtility.PingObject(unityObjectService);
					
					if(Event.current == null)
					{
						return;
					}

					if(Event.current.type != EventType.MouseDown && Event.current.type != EventType.MouseUp && Event.current.type != EventType.ContextClick && Event.current.type != EventType.KeyDown && Event.current.type != EventType.KeyUp)
					{
						return;
					}

					LayoutUtility.ExitGUI();
				}
			}
			catch(TargetInvocationException)
			{

			}

			var typeToPing = ServiceInjector.GetClassWithServiceAttribute(serviceDefiningType);
			if(ServiceAttributeUtility.definingTypes.TryGetValue(serviceDefiningType, out var serviceInfo))
			{
				typeToPing = serviceInfo.serviceOrProviderType ?? serviceDefiningType;
				if(serviceInfo.FindFromScene)
				{
					if(Find.Any(serviceDefiningType, out var service) && Find.GameObjectOf(service, out GameObject serviceGameObject))
					{
						EditorGUIUtility.PingObject(serviceGameObject);
						LayoutUtility.ExitGUI();
						return;
					}

					if(serviceInfo.SceneBuildIndex >= 0)
					{
						var sceneAsset = Find.SceneAssetByBuildIndex(serviceInfo.SceneBuildIndex);
						if(sceneAsset)
						{
							EditorGUIUtility.PingObject(sceneAsset);
							LayoutUtility.ExitGUI();
							return;
						}
					}
					else if(serviceInfo.SceneName is { Length: > 0 } sceneName)
					{
						var sceneAsset = Find.SceneAssetByName(sceneName);
						EditorGUIUtility.PingObject(sceneAsset);
						LayoutUtility.ExitGUI();
					}
				}
				else if(serviceInfo.ResourcePath is { Length: > 0 } resourcePath)
				{
					var service = Resources.Load<Object>(resourcePath);
					if(service)
					{
						EditorGUIUtility.PingObject(service);
						LayoutUtility.ExitGUI();
						return;
					}
				}
				#if UNITY_ADDRESSABLES_1_17_4_OR_NEWER
				else if(serviceInfo.AddressableKey is { Length: > 0 } addressableKey)
				{
					var service = Addressables.LoadAssetAsync<Object>(addressableKey).WaitForCompletion();
					if(service)
					{
						EditorGUIUtility.PingObject(service);
						LayoutUtility.ExitGUI();
						return;
					}
				}
				#endif
			}
			else
			{
				typeToPing = serviceDefiningType;
			}

			var script = Find.Script(typeToPing);
			if(!script && typeToPing != serviceDefiningType)
			{
				script = Find.Script(serviceDefiningType);
			}

			if(script)
			{
				EditorGUIUtility.PingObject(script);
				LayoutUtility.ExitGUI();
			}
			else
			{
				EditorApplication.ExecuteMenuItem("Window/General/Console");
				Debug.Log($"Could not locate the script that contains the type '{TypeUtility.ToString(typeToPing)}'.\nThis can happen when the name of the script does not match the type name.", client);
			}
		}

		/// <param name="anyProperty"> SerializedProperty of <see cref="Any{T}"/> or some other type field. </param>
		internal static void OnServiceTagClicked(Rect controlRect, SerializedProperty anyProperty)
		{
			if(anyProperty == null)
			{
				#if DEV_MODE
				Debug.LogWarning($"OnServiceTagClicked called but {nameof(anyProperty)} was null.");
				#endif
				return;
			}

			var propertyValue = anyProperty.GetValue();
			if(propertyValue == null)
			{
				#if DEV_MODE
				Debug.LogWarning($"OnServiceTagClicked called but GetValue returned null for {nameof(anyProperty)} '{anyProperty.name}' ('{anyProperty.propertyPath}').");
				#endif
				return;
			}

			Type propertyType = propertyValue.GetType();

			Type serviceType;
			if(typeof(IAny).IsAssignableFrom(propertyType) && propertyType.IsGenericType)
			{
				serviceType = propertyType.GetGenericArguments()[0];
			}
			else
			{
				serviceType = propertyType;
			}

			switch(Event.current.button)
			{
				case 0:
				case 2:
					var targetObject = anyProperty.serializedObject.targetObject;
					PingServiceOfClient(targetObject, serviceType);
					return;
				case 1:
					AnyPropertyDrawer.OpenDropdown(controlRect, anyProperty);
					return;
			}
		}

		internal static void PingService(Object service)
		{
			EditorGUIUtility.PingObject(service);
			LayoutUtility.ExitGUI();
		}
		
		internal static bool PingServiceFor(Object client, Type serviceType)
		{
			if(ServiceUtility.TryGetFor(client, serviceType, out var service) && Find.In(service, out Object unityObject))
			{
				PingService(unityObject);
				return true;
			}

			var typeToPing = ServiceAttributeUtility.definingTypes.TryGetValue(serviceType, out var serviceInfo) ? serviceInfo.serviceOrProviderType ?? serviceType : serviceType;
			if(Find.Script(typeToPing, out var scriptToPing))
			{
				PingService(scriptToPing);
				return true;
			}

			return false;
		}

		internal static void SelectAllGlobalServiceClientsInScene(object serviceOrServiceProvider)
			=> Selection.objects = GetServiceDefiningTypes(serviceOrServiceProvider).ToArray().SelectMany(FindAllReferences).Distinct().ToArray();

		internal static void SelectAllReferencesInScene(object serviceOrServiceProvider, Type[] definingTypes, Clients clients, Component registerer)
			=> Selection.objects = definingTypes.SelectMany(FindAllReferences).Distinct().Where(go => Service.IsAccessibleTo(go.transform, registerer, clients)).ToArray();

		/// <summary>
		/// Ping MonoScript or GameObject containing the configuration that causes the object, or the value provided by the object
		/// (in the case of a wrapper component etc.), to be a service.
		/// </summary>
		/// <param name="serviceOrServiceProvider">
		/// An object which is a service, or an object which provides the services, such as an <see cref="IWrapper"/>.
		/// </param>
		internal static void PingServiceDefiningObject(Component serviceOrServiceProvider)
		{
			// Ping services component that defines the service, if any...
			var services = Find.All<Services>().FirstOrDefault(s => s.providesServices.Any(i => AreEqual(i.service, serviceOrServiceProvider)));
			if(services)
			{
				EditorGUIUtility.PingObject(services);
				return;
			}

			// Ping MonoScript that contains the ServiceAttribute, if found...
			var serviceOrServiceProviderType = serviceOrServiceProvider.GetType();
			if(HasServiceAttribute(serviceOrServiceProviderType))
			{
				var scriptWithServiceAttribute = Find.Script(serviceOrServiceProviderType);
				if(scriptWithServiceAttribute)
				{
					EditorGUIUtility.PingObject(scriptWithServiceAttribute);
					return;
				}
			}

			// Ping MonoScript of ServiceInitializer
			foreach(Type serviceInitializerType in TypeUtility.GetImplementingTypes<IServiceInitializer>(typeof(ServiceAttribute).Assembly, false, 16))
			{
				if(serviceInitializerType.IsGenericType && !serviceInitializerType.IsGenericTypeDefinition && !serviceInitializerType.IsAbstract
				&& serviceInitializerType.GetGenericArguments()[0] == serviceOrServiceProviderType && HasServiceAttribute(serviceInitializerType)
				&& Find.Script(serviceInitializerType, out var serviceInitializerScript))
				{
					EditorGUIUtility.PingObject(serviceInitializerScript);
					return;
				}
			}

			if(serviceOrServiceProvider is IWrapper)
			{
				foreach(Type interfaceType in serviceOrServiceProviderType.GetInterfaces())
				{
					if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IWrapper<>)
					&& Find.Script(interfaceType.GetGenericArguments()[0], out var serviceInitializerScript))
					{
						EditorGUIUtility.PingObject(serviceInitializerScript);
						return;
					}
				}
			}
		}

		internal static void PingDefiningObject(Type definingType)
		{
			// Ping services component that defines the service, if any...
			var services = Find.All<Services>().FirstOrDefault(s => s.providesServices.Any(i => i.definingType.Value == definingType));
			if(services)
			{
				EditorGUIUtility.PingObject(services);
				return;
			}

			if(ServiceAttributeUtility.definingTypes.TryGetValue(definingType, out var serviceInfo) && Find.Script(serviceInfo.serviceOrProviderType ?? serviceInfo.concreteType ?? definingType, out MonoScript scriptWithAttribute))
			{
				EditorGUIUtility.PingObject(scriptWithAttribute);
				return;
			}

			// Ping MonoScript that contains the ServiceAttribute, if found...
			if(HasServiceAttribute(definingType) && Find.Script(definingType, out var scriptWithServiceAttribute))
			{
				EditorGUIUtility.PingObject(scriptWithServiceAttribute);
			}
		}

		private static bool HasServiceAttribute(Type type) => type.GetCustomAttributes<ServiceAttribute>().Any();

		internal static bool CanAddServiceTag(Component service)
		{
			if(HasServiceAttribute(service.GetType()))
			{
				return false;
			}

			return !Find.All<Services>().Any(s => s.enabled && s.providesServices.Any(i => AreEqual(i.service, service)));
		}

		internal static void OpenToClientsMenu(Component serviceOrServiceProvider, Rect tagRect)
		{
			var tags = GetServiceTags(serviceOrServiceProvider).ToArray();
			
			// If has no ServiceTag component then ping defining object.
			if(tags.Length == 0)
			{
				PingServiceDefiningObject(serviceOrServiceProvider);
				return;
			}

			GUI.changed = true;

			var names = Enum.GetNames(typeof(Clients));
			int selectedIndex = (int)tags[0].ToClients;

			names[0] = "In GameObject";
			int count = names.Length;
			for(int i = 1; i < count; i++)
			{
				names[i] = ObjectNames.NicifyVariableName(names[i]);
			}

			var enumValues = Enum.GetValues(typeof(Clients));
			var values = new object[count];
			for(int i = 0; i < count; i++)
			{
				values[i] = (Clients)enumValues.GetValue(i);
			}

			string selectedValueName = names[selectedIndex];
			DropdownWindow.Show(tagRect, names, values, Enumerable.Repeat(selectedValueName, 1), OnItemSelected, "Availability");

			void OnItemSelected(object value)
			{
				Undo.RecordObjects(tags, "Set Service Clients");
				var toClients = (Clients)value;
				foreach(var serviceTag in tags)
				{
					if(toClients == serviceTag.ToClients)
					{
						Undo.DestroyObjectImmediate(serviceTag);
					}
					else
					{
						serviceTag.ToClients = toClients;
					}
				}
			}
		}

		internal static void OpenContextMenuForService(Component serviceOrServiceProvider, Type[] definingTypes, Clients clients, Component registerer, Rect tagRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Find Clients In Scenes"), false, () => SelectAllReferencesInScene(serviceOrServiceProvider, definingTypes, clients, registerer));

			if(HasServiceTag(serviceOrServiceProvider))
			{
				menu.AddItem(new GUIContent("Set Service Types..."), false, () => openSelectTagsMenuFor = serviceOrServiceProvider);

				var tagRectScreenSpace = tagRect;
				tagRectScreenSpace.y += GUIUtility.GUIToScreenPoint(Vector2.zero).y;
				if(EditorWindow.mouseOverWindow)
				{
					tagRectScreenSpace.y -= EditorWindow.mouseOverWindow.position.y;
				}

				menu.AddItem(new GUIContent("Set Availability..."), false, () => OpenToClientsMenu(serviceOrServiceProvider, tagRectScreenSpace));
			}
			else if(!(serviceOrServiceProvider is Services))
			{
				menu.AddItem(new GUIContent("Find Defining Object"), false, () => PingServiceDefiningObject(serviceOrServiceProvider));
			}

			menu.DropDown(tagRect);
		}
		
		internal static void OpenContextMenuForServiceOfClient(Object client, Type serviceType, Rect tagRect)
		{
			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Find Service"), false, () => PingServiceOfClient(client, serviceType));

			if(ServiceUtility.TryGetFor(client, serviceType, out object service, Context.MainThread))
			{
				menu.AddItem(new GUIContent("Find Defining Object"), false, () => PingDefiningObject(serviceType));
			}

			menu.DropDown(tagRect);
		}

		internal static void OpenSelectTagsMenu(Component service, Rect tagRect)
		{
			if(!CanAddServiceTag(service) && !HasServiceTag(service))
			{
				return;
			}

			GUI.changed = true;

			var typeOptions = GetAllDefiningTypeOptions(service);
			var selectedTypes = GetServiceTags(service).Select(tag => tag.DefiningType).ToList();
			TypeDropdownWindow.Show(tagRect, typeOptions, selectedTypes, OnTypeSelected, "Service Types", GetItemContent);

			void OnTypeSelected(Type selectedType)
			{
				Undo.RecordObject(service, "Set Service Type");

				if(ServiceTag.Remove(service, selectedType))
				{
					return;
				}

				ServiceTag.Add(service, selectedType);
				InspectorContents.RepaintEditorsWithTarget(service);
			}

			(string fullPath, Texture icon) GetItemContent(Type type)
			{
				if(type is null)
				{
					return ("Service", null);
				}

				if(type == typeof(Object))
				{
					return ("Reference", EditorGUIUtility.FindTexture("DotSelection"));
				}

				if(!type.IsInstanceOfType(service) || (typeof(IValueProvider).IsAssignableFrom(type) && type.IsInterface))
				{
					return ("Value Provider/" + TypeUtility.ToString(type), EditorGUIUtility.ObjectContent(null, type).image);
				}
				
				if(type.GetCustomAttribute<ValueProviderMenuAttribute>() is ValueProviderMenuAttribute attribute && !string.IsNullOrEmpty(attribute.ItemName))
				{ 
					return (attribute.ItemName, EditorGUIUtility.FindTexture("eyeDropper.Large"));
				}

				return (TypeUtility.ToString(type), EditorGUIUtility.ObjectContent(null, type).image);
			}
		}

		private static bool AreEqual(object x, object y)
		{
			if(ReferenceEquals(x, y))
			{
				return true;
			}

			if(x is IValueProvider xValueProvider)
			{
				object xValue = xValueProvider.Value;
				if(ReferenceEquals(xValue, y))
				{
					return true;
				}
				
				if(y is IValueProvider yValueProvider)
				{
					if(ReferenceEquals(xValue, yValueProvider.Value))
					{
						return true;
					}
				}
			}
			else if(y is IValueProvider yValueProvider)
			{
				if(ReferenceEquals(x, yValueProvider.Value))
				{
					return true;
				}
			}

			return false;
		}

		private static IEnumerable<GameObject> FindAllReferences(Type serviceType)
		{
			for(int s = SceneManager.sceneCount - 1; s >= 0; s--)
			{
				var scene = SceneManager.GetSceneAt(s);
				var rootGameObjects = scene.GetRootGameObjects();
				for(int r = rootGameObjects.Length - 1; r >= 0; r--)
				{
					foreach(var reference in FindAllReferences(rootGameObjects[r].transform, serviceType))
					{
						yield return reference;
					}
				}
			}
		}

		private static IEnumerable<GameObject> FindAllReferences(Transform transform, Type serviceType)
		{
			var components = transform.gameObject.GetComponentsNonAlloc<Component>();

			// Skip component at index 0 which is most likely a Transform.
			for(int c = components.Count - 1; c >= 1; c--)
			{
				var component = components[c];
				if(!component)
				{
					continue;
				}

				var componentType = component.GetType();

				if(component is IOneArgument)
				{
					if(componentType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<>)) is Type initializableType)
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
					if(componentType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,>)) is Type initializableType)
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
					if(componentType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,>)) is Type initializableType)
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
					if(componentType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,,>)) is Type initializableType)
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
					if(componentType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IArgs<,,,,>)) is Type initializableType)
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
							}
						}
						else if((string.Equals(property.type, serviceTypeName) || string.Equals(property.type, serviceTypeNameAlt)) && property.GetType() == serviceType)
						{
							yield return component.gameObject;
						}
					}
					// Checking only surface level fields, not nested fields, for performance reasons.
					while(property.NextVisible(false));
				}
			}

			for(int i = transform.childCount - 1; i >= 0; i--)
			{
				foreach(var reference in FindAllReferences(transform.GetChild(i), serviceType))
				{
					yield return reference;
				}
			}
		}

		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker getServiceDefiningTypesMarker = new(ProfilerCategory.Gui, "EditorServiceTagUtility.GetServiceDefiningTypes");
		#endif
	}
}