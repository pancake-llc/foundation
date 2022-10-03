using System;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Pancake.Init
{
	/// <summary>
	/// Class responsible for detecting when an <see cref="Initializer"/> asset is imported
	/// and giving the script the Initializer icon and reapplying script execution orders
	/// for all initializers so that all dependencies are initialized before the dependent objects.
	/// </summary>
	internal sealed class InitializerAssetPostprocessor : AssetPostprocessor
    {
		private const string EditorPrefsKey = "InitArgs.DelayedProcessingQueue";
		private static Texture2D initializerIcon = null;

        [UsedImplicitly]
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            for(int i = importedAssets.Length - 1; i >= 0; i--)
            {
                PostProcessAsset(importedAssets[i]);
            }
        }

		[DidReloadScripts]
		private static void OnScriptsReloaded()
		{
			EditorApplication.delayCall -= HandleProcessQueuedAssets;
			EditorApplication.delayCall += HandleProcessQueuedAssets;
		}

        private static void PostProcessAsset(string path)
		{
			if(TryDetermineIsInitializerAsset(path, out bool isInitializer) && !isInitializer)
			{
				return;
			}

			string existingPaths = EditorPrefs.GetString(EditorPrefsKey, "");
			if(existingPaths.Length > 0)
			{
				EditorPrefs.SetString(EditorPrefsKey, existingPaths + "|" + path);
			}
			else
			{
				EditorPrefs.SetString(EditorPrefsKey, path);
			}

			EditorApplication.delayCall -= HandleProcessQueuedAssets;
			EditorApplication.delayCall += HandleProcessQueuedAssets;
		}

		private static bool TryDetermineIsInitializerAsset(string path, out bool isInitializer)
		{
			if(!path.EndsWith(".cs", StringComparison.Ordinal))
			{
				isInitializer = false;
				return true;
			}

			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			if(script == null)
			{
				isInitializer = false;
				return true;
			}

			var type = script.GetClass();
			if(type != null && typeof(IInitializer).IsAssignableFrom(type) && !type.IsAbstract)
			{
				isInitializer = true;
				return true;
			}

			isInitializer = false;

			// Initializer type can be null while scripts are still being compiled.
			return !EditorApplication.isCompiling && !EditorApplication.isUpdating;
		}

		private static bool TryGetInitializerScript(string path, out MonoScript script)
		{
			script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			if(script == null)
			{
				return false;
			}

			var type = script.GetClass();
			if(type != null && typeof(IInitializer).IsAssignableFrom(type) && !type.IsAbstract)
			{
				return true;
			}

			script = null;
			return false;
		}

		private static void HandleProcessQueuedAssets()
		{
			if(EditorApplication.isCompiling || EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += HandleProcessQueuedAssets;
				return;
			}

			string unprocessed = EditorPrefs.GetString(EditorPrefsKey, "");
			if(unprocessed.Length == 0)
			{
				return;
			}

			bool atLeastOneNewInitializerImported = false;
			string[] paths = unprocessed.Split('|');
			foreach(string path in paths)
			{
				if(TryGetInitializerScript(path, out var script))
				{
					atLeastOneNewInitializerImported = true;
					OnImportedInitializerAsset(script);
				}
			}

			if(atLeastOneNewInitializerImported)
			{
				AssetDatabase.StartAssetEditing();
				var sorter = new InitializerExecutionOrderApplier();
				sorter.UpdateExecutionOrderOfAllInitializers();
				AssetDatabase.StopAssetEditing();
			}

			EditorPrefs.DeleteKey(EditorPrefsKey);
		}

		private static void OnImportedInitializerAsset(MonoScript script) => SetInitializerIcon(script);

		private static void SetInitializerIcon(MonoScript script)
        {
            if(initializerIcon == null)
			{
				initializerIcon = EditorGUIUtility.IconContent("AnimatorStateTransition Icon").image as Texture2D;

				if(initializerIcon == null)
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
