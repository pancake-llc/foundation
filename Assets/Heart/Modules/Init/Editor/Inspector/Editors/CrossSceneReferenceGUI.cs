#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Sisus.Init.EditorOnly.Internal;
using Sisus.Init.Internal;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using static Sisus.NullExtensions;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Responsible for drawing GUI for <see cref="CrossSceneReference"/>. 
	/// </summary>
	internal sealed class CrossSceneReferenceGUI : IDisposable
	{
		private static GUIContent sceneIcon;

		private readonly Type objectType;
		private readonly Type anyType;
		private SerializedObject serializedObject;
		private SerializedProperty targetProperty;
		private SerializedProperty targetNameProperty;
		private SerializedProperty globalObjectIdSlowProperty;
		private SerializedProperty sceneAssetProperty;
		private SerializedProperty sceneNameProperty;
		private SerializedProperty sceneOrAssetGuidProperty;
		private SerializedProperty isCrossSceneProperty;
		private SerializedProperty iconProperty;

		public CrossSceneReferenceGUI(Type objectType)
		{
			this.objectType = objectType;
			anyType = typeof(Any<>).MakeGenericType(objectType);
		}

		/// <summary>
		/// Draw GUI for <see cref="CrossSceneReference"/> or <see cref="CrossSceneRef"/>.
		/// </summary>
		public void OnGUI(Rect position, SerializedProperty referenceProperty, GUIContent label)
		{
			UpdateSerializedObjects(referenceProperty, ref serializedObject);

			var crossSceneReference = GetCrossSceneReferenceObject(referenceProperty);
			if(crossSceneReference == Null)
			{
				EditorGUI.PropertyField(position, referenceProperty, label);
				return;
			}

			Object target;
			string targetName;
			string globalObjectIdSlow;
			SceneAsset sceneAsset;
			string sceneName;
			string sceneOrAssetGuid;
			bool isCrossScene;
			Texture icon;

			targetProperty = serializedObject.FindProperty(nameof(CrossSceneReference.target));
			targetNameProperty = serializedObject.FindProperty(nameof(CrossSceneReference.targetName));
			globalObjectIdSlowProperty = serializedObject.FindProperty(nameof(CrossSceneReference.globalObjectIdSlow));
			sceneAssetProperty = serializedObject.FindProperty(nameof(CrossSceneReference.sceneAsset));
			sceneNameProperty = serializedObject.FindProperty(nameof(CrossSceneReference.sceneName));
			sceneOrAssetGuidProperty = serializedObject.FindProperty(nameof(CrossSceneReference.sceneOrAssetGuid));
			isCrossSceneProperty = serializedObject.FindProperty(nameof(CrossSceneReference.isCrossScene));
			iconProperty = serializedObject.FindProperty(nameof(CrossSceneReference.icon));

			target = targetProperty.objectReferenceValue;
			targetName = targetNameProperty.stringValue;
			globalObjectIdSlow = globalObjectIdSlowProperty.stringValue;
			sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;
			sceneName = sceneNameProperty.stringValue;
			sceneOrAssetGuid = sceneOrAssetGuidProperty.stringValue;
			isCrossScene = isCrossSceneProperty.boolValue;
			icon = iconProperty.objectReferenceValue as Texture;

			sceneIcon ??= new(EditorGUIUtility.IconContent("SceneAsset Icon"));

			EditorGUI.BeginProperty(position, label, targetProperty);

			// In older versions of Unity an error will occur if GlobalObjectId.GlobalObjectIdentifierToObjectSlow
			// is called and the target object is in an unloaded scene.
			bool isSceneLoaded = IsSceneLoaded();
			if(isSceneLoaded && !target && !string.IsNullOrEmpty(globalObjectIdSlow)
				&& GlobalObjectId.TryParse(globalObjectIdSlow, out GlobalObjectId globalObjectId))
			{
				target = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
				targetProperty.objectReferenceValue = target;
			}

			const float iconOffset = 2f;
			float iconWidth = EditorGUIUtility.singleLineHeight;
			Rect iconRect = position;
			if(isCrossScene)
			{
				position = EditorGUI.PrefixLabel(position, label);

				float remainingWidth = position.width;
				iconRect = position;

				iconRect.width = iconWidth;
				remainingWidth -= iconWidth + iconOffset;
				sceneIcon.tooltip = "In Scene '" + sceneName + "'";

				GUI.Label(iconRect, sceneIcon);
				if(Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition))
				{
					OnSceneIconClicked(sceneAsset, isSceneLoaded);
					Event.current.Use();
				}

				position.x += iconWidth + iconOffset;
				position.width = remainingWidth;
				label = GUIContent.none;
			}

			bool preventCrossSceneReferencesWas = EditorSceneManager.preventCrossSceneReferences;
			EditorSceneManager.preventCrossSceneReferences = false;
			bool isContainingSceneMissing = !string.IsNullOrEmpty(sceneOrAssetGuid) && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(sceneOrAssetGuid));

			var targetWas = target;

			bool draggedObjectIsAssignable;
			Type objectFieldConstraint;

			bool dragging = DragAndDrop.objectReferences.Length > 0;
			if(dragging && InitializerEditorUtility.TryGetAssignableType(DragAndDrop.objectReferences[0], referenceProperty.serializedObject.targetObject, anyType, objectType, out Type assignableType))
			{
				draggedObjectIsAssignable = true;
				objectFieldConstraint = assignableType;
			}
			else
			{
				draggedObjectIsAssignable = false;
				var objectReferenceValue = referenceProperty.objectReferenceValue;
				bool hasObjectReferenceValue = objectReferenceValue;
				var objectReferenceValueType = hasObjectReferenceValue ? objectReferenceValue.GetType() : null;

				if(typeof(Object).IsAssignableFrom(objectType) || objectType.IsInterface)
				{
					objectFieldConstraint = !hasObjectReferenceValue || objectType.IsAssignableFrom(objectReferenceValueType) ? objectType : typeof(Object);
				}
				else
				{
					var valueProviderType = typeof(IValueProvider<>).MakeGenericType(objectType);
					objectFieldConstraint = !hasObjectReferenceValue || valueProviderType.IsAssignableFrom(objectReferenceValueType) ? valueProviderType : typeof(Object);
				}
			}

			if(target || draggedObjectIsAssignable || !isCrossScene)
			{
				OverrideObjectPicker(position, referenceProperty, objectType);
				target = EditorGUI.ObjectField(position, label, target, objectFieldConstraint, true);
			}
			else
			{
				Rect overlayRect = position;
				overlayRect.width -= 19f;

				Rect clickToPingRect = position;
				clickToPingRect.width -= EditorGUIUtility.singleLineHeight;
				if(Event.current.type == EventType.MouseDown && clickToPingRect.Contains(Event.current.mousePosition))
				{
					OnSceneIconClicked(sceneAsset, isSceneLoaded);
					Event.current.Use();
				}

				OverrideObjectPicker(position, referenceProperty, objectType);
				target = EditorGUI.ObjectField(position, label, target, objectType, true);

				var targetLabel = new GUIContent(targetName);
				if(targetLabel.text.Length == 0)
				{
					targetLabel.text = " ";
				}

				targetLabel.tooltip = isContainingSceneMissing ? "Containing Scene Not Found" : !isSceneLoaded ? "Containing Scene not loaded" : "Missing Target";
				var clipRect = overlayRect;
				clipRect.x += 2f;
				clipRect.y += 2f;
				clipRect.width -= 4f;
				clipRect.height -= 3f;
				GUI.BeginClip(clipRect);
				overlayRect.x = -2f;
				overlayRect.y = -2f;
				if(isCrossScene && icon)
				{
					EditorGUI.LabelField(overlayRect, GUIContent.none, GUIContent.none, EditorStyles.objectField);
					float roomForIcon = 14f + 1f;
					clipRect.x += roomForIcon;
					clipRect.width -= roomForIcon;
					overlayRect.width -= roomForIcon;
					GUI.EndClip();
					GUI.BeginClip(clipRect);
					overlayRect.x = -3f;
				}

				EditorGUI.LabelField(overlayRect, label, targetLabel, EditorStyles.objectField);
				GUI.EndClip();
			}

			if(isCrossScene && icon)
			{
				iconRect.x += iconWidth + iconOffset;
				iconRect.x += 2f;
				iconRect.y += 2f;
				iconRect.width = 14f;
				iconRect.height = 14f;
				if(GUI.Button(iconRect, icon, EditorStyles.label))
				{
					OnSceneIconClicked(sceneAsset, isSceneLoaded);
				}
			}

			bool valueChanged = targetWas != target;

			if(valueChanged)
			{
				#if DEV_MODE && DEBUG_ENABLED
				Debug.Log($"CrossSceneReferenceGUI: value changed from {targetWas} to {target}.");
				#endif

				targetProperty.objectReferenceValue = target;
				targetProperty.serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.EndProperty();

			EditorSceneManager.preventCrossSceneReferences = preventCrossSceneReferencesWas;

			if(!valueChanged)
			{
				return;
			}

			if(!target)
			{
				#if DEV_MODE
				Debug.LogWarning($"Object reference has changed from {targetWas} to Null. targetName: '{targetName}'.");
				#endif

				referenceProperty.objectReferenceValue = null;
				referenceProperty.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				var targetGameObject = GetGameObject(target);
				var targetScene = !targetGameObject ? default : targetGameObject.scene;
				targetName = target.name;
				if(referenceProperty.type != "GameObject")
				{
					targetName += " (" + referenceProperty.type + ")";
				}

				#if DEV_MODE
				Debug.LogWarning($"Object reference has changed from {targetWas} to {target}. targetName: '{targetName}'.");
				#endif

				targetNameProperty.stringValue = targetName;
				sceneAssetProperty.objectReferenceValue = targetScene.IsValid() ? AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScene.path) : null;
				globalObjectIdSlowProperty.stringValue = GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
				isCrossScene = targetScene.IsValid() && targetScene != GetScene(referenceProperty.serializedObject.targetObject);
				isCrossSceneProperty.boolValue = isCrossScene;
				sceneOrAssetGuidProperty.stringValue = targetScene.IsValid() ? AssetDatabase.AssetPathToGUID(targetScene.path) : "";
				sceneNameProperty.stringValue = targetScene.name;
				iconProperty.objectReferenceValue = EditorGUIUtility.ObjectContent(target, target.GetType()).image; ;
				if(!isCrossScene)
				{
					referenceProperty.objectReferenceValue = target;
					referenceProperty.serializedObject.ApplyModifiedProperties();
				}
			}

			serializedObject.ApplyModifiedProperties();
			referenceProperty.serializedObject.ApplyModifiedProperties();

			bool IsSceneLoaded()
			{
				if(!isCrossScene)
				{
					return true;
				}

				string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
				for(int i = 0, count = SceneManager.sceneCount; i < count; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if(scene.isLoaded && string.Equals(scene.path, assetPath))
					{
						return true;
					}
				}

				return false;
			}
		}

		private static object GetCrossSceneReferenceObject(SerializedProperty crossSceneReferenceProperty) => crossSceneReferenceProperty.GetValue();

		private static void UpdateSerializedObjects(SerializedProperty crossSceneReferenceProperty, ref SerializedObject serializedObject)
		{
			crossSceneReferenceProperty.serializedObject.Update();

			if(crossSceneReferenceProperty.propertyType is not SerializedPropertyType.ObjectReference)
			{
				serializedObject = crossSceneReferenceProperty.serializedObject;
				return;
			}

			var crossSceneReferenceScriptableObject = crossSceneReferenceProperty.objectReferenceValue;
			if(!crossSceneReferenceScriptableObject)
			{
				serializedObject = null;
				return;
			}

			if(serializedObject == null || serializedObject.targetObject != crossSceneReferenceScriptableObject)
			{
				serializedObject = new(crossSceneReferenceScriptableObject);
			}
			else
			{
				serializedObject.Update();
			}
		}

		[Conditional("UNITY_2022_3"), Conditional("UNITY_2023_2_OR_NEWER")]
		public static void OverrideObjectPicker(Rect objectFieldRect, SerializedProperty referenceProperty, Type objectType)
		{
			#if UNITY_2022_3 || UNITY_2023_2_OR_NEWER
			var objectPickerRect = objectFieldRect;
			objectPickerRect.width = 19f;
			objectPickerRect.x += objectFieldRect.width - objectPickerRect.width;
			if(GUI.Button(objectPickerRect, GUIContent.none, EditorStyles.label))
			{
				ShowObjectPicker(type:objectType, referenceProperty:referenceProperty);
			}
			#endif
		}

		#if UNITY_2022_3 || UNITY_2023_2_OR_NEWER
		private static void ShowObjectPicker
		(
			Type type,
			SerializedProperty referenceProperty,
			//Action<Object> selectHandler,
			SearchFlags flags = SearchFlags.None,
			string searchText = "",
			float defaultWidth = 850f,
			float defaultHeight = 539f
		)
		{
			#if DEV_MODE
			Debug.Log(TypeUtility.ToString(type));
			#endif

			var filterTypes = Find.typesToFindableTypes.TryGetValue(type, out var types) ? types : new []{ typeof(Object) };
			var providers = new List<SearchProvider>();
			if(filterTypes.Any(t => typeof(Component).IsAssignableFrom(t) || t == typeof(GameObject)))
			{
				providers.Add(SearchService.GetProvider("scene"));
			}

			if(filterTypes.Any(t => !typeof(Component).IsAssignableFrom(t) && t != typeof(GameObject)))
			{
				providers.Add(SearchService.GetProvider("asset"));
			}

			var context = SearchService.CreateContext(providers, searchText, flags | SearchFlags.FocusContext);
			var typeName = TypeUtility.ToStringNicified(type);
			var pickerState = SearchViewState.CreatePickerState
			(
				title:"Select " + typeName,
				context:context,
				selectObjectHandler:OnSelectionConfirmed,
				trackingObjectHandler:OnSelectionChanged,
				typeName:typeName,
				filterType:GetSharedBaseType(filterTypes)
			);

			pickerState.position = new(0f, 0f, defaultWidth, defaultHeight);
			SearchService.ShowPicker(pickerState);

			void OnSelectionConfirmed(Object target, bool flag) => OnSelectionChanged(target);

			void OnSelectionChanged(Object target)
			{
				if(target is not null && !type.IsInstanceOfType(target))
				{
					if(!Find.In(GetGameObject(target), type, out var obj))
					{
						#if DEV_MODE
						Debug.LogWarning($"ObjectPicker selected object of type {target.GetType().Name} which is not assignable to {type.Name}. Setting target to null.");
						#endif
						target = null;
					}
					else if(obj is Object unityObject)
					{
						#if DEV_MODE
						Debug.Log($"Converting selection from {target.GetType().Name} to {unityObject.GetType().Name}.");
						#endif
						target = unityObject;
					}
					else if(Find.WrapperOf(obj) is Object wrapper)
					{
						#if DEV_MODE
						Debug.Log($"Converting selection from {target.GetType().Name} to {wrapper.GetType().Name}.");
						#endif
						target = wrapper;
					}
					else
					{
						#if DEV_MODE
						Debug.LogWarning($"ObjectPicker selected object of type {target.GetType().Name} which is not an Object. Setting target to null.");
						#endif
						target = null;
					}
				}

				if(target == referenceProperty.objectReferenceValue)
				{
					return;
				}

				#if DEV_MODE
				Debug.Log($"Value changed from {referenceProperty.objectReferenceValue} to {target}.");
				#endif

				referenceProperty.objectReferenceValue = target;
				referenceProperty.serializedObject.ApplyModifiedProperties();
			}

			static Type GetSharedBaseType(Type[] types)
			{
				int count = types.Length;
				if(count == 0)
				{
					return typeof(Object);
				}

				var result = types[0];

				for(int i = count - 1; i >= 1; i--)
				{
					if(types[i].IsAssignableFrom(result))
					{
						result = types[i];
						continue;
					}

					while(!result.IsAssignableFrom(types[i]))
					{
						result = result.BaseType;
					}
				}

				return result == typeof(object) ? typeof(Object) : result;
			}
		}
		#endif

		private void OnSceneIconClicked(SceneAsset sceneAsset, bool isSceneLoaded)
		{
			if(!sceneAsset)
			{
				return;
			}

			switch(Event.current.button)
			{
				case 0 when Event.current.clickCount == 2:
					if(!isSceneLoaded)
					{
						OpenScene();
					}
					EditorApplication.delayCall += SelectTarget;
					return;
				case 0:
					EditorGUIUtility.PingObject(sceneAsset);
					return;
				case 1:
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Ping Scene"), false, () => EditorGUIUtility.PingObject(sceneAsset));
					if(!isSceneLoaded)
					{
						menu.AddItem(new GUIContent("Load Scene"), false, OpenScene);
					}
					menu.ShowAsContext();
					return;
			}

			void OpenScene() => EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset), OpenSceneMode.Additive);
			void SelectTarget() => Selection.activeGameObject = GetGameObject(targetProperty?.objectReferenceValue);
		}

		public void Dispose() => serializedObject.Dispose();

		private static Scene GetScene(Object target) => target is Component component && component ? component.gameObject.scene : target is GameObject gameObject && gameObject ? gameObject.scene : default;
		private static GameObject GetGameObject(Object target) => target is Component component && component ? component.gameObject : target as GameObject;
	}
}