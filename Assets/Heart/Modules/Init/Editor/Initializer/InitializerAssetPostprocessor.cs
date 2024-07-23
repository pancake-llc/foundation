using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class responsible for detecting when an <see cref="Initializer"/> asset is imported
	/// and giving the script the Initializer icon and reapplying script execution orders
	/// for all initializers so that all dependencies are initialized before the dependent objects.
	/// </summary>
	internal sealed class InitializerAssetPostprocessor : AssetPostprocessor
	{
		private const string EDITOR_PREFS_KEY = "InitArgs.Initializers.DelayedProcessingQueue";
		private const string ICON_NAME = "AnimatorStateTransition Icon";
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
			if(TryDetermineIsInitializerScript(path, out bool isInitializer) && !isInitializer)
			{
				return;
			}

			string existingPaths = EditorPrefs.GetString(EDITOR_PREFS_KEY, "");
			if(existingPaths.Length > 0)
			{
				EditorPrefs.SetString(EDITOR_PREFS_KEY, existingPaths + "|" + path);
			}
			else
			{
				EditorPrefs.SetString(EDITOR_PREFS_KEY, path);
			}

			EditorApplication.delayCall -= HandleProcessQueuedAssets;
			EditorApplication.delayCall += HandleProcessQueuedAssets;
		}

		private static bool TryDetermineIsInitializerScript(string path, out bool isInitializer)
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
			if(type == null)
			{
				isInitializer = false;
				// MonoScript.GetClass() result can be temporarily null
				// for value providers when scripts are still being compiled.
				return !EditorApplication.isCompiling && !EditorApplication.isUpdating;
			}

			if(typeof(IInitializer).IsAssignableFrom(type) && !type.IsAbstract)
			{
				isInitializer = true;
				return true;
			}

			isInitializer = false;
			return true;
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

			string unprocessed = EditorPrefs.GetString(EDITOR_PREFS_KEY, "");
			if(unprocessed.Length == 0)
			{
				return;
			}

			bool startedAssetEditing = false;

			bool atLeastOneNewInitializerImported = false;
			string[] paths = unprocessed.Split('|');
			foreach(string path in paths)
			{
				if(TryGetInitializerScript(path, out var script))
				{
					if(!startedAssetEditing)
					{
						startedAssetEditing = true;
						AssetDatabase.StartAssetEditing();
					}

					atLeastOneNewInitializerImported = true;
					OnImportedInitializerAsset(script);
				}
			}

			if(atLeastOneNewInitializerImported)
			{
				if(!startedAssetEditing)
				{
					startedAssetEditing = true;
					AssetDatabase.StartAssetEditing();
				}

				var sorter = new InitializerExecutionOrderApplier();
				sorter.UpdateExecutionOrderOfAllInitializers();
			}

			if(startedAssetEditing)
			{
				AssetDatabase.StopAssetEditing();
			}

			EditorPrefs.DeleteKey(EDITOR_PREFS_KEY);
		}

		private static void OnImportedInitializerAsset(MonoScript script) => SetInitializerIcon(script);

		private static void SetInitializerIcon(MonoScript script)
		{
			if(initializerIcon == null)
			{
				initializerIcon = EditorGUIUtility.IconContent(ICON_NAME).image as Texture2D;

				if(initializerIcon == null)
				{
					if(EditorApplication.isUpdating)
					{
						EditorApplication.delayCall += ()=> SetInitializerIcon(script);
						return;
					}

					#if DEV_MODE
					Debug.LogWarning($"Initializer icon '{ICON_NAME}' not found.");
					#endif
					return;
				}
			}

			SetIcon(script, initializerIcon);
		}

		private static void SetIcon(MonoScript script, Texture2D icon)
		{
			var currentIcon = EditorGUIUtility.GetIconForObject(script);
			if(currentIcon != icon)
			{
				EditorGUIUtility.SetIconForObject(script, icon);
			}
		}
	}
}
