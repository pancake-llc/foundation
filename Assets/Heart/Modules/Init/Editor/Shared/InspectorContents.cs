//#define DEBUG_REPAINT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
using Unity.Profiling;
#endif

namespace Sisus.Shared.EditorOnly
{
	public static class InspectorContents
	{
		private static readonly List<(Editor, IMGUIContainer)> headersList = new();
		private static readonly List<(InspectorType inspectorType, InspectorElement inspectorElement)> inspectorElementsList = new();

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
			#endif

			#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
			using var x = repaintMarker.Auto();
			#endif

			foreach(var propertyEditor in allPropertyEditors)
			{
				propertyEditor.Repaint();
			}
		}

		public static List<(InspectorType inspectorType, InspectorElement inspectorElement)> GetAllInspectorElements(Editor editor)
		{
			inspectorElementsList.Clear();

			foreach(var propertyEditor in allPropertyEditors)
			{
				if(Array.IndexOf(propertyEditor.tracker.activeEditors, editor) == -1)
				{
					continue;
				}

				var inspectorType = propertyEditor switch
				{
					InspectorWindow inspector => inspector.inspectorMode is InspectorMode.Normal ? InspectorType.InspectorWindow : InspectorType.InspectorWindowDebugMode,
					_ => InspectorType.PropertiesWindow
				};

				if(TryFindEditorElement(propertyEditor.rootVisualElement, editor, out var editorElement)
				// All EditorElements should be parented under the same editors list element
				&& editorElement.parent is { } editorsListElement)
				{
					foreach(var inspectorElement in editorsListElement.Query<InspectorElement>().Build())
					{
						inspectorElementsList.Add((inspectorType, inspectorElement));
					}
				}
			}

			return inspectorElementsList;
		}

		public static void RepaintEditorsWithTarget(Component target)
		{
			foreach(var propertyEditor in allPropertyEditors)
			{
				foreach(var editor in propertyEditor.tracker.activeEditors)
				{
					if(editor.target == target)
					{
						#if DEV_MODE && DEBUG_REPAINT
						Debug.Log(editor.target.GetType().Name + ".Repaint");
						#endif

						#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
						using var x = repaintEditorsWithTargetMarker.Auto();
						#endif

						editor.Repaint();
						break;
					}
				}
			}
		}
		
		internal static bool TryGetGameObjectHeaderElement([DisallowNull] Editor editor, [MaybeNullWhen(false), NotNullWhen(true)] out IMGUIContainer headerGUIContainer)
		{
			if(editor.target is not GameObject)
			{
				headerGUIContainer = null;
				return false;
			}
			
			#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
			using var x = getGameObjectHeaderElementMarker.Auto();
			#endif

			foreach(var propertyEditor in allPropertyEditors)
			{
				if(Array.IndexOf(propertyEditor.tracker.activeEditors, editor) == -1)
				{
					continue;
				}

				if(!TryFindEditorElement(propertyEditor.rootVisualElement, editor, out var editorElement))
				{
					continue;
				}

				foreach(var child in editorElement.Children())
				{
					headerGUIContainer = child as IMGUIContainer; 
					if(headerGUIContainer?.name.Contains("Header") ?? false)
					{
						return true;
					}
				}
			}

			headerGUIContainer = null;
			return false;
		}

		internal static List<(Editor editor, IMGUIContainer header)> GetAllComponentHeaderElements(Editor editor)
		{
			#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
			using var x = getAllHeaderElementsMarker.Auto();
			#endif

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
			#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
			using var x = tryFindEditorElementMarker.Auto();
			#endif

			editorElement = rootVisualElement?.Query<EditorElement>()
				.Where(editorElement => editorElement.editor == editor)
				.First();

			return editorElement is not null;
		}

		private static void GetAllComponentHeaderElements([DisallowNull] VisualElement editorsListElement, List<(Editor, IMGUIContainer)> results)
		{
			#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
			using var x = getAllComponentHeaderElementsMarker.Auto();
			#endif

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
		
		#if DEV_MODE && DEBUG && !SISUS_DISABLE_PROFILING
		private static readonly ProfilerMarker repaintMarker = new(ProfilerCategory.Gui, "InspectorContents.Repaint");
		private static readonly ProfilerMarker repaintEditorsWithTargetMarker = new(ProfilerCategory.Gui, "InspectorContents.RepaintEditorsWithTarget");
		private static readonly ProfilerMarker tryFindEditorElementMarker = new(ProfilerCategory.Gui, "InspectorContents.TryFindEditorElement");
		private static readonly ProfilerMarker getGameObjectHeaderElementMarker = new(ProfilerCategory.Gui, "InspectorContents.GetGameObjectHeaderElement");
		private static readonly ProfilerMarker getAllHeaderElementsMarker = new(ProfilerCategory.Gui, "InspectorContents.GetAllHeaderElements");
		private static readonly ProfilerMarker getAllComponentHeaderElementsMarker = new(ProfilerCategory.Gui, "InspectorContents.GetAllComponentHeaderElements");
		#endif
	}
}