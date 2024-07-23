using Sisus.Init.Internal;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly
{
	public sealed class CrossSceneReferenceGUI : IDisposable
	{
		private static GUIContent sceneIcon;

		private readonly Type objectType;
		private SerializedObject serializedObject;
		private SerializedProperty guidProperty;
		private SerializedProperty targetProperty;
		private SerializedProperty targetNameProperty;
		private SerializedProperty globalObjectIdSlowProperty;
		private SerializedProperty sceneAssetProperty;
		private SerializedProperty sceneNameProperty;
		private SerializedProperty sceneOrAssetGuidProperty;
		private SerializedProperty isCrossSceneProperty;
		private SerializedProperty iconProperty;

		public CrossSceneReferenceGUI(Type objectType) => this.objectType = objectType;

		public void OnGUI(Rect position, SerializedProperty referenceProperty, GUIContent label)
		{
			referenceProperty.serializedObject.Update();

			var crossSceneReference = referenceProperty.objectReferenceValue;
			if(crossSceneReference == null)
			{
				EditorGUI.PropertyField(position, referenceProperty, label);
				return;
			}

			if(serializedObject == null || serializedObject.targetObject != crossSceneReference)
			{
				serializedObject = new SerializedObject(crossSceneReference);
			}
			else
			{
				serializedObject.Update();
			}

			Id guid;
			Object target;
			string targetName;
			string globalObjectIdSlow;
			SceneAsset sceneAsset;
			string sceneName;
			string sceneOrAssetGuid;
			bool isCrossScene;
			Texture icon;

			guidProperty = serializedObject.FindProperty(nameof(guid));
			targetProperty = serializedObject.FindProperty(nameof(target));
			targetNameProperty = serializedObject.FindProperty(nameof(targetName));
			globalObjectIdSlowProperty = serializedObject.FindProperty(nameof(globalObjectIdSlow));
			sceneAssetProperty = serializedObject.FindProperty(nameof(sceneAsset));
			sceneNameProperty = serializedObject.FindProperty(nameof(sceneName));
			sceneOrAssetGuidProperty = serializedObject.FindProperty(nameof(sceneOrAssetGuid));
			isCrossSceneProperty = serializedObject.FindProperty(nameof(isCrossScene));
			iconProperty = serializedObject.FindProperty(nameof(icon));

			target = targetProperty.objectReferenceValue;
			targetName = targetNameProperty.stringValue;
			globalObjectIdSlow = globalObjectIdSlowProperty.stringValue;
			sceneAsset = sceneAssetProperty.objectReferenceValue as SceneAsset;
			sceneName = sceneNameProperty.stringValue;
			sceneOrAssetGuid = sceneOrAssetGuidProperty.stringValue;
			isCrossScene = isCrossSceneProperty.boolValue;
			icon = iconProperty.objectReferenceValue as Texture;

			if(sceneIcon == null)
			{
				sceneIcon = new GUIContent(EditorGUIUtility.IconContent("SceneAsset Icon"));
			}

			EditorGUI.BeginProperty(position, label, targetProperty);

			// In older versions of Unity an error will occur if GlobalObjectId.GlobalObjectIdentifierToObjectSlow
			// is called and the target object is in an unloaded scene.
			bool isSceneLoaded = IsSceneLoaded();
			if(isSceneLoaded && target == null && !string.IsNullOrEmpty(globalObjectIdSlow)
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
			if(target != null || AssignableObjectIsBeingDraggedOver(position, objectType) || !isCrossScene)
			{
				target = EditorGUI.ObjectField(position, label, target, objectType, true);
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

				// How to allow assigning null? Pass in a fake reference of some sort?
				// But then can't pass in objectType? Need to just use typeof(Object).
				// I guess it's still the best route, since "null" is such a common choice?
				// Or do something like PI's ObjectPicker class and ObjectReferenceDrawer?
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
				clipRect.height -= 4f;
				GUI.BeginClip(clipRect);
				overlayRect.x = -2f;
				overlayRect.y = -2f;
				if(isCrossScene && icon != null)
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

			if(isCrossScene && icon != null)
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
				#if DEV_MODE
				Debug.LogWarning($"Value changed from {targetWas} to {target}.");
				#endif

				targetProperty.objectReferenceValue = target;
			}

			EditorGUI.EndProperty();

			EditorSceneManager.preventCrossSceneReferences = preventCrossSceneReferencesWas;

			if(!valueChanged)
			{
				return;
			}

			if(target == null)
			{
				#if DEV_MODE
				Debug.LogWarning("Object reference is null. Resetting Ref...");
				#endif

				guidProperty.FindPropertyRelative("a").intValue = default;
				guidProperty.FindPropertyRelative("b").intValue = default;
				guidProperty.FindPropertyRelative("c").intValue = default;
				guidProperty.FindPropertyRelative("d").intValue = default;
				guidProperty.FindPropertyRelative("e").intValue = default;
				guidProperty.FindPropertyRelative("f").intValue = default;
				guidProperty.FindPropertyRelative("g").intValue = default;
				guidProperty.FindPropertyRelative("h").intValue = default;
				guidProperty.FindPropertyRelative("i").intValue = default;
				guidProperty.FindPropertyRelative("j").intValue = default;
				guidProperty.FindPropertyRelative("k").intValue = default;
				targetNameProperty.stringValue = "";
				globalObjectIdSlowProperty.stringValue = new GlobalObjectId().ToString();
				isCrossSceneProperty.boolValue = false;
				sceneOrAssetGuidProperty.stringValue = "";
				sceneNameProperty.stringValue = "";
				iconProperty.objectReferenceValue = null;
			}
			else
			{
				var targetGameObject = GetGameObject(target);
				var targetScene = targetGameObject == null ? default : targetGameObject.scene;
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

				string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
				for(int i = 0, count = SceneManager.sceneCount; i < count; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if(scene.isLoaded && string.Equals(scene.path, scenePath))
					{
						return true;
					}
				}

				return false;
			}
		}

		private void OnSceneIconClicked(SceneAsset sceneAsset, bool isSceneLoaded)
		{
			if(sceneAsset == null)
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
			void SelectTarget() => Selection.activeGameObject = this == null ? null : GetGameObject(targetProperty?.objectReferenceValue);
		}

		private static bool AssignableObjectIsBeingDraggedOver(Rect position, Type requiredType)
        {
			if(!position.Contains(Event.current.mousePosition) || DragAndDrop.visualMode == DragAndDropVisualMode.None)
			{
				return false;
			}

			if(DragAndDrop.objectReferences.Length <= 0)
			{
				return false;
			}

			var draggedObject = DragAndDrop.objectReferences[0];
			if(draggedObject == null)
			{
				return false;
			}

			if(requiredType.IsInstanceOfType(draggedObject))
			{
				return true;
			}

			if(draggedObject is GameObject gameObject && requiredType != typeof(GameObject) && requiredType != typeof(Object) && requiredType != typeof(object))
			{
				foreach(var component in gameObject.GetComponentsNonAlloc<Component>())
                {
					if(requiredType.IsInstanceOfType(component))
                    {
						return true;
                    }
                }

				return false;
            }

			return false;
        }

		public void Dispose() => serializedObject.Dispose();

		private static Scene GetScene(Object target) => target is Component component && component != null ? component.gameObject.scene : target is GameObject gameObject && gameObject != null ? gameObject.scene : default;
		private static GameObject GetGameObject(Object target) => target is Component component && component != null ? component.gameObject : target as GameObject;
	}
}