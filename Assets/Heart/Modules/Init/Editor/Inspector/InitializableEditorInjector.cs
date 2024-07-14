//#define DEBUG_OVERRIDE_EDITOR
//#define DRAW_INIT_SECTION_WITHOUT_EDITOR

#if UNITY_2023_1_OR_NEWER
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
	[InitializeOnLoad]
	internal static class InitializableEditorInjector
	{
		public static bool IsDone { get; private set; } = false;

		static InitializableEditorInjector() => InjectCustomEditorsWhenReady();

		private static void InjectCustomEditorsWhenReady()
		{
			IsDone = false;

			InternalCustomEditorCache internalCustomEditorCache = new InternalCustomEditorCache();

			#if ODIN_INSPECTOR
			if(ShouldWaitForOdinToInjectItsEditor(internalCustomEditorCache))
			{
				EditorApplication.delayCall += InjectCustomEditorsWhenReady;
				return;
			}
			#endif
			
			InjectCustomEditors(internalCustomEditorCache);
			if(Selection.objects.Length > 0)
			{
				var selectionWas = Selection.objects;
				Selection.objects = Array.Empty<Object>();
				EditorApplication.delayCall += ()=>
				{
					if(Selection.objects.Length == 0)
					{
						Selection.objects = selectionWas;
					}
				};
			}

			IsDone = true;
		}

		private static void InjectCustomEditors(InternalCustomEditorCache customEditorCache)
		{
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
				if(inspectableType.IsAbstract || !InitializerEditorUtility.TryGetEditorOverrideType(inspectableType, out Type overrideEditorType))
				{
					continue;
				}

				Type customEditorType = GetCustomEditorType(inspectableType, false);
				if(customEditorType.Assembly == thisAssembly)
				{
					#if DEV_MODE && DEBUG_OVERRIDE_EDITOR
                    Debug.Log($"Won't override {type.Name} existing editor {GetCustomEditorType(type, false).Name}");
					#endif
					continue;
				}

				#if DRAW_INIT_SECTION_WITHOUT_EDITOR
				// Still use the custom editor when possible,
				// because it visualizes non-serialized fields in play mode.
				if(!CustomEditorUtility.IsGenericInspectorType(customEditorType))
				{
					continue;
				}
				#endif

				customEditorCache.InjectCustomEditor(inspectableType, overrideEditorType, canEditMultipleObjects:true, editorForChildClasses:false, isFallback:false);
			}

			// Restore selection to rebuild inspectors.
			Selection.objects = selectionWas;
		}

		public static void CreateCachedEditor(Object[] targets, ref Editor editor)
		{
			var editorType = GetCustomEditorType(targets[0].GetType(), targets.Length > 1);

			if(InitializerEditorUtility.IsInitializableEditorType(editorType))
			{
				#if DEV_MODE
				Debug.LogWarning(targets[0]);
				#endif

				editorType = CustomEditorUtility.GetGenericInspectorType();
			}

			Editor.CreateCachedEditor(targets[0], editorType, ref editor);
		}

		public static bool HasCustomEditor(Type componentType) => GetCustomEditorType(componentType, false) != CustomEditorUtility.genericInspectorType;

		public static Type GetCustomEditorType([DisallowNull] Type componentType, bool multiEdit)
		{
			#if DEV_MODE
			Debug.Assert(componentType != null);
			#endif

			var inspectedType = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			var editorForChildClasses = typeof(CustomEditor).GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			Type result = null;

			foreach(var customEditorType in TypeCache.GetTypesWithAttribute<CustomEditor>())
			{
				foreach(var attribute in customEditorType.GetCustomAttributes<CustomEditor>())
				{
					var targetType = inspectedType.GetValue(attribute) as Type;
					if(targetType == null)
					{
						continue;
					}

					if(targetType == componentType)
					{
						if(!attribute.isFallback)
						{
							return customEditorType;
						}

						result = customEditorType;
						break;
					}

					if(result == null && targetType.IsAssignableFrom(componentType) && (bool)editorForChildClasses.GetValue(attribute))
					{
						result = customEditorType;
						break;
					}
				}
			}

			if(result != null)
			{
				return result;
			}

			return CustomEditorUtility.GetGenericInspectorType();
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