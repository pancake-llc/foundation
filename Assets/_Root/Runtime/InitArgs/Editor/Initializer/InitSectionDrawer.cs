#if DRAW_INIT_SECTION_WITHOUT_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Init.EditorOnly
{
	[InitializeOnLoad]
	internal static class InitSectionDrawer
	{
		private static readonly Dictionary<Component, InitializerDrawer> drawers = new Dictionary<Component, InitializerDrawer>();

		static InitSectionDrawer()
		{
			ComponentHeader.AfterHeaderGUI -= OnAfterHeaderGUI;
			ComponentHeader.AfterHeaderGUI += OnAfterHeaderGUI;
			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
			EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
			EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
		}

		private static void OnPlaymodeStateChanged(PlayModeStateChange stateChange)
		{
			switch(stateChange)
			{
				case PlayModeStateChange.ExitingEditMode:
				case PlayModeStateChange.EnteredPlayMode:
					foreach(var drawer in drawers.Values)
					{
						drawer.Dispose();
					}
					drawers.Clear();
					break;
			}
		}

		private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] compilerMessages)
		{
			string assemblyName = Path.GetFileName(assemblyPath);
			var drawersToCheck = drawers.ToList();
			drawers.Clear();
			foreach(var drawer in drawersToCheck)
			{
				var potentiallyDestroy = drawer.Value;
				InitializerDrawer.OnAssemblyCompilationStarted(ref potentiallyDestroy, assemblyName);
				if(potentiallyDestroy != null)
				{
					drawers[drawer.Key] = potentiallyDestroy;
				}
			}
		}

		private static float OnAfterHeaderGUI(Component component, Rect headerRect, bool HeaderIsSelected, bool supportsRichText)
		{
			if(!IsInitializable(component) || !InternalEditorUtility.GetIsInspectorExpanded(component))
			{
				return 0f;
			}

			if(!drawers.TryGetValue(component, out InitializerDrawer initializerDrawer))
			{
				initializerDrawer = new InitializerDrawer(component.GetType(), new Object[] { component }, new Type[] { }, null, new GameObject[] { component.gameObject });
				drawers[component] = initializerDrawer;
			}

			EditorGUILayout.BeginVertical(EditorStyles.inspectorDefaultMargins);
			initializerDrawer.OnInspectorGUI();
			EditorGUILayout.EndVertical();
			GUILayout.Label(" ", GUILayout.Height(0f));
			var endRect = GUILayoutUtility.GetLastRect();
			return Mathf.Max(0f, endRect.yMax - headerRect.yMin);
		}

		private static bool IsInitializable(Component component)
			=> component is IOneArgument || component is ITwoArguments || component is IThreeArguments
			|| component is IFourArguments || component is IFiveArguments || component is ISixArguments;

		private static void OnSelectionChanged()
		{
			var drawersToCheck = drawers.ToList();
			drawers.Clear();
			foreach(var drawer in drawersToCheck)
			{
				if(drawer.Key == null || Array.IndexOf(Selection.gameObjects, drawer.Key.gameObject) != -1)
				{
					drawer.Value.Dispose();
				}
				else
				{
					drawers[drawer.Key] = drawer.Value;
				}
			}
		}
	}
}
#endif