using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Sisus.ComponentNames.Editor
{
	internal static class UnityEventDrawerUtility
	{
		private const string kNoFunctionString = "No Function";

		private const string kInstancePath = "m_Target";
		private const string kInstanceTypePath = "m_TargetAssemblyTypeName";
		private const string kArgumentsPath = "m_Arguments";
		private const string kModePath = "m_Mode";
		private const string kMethodNamePath = "m_MethodName";
		private const string kCallsPath = "m_PersistentCalls.m_Calls";

		internal const string kObjectArgument = "m_ObjectArgument";
		internal const string kObjectArgumentAssemblyTypeName = "m_ObjectArgumentAssemblyTypeName";

		public static GenericMenu BuildFunctionSelectDropdownMenu(SerializedProperty unityEventProperty, int index)
		{
			var dummyEvent = GetDummyEvent(unityEventProperty);
			var propertyRelative = unityEventProperty.FindPropertyRelative(kCallsPath);
			var listener = propertyRelative.GetArrayElementAtIndex(index);
			if(listener is null)
			{
				#if DEV_MODE
				Debug.LogWarning(propertyRelative.propertyPath + "[" + index + "] is null");
				#endif

				return null;
			}

			var listenerTarget = listener.FindPropertyRelative(kInstancePath).objectReferenceValue;

			// Use more reflection if possible to call some of Unity's internal methods.
			// This way if Unity updates those methods, I will automatically also get all the updates.
			if(!BuildFunctionSelectDropdownMenuUsingInternalMethod(listener, dummyEvent, listenerTarget, out var menu))
			{
				menu = BuildFunctionSelectDropdownMenu(listenerTarget, dummyEvent, listener);
			}

			OverrideNames(listenerTarget, ref menu);

			return menu;
		}

		private static void OverrideNames(Object listenerTarget, ref GenericMenu menu)
		{
			if(menu is null)
			{
				return;
			}

			Component[] components;
			GameObject gameObject = listenerTarget as GameObject;
			if(gameObject == null)
			{
				if(!(listenerTarget is Component listenerComponent) || listenerComponent == null)
				{
					return;
				}
				
				gameObject = listenerComponent.gameObject;
			}

			components = gameObject.GetComponents<Component>();

			var menuItemsProperty = typeof(GenericMenu).GetProperty("menuItems", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			var menuItems = menuItemsProperty.GetValue(menu, null) as IList;
			int count = menuItems.Count;
			if(count == 0)
			{
				return;
			}

			var menuItemType = menuItems[0].GetType();
			var contentField = menuItemType.GetField("content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(contentField is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"Field {menuItemType.FullName}.content not found.");
				#endif
				return;
			}

			Dictionary<string, string> namesAndOverridesCache = new Dictionary<string, string>();

			for(int i = 0; i < count; i++)
			{
				var menuItem = menuItems[i];
				var label = (GUIContent)contentField.GetValue(menuItem);
				string originalPath = label.text;
				string overridePath = GetOverridePath(components, originalPath, namesAndOverridesCache);
				label.text = overridePath;
			}
		}

		private static string GetOverridePath(Component[] components, string originalPath, Dictionary<string, string> cache)
		{
			int originalNameEnd = originalPath.IndexOf('/');
			if(originalNameEnd <= 0)
			{
				return originalPath;
			}
				
			string originalName = originalPath.Substring(0, originalNameEnd);
			if(cache.TryGetValue(originalName, out string overrideName))
			{
				return overrideName + originalPath.Substring(originalNameEnd);
			}

			int nth = 0;
			string originalNameWithoutSuffix = originalName;
			if(originalName.EndsWith(")", StringComparison.Ordinal) && int.TryParse(originalName.Substring(originalName.Length - 2, 1), out int parsedNth))
			{
				nth = parsedNth;
				originalNameWithoutSuffix = originalName.Substring(0, originalName.Length - 4); // For example " (1)";
			}

			bool isFullName = originalNameWithoutSuffix.IndexOf('.') != -1;
			Component target;
			if(isFullName)
			{
				target = components.Where(c => c != null && string.Equals(c.GetType().FullName, originalNameWithoutSuffix)).ElementAtOrDefault(nth);
			}
			else
			{
				target = components.Where(c => c != null && string.Equals(c.GetType().Name, originalNameWithoutSuffix)).ElementAtOrDefault(nth);
			}

			if(target == null)
			{
				return originalPath;
			}

			overrideName = target.GetName();
			if(overrideName.EndsWith(" ()"))
			{
				overrideName = overrideName.Substring(0, overrideName.Length - 3);
			}

			int index = 0;
			string baseName = overrideName;
			while(cache.ContainsValue(overrideName))
			{
				index++;
				overrideName = baseName + " (" + index + ")";
			}
			cache.Add(originalName, overrideName);

			string overridePath = overrideName + originalPath.Substring(originalNameEnd);
			return overridePath;
		}

		private static bool BuildFunctionSelectDropdownMenuUsingInternalMethod(SerializedProperty listenerProperty, UnityEventBase dummyEvent, Object listenerTarget, out GenericMenu menu)
		{
			menu = default;

			if(listenerProperty == null)
			{
				#if DEV_MODE
				Debug.LogWarning("Listener SerializedProperty null.");
				#endif

				return false;
			}

			var buildPopupListMethod = typeof(UnityEventDrawer).GetMethod("BuildPopupList", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			if(buildPopupListMethod == null)
			{
				#if DEV_MODE
				Debug.LogWarning("Method UnityEventDrawer.BuildPopupList not found.");
				#endif

				return false;
			}

			var parameters = buildPopupListMethod.GetParameters();
			if(parameters.Length != 3
				|| parameters[0].ParameterType != typeof(Object)
				|| parameters[1].ParameterType != typeof(UnityEventBase)
				|| parameters[2].ParameterType != typeof(SerializedProperty))
			{
				#if DEV_MODE
				Debug.LogWarning("Method UnityEventDrawer.BuildPopupList parameter list was not expected.");
				#endif

				return false;
			}

			var args = new object[] { listenerTarget, dummyEvent, listenerProperty };
			try
			{
				menu = buildPopupListMethod.Invoke(null, args) as GenericMenu;
				if(menu is null)
				{
					#if DEV_MODE
					Debug.LogWarning("UnityEventDrawer.BuildPopupList return value was not castable to GenericMenu.");
					#endif
					return false;
				}

				return true;
			}
			#if DEV_MODE
			catch(Exception ex)
			{
				Debug.LogWarning(ex.ToString());
			#else
			catch
			{
			#endif

				return false;
			}
		}

		private static UnityEventBase GetDummyEvent(SerializedProperty unityEventProperty)
		{
			//Use the SerializedProperty path to iterate through the fields of the inspected targetObject
			Object targetObject = unityEventProperty.serializedObject.targetObject;
			if(targetObject == null)
			{
				return new UnityEvent();
			}

			var staticType = GetStaticTypeFromProperty(unityEventProperty);
			if(staticType.IsSubclassOf(typeof(UnityEventBase)))
			{
				return Activator.CreateInstance(staticType) as UnityEventBase;
			}
			return new UnityEvent();
		}

		private static Type GetStaticTypeFromProperty(SerializedProperty property)
		{
			var classType = GetScriptTypeFromProperty(property);
			if(classType == null)
			{
				return null;
			}

			var fieldPath = property.propertyPath;
			
			var isReferencingAManagedReferenceFieldProperty = typeof(SerializedProperty).GetProperty("isReferencingAManagedReferenceField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			bool isReferencingAManagedReference = (bool)isReferencingAManagedReferenceFieldProperty.GetValue(property, null);
			if(isReferencingAManagedReference)
			{
				// When the field we are trying to access is a dynamic instance, things are a bit more tricky
				// since we cannot "statically" (looking only at the parent class field types) know the actual
				// "classType" of the parent class.

				// The issue also is that at this point our only view on the object is the very limited SerializedProperty.

				// So we have to:
				// 1. try to get the FQN from for the current managed type from the serialized data,
				// 2. get the path *in the current managed instance* of the field we are pointing to,
				// 3. foward that to 'GetFieldInfoFromPropertyPath' as if it was a regular field,

				var objectTypenameMethod = typeof(SerializedProperty).GetMethod("GetFullyQualifiedTypenameForCurrentTypeTreeInternal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				var objectTypename = objectTypenameMethod.Invoke(property, null) as string;
				GetTypeFromManagedReferenceFullTypeName(objectTypename, out classType);

				var fieldPathMethod = typeof(SerializedProperty).GetMethod("GetPropertyPathInCurrentManagedTypeTreeInternal", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				fieldPath = fieldPathMethod.Invoke(property, null) as string;
			}

			if(classType == null)
			{
				return null;
			}

			return GetStaticTypeFromPropertyPath(classType, fieldPath);
		}

		private static bool GetTypeFromManagedReferenceFullTypeName(string managedReferenceFullTypename, out Type managedReferenceInstanceType)
        {
            managedReferenceInstanceType = null;

            var parts = managedReferenceFullTypename.Split(' ');
            if (parts.Length == 2)
            {
                var assemblyPart = parts[0];
                var nsClassnamePart = parts[1];
                managedReferenceInstanceType = Type.GetType($"{nsClassnamePart}, {assemblyPart}");
            }

            return managedReferenceInstanceType != null;
        }

		private static Type GetStaticTypeFromPropertyPath(Type host, string path)
		{
			const string arrayData = @"\.Array\.data\[[0-9]+\]";
			// we are looking for array element only when the path ends with Array.data[x]
			var lookingForArrayElement = Regex.IsMatch(path, arrayData + "$");
			// remove any Array.data[x] from the path because it is prevents cache searching.
			path = Regex.Replace(path, arrayData, ".___ArrayElement___");

			var type = host;
			string[] parts = path.Split('.');
			for(int i = 0; i < parts.Length; i++)
			{
				string member = parts[i];
				// GetField on class A will not find private fields in base classes to A,
				// so we have to iterate through the base classes and look there too.
				// Private fields are relevant because they can still be shown in the Inspector,
				// and that applies to private fields in base classes too.
				FieldInfo foundField = null;
				for(Type currentType = type; foundField == null && currentType != null; currentType = currentType.BaseType)
				{
					foundField = currentType.GetField(member, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}

				if(foundField == null)
				{
					return null;
				}

				type = foundField.FieldType;
				// we want to get the element type if we are looking for Array.data[x]
				if(i < parts.Length - 1 && parts[i + 1] == "___ArrayElement___" && type.IsArrayOrList())
				{
					i++; // skip the "___ArrayElement___" part
					type = type.GetArrayOrListElementType();
				}
			}

			// we want to get the element type if we are looking for Array.data[x]
			if(lookingForArrayElement && type != null && type.IsArrayOrList())
			{
				type = type.GetArrayOrListElementType();
			}


			return type;
		}

		private static bool IsArrayOrList(this Type listType)
		{
			if(listType.IsArray)
			{
				return true;
			}

			if(listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
			{
				return true;
			}

			return false;
		}

		private static Type GetArrayOrListElementType(this Type listType)
		{
			if(listType.IsArray)
			{
				return listType.GetElementType();
			}

			if(listType.IsGenericType && listType.GetGenericTypeDefinition() == typeof(List<>))
			{
				return listType.GetGenericArguments()[0];
			}

			return null;
		}

		private static Type GetScriptTypeFromProperty(SerializedProperty property)
		{
			if(property.serializedObject.targetObject != null)
			{
				return property.serializedObject.targetObject.GetType();
			}

			// Fallback in case the targetObject has been destroyed but the property is still valid.
			SerializedProperty scriptProp = property.serializedObject.FindProperty("m_Script");

			if(scriptProp == null)
			{
				return null;
			}

			MonoScript script = scriptProp.objectReferenceValue as MonoScript;

			if(script == null)
			{
				return null;
			}

			return script.GetClass();
		}

		public static GenericMenu BuildFunctionSelectDropdownMenu(Object target, UnityEventBase dummyEvent, SerializedProperty listener)
		{
			//special case for components... we want all the game objects targets there!
			var targetToUse = target;
			if(targetToUse is Component)
			{
				targetToUse = (target as Component).gameObject;
			}

			// find the current event target...
			var methodName = listener.FindPropertyRelative(kMethodNamePath);

			var menu = new GenericMenu();

			menu.AddItem
			(
				new GUIContent(kNoFunctionString),
				string.IsNullOrEmpty(methodName.stringValue),
				ClearEventFunction,
				new UnityEventFunction(listener, null, null, PersistentListenerMode.EventDefined)
			);

			if(targetToUse == null)
			{
				return menu;
			}

			menu.AddSeparator("");

			// figure out the signature of this delegate...
			// The property at this stage points to the 'container' and has the field name
			Type delegateType = dummyEvent.GetType();

			// check out the signature of invoke as this is the callback!
			MethodInfo delegateMethod = delegateType.GetMethod("Invoke");
			var delegateArgumentsTypes = delegateMethod.GetParameters().Select(x => x.ParameterType).ToArray();

			var duplicateNames = new Dictionary<string, int>();
			var duplicateFullNames = new Dictionary<string, int>();

			GeneratePopUpForType(menu, targetToUse, targetToUse.GetType().Name, listener, delegateArgumentsTypes);
			duplicateNames[targetToUse.GetType().Name] = 0;
			if(targetToUse is GameObject)
			{
				Component[] comps = (targetToUse as GameObject).GetComponents<Component>();

				// Collect all the names and record how many times the same name is used.
				foreach(Component comp in comps)
				{
					if(comp == null)
					{
						continue;
					}

					var componentTypeName = comp.GetType().Name;

					var duplicateIndex = 0;
					if(duplicateNames.TryGetValue(componentTypeName, out duplicateIndex))
					{
						duplicateIndex++;
					}

					duplicateNames[componentTypeName] = duplicateIndex;
				}

				foreach(Component comp in comps)
				{
					if(comp == null)
					{
						continue;
					}

					var compType = comp.GetType();
					string targetName = compType.Name;
					int duplicateIndex = 0;

					// Is this name used multiple times? If so then use the full name plus an index if there are also duplicates of this. (case 1309997)
					if(duplicateNames[compType.Name] > 0)
					{
						if(duplicateFullNames.TryGetValue(compType.FullName, out duplicateIndex))
						{
							targetName = $"{compType.FullName} ({duplicateIndex})";
						}
						else
						{
							targetName = compType.FullName;
						}
					}

					GeneratePopUpForType(menu, comp, targetName, listener, delegateArgumentsTypes);
					duplicateFullNames[compType.FullName] = duplicateIndex + 1;
				}
			}

			return menu;
		}

		private static void GeneratePopUpForType(GenericMenu menu, Object target, string targetName, SerializedProperty listener, Type[] delegateArgumentsTypes)
		{
			var methods = new List<ValidMethodMap>();
			bool didAddDynamic = false;

			// skip 'void' event defined on the GUI as we have a void prebuilt type!
			if(delegateArgumentsTypes.Length != 0)
			{
				GetMethodsForTargetAndMode(target, delegateArgumentsTypes, methods, PersistentListenerMode.EventDefined);
				if(methods.Count > 0)
				{
					menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " + string.Join(", ", delegateArgumentsTypes.Select(e => GetTypeName(e)).ToArray())));
					AddMethodsToMenu(menu, listener, methods, targetName);
					didAddDynamic = true;
				}
			}

			methods.Clear();
			GetMethodsForTargetAndMode(target, new[] { typeof(float) }, methods, PersistentListenerMode.Float);
			GetMethodsForTargetAndMode(target, new[] { typeof(int) }, methods, PersistentListenerMode.Int);
			GetMethodsForTargetAndMode(target, new[] { typeof(string) }, methods, PersistentListenerMode.String);
			GetMethodsForTargetAndMode(target, new[] { typeof(bool) }, methods, PersistentListenerMode.Bool);
			GetMethodsForTargetAndMode(target, new[] { typeof(Object) }, methods, PersistentListenerMode.Object);
			GetMethodsForTargetAndMode(target, new Type[] { }, methods, PersistentListenerMode.Void);
			if(methods.Count > 0)
			{
				if(didAddDynamic)
				{
					// AddSeperator doesn't seem to work for sub-menus, so we have to use this workaround instead of a proper separator for now.
					menu.AddItem(new GUIContent(targetName + "/ "), false, null);
				}

				if(delegateArgumentsTypes.Length != 0)
				{
					menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
				}

				AddMethodsToMenu(menu, listener, methods, targetName);
			}
		}

		private static void AddMethodsToMenu(GenericMenu menu, SerializedProperty listener, List<ValidMethodMap> methods, string targetName)
		{
			// Note: sorting by a bool in OrderBy doesn't seem to work for some reason, so using numbers explicitly.
			IEnumerable<ValidMethodMap> orderedMethods = methods.OrderBy(e => e.methodInfo.Name.StartsWith("set_") ? 0 : 1).ThenBy(e => e.methodInfo.Name);
			foreach(var validMethod in orderedMethods)
			{
				AddFunctionsForScript(menu, listener, validMethod, targetName);
			}
		}

		private static void GetMethodsForTargetAndMode(Object target, Type[] delegateArgumentsTypes, List<ValidMethodMap> methods, PersistentListenerMode mode)
		{
			IEnumerable<ValidMethodMap> newMethods = CalculateMethodMap(target, delegateArgumentsTypes, mode == PersistentListenerMode.Object);
			foreach(var m in newMethods)
			{
				var method = m;
				method.mode = mode;
				methods.Add(method);
			}
		}

		private static IEnumerable<ValidMethodMap> CalculateMethodMap(Object target, Type[] t, bool allowSubclasses)
		{
			var validMethods = new List<ValidMethodMap>();
			if(target == null || t == null)
			{
				return validMethods;
			}

			// find the methods on the behaviour that match the signature
			Type componentType = target.GetType();
			var componentMethods = componentType.GetMethods().Where(x => !x.IsSpecialName).ToList();

			var wantedProperties = componentType.GetProperties().AsEnumerable();
			wantedProperties = wantedProperties.Where(x => x.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length == 0 && x.GetSetMethod() != null);
			componentMethods.AddRange(wantedProperties.Select(x => x.GetSetMethod()));

			foreach(var componentMethod in componentMethods)
			{
				//Debug.Log ("Method: " + componentMethod);
				// if the argument length is not the same, no match
				var componentParamaters = componentMethod.GetParameters();
				if(componentParamaters.Length != t.Length)
				{
					continue;
				}

				// Don't show obsolete methods.
				if(componentMethod.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0)
				{
					continue;
				}

				if(componentMethod.ReturnType != typeof(void))
				{
					continue;
				}

				// if the argument types do not match, no match
				bool paramatersMatch = true;
				for(int i = 0; i < t.Length; i++)
				{
					if(!componentParamaters[i].ParameterType.IsAssignableFrom(t[i]))
					{
						paramatersMatch = false;
					}

					if(allowSubclasses && t[i].IsAssignableFrom(componentParamaters[i].ParameterType))
					{
						paramatersMatch = true;
					}
				}

				// valid method
				if(paramatersMatch)
				{
					var vmm = new ValidMethodMap
					{
						target = target,
						methodInfo = componentMethod
					};

					validMethods.Add(vmm);
				}
			}
			return validMethods;
		}

		private static void AddFunctionsForScript(GenericMenu menu, SerializedProperty listener, ValidMethodMap method, string targetName)
		{
			PersistentListenerMode mode = method.mode;

			// find the current event target...
			var listenerTarget = listener.FindPropertyRelative(kInstancePath).objectReferenceValue;
			var methodName = listener.FindPropertyRelative(kMethodNamePath).stringValue;
			var setMode = GetMode(listener.FindPropertyRelative(kModePath));
			var typeName = listener.FindPropertyRelative(kArgumentsPath).FindPropertyRelative(kObjectArgumentAssemblyTypeName);

			var args = new StringBuilder();
			var count = method.methodInfo.GetParameters().Length;
			for(int index = 0; index < count; index++)
			{
				var methodArg = method.methodInfo.GetParameters()[index];
				args.Append(string.Format("{0}", GetTypeName(methodArg.ParameterType)));

				if(index < count - 1)
				{
					args.Append(", ");
				}
			}

			var isCurrentlySet = listenerTarget == method.target
				&& methodName == method.methodInfo.Name
				&& mode == setMode;

			if(isCurrentlySet && mode == PersistentListenerMode.Object && method.methodInfo.GetParameters().Length == 1)
			{
				isCurrentlySet &= (method.methodInfo.GetParameters()[0].ParameterType.AssemblyQualifiedName == typeName.stringValue);
			}

			string path = GetFormattedMethodName(targetName, method.methodInfo.Name, args.ToString(), mode == PersistentListenerMode.EventDefined);
			menu.AddItem
			(
				new GUIContent(path),
				isCurrentlySet,
				SetEventFunction,
				new UnityEventFunction(listener, method.target, method.methodInfo, mode)
			);
		}

		private static PersistentListenerMode GetMode(SerializedProperty mode)
		{
			return (PersistentListenerMode)mode.enumValueIndex;
		}

		private static string GetTypeName(Type t)
		{
			if(t == typeof(int))
				return "int";
			if(t == typeof(float))
				return "float";
			if(t == typeof(string))
				return "string";
			if(t == typeof(bool))
				return "bool";
			return t.Name;
		}

		private static string GetFormattedMethodName(string targetName, string methodName, string args, bool dynamic)
		{
			if(dynamic)
			{
				if(methodName.StartsWith("set_"))
					return string.Format("{0}/{1}", targetName, methodName.Substring(4));
				else
					return string.Format("{0}/{1}", targetName, methodName);
			}
			else
			{
				if(methodName.StartsWith("set_"))
					return string.Format("{0}/{2} {1}", targetName, methodName.Substring(4), args);
				else
					return string.Format("{0}/{1} ({2})", targetName, methodName, args);
			}
		}

		private static void SetEventFunction(object source)
		{
			((UnityEventFunction)source).Assign();
		}

		private static void ClearEventFunction(object source)
		{
			((UnityEventFunction)source).Clear();
		}

		private struct UnityEventFunction
		{
			private readonly SerializedProperty listener;
			private readonly Object target;
			private readonly MethodInfo method;
			private readonly PersistentListenerMode mode;

			public UnityEventFunction(SerializedProperty listener, Object target, MethodInfo method, PersistentListenerMode mode)
			{
				this.listener = listener;
				this.target = target;
				this.method = method;
				this.mode = mode;
			}

			public void Assign()
			{
				// find the current event target...
				var listenerTarget = listener.FindPropertyRelative(kInstancePath);
				var listenerTargetType = listener.FindPropertyRelative(kInstanceTypePath);
				var methodName = listener.FindPropertyRelative(kMethodNamePath);
				var mode = listener.FindPropertyRelative(kModePath);
				var arguments = listener.FindPropertyRelative(kArgumentsPath);

				listenerTarget.objectReferenceValue = target;
				listenerTargetType.stringValue = method.DeclaringType.AssemblyQualifiedName;
				methodName.stringValue = method.Name;
				mode.enumValueIndex = (int)this.mode;

				if(this.mode == PersistentListenerMode.Object)
				{
					var fullArgumentType = arguments.FindPropertyRelative(kObjectArgumentAssemblyTypeName);
					var argParams = method.GetParameters();
					if(argParams.Length == 1 && typeof(Object).IsAssignableFrom(argParams[0].ParameterType))
						fullArgumentType.stringValue = argParams[0].ParameterType.AssemblyQualifiedName;
					else
						fullArgumentType.stringValue = typeof(Object).AssemblyQualifiedName;
				}

				ValidateObjectParamater(arguments, this.mode);

				listener.serializedObject.ApplyModifiedProperties();
			}

			private void ValidateObjectParamater(SerializedProperty arguments, PersistentListenerMode mode)
			{
				var fullArgumentType = arguments.FindPropertyRelative(kObjectArgumentAssemblyTypeName);
				var argument = arguments.FindPropertyRelative(kObjectArgument);
				var argumentObj = argument.objectReferenceValue;

				if(mode != PersistentListenerMode.Object)
				{
					fullArgumentType.stringValue = typeof(Object).AssemblyQualifiedName;
					argument.objectReferenceValue = null;
					return;
				}

				if(argumentObj == null)
					return;

				Type t = Type.GetType(fullArgumentType.stringValue, false);
				if(!typeof(Object).IsAssignableFrom(t) || !t.IsInstanceOfType(argumentObj))
					argument.objectReferenceValue = null;
			}

			public void Clear()
			{
				// find the current event target...
				var methodName = listener.FindPropertyRelative(kMethodNamePath);
				methodName.stringValue = null;

				var mode = listener.FindPropertyRelative(kModePath);
				mode.enumValueIndex = (int)PersistentListenerMode.Void;

				listener.serializedObject.ApplyModifiedProperties();
			}
		}
	}

	struct ValidMethodMap
	{
		public Object target;
		public MethodInfo methodInfo;
		public PersistentListenerMode mode;
	}
}