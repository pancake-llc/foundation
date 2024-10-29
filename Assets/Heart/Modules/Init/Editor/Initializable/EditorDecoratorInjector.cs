//#define DEBUG_REPAINT
#define DEBUG_INJECT
#define DEBUG_REMOVE

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

			foreach(var inspectorElement in GetAllInspectorElements(rootEditor))
			{
				ProcessInspectorElement(rootEditor, inspectorElement);
			}
		}

		private static void ProcessInspectorElement(Editor rootEditor, InspectorElement inspectorElement)
		{
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

					#if DEV_MODE && DEBUG_REPAINT
					Debug.Log(rootEditor.GetType().Name + "Repaint");
					UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
					#endif

					rootEditor.Repaint();

					#if DEV_MODE && DEBUG_REPAINT
					UnityEngine.Profiling.Profiler.EndSample();
					#endif
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

				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(rootEditor.GetType().Name + "Repaint");
				UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
				#endif

				rootEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
				UnityEngine.Profiling.Profiler.EndSample();
				#endif
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

				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(rootEditor.GetType().Name + $"({targetType.Name}).Repaint");
				UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
				#endif

				rootEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
				UnityEngine.Profiling.Profiler.EndSample();
				#endif
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

				#if DEV_MODE && DEBUG_REPAINT
				Debug.Log(rootEditor.GetType().Name + $"({targetType.Name}).Repaint");
				UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
				#endif

				rootEditor.Repaint();

				#if DEV_MODE && DEBUG_REPAINT
				UnityEngine.Profiling.Profiler.EndSample();
				#endif
			}
		}

		public static void RepaintAll()
		{
			for(int i = activeDecorators.Count - 1; i >= 0; i--)
			{
				var decorator = activeDecorators[i];
				if(decorator.panel is not null)
				{
#if DEV_MODE && DEBUG_REPAINT
					decorator.GetType().Name + "Repaint");
					UnityEngine.Profiling.Profiler.BeginSample("Sisus.Repaint");
#endif

					decorator.DecoratedEditor.Repaint();

#if DEV_MODE && DEBUG_REPAINT
					UnityEngine.Profiling.Profiler.EndSample();
#endif
				}
			}
		}
		
		private static bool IsMissingComponent(Object target) => !target || target.GetType() == typeof(MonoBehaviour);
	}
}