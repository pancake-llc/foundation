//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;
using static Sisus.Shared.EditorOnly.InspectorContents;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class EditorDecoratorInjector
	{
		private static readonly Assembly sisusEditorAssembly = typeof(InitializableEditorDecorator).Assembly;
		private static readonly List<EditorDecorator> activeDecorators = new();

		static EditorDecoratorInjector()
		{
			Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
			Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
		}

		private static void OnBeforeAssemblyReload()
		{
			// Dispose all decorators to avoid memory leaks.
			for(int i = activeDecorators.Count - 1; i >= 0; i--)
			{
				activeDecorators[i].Dispose();
			}

			activeDecorators.Clear();
		}

		private static void AfterInspectorRootEditorHeaderGUI(Editor rootEditor)
		{
			if(EditorApplication.isCompiling)
			{
				return;
			}

			if(Event.current.type != EventType.Repaint)
			{
				rootEditor.Repaint();
				return;
			}

			foreach(var inspectorElement in GetAllInspectorElements(rootEditor))
			{
				var editor = inspectorElement.GetEditor();
				if(editor.GetType().Assembly == sisusEditorAssembly)
				{
					continue;
				}

				var targetType = editor.target.GetType();
				if(!InitializerEditorUtility.TryGetEditorDecoratorType(targetType, out Type editorWrapperType))
				{
					continue;
				}

				// Dispose decorators that are no longer being used.
				for(int i = activeDecorators.Count - 1; i >= 0; i--)
				{
					if(activeDecorators[i].panel is null)
					{
						activeDecorators[i].Dispose();
						activeDecorators.RemoveAt(i);
					}
				}

				for(int i = inspectorElement.childCount - 1; i >= 0; i--)
				{
					if(inspectorElement[i] is not EditorDecorator decorator)
					{
						continue;
					}

					// If a valid decorator already exits, we are done here.
					if(decorator.IsValid() && decorator.targets.SequenceEqual(inspectorElement.GetEditor().targets))
					{
						return;
					}

					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Removing {decorator.GetType().Name} with IsValid:{decorator.IsValid()}, targets:{decorator.targets} vs inspectorElement.editor.targets:{inspectorElement.GetEditor().targets}");
					#endif

					inspectorElement.RemoveAt(i);
					activeDecorators.Remove(decorator);
					decorator.Dispose();
				}

				if(EditorDecorator.CreateBeforeInspectorGUI(editorWrapperType, editor, out var beforeInspectorGUI))
				{
					activeDecorators.Add(beforeInspectorGUI);

					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Injecting {editorWrapperType.Name}.{nameof(EditorDecorator.OnBeforeInspectorGUI)} to {targetType.Name}'s editor {editor.GetType().Name} (in assembly {editor.GetType().Assembly.GetName().Name})...");
					#endif

					beforeInspectorGUI.name = nameof(EditorDecorator.OnBeforeInspectorGUI);
					inspectorElement.Insert(0, beforeInspectorGUI);
				}

				if(EditorDecorator.CreateAfterInspectorGUI(editorWrapperType, editor, out var afterInspectorGUI))
				{
					activeDecorators.Add(afterInspectorGUI);

					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Injecting {editorWrapperType.Name}.{nameof(EditorDecorator.OnAfterInspectorGUI)} to {targetType.Name}'s editor {editor.GetType().Name} (in assembly {editor.GetType().Assembly.GetName().Name})...");
					#endif

					afterInspectorGUI.name = nameof(EditorDecorator.OnAfterInspectorGUI);
					inspectorElement.Insert(inspectorElement.childCount, afterInspectorGUI);
				}
			}
		}
	}
}