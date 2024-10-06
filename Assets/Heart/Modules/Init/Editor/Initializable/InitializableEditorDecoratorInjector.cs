#define DEBUG_ENABLED

using System;
using System.Linq;
using System.Reflection;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEngine;
using static Sisus.Shared.EditorOnly.InspectorContents;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class InitializableEditorDecoratorInjector
	{
		private static readonly Assembly sisusEditorAssembly = typeof(InitializableEditorDecorator).Assembly;

		static InitializableEditorDecoratorInjector()
		{
			Editor.finishedDefaultHeaderGUI -= AfterInspectorRootEditorHeaderGUI;
			Editor.finishedDefaultHeaderGUI += AfterInspectorRootEditorHeaderGUI;
		}

		private static void AfterInspectorRootEditorHeaderGUI(Editor rootEditor)
		{
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
				}

				if(EditorDecorator.CreateBeforeInspectorGUI(editorWrapperType, editor, out var beforeInspectorGUI))
				{
					#if DEV_MODE && DEBUG_ENABLED
					Debug.Log($"Injecting {editorWrapperType.Name}.{nameof(EditorDecorator.OnBeforeInspectorGUI)} to {targetType.Name}'s editor {editor.GetType().Name} (in assembly {editor.GetType().Assembly.GetName().Name})...");
					#endif

					beforeInspectorGUI.name = nameof(EditorDecorator.OnBeforeInspectorGUI);
					inspectorElement.Insert(0, beforeInspectorGUI);
				}

				if(EditorDecorator.CreateAfterInspectorGUI(editorWrapperType, editor, out var afterInspectorGUI))
				{
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