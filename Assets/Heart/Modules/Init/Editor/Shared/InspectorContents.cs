using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sisus.Init.EditorOnly
{
	public static class InspectorContents
	{
		private static readonly IList allInspectors = null;
		private static readonly List<EditorWindow> allPropertyEditors = new List<EditorWindow>();
		private static readonly FieldInfo inspectorElementEditorField;
		private static readonly Type propertyEditorType;
		private static bool shouldUpdatePropertyEditors = true;

		private static IEnumerable<EditorWindow> AllEditorWindows
		{
			get
			{
				foreach(var inspector in AllInspectors)
				{
					yield return inspector;
				}

				foreach(var propertyEditors in AllPropertyEditors)
				{
					yield return propertyEditors;
				}
			}
		}

		private static IEnumerable<EditorWindow> AllInspectors
		{
			get
			{
				foreach(var inspector in allInspectors)
				{
					yield return inspector as EditorWindow;
				}
			}
		}

		private static List<EditorWindow> AllPropertyEditors
		{
			get
			{
				if(shouldUpdatePropertyEditors)
				{
					shouldUpdatePropertyEditors = false;
					UpdateAllPropertyEditors();
				}
				else
				{
					var focusedWindow = EditorWindow.focusedWindow;
					if(focusedWindow != null && focusedWindow.GetType() == propertyEditorType && !allPropertyEditors.Contains(focusedWindow))
					{
						allPropertyEditors.Add(focusedWindow);
					}

					var mouseOverWindow = EditorWindow.mouseOverWindow;
					if(mouseOverWindow != null && mouseOverWindow.GetType() == propertyEditorType && !allPropertyEditors.Contains(mouseOverWindow))
					{
						allPropertyEditors.Add(mouseOverWindow);
					}
				}

				return allPropertyEditors;
			}
		}

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
				allInspectors = new List<EditorWindow>(GetAllInspectorWindowsFallback());
				return;
			}

			FetchAllInspectors(out allInspectors);

			propertyEditorType = unityEditorAssembly.GetType("UnityEditor.PropertyEditor");
			
			void FetchAllInspectors(out IList allInspectors)
			{
				var allInspectorsField = inspectorType.GetField("m_AllInspectors", BindingFlags.Static | BindingFlags.NonPublic);
				if(allInspectorsField is null)
				{
					#if DEV_MODE
					Debug.LogWarning($"Field m_AllInspectors was not found on class UnityEditor.InspectorWindow.");
					#endif
					allInspectors = new List<EditorWindow>(GetAllInspectorWindowsFallback());
					return;
				}

				allInspectors = allInspectorsField.GetValue(null) as IList;
			}
		}

		public static void Repaint()
		{
			foreach(var inspector in AllInspectors)
			{
				inspector.Repaint();
			}

			foreach(var propertyEditor in AllPropertyEditors)
			{
				propertyEditor.Repaint();
			}
		}

		internal static IEnumerable<(Editor, IMGUIContainer)> GetComponentHeaderElementsFromEditorWindowOf(Editor gameObjectEditor)
		{
			var inspector = GetGameObjectEditorWindow(gameObjectEditor);
			if(inspector == null)
			{
				return Enumerable.Empty<(Editor, IMGUIContainer)>();
			}

			return GetAllComponentHeaderElements(inspector);
		}

		internal static (Editor, IMGUIContainer)? GetComponentHeaderElementFromPropertyEditorOf(Editor componentEditor)
		{
			var propertyEditor = GetComponentPropertyEditor(componentEditor);
			return propertyEditor == null ? default : GetFirstComponentHeaderElement(propertyEditor.rootVisualElement);
		}

		private static EditorWindow GetGameObjectEditorWindow(Editor gameObjectEditor)
		{
			foreach(var window in AllEditorWindows)
			{
				if(ContainsGameObjectEditor(window.rootVisualElement, gameObjectEditor))
				{
					return window;
				}
			}

			return null;
		}

		private static EditorWindow GetComponentPropertyEditor(Editor componentEditor)
		{
			for(int i = AllPropertyEditors.Count - 1; i >= 0; i--)
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

		private static IEnumerable<(Editor, IMGUIContainer)> GetAllComponentHeaderElements(EditorWindow inspector)
		{
			return GetAllComponentHeaderElements(inspector.rootVisualElement);
		}

		private static bool ContainsGameObjectEditor(VisualElement parentElement, Editor gameObjectEditor)
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
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;
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

		private static IEnumerable<(Editor, IMGUIContainer)> GetAllComponentHeaderElements(VisualElement parentElement)
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
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;

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

		private static (Editor editor, IMGUIContainer header)? GetFirstComponentHeaderElement(VisualElement parentElement)
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
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;

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

		private static void UpdateAllPropertyEditors()
		{
			allPropertyEditors.Clear();
			allPropertyEditors.AddRange(Resources.FindObjectsOfTypeAll(propertyEditorType) as EditorWindow[]);
		}
	}
}