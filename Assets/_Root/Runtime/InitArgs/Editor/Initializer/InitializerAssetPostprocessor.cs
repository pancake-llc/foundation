using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using JetBrains.Annotations;
using UnityEditor.Callbacks;

namespace Pancake.Init
{
    internal class InitializerAssetPostprocessor : AssetPostprocessor
    {
		const string EditorPrefsKey = "InitializerAssetPostprocessor.Queue";
		private static Texture2D initializerIcon = null;

        [UsedImplicitly]
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for(int i = importedAssets.Length - 1; i >= 0; i--)
            {
                PostProcessAsset(importedAssets[i]);
            }
        }

        private static void PostProcessAsset(string path)
        {
            if(!path.EndsWith(".cs", StringComparison.Ordinal))
            {
                return;
            }

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if(script == null)
            {
				return;
			}

            var type = script.GetClass();

			if(type == null)
            {
				// Initializer type can be null while still being compiled.
				if(EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    string existing = EditorPrefs.GetString(EditorPrefsKey, "");
                    if(existing.Length > 0)
                    {
                        EditorPrefs.SetString(EditorPrefsKey, existing + "|" + path);
                    }
                    else
                    {
                        EditorPrefs.SetString(EditorPrefsKey, path);
                    }
                }
                return;
            }

			if(!typeof(IInitializer).IsAssignableFrom(type) || type.IsAbstract)
            {
				return;
            }

			if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                string existing = EditorPrefs.GetString(EditorPrefsKey, "");
                if(existing.Length > 0)
                {
                    EditorPrefs.SetString(EditorPrefsKey, existing + "|" + path);
                }
                else
                {
                    EditorPrefs.SetString(EditorPrefsKey, path);
                }
				return;
            }

            OnImportedInitializerAsset(script);
        }

        [DidReloadScripts]
		private static void OnScriptsReloaded() => EditorApplication.delayCall += OnScriptsReloadedDelayed;

		private static void OnScriptsReloadedDelayed()
        {
			string unprocessed = EditorPrefs.GetString(EditorPrefsKey, "");
			if(unprocessed.Length == 0)
            {
				return;
            }

			if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
				EditorApplication.delayCall += OnScriptsReloadedDelayed;
				return;
			}

			EditorPrefs.DeleteKey(EditorPrefsKey);

			string[] paths = unprocessed.Split('|');
			foreach(string path in paths)
            {
				PostProcessAsset(path);
			}
		}

		private static void OnImportedInitializerAsset(MonoScript script)
		{
			int executionOrder = ExecutionOrder.Initializer;
			if(script.GetClass() is Type initializerType)
            {
				foreach(var interfaceType in initializerType.GetInterfaces())
                {
					if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IValueProvider<>) && ServiceUtility.IsDefiningTypeOfAnyServiceAttribute(interfaceType.GetGenericArguments()[0]))
                    {
						executionOrder = ExecutionOrder.ServiceInitializer;
						break;
					}
                }
			}

			SetScriptExecutionOrder(script, executionOrder);
			SetInitializerIcon(script);
		}

		private static void SetScriptExecutionOrder(MonoScript script, int executionOrder)
		{
			int executionOrderWas = MonoImporter.GetExecutionOrder(script);
			if(executionOrderWas != executionOrder && executionOrderWas == 0)
			{
				MonoImporter.SetExecutionOrder(script, executionOrder);
			}
		}

        private static void SetInitializerIcon(MonoScript script)
        {
            if(initializerIcon == null)
			{
				initializerIcon = EditorGUIUtility.IconContent("AnimatorStateTransition Icon").image as Texture2D;

				if(initializerIcon is null)
                {
					if(EditorApplication.isUpdating)
					{
						EditorApplication.delayCall += ()=> SetInitializerIcon(script);
						return;
					}

					#if DEV_MODE
					Debug.LogWarning("Initializer icon 'AnimatorStateTransition Icon' not found.");
					#endif
					return;
                }
			}

			SetIcon(script, initializerIcon);
        }

		private static void SetIcon(MonoScript script, Texture2D icon)
        {
			#if UNITY_2021_2_OR_NEWER
			var currentIcon = EditorGUIUtility.GetIconForObject(script);
			if(currentIcon != icon)
			{
				EditorGUIUtility.SetIconForObject(script, icon);
			}
			#else
			MethodInfo getIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
			var currentIcon = getIconForObject.Invoke(null, new object[] { script }) as Texture2D;
			if(currentIcon != icon)
			{
				MethodInfo setIconForObject = typeof(EditorGUIUtility).GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo copyMonoScriptIconToImporters = typeof(MonoImporter).GetMethod("CopyMonoScriptIconToImporters", BindingFlags.Static | BindingFlags.NonPublic);
				setIconForObject.Invoke(null, new object[] { script, icon });
				copyMonoScriptIconToImporters.Invoke(null, new object[] { script });
			}
			#endif
        }
    }
}