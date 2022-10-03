/*
#if UNITY_2018_2_OR_NEWER // Editor.finishedDefaultHeaderGUI doesn't exist in Unity versions older than 2018.2
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using Pancake.Init.Internal;

namespace Pancake.Init.EditorOnly
{
	[InitializeOnLoad]
	internal static class ReferenceableLabelDrawer
	{
		private static readonly GUIContent label = new GUIContent("Referenceable");

		static ReferenceableLabelDrawer()
		{
			Editor.finishedDefaultHeaderGUI -= OnAfterGameObjectHeaderGUI;
			Editor.finishedDefaultHeaderGUI += OnAfterGameObjectHeaderGUI;
		}

		private static void OnAfterGameObjectHeaderGUI(Editor editor)
		{
			if(editor == null)
			{
				return;
			}

			var targets = editor.targets;
			if(targets.Length == 0)
			{
				return;
			}

			var gameObject = targets[0] as GameObject;
			if(gameObject == null)
			{
				return;
			}

			if(!gameObject.TryGetComponent(out Referenceable referenceable))
			{
				return;
			}

			var width = Styles.ServiceTag.CalcSize(label).x;
			string guid = referenceable.Guid.ToString();
			label.tooltip = guid;
			if(GUILayout.Button(label, Styles.ServiceTag, GUILayout.Width(width)))
			{
				if(Event.current.button == 1)
				{
					var menu = new GenericMenu();
					menu.AddItem(new GUIContent("Find All References"), false, FindAllReferences);
					menu.ShowAsContext();
				}
			}

			void FindAllReferences()
			{
				var gameObjects = new List<GameObject>();
				var components = new List<Component>();

				// TODO: Also loop through all ScriptableObjects, prefabs... maybe as a separate menu item.
				foreach(string sceneAssetGuid in AssetDatabase.FindAssets("t:SceneAsset"))
				{
					bool leaveSceneOpen = true;

					string sceneAssetPath = AssetDatabase.GUIDToAssetPath(sceneAssetGuid);
					var scene = SceneManager.GetSceneByPath(sceneAssetPath);
					if(!scene.IsValid())
					{
						leaveSceneOpen = false;
						scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Additive);
					}

					scene.GetRootGameObjects(gameObjects);
					foreach(var gameObject in gameObjects)
					{
						gameObject.GetComponentsInChildren(components);
						foreach(var component in components)
						{
							if(component == null)
							{
								continue;
							}

							FindAllReferencesInTarget(component, ref leaveSceneOpen);
						}

						components.Clear();
					}

					gameObjects.Clear();

					if(!leaveSceneOpen)
					{
						EditorSceneManager.CloseScene(scene, true);
					}
				}
			}

			void FindAllReferencesInTarget(Object target, ref bool found)
			{
				for(var componentType = target.GetType(); !TypeUtility.IsNullOrBaseType(componentType); componentType = componentType.BaseType)
				{
					foreach(var field in componentType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
					{
						FindAllReferencesInField(target, field, ref found);
					}
				}
			}

			void FindAllReferencesInField(Object target, FieldInfo field, ref bool found)
			{
				var fieldType = field.FieldType;
				if(!fieldType.IsGenericType)
				{
					return;
				}

				var typeDefinition = fieldType.GetGenericTypeDefinition();
				if(typeDefinition == typeof(Any<>))
				{
					var valueType = fieldType.GetGenericArguments()[0];
					if(!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(Ref<>))
					{
						return;
					}

					var any = field.GetValue(target);
					var args = new object[] {valueType, Context.MainThread, null};
					bool hasValue = (bool)any.GetType().GetMethod(nameof(Any<object>.TryGetValue)).Invoke(any, args);
					if(!hasValue)
					{
						return;
					}
									
					var @ref = args[2];
					if(guid == (string)@ref.GetType().GetProperty(nameof(Ref<Object>.Guid)).GetValue(@ref, null))
					{
						found = true;
						LogReference(target, field);
						return;
					}
				}
								
				if(typeDefinition == typeof(Ref<>))
				{
					var @ref = field.GetValue(target);
					if(guid == (string)@ref.GetType().GetProperty(nameof(Ref<Object>.Guid)).GetValue(@ref, null))
					{
						found = true;
						LogReference(target, field);
						return;
					}
				}
			}

			void LogReference(Object obj, FieldInfo field)
			{
				if(obj is Component component)
				{
					Debug.Log($"'{field.Name}' on component '{component.GetType().Name}' on GameObject '{component.name}' in scene '{component.gameObject.scene.name}'.", component);
				}
				else
				{
					Debug.Log($"'{field.Name}' on '{obj.GetType().Name}' at '{AssetDatabase.GetAssetPath(obj)}'.", obj);
				}
			}
		}
	}
}
#endif
*/