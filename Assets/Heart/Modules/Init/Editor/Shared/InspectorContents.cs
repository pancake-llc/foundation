using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
		private static readonly List<EditorWindow> allPropertyEditors = new();
		private static readonly FieldInfo inspectorElementEditorField;
		private static readonly Type propertyEditorType;
		private static bool shouldUpdatePropertyEditors = true;

		private static IEnumerable<EditorWindow> AllEditorWindows
		{
			get
			{
				foreach(var inspector in allInspectors)
				{
					yield return inspector as EditorWindow;
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
					allPropertyEditors.Clear();
					foreach(var window in Resources.FindObjectsOfTypeAll(propertyEditorType))
					{
						if(window.GetType() == propertyEditorType)
						{
							allPropertyEditors.Add((EditorWindow)window);
						}
					}
				}
				else
				{
					var focusedWindow = EditorWindow.focusedWindow;
					if(focusedWindow && focusedWindow.GetType() == propertyEditorType && !allPropertyEditors.Contains(focusedWindow))
					{
						allPropertyEditors.Add(focusedWindow);
					}

					var mouseOverWindow = EditorWindow.mouseOverWindow;
					if(mouseOverWindow && mouseOverWindow.GetType() == propertyEditorType && !allPropertyEditors.Contains(mouseOverWindow))
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

		private static readonly List<List<(Editor, IMGUIContainer)>> listOfListOfHeaders = new();
		private static readonly List<(Editor, IMGUIContainer)> listOfHeaders = new();
		private static readonly Stack<List<(Editor, IMGUIContainer)>> listOfHeadersPool = new();

		internal static List<List<(Editor, IMGUIContainer)>> GetComponentHeaderElementsFromInspectorWindows(Editor gameObjectEditor)
		{
			foreach(var list in listOfListOfHeaders)
			{
				list.Clear();
				listOfHeadersPool.Push(list);
			}

			listOfListOfHeaders.Clear();

			foreach(EditorWindow inspectorWindow in allInspectors)
			{
				if(TryFindEditorElement(inspectorWindow.rootVisualElement, gameObjectEditor, out var gameObjectEditorElement)
				// All EditorElements should be parented under the same editors list element
				&& gameObjectEditorElement.parent is { } editorsListElement)
				{
					var headers = listOfHeadersPool.Count > 0 ? listOfHeadersPool.Pop() : new List<(Editor, IMGUIContainer)>();
					GetAllComponentHeaderElements(editorsListElement, headers);
					if(headers.Count > 0)
					{
						listOfListOfHeaders.Add(headers);
					}
					else
					{
						listOfHeadersPool.Push(headers);
					}
				}
			}

			return listOfListOfHeaders;
		}

		internal static List<(Editor, IMGUIContainer)> GetComponentHeaderElementFromPropertyEditorWindows(Editor componentEditor)
		{
			listOfHeaders.Clear();

			for(int i = AllPropertyEditors.Count - 1; i >= 0; i--)
			{
				var propertyEditor = allPropertyEditors[i];
				if(!propertyEditor)
				{
					allPropertyEditors.RemoveAt(i);
					continue;
				}

				if(TryFindInspectorElement(propertyEditor.rootVisualElement, componentEditor, out var inspectorElement)
					&& TryFindComponentHeaderElement(inspectorElement, out var header))
				{
					listOfHeaders.Add(header);
				}
			}

			return listOfHeaders;
		}

		private static bool TryFindEditorElement(VisualElement rootVisualElement, Editor editor, [MaybeNullWhen(false), NotNullWhen(true)] out VisualElement editorElement)
		{
			// InspectorElement should be a child of the EditorElement
			editorElement = rootVisualElement?.Query<InspectorElement>()
											 .Where(inspectorElement => inspectorElementEditorField.GetValue(inspectorElement) as Editor == editor)
											 .First()?.parent;

			return editorElement is not null;
		}

		private static bool TryFindInspectorElement(VisualElement rootVisualElement, Editor editor, [MaybeNullWhen(false), NotNullWhen(true)] out InspectorElement inspectorElement)
		{
			inspectorElement = rootVisualElement?.Query<InspectorElement>()
				.Where(inspectorElement => inspectorElementEditorField.GetValue(inspectorElement) as Editor == editor)
				.First();
			return inspectorElement is not null;
		}

		private static void GetAllComponentHeaderElements([DisallowNull] VisualElement editorsListElement, List<(Editor, IMGUIContainer)> results)
		{
			foreach(var inspectorElement in editorsListElement.Query<InspectorElement>().Build())
			{
				if(inspectorElement.parent is not { } editorElement)
				{
					continue;
				}

				foreach(var editorElementChild in editorElement.Children())
				{
					if(TryGetComponentHeaderElement(editorElementChild, out var header))
					{
						var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;
						if(editor && editor.target as Component)
						{
							results.Add((editor, header));
						}
					}
				}
			}
		}

		private static bool TryFindComponentHeaderElement(InspectorElement inspectorElement, out (Editor editor, IMGUIContainer header) result)
		{
			// Both InspectorElement and header element should be nested under the same EditorElement
			if(inspectorElement.parent is not { } editorElement)
			{
				result = default;
				return false;
			}

			foreach(var editorElementChild in editorElement.Children())
			{
				if(TryGetComponentHeaderElement(editorElementChild, out var header))
				{
					var editor = inspectorElementEditorField.GetValue(inspectorElement) as Editor;
					if(editor && editor.target as Component)
					{
						result = (editor, header);
						return true;
					}
				}
			}

			result = default;
			return false;
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