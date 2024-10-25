//#define DEBUG_REPAINT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sisus.Shared.EditorOnly
{
	public static class InspectorContents
	{
		private static readonly List<(Editor, IMGUIContainer)> headersList = new();
		private static readonly List<InspectorElement> inspectorElementsList = new();

		/// <summary>
		/// All 'Inspector' and 'Properties...' windows.
		/// </summary>
		private static
		#if UNITY_2023_3_OR_NEWER // GetPropertyEditors was added in Unity 2023.3.0a4
		readonly IEnumerable<PropertyEditor> allPropertyEditors = PropertyEditor.GetPropertyEditors();
		#else
		IEnumerable<PropertyEditor> allPropertyEditors => System.Linq.Enumerable.Where(Resources.FindObjectsOfTypeAll<PropertyEditor>(), e => e);
		#endif

		public static void Repaint()
		{
#if DEV_MODE && DEBUG_REPAINT
			Debug.Log("InitArgs.Repaint");
			UnityEngine.Profiling.Profiler.BeginSample("InspectorContents.Repaint");
#endif
			
			foreach(var propertyEditor in allPropertyEditors)
			{
				propertyEditor.Repaint();
			}
			
#if DEV_MODE && DEBUG_REPAINT
			UnityEngine.Profiling.Profiler.EndSample();
#endif
		}

		public static List<InspectorElement> GetAllInspectorElements(Editor editor)
		{
			inspectorElementsList.Clear();

			foreach(var propertyEditor in allPropertyEditors)
			{
				if(Array.IndexOf(propertyEditor.tracker.activeEditors, editor) == -1)
				{
					continue;
				}

				if(TryFindEditorElement(propertyEditor.rootVisualElement, editor, out var editorElement)
				// All EditorElements should be parented under the same editors list element
				&& editorElement.parent is { } editorsListElement)
				{
					foreach(var inspectorElement in editorsListElement.Query<InspectorElement>().Build())
					{
						inspectorElementsList.Add(inspectorElement);
					}
				}
			}

			return inspectorElementsList;
		}

		internal static List<(Editor editor, IMGUIContainer header)> GetAllHeaderElements(Editor editor)
		{
			headersList.Clear();

			foreach(var propertyEditor in allPropertyEditors)
			{
				if(Array.IndexOf(propertyEditor.tracker.activeEditors, editor) == -1)
				{
					continue;
				}

				if(TryFindEditorElement(propertyEditor.rootVisualElement, editor, out var editorElement)
				// All EditorElements should be parented under the same editors list element
				&& editorElement.parent is { } editorsListElement)
				{
					GetAllComponentHeaderElements(editorsListElement, headersList);
				}
			}

			return headersList;
		}

		private static bool TryFindEditorElement(VisualElement rootVisualElement, Editor editor, [MaybeNullWhen(false), NotNullWhen(true)] out EditorElement editorElement)
		{
			editorElement = rootVisualElement?.Query<EditorElement>()
				.Where(editorElement => editorElement.editor == editor)
				.First();

			return editorElement is not null;
		}

		private static void GetAllComponentHeaderElements([DisallowNull] VisualElement editorsListElement, List<(Editor, IMGUIContainer)> results)
		{
			foreach(var child in editorsListElement.Children())
			{
				if(child is not EditorElement editorElement)
				{
					continue;
				}

				var editor = editorElement.editor;
				if(!editor || editor.target is not Component)
				{
					continue;
				}

				foreach(var editorElementChild in editorElement.Children())
				{
					if(editorElementChild is IMGUIContainer imguiContainer && imguiContainer.name.EndsWith("Header", StringComparison.Ordinal))
					{
						results.Add((editor, imguiContainer));
					}
				}
			}
		}

		public static void SetEditor(this InspectorElement inspectorElement, Editor newEditor)
		{
			if(inspectorElement.parent is EditorElement editorElement)
			{
				var oldEditor = inspectorElement.editor;
				ReplaceInstances(editorElement.Editors, oldEditor, newEditor);
			}
		}

		public static Editor GetEditor(this InspectorElement inspectorElement) => inspectorElement.editor;

		private static void ReplaceInstances(IEnumerable<Editor> editors, Editor oldEditor, Editor newEditor)
		{
			if (editors is not IList<Editor> list)
			{
				#if DEV_MODE
				Debug.LogError(editors?.GetType().Name ?? "null");
				#endif
				return;
			}

			for(int i = 0; i < list.Count; i++)
			{
				if(list[i] == oldEditor)
				{
					list[i] = newEditor;
				}
			}
		}

		public static void SetEditorElementEditor(VisualElement visualElement, Editor newEditor)
		{
			//var editorElement = (EditorElement)visualElement;
			//ReplaceAll(editorElement.Editors, newEditor);
			foreach(var child in visualElement.Children())
			{
				if(child is InspectorElement inspectorElement)
				{
					inspectorElement.SetEditor(newEditor);
				}
			}
		}
	}
}