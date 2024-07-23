//#define DEBUG_SETUP_DURATION
//#define DEBUG_OVERRIDE_EDITOR
//#define DRAW_INIT_SECTION_WITHOUT_EDITOR

#if UNITY_2023_1_OR_NEWER
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sisus.Init.Internal;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class InitializableEditorInjector
	{
		public static bool IsDone { get; private set; }

		#if DEV_MODE && DEBUG_SETUP_DURATION
		private static readonly System.Diagnostics.Stopwatch tryGetEditorOverrideTypeTimer = new();
		private static readonly System.Diagnostics.Stopwatch getCustomEditorTypeTimer = new();
		private static readonly System.Diagnostics.Stopwatch injectCustomEditorTimer = new();
		#endif

		static InitializableEditorInjector() => InjectCustomEditorsWhenReady();

		private static async void InjectCustomEditorsWhenReady()
		{
			IsDone = false;

			await Until.UnitySafeContext();

			InternalCustomEditorCache internalCustomEditorCache = new();

			#if ODIN_INSPECTOR
			while(ShouldWaitForOdinToInjectItsEditor(internalCustomEditorCache))
			{
				await Until.UnitySafeContext();
			}
			#endif

			InjectCustomEditors(internalCustomEditorCache);

			IsDone = true;

			if (Selection.objects.Length <= 0)
			{
				return;
			}

			var selectionWas = Selection.objects;
			Selection.objects = Array.Empty<Object>();

			await Until.UnitySafeContext();

			if(Selection.objects.Length == 0)
			{
				Selection.objects = selectionWas;
			}
		}

		private static void InjectCustomEditors(InternalCustomEditorCache customEditorCache)
		{
			#if DEV_MODE
			UnityEngine.Profiling.Profiler.BeginSample("InjectCustomEditors");
			#if DEBUG_SETUP_DURATION
			var timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			#endif
			#endif
			
			// Temporarily clear selection and restore later in order to force all
			// open Inspectors to update their contents using the updated inspectors.
			// NOTE: Does not handle locked Inspector nor Properties... windows at the moment.
			var selectionWas = Selection.objects;
			Selection.objects = Array.Empty<Object>();

			var thisAssembly = typeof(InitializableEditorInjector).Assembly;

			foreach(var inspectableType in TypeCache.GetTypesDerivedFrom<Component>()
								.Concat(TypeCache.GetTypesDerivedFrom<ScriptableObject>())
								.Concat(TypeCache.GetTypesDerivedFrom<StateMachineBehaviour>()))
			{
				HandleInjectCustomEditor(customEditorCache, inspectableType, thisAssembly);
			}

			// Restore selection to rebuild inspectors.
			Selection.objects = selectionWas;

			#if DEV_MODE
			UnityEngine.Profiling.Profiler.EndSample();
			#if DEBUG_SETUP_DURATION
			Debug.Log("InjectCustomEditors in total took " + timer.Elapsed.TotalSeconds + "s.\n"+
			"    TryGetEditorOverrideType took " + tryGetEditorOverrideTypeTimer.Elapsed.TotalSeconds + "s.\n"+
			"    GetCustomEditorType took " + getCustomEditorTypeTimer.Elapsed.TotalSeconds + "s.\n"+
			"    InjectCustomEditor took " + injectCustomEditorTimer.Elapsed.TotalSeconds + "s.");
			#endif
			#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void HandleInjectCustomEditor(InternalCustomEditorCache customEditorCache, Type inspectableType, Assembly thisAssembly)
		{
			#if DEV_MODE && DEBUG_SETUP_DURATION
			InitializableEditorInjector.tryGetEditorOverrideTypeTimer.Start();
			#endif

			if(inspectableType.IsAbstract || !InitializerEditorUtility.TryGetEditorOverrideType(inspectableType, out Type overrideEditorType))
			{
				#if DEV_MODE && DEBUG_SETUP_DURATION
				InitializableEditorInjector.tryGetEditorOverrideTypeTimer.Stop();
				#endif
				return;
			}
						
			#if DEV_MODE && DEBUG_SETUP_DURATION
			InitializableEditorInjector.tryGetEditorOverrideTypeTimer.Stop();
			InitializableEditorInjector.getCustomEditorTypeTimer.Start();
			#endif
					
			Type customEditorType = CustomEditorType.Get(inspectableType, false);

			#if DEV_MODE && DEBUG_SETUP_DURATION
			InitializableEditorInjector.getCustomEditorTypeTimer.Stop();
			#endif

			if(customEditorType.Assembly == thisAssembly)
			{
				#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
				Debug.Log($"Won't override {type.Name} existing editor {GetCustomEditorType(type, false).Name}");
				#endif
				return;
			}

			#if DRAW_INIT_SECTION_WITHOUT_EDITOR
			// Still use the custom editor when possible,
			// because it visualizes non-serialized fields in play mode.
			if(!CustomEditorUtility.IsGenericInspectorType(customEditorType))
			{
				continue;
			}
			#endif

			#if DEV_MODE && DEBUG_SETUP_DURATION
			InitializableEditorInjector.injectCustomEditorTimer.Start();
			#endif

			customEditorCache.InjectCustomEditor(inspectableType, overrideEditorType, canEditMultipleObjects:true, editorForChildClasses:false, isFallback:false);
				
			#if DEV_MODE && DEBUG_SETUP_DURATION
			InitializableEditorInjector.injectCustomEditorTimer.Stop();
			#endif
		}

		#if ODIN_INSPECTOR
		private static int timesToWaitForOdin = 50;
		private static bool ShouldWaitForOdinToInjectItsEditor(InternalCustomEditorCache customEditorCache)
		{
			if(timesToWaitForOdin <= 0)
			{
				#if DEV_MODE
				Debug.Log("Waited long enough for Odin. Injecting custom Initializer Editors...");
				#endif
				return false;
			}

			if(customEditorCache.ContainsEditorType(CustomEditorUtility.odinEditorType))
			{
				timesToWaitForOdin = 0;
				return false;
			}

			timesToWaitForOdin--;
			return true;
		}
		#endif
	}
}
#endif