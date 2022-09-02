using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Editor
{
	public static class InspectorContents
	{
		private static readonly List<EditorWindow> allInspectors = null;
		private static readonly List<EditorWindow> allPropertyEditors = new List<EditorWindow>();
		private static readonly FieldInfo inspectorElementEditorField;

		static InspectorContents()
		{
			inspectorElementEditorField = typeof(InspectorElement).GetField("m_Editor", BindingFlags.Instance | BindingFlags.NonPublic);

			var unityEditorAssembly = typeof(EditorWindow).Assembly;
			
			var inspectorType = unityEditorAssembly.GetType("UnityEditor.InspectorWindow");

			if(inspectorType is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"Type UnityEditor.InspectorWindow was not found in assembly {unityEditorAssembly.GetName().Name}.");
				#endif
				allInspectors.AddRange(GetAllInspectorWindowsFallback());
				return;
			}

			var allInspectorsField = inspectorType.GetField("m_AllInspectors", BindingFlags.Static | BindingFlags.NonPublic);
			if(allInspectorsField is null)
			{
				#if DEV_MODE
				Debug.LogWarning($"Field m_AllInspectors was not found on class UnityEditor.InspectorWindow.");
				#endif
				allInspectors.AddRange(GetAllInspectorWindowsFallback());
				return;
			}

			allInspectors = new List<EditorWindow>();
			foreach(var inspectorWindow in allInspectorsField.GetValue(null) as IList)
			{
				allInspectors.Add(inspectorWindow as EditorWindow);
			}
		}

		public static void Repaint()
		{
			foreach(var inspector in allInspectors)
			{
				inspector.Repaint();
			}
		}

		internal static IEnumerable<(UnityEditor.Editor, IMGUIContainer)> GetComponentHeaderElementsFromInspector(UnityEditor.Editor gameObjectEditor)
		{
			var inspector = GetGameObjectEditorInspector(gameObjectEditor);
			if(inspector == null)
			{
				return Enumerable.Empty<(UnityEditor.Editor, IMGUIContainer)>();
			}

			return GetAllComponentHeaderElements(inspector);
		}

		internal static (UnityEditor.Editor, IMGUIContainer)? GetComponentHeaderElementFromPropertyEditorOf(UnityEditor.Editor componentEditor)
		{
			var propertyEditor = GetPropertyEditorWindow(componentEditor);
			return propertyEditor == null ? default : GetFirstComponentHeaderElement(propertyEditor.rootVisualElement);
		}

		private static EditorWindow GetGameObjectEditorInspector(UnityEditor.Editor gameObjectEditor)
		{
			foreach(var inspector in allInspectors)
			{
				if(ContainsGameObjectEditor(inspector.rootVisualElement, gameObjectEditor))
				{
					return inspector;
				}
			}

			return null;
		}

		private static EditorWindow GetPropertyEditorWindow(UnityEditor.Editor componentEditor)
		{
			var focusedWindow = EditorWindow.focusedWindow;
			if(focusedWindow != null && focusedWindow.GetType().Name == "PropertyEditor" && !allPropertyEditors.Contains(focusedWindow))
			{
				allPropertyEditors.Add(focusedWindow);
			}

			var mouseOverWindow = EditorWindow.mouseOverWindow;
			if(mouseOverWindow != null && mouseOverWindow.GetType().Name == "PropertyEditor" && !allPropertyEditors.Contains(mouseOverWindow))
			{
				allPropertyEditors.Add(mouseOverWindow);
			}

			for(int i = allPropertyEditors.Count - 1; i >= 0; i--)
			{
				var propertyEditor = allPropertyEditors[i];
				if(propertyEditor == null)
				{
					allPropertyEditors.RemoveAt(i);
					continue;
				}
				if(ContainsGameObjectEditor(propertyEditor.rootVisualElement, componentEditor))
				{
					return propertyEditor;
				}
			}

			return null;
		}

		private static IEnumerable<(UnityEditor.Editor, IMGUIContainer)> GetAllComponentHeaderElements(EditorWindow inspector)
		{
			return GetAllComponentHeaderElements(inspector.rootVisualElement);
		}

		private static bool ContainsGameObjectEditor(VisualElement parentElement, UnityEditor.Editor gameObjectEditor)
		{
			if(parentElement is null)
			{
				return false;
			}

			if(parentElement.GetType().Name == "EditorElement")
			{
				foreach(var child in parentElement.Children())
				{
					if(child is InspectorElement inspectorElement)
					{
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as UnityEditor.Editor;
						if(editor == gameObjectEditor)
						{
							return true;
						}

						if(editor != null && editor.target as GameObject != null)
						{
							return false;
						}
					}
				}
			}

			foreach(var child in parentElement.Children())
			{
				if(ContainsGameObjectEditor(child, gameObjectEditor))
				{
					return true;
				}
			}

			return false;
		}

		private static IEnumerable<(UnityEditor.Editor, IMGUIContainer)> GetAllComponentHeaderElements(VisualElement parentElement)
		{
			if(parentElement is null)
			{
				yield break;
			}

			if(parentElement.GetType().Name == "EditorElement")
			{
				IMGUIContainer header = null;
				foreach(var child in parentElement.Children())
				{
					if(TryGetComponentHeaderElement(child, out var headerOrNull))
					{
						header = headerOrNull;
					}
					else if(header is null)
					{
						continue;
					}

					if(child is InspectorElement inspectorElement)
					{
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as UnityEditor.Editor;

						if(editor != null && editor.target as Component != null)
						{
							yield return (editor, header);
						}

						break;
					}
				}
			}

			foreach(var child in parentElement.Children())
			{
				foreach(var childHeader in GetAllComponentHeaderElements(child))
				{
					yield return childHeader;
				}
			}
		}

		private static (UnityEditor.Editor, IMGUIContainer)? GetFirstComponentHeaderElement(VisualElement parentElement)
		{
			if(parentElement is null)
			{
				return null;
			}

			if(parentElement.GetType().Name == "EditorElement")
			{
				IMGUIContainer header = null;
				foreach(var child in parentElement.Children())
				{
					if(TryGetComponentHeaderElement(child, out var headerOrNull))
					{
						header = headerOrNull;
					}
					else if(header is null)
					{
						continue;
					}

					if(child is InspectorElement inspectorElement)
					{
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as UnityEditor.Editor;

						if(editor != null && editor.target as Component != null)
						{
							return (editor, header);
						}
						break;
					}
				}
			}

			foreach(var child in parentElement.Children())
			{
				var result = GetFirstComponentHeaderElement(child);
				if(result.HasValue)
				{
					return result;
				}
			}

			return null;
		}

		private static bool TryGetComponentHeaderElement(VisualElement visualElement, out IMGUIContainer header)
		{
			if(visualElement is IMGUIContainer imguiContainer && visualElement.name.EndsWith("Header", StringComparison.Ordinal))
			{
				header = imguiContainer;
				return true;
			}

			header = default;
			return false;
		}

		private static IEnumerable<EditorWindow> GetAllInspectorWindowsFallback()
			=> Resources.FindObjectsOfTypeAll<EditorWindow>().Where(window => window.GetType().Name == "InspectorWindow");
	}
}