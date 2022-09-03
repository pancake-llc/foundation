using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Pancake.Init;
using Pancake.Init.Internal;
using Pancake.Init.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor.Init
{
	internal static class InitializerEditorUtility
	{
		private static readonly GUIContent clientNullTooltip = new GUIContent("", "A new Instance will be added to this GameObject during initialization.");
		private static readonly GUIContent clientPrefabTooltip = new GUIContent("", "A new Instance will be created by cloning this prefab during initialization.");
		private static readonly GUIContent clientInstantiateTooltip = new GUIContent("", "A new Instance will be created by cloning this scene object during initialization.");
		private static readonly GUIContent clientNotInitializableTooltip = new GUIContent("", "Can not inject arguments to client because it does not implement IInitializable.");
		private static readonly GUIContent serviceLabel = new GUIContent("Service", "An Instance of this service will be automatically provided during initialization.");
		private static readonly GUIContent valueText = new GUIContent("Value");
		private static readonly GUIContent blankLabel = new GUIContent(" ");
		private static GUIContent warningIcon;
		private static GUIContent prefabIcon;
		private static GUIContent gameObjectIcon;
		private static GUIContent scriptableObjectIcon;

		private static Color ObjectFieldBackgroundColor => EditorGUIUtility.isProSkin ? new Color32(42, 42, 42, 255) : new Color32(237, 237, 237, 255);

		private static readonly Dictionary<Type, Dictionary<Type, bool>> isAssignableCaches = new Dictionary<Type, Dictionary<Type, bool>>();
		private static int lastDraggedObjectCount = 0;

		/// <summary>
		/// Draws Init argument field of an Initializer.
		/// </summary>
		/// <param name="anyProperty"> The <see cref="Any{T}"/> type field that holds the argument. </param>
		/// <param name="label"> Label for the Init argument field. </param>
		/// <param name="customDrawer">
		/// Custom property drawer that was defined for the field via a PropertyAttribute added to a field
		/// on an Init class nested inside the Initializer.
		/// <para>
		/// <see langword="null"/> if no custom drawer has been defined, in which case <see cref="AnyDrawer"/>
		/// is used to draw the field instead.
		/// </para>
		/// </param>
		/// <param name="isService">
		/// Is the argument a service?
		/// <para>
		/// If <see langword="true"/> then the field is drawn as a service tag.
		/// </para>
		/// </param>
		/// <param name="canBeNull">
		/// Is the argument allowed to be null or not?
		/// <para>
		/// If <see langword="false"/>, then the field will be tinted red if it has a null value.
		/// </para>
		/// </param>
		internal static void DrawArgumentField(SerializedProperty anyProperty, Type argumentType, GUIContent label, [CanBeNull] PropertyDrawer customDrawer, bool isService, bool canBeNull)
		{
			// Repaint whenever dragged object references change because
			// the controls can change in reaction to objects being dragged.
			if(lastDraggedObjectCount != DragAndDrop.objectReferences.Length)
			{
				GUI.changed = true;
				lastDraggedObjectCount = DragAndDrop.objectReferences.Length;
			}

			var any = anyProperty.GetValue();
			Type anyType = any.GetType();
			var targetObject = anyProperty.serializedObject.targetObject;
			bool hasSerializedValue = (bool)anyType.GetMethod("HasSerializedValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(any, null);

			if(isService && !hasSerializedValue && !IsDraggingObjectReferenceThatIsAssignableToProperty())
			{
				var totalRect = EditorGUILayout.GetControlRect();
				var controlRect = EditorGUI.PrefixLabel(totalRect, blankLabel);
				controlRect.width = Styles.ServiceTag.CalcSize(serviceLabel).x;

				ServiceTagUtility.Draw(controlRect, () => ServiceTagUtility.Ping(argumentType, anyProperty.serializedObject.targetObject as Component));
				
				var labelRect = totalRect;
				labelRect.width -= controlRect.width;
				GUI.Label(labelRect, label);
				return;
			}

			if(anyProperty == null)
			{
				GUI.color = Color.red;
				EditorGUILayout.LabelField(label.text, "NULL");
				GUI.color = Color.white;
				return;
			}

			bool tintValueRed = !canBeNull && !(bool)anyType.GetMethod(nameof(Any<object>.GetHasValue)).MakeGenericMethod(new Type[] { targetObject.GetType() }).Invoke(any, new object[] { targetObject, Context.MainThread });
			if(tintValueRed)
			{
				if(customDrawer != null)
				{
					DrawTintedRed(DrawUsingCustomPropertyDrawer);
				}
				else
				{
					DrawTintedRed(DrawUsingDefaultDrawer);
				}
			}
			else if(customDrawer != null)
			{
				DrawUsingCustomPropertyDrawer();
			}
			else
			{
				DrawUsingDefaultDrawer();
			}

			anyProperty.serializedObject.ApplyModifiedProperties();

			bool IsDraggingObjectReferenceThatIsAssignableToProperty()
			{
				if(DragAndDrop.objectReferences.Length == 0)
				{
					return false;
				}

				if(!isAssignableCaches.TryGetValue(argumentType, out var isAssignableCache))
				{
					isAssignableCache = new Dictionary<Type, bool>();
					isAssignableCaches.Add(argumentType, isAssignableCache);
				}

				var anyType = typeof(Any<>).MakeGenericType(argumentType);
				return AnyDrawer.TryGetAssignableTypeFromDraggedObject(DragAndDrop.objectReferences[0], anyType, argumentType, isAssignableCache, out _);
			}

			void DrawUsingCustomPropertyDrawer()
			{
				var valueProperty = anyProperty.FindPropertyRelative("value");
				if(valueProperty == null)
				{
					valueProperty = anyProperty.FindPropertyRelative("reference");
				}

				float height = customDrawer.GetPropertyHeight(valueProperty, label);
				if(height < 0f)
				{
					height = EditorGUIUtility.singleLineHeight;
				}
				var rect = EditorGUILayout.GetControlRect(GUILayout.Height(height));
				customDrawer.OnGUI(rect, valueProperty, label);
			}

			void DrawUsingDefaultDrawer() => EditorGUILayout.PropertyField(anyProperty, label);

			void DrawTintedRed(Action drawControl)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(label);

				GUI.color = Color.red;
				int indentLevelWas = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				label = GUIContent.none;
				drawControl();

				EditorGUI.indentLevel = indentLevelWas;
				GUI.color = Color.white;

				GUILayout.EndHorizontal();
			}
		}

		internal static void DrawArgumentField(SerializedProperty argument, GUIContent label, Type type, bool isService, bool isUnityObject, bool canBeNull, FieldInfo fieldInfo)
		{
			int indentLevelWas = EditorGUI.indentLevel;

			if(isService)
			{
				var labelRect = EditorGUILayout.GetControlRect();
				
				var controlRect = EditorGUI.PrefixLabel(labelRect, blankLabel);
				
				GUI.enabled = false;
				EditorGUI.indentLevel = 0;
				GUI.Label(controlRect, "Service");
				EditorGUI.indentLevel = indentLevelWas;
				GUI.enabled = true;

				labelRect.width -= controlRect.width;
				GUI.Label(labelRect, label);
				return;
			}

			if(argument == null)
			{
				GUI.color = Color.red;
				EditorGUILayout.LabelField(label.text, "NULL");
				GUI.color = Color.white;
				return;
			}

			if(isUnityObject)
			{
				GUI.color = argument.objectReferenceValue == null && !canBeNull ? Color.red : Color.white;
				EditorGUILayout.ObjectField(argument, type, label);
				GUI.color = Color.white;
			}
			else if(argument.propertyType != SerializedPropertyType.ManagedReference)
			{
				EditorGUILayout.PropertyField(argument, label, true);
			}
			else
			{
				var rect = EditorGUILayout.GetControlRect();
				var buttonRect = EditorGUI.PrefixLabel(rect, label);

				DrawArgumentFieldTypeSelectorButton(buttonRect, argument, type);

				var typeOfValue = GetType(argument, fieldInfo);
				if(typeOfValue is null)
                {
					return;
                }
				else if(TypeUtility.IsSerializableByUnity(type))
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(argument, valueText, true);
					EditorGUI.indentLevel--;
				}
				else
				{
					EditorGUILayout.HelpBox(new GUIContent(typeOfValue.Name + " is missing the [Serializable] attribute."));
				}
			}

			argument.serializedObject.ApplyModifiedProperties();
		}

		private static Type GetType(SerializedProperty argument, FieldInfo fieldInfo)
		{
			var initializer = argument.serializedObject.targetObject;
			object value = fieldInfo.GetValue(initializer);
			return value is null ? null : value.GetType();
		}

		internal static IEnumerable<Type> GetInitializerTypes(Type clientType)
		{
			foreach(var type in TypeCache.GetTypesDerivedFrom(typeof(IInitializer)))
			{
				if(IsInitializerFor(type, clientType))
				{
					yield return type;
				}
			}
		}

		private static bool IsInitializerFor(Type initializerType, Type clientType)
		{
			if(initializerType.IsAbstract)
			{
				return false;
			}

			for(var baseType = initializerType; baseType != null; baseType = baseType.BaseType)
			{
				if(!baseType.IsGenericType)
				{
					continue;
				}

				var arguments = baseType.GetGenericArguments();
				if(arguments[0] == clientType)
				{
					return true;
				}
			}

			return false;
		}

		internal static Type GetInitializerType(SerializedObject serializedObject)
        {
			var initializerType = serializedObject.targetObject.GetType().BaseType;
            while(!initializerType.IsGenericType || (!initializerType.Name.StartsWith("Initializer") && !initializerType.Name.StartsWith("WrapperInitializer")))
            {
                initializerType = initializerType.BaseType;
            }
			return initializerType;
		}

		private static void DrawArgumentFieldTypeSelectorButton(Rect buttonRect, SerializedProperty argument, Type type)
		{
			string selectedTypeName = "";
			if(argument.hasMultipleDifferentValues)
			{
				bool showMixedValueWas = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = false;
				if(!EditorGUI.DropdownButton(buttonRect, new GUIContent(""), FocusType.Keyboard))
				{
					return;
				}
				EditorGUI.showMixedValue = showMixedValueWas;
			}
			else
			{
				selectedTypeName = argument.type;
				if(selectedTypeName.StartsWith("managedReference<"))
				{
					selectedTypeName = selectedTypeName.Substring(17, selectedTypeName.Length - 18);
				}

				string buttonLabel = selectedTypeName.Length == 0 ? "Null" : ObjectNames.NicifyVariableName(selectedTypeName);
				if(!EditorGUI.DropdownButton(buttonRect, new GUIContent(buttonLabel), FocusType.Keyboard))
				{
					return;
				}
			}

			var menu = new GenericMenu();

			menu.AddItem(new GUIContent("Null"), selectedTypeName.Length == 0, () => SetArgumentType(argument, null));

			if(!type.IsAbstract)
            {
				string typeName = type.Name;
				GUIContent itemLabel = new GUIContent(ObjectNames.NicifyVariableName(typeName), type.FullName);
				bool isSelected = string.Equals(typeName, selectedTypeName);
				menu.AddItem(itemLabel, isSelected, () => SetArgumentType(argument, type));
				menu.DropDown(buttonRect);
				return;
			}

			bool hasItems = false;
			foreach(Type derivedType in TypeCache.GetTypesDerivedFrom(type))
            {
				if(derivedType.IsAbstract || typeof(Object).IsAssignableFrom(derivedType) || derivedType.IsGenericTypeDefinition)
                {
					continue;
                }

				string derivedTypeName = derivedType.Name;
				GUIContent itemLabel = new GUIContent(ObjectNames.NicifyVariableName(derivedTypeName), derivedType.FullName);
				bool isSelected = string.Equals(derivedTypeName, selectedTypeName);
				menu.AddItem(itemLabel, isSelected, ()=> SetArgumentType(argument, derivedType));

				hasItems = true;
			}

			if(hasItems)
			{
				menu.DropDown(buttonRect);
			}
        }

		private static void SetArgumentType(SerializedProperty argument, Type type)
        {
			argument.managedReferenceValue = type == null ? null : CreateInstance(type);
			argument.serializedObject.ApplyModifiedProperties();
		}

		internal static object CreateInstance(Type type)
        {
			try
			{
				return Activator.CreateInstance(type);
			}
			catch(Exception)
			{
				return FormatterServices.GetUninitializedObject(type);
			}
        }

		internal static bool CanAssignUnityObjectToField(Type type)
		{
			if(typeof(Object).IsAssignableFrom(type))
			{
				return true;
			}

			if(!type.IsInterface)
			{
				return false;
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

		internal static void DrawClientField(Rect rect, SerializedProperty client, GUIContent clientLabel, bool isInitializable)
		{
			rect.y += 5f;
			rect.width -= 28f;
			var fieldRect = EditorGUI.PrefixLabel(rect, new GUIContent(" "));

			var reference = client.objectReferenceValue;

			EditorGUI.ObjectField(fieldRect, client, GUIContent.none);

			bool mouseovered = rect.Contains(Event.current.mousePosition) && DragAndDrop.visualMode != DragAndDropVisualMode.None;

			fieldRect.x += 2f;
			fieldRect.width -= 21f;
			fieldRect.y += 2f;
			fieldRect.height -= 3f;

			if(!isInitializable && !mouseovered)
			{
				GUI.color = Color.red;
			}

			if(reference == null)
			{
				if(mouseovered)
				{
					return;
				}

				clientLabel.tooltip = isInitializable ? clientNullTooltip.text : clientNotInitializableTooltip.text;

				EditorGUI.DrawRect(fieldRect, ObjectFieldBackgroundColor);
				GUI.Label(fieldRect, clientLabel);
			}
			else
			{
				Component component = reference as Component;
				var gameObject = component != null ? component.gameObject : null;
				bool isPrefab = gameObject != null && !gameObject.scene.IsValid();
				bool isSceneObject = gameObject != null && gameObject.scene.IsValid();
				bool isScriptableObject = reference is ScriptableObject;

				GUIContent icon;
				if(!isInitializable)
				{
					if(warningIcon == null)
					{
						warningIcon = EditorGUIUtility.IconContent("Warning");
					}
					icon = warningIcon;
				}
				else if(isPrefab)
				{
					if(prefabIcon == null)
					{
						prefabIcon = EditorGUIUtility.IconContent("Prefab Icon");
					}
					icon = prefabIcon;
				}
				else if(isSceneObject)
				{
					if(gameObjectIcon == null)
					{
						gameObjectIcon = EditorGUIUtility.IconContent("GameObject Icon");
					}
					icon = gameObjectIcon;
				}
				else if(isScriptableObject)
				{
					if(scriptableObjectIcon == null)
					{
						scriptableObjectIcon = EditorGUIUtility.IconContent("ScriptableObject Icon");
					}
					icon = scriptableObjectIcon;
				}
				else
				{
					icon = GUIContent.none;
				}

				string tooltip = GetReferenceTooltip(client.serializedObject.targetObject, reference, isInitializable).tooltip;
				clientLabel.tooltip = tooltip;
				icon.tooltip = tooltip;

				GUI.Label(fieldRect, new GUIContent("", clientLabel.tooltip));

				var iconSize = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(15f, 15f));
				
				var iconRect = fieldRect;
				iconRect.y -= 4f;
				iconRect.x -= 22f;
				iconRect.width = 20f;
				iconRect.height = 20f;
				if(GUI.Button(iconRect, icon, EditorStyles.label))
				{
					EditorGUIUtility.PingObject(gameObject != null ? gameObject : reference);
				}

				EditorGUIUtility.SetIconSize(iconSize);
			}

			GUI.color = Color.white;
		}

		internal static GUIContent GetReferenceTooltip(Object objectWithField, Object reference, bool isInitializable)
		{
			if(!isInitializable)
			{
				return clientNotInitializableTooltip;
			}

			if(reference == null)
			{
				return clientNullTooltip;
			}

			Component component = reference as Component;
			if(component == null)
			{
				return GUIContent.none;
			}

			var gameObject = component.gameObject;
			if(gameObject == null)
			{
				return GUIContent.none;
			}

			var gameObjectWithField = objectWithField is Component componentWithField ? componentWithField.gameObject : null;
			if(gameObjectWithField == null || gameObjectWithField == gameObject)
			{
				return GUIContent.none;
			}

			bool isPrefab = !gameObject.scene.IsValid();
			return isPrefab ? clientPrefabTooltip : clientInstantiateTooltip;
		}

		internal static GUIContent GetLabel([NotNull] Type type)
		{
			if(!(type.GetCustomAttribute<AddComponentMenu>() is AddComponentMenu addComponentMenu))
			{
				return GetLabel(TypeUtility.ToString(type));
			}

			string menuPath = addComponentMenu.componentMenu;
			if(string.IsNullOrEmpty(menuPath))
			{
				return GetLabel(TypeUtility.ToString(type));
			}

			int nameStart = menuPath.LastIndexOf('/') + 1;
			return new GUIContent(nameStart <= 0 ? menuPath : menuPath.Substring(nameStart));
		}

		internal static GUIContent GetLabel(string typeOfFieldName)
		{
			typeOfFieldName = ObjectNames.NicifyVariableName(typeOfFieldName);
			return new GUIContent(typeOfFieldName.StartsWith("I ") ? typeOfFieldName.Substring(2) : typeOfFieldName);
		}

		private static IEnumerable<PropertyAttribute> GetPropertyAttributes(Type metadataClass, Type argumentType)
		{
			if(TryGetArgumentTargetFieldName(metadataClass, argumentType, out string fieldName))
            {
                var field = metadataClass.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return field.GetCustomAttributes<PropertyAttribute>();
            }

			return Array.Empty<PropertyAttribute>();
		}

		internal static bool TryGetAttributeBasedPropertyDrawer([NotNull] Type metadataClass, [NotNull] SerializedProperty serializedProperty, [NotNull] Type argumentType, out PropertyDrawer propertyDrawer)
		{
			foreach(var propertyAttribute in GetPropertyAttributes(metadataClass, argumentType))
			{
				if(TryGetAttributeBasedPropertyDrawer(serializedProperty, propertyAttribute, out propertyDrawer))
				{
					return true;
				}
			}

			propertyDrawer = null;
			return false;
		}

		internal static bool TryGetAttributeBasedPropertyDrawer([NotNull] SerializedProperty serializedProperty, [CanBeNull] PropertyAttribute propertyAttribute, out PropertyDrawer propertyDrawer)
		{
			if(!TryGetDrawerType(propertyAttribute, out Type drawerType))
			{
				propertyDrawer = null;
				return false;
			}

			propertyDrawer = CreateInstance(drawerType) as PropertyDrawer;

			if(propertyDrawer == null)
			{
				return false;
			}

			if(propertyAttribute != null)
			{
				var attributeField = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
				#if UNITY_2019_1_OR_NEWER && ENABLE_UI_SUPPORT
				//var element = propertyDrawer.CreatePropertyGUI(serializedProperty);
				#endif
				attributeField.SetValue(propertyDrawer, propertyAttribute);
			}

			if(serializedProperty.GetMemberInfo() is FieldInfo fieldInfo)
			{
				typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(propertyAttribute, fieldInfo);
			}

			return true;
		}

		private static bool TryGetDrawerType([NotNull] PropertyAttribute propertyAttribute, out Type drawerType)
		{
			var propertyAttributeType = propertyAttribute.GetType();
			var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
			var useForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.NonPublic | BindingFlags.Instance);
			drawerType = null;

			foreach(var propertyDrawerType in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
			{
				var attribute = propertyDrawerType.GetCustomAttribute<CustomPropertyDrawer>();
				var targetType = typeField.GetValue(attribute) as Type;
				if(targetType == propertyAttributeType)
				{
					drawerType = propertyDrawerType;
					return true;
				}

				if(targetType.IsAssignableFrom(propertyAttributeType) && (bool)useForChildrenField.GetValue(attribute))
				{
					drawerType = propertyDrawerType;
				}
			}

			return drawerType != null;
		}

		internal static GUIContent GetArgumentLabel(Type clientType, Type parameterType)
		{
			if(TryGetArgumentTargetMember(clientType, parameterType, out var member))
			{
				var label = GetLabel(member);
				if(member.GetCustomAttribute<TooltipAttribute>() is TooltipAttribute tooltip)
				{
					label.tooltip = tooltip.tooltip;
				}
				return label;
			}
			
			return GetLabel(parameterType);
		}

		internal static GUIContent GetLabel([NotNull] MemberInfo member)
		{
			var label = GetLabel(member.Name);
			label.tooltip = GetTooltip(member);
			return label;
		}

		internal static string GetTooltip(MemberInfo member) => member.GetCustomAttribute<TooltipAttribute>() is TooltipAttribute tooltip ? tooltip.tooltip : "";

		internal static bool TryGetArgumentTargetMember(Type clientType, Type parameterType, out MemberInfo member)
			=> InjectionUtility.TryGetConstructorArgumentTarget(clientType, parameterType, parameterType.Name, out member);

		internal static bool TryGetArgumentTargetFieldName(Type clientType, Type parameterType, out string targetFieldName)
		{
			if(InjectionUtility.TryGetConstructorArgumentTarget(clientType, parameterType, parameterType.Name, out var member))
			{
				targetFieldName = member.Name;
				return true;
			}

			targetFieldName = null;
			return false;
		}

		internal static bool IsInitializable(Type clientType) => GetInitArgumentCount(clientType) > 0;

		internal static int GetInitArgumentCount(Type clientType)
		{
			if(typeof(IOneArgument).IsAssignableFrom(clientType))
			{
				return 1;
			}

			if(typeof(ITwoArguments).IsAssignableFrom(clientType))
			{
				return 2;
			}

			if(typeof(IThreeArguments).IsAssignableFrom(clientType))
			{
				return 3;
			}

			if(typeof(IFourArguments).IsAssignableFrom(clientType))
			{
				return 4;
			}

			if(typeof(IFiveArguments).IsAssignableFrom(clientType))
			{
				return 5;
			}

			return 0;
		}

        internal static bool IsService(Type type) => ServiceUtility.IsDefiningTypeOfAnyServiceAttribute(type);
	}
}