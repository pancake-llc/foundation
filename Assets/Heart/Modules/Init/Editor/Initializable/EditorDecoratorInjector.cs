//#define DEBUG_REPAINT
//#define DEBUG_INJECT
//#define DEBUG_REMOVE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using static Sisus.Shared.EditorOnly.InspectorContents;
using Object = UnityEngine.Object;

#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
using Unity.Profiling;
#endif

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
			ObjectChangeEvents.changesPublished -= OnObjectChangesPublished;
			ObjectChangeEvents.changesPublished += OnObjectChangesPublished;
		}

		private static void OnObjectChangesPublished(ref ObjectChangeEventStream stream)
		{
			for(int s = stream.length - 1; s >= 0; s--)
			{
				var type = stream.GetEventType(s);
				if(type is ObjectChangeKind.DestroyAssetObject or ObjectChangeKind.DestroyGameObjectHierarchy)
				{
					activeDecorators.RemoveAll(decorator => !decorator.DecoratedEditor || !decorator.DecoratedEditor.target);
				}
			}
		}

		public static void RemoveFrom(Editor editor, ExecutionOptions executionOptions = ExecutionOptions.Default)
		{
			LayoutUtility.OnRepaintEvent(() =>
			{
				for(int i = activeDecorators.Count - 1; i >= 0; i--)
				{
					if(ReferenceEquals(activeDecorators[i].DecoratedEditor, editor))
					{
						activeDecorators[i].Dispose();
						activeDecorators.RemoveAt(i);
					}
				}
			}, null, executionOptions);
		}

		public static void RemoveFrom(Type targetType)
		{
			LayoutUtility.OnRepaintEvent(() =>
			{
				for(int i = activeDecorators.Count - 1; i >= 0; i--)
				{
					var decorator = activeDecorators[i];
					foreach(var target in decorator.DecoratedEditor.targets)
					{
						if (target.GetType() != targetType)
						{
							continue;
						}

						if(decorator.IsValid())
						{
							LayoutUtility.Repaint(decorator.DecoratedEditor);
						}

						decorator.parent?.Remove(decorator);
						decorator.Dispose();
						activeDecorators.RemoveAt(i);
						break;
					}
				}
			});
		}

		private static void OnBeforeAssemblyReload()
		{
			// Dispose all decorators to avoid memory leaks.
			for(int i = activeDecorators.Count - 1; i >= 0; i--)
			{
				var decorator = activeDecorators[i];
				decorator.parent?.Remove(decorator);
				decorator.Dispose();
			}

			activeDecorators.Clear();
		}

		private static void AfterInspectorRootEditorHeaderGUI(Editor rootEditor)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = afterRootHeaderGUIMarker.Auto();
			#endif

			if(EditorApplication.isCompiling)
			{
				return;
			}

			foreach((var inspectorType, var inspectorElement) in GetAllInspectorElements(rootEditor))
			{
				ProcessInspectorElement(rootEditor, inspectorType, inspectorElement);
			}
		}

		private static void ProcessInspectorElement(Editor rootEditor, InspectorType inspectorType, InspectorElement inspectorElement)
		{
			#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
			using var x = processInspectorElementMarker.Auto();
			#endif

			var editor = inspectorElement.GetEditor();
			if(editor.GetType().Assembly == sisusEditorAssembly)
			{
				return;
			}

			var target = editor.target;
			var targetType = target?.GetType();
			if(!InitializerEditorUtility.TryGetEditorDecoratorType(targetType, out Type editorDecoratorType))
			{
				return;
			}

			// Dispose decorators that are no longer being used.
			for(int i = activeDecorators.Count - 1; i >= 0; i--)
			{
				if(activeDecorators[i].panel is null)
				{
					activeDecorators[i].Dispose();
					activeDecorators.RemoveAt(i);
					LayoutUtility.Repaint(rootEditor);
				}
			}

			bool isDebugMode = inspectorType is InspectorType.InspectorWindowDebugMode;

			for(int i = inspectorElement.childCount - 1; i >= 0; i--)
			{
				if(inspectorElement[i] is not EditorDecorator decorator)
				{
					continue;
				}

				// If a valid decorator already exits, we are done here.
				if(!isDebugMode && decorator.IsValid() && decorator.targets.SequenceEqual(inspectorElement.GetEditor().targets))
				{
					return;
				}

				// VisualElement.Remove can not be called during a layout event.
				if(Event.current.type is EventType.Repaint)
				{
					#if DEV_MODE && DEBUG_REMOVE
					Debug.Log($"Removing {decorator.GetType().Name} with IsValid:{decorator.IsValid()}, targets:{string.Join(", ", decorator.targets.Select(t => "" + t))} vs inspectorElement.editor.targets:{string.Join(", ", inspectorElement.GetEditor().targets.Select(t => "" + t))}");
					#endif

					inspectorElement.RemoveAt(i);
					activeDecorators.Remove(decorator);
					decorator.Dispose();
				}
				else
				{
					decorator.visible = false;
				}

				LayoutUtility.Repaint(rootEditor);
			}

			// Don't draw decorators in debug mode to avoid clutter, duplicated information,
			// and to minimize the risk of crashes.
			if(isDebugMode)
			{
				return;
			}

			if(IsMissingComponent(target))
			{
				return;
			}

			if(EditorDecorator.ShouldCreateBeforeInspectorGUI(editorDecoratorType))
			{
				// VisualElement.Insert can not be called during a layout event.
				if(Event.current.type is EventType.Repaint)
				{
					var beforeInspectorGUI = EditorDecorator.CreateBeforeInspectorGUI(editorDecoratorType, editor);

					activeDecorators.Add(beforeInspectorGUI);

					#if DEV_MODE && DEBUG_INJECT
					Debug.Log($"Injecting {editorDecoratorType.Name}.{nameof(EditorDecorator.OnBeforeInspectorGUI)} to {targetType.Name}'s editor {editor.GetType().Name} (in assembly {editor.GetType().Assembly.GetName().Name})...");
					#endif

					beforeInspectorGUI.name = nameof(EditorDecorator.OnBeforeInspectorGUI);
					inspectorElement.Insert(0, beforeInspectorGUI);
				}

				LayoutUtility.Repaint(rootEditor);
			}

			if(EditorDecorator.ShouldCreateAfterInspectorGUI(editorDecoratorType))
			{
				// VisualElement.Insert can not be called during a layout event.
				if(Event.current.type is EventType.Repaint)
				{
					var afterInspectorGUI = EditorDecorator.CreateAfterInspectorGUI(editorDecoratorType, editor);

					activeDecorators.Add(afterInspectorGUI);

					#if DEV_MODE && DEBUG_INJECT
					Debug.Log($"Injecting {editorDecoratorType.Name}.{nameof(EditorDecorator.OnAfterInspectorGUI)} to {targetType.Name}'s editor {editor.GetType().Name} (in assembly {editor.GetType().Assembly.GetName().Name})...");
					#endif

					afterInspectorGUI.name = nameof(EditorDecorator.OnAfterInspectorGUI);
					inspectorElement.Insert(inspectorElement.childCount, afterInspectorGUI);
				}

				LayoutUtility.Repaint(rootEditor);
			}
		}

		public static void RepaintAll()
		{
			for(int i = activeDecorators.Count - 1; i >= 0; i--)
			{
				var decorator = activeDecorators[i];
				if(decorator.panel is not null)
				{
					LayoutUtility.Repaint(decorator.DecoratedEditor);
				}
			}
		}

		private static bool IsMissingComponent(Object target) => !target || target.GetType() == typeof(MonoBehaviour);

		#if DEV_MODE && DEBUG && !INIT_ARGS_DISABLE_PROFILING
		private static readonly ProfilerMarker afterRootHeaderGUIMarker = new(ProfilerCategory.Gui, "EditorDecoratorInjector.AfterInspectorRootEditorHeaderGUI");
		private static readonly ProfilerMarker processInspectorElementMarker = new(ProfilerCategory.Gui, "EditorDecoratorInjector.ProcessInspectorElement");
		#endif
	}
}