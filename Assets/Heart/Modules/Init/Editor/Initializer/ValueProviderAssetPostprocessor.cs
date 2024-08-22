using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Class responsible for detecting when a value provider script that derives from
	/// <see cref="ScriptableObject"/> is imported and giving the script the Value Provider icon.
	/// </summary>
	internal sealed class ValueProviderAssetPostprocessor : AssetPostprocessor
	{
		private const string EDITOR_PREFS_KEY = "InitArgs.ValueProviders.DelayedProcessingQueue";
		private const string ICON_NAME = "AreaEffector2D Icon";
		private static Texture2D valueProviderIcon = null;

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
			if(TryDetermineIsValueProviderScript(path, out bool isValueProvider) && !isValueProvider)
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

		private static bool TryDetermineIsValueProviderScript(string path, out bool isValueProvider)
		{
			if(!path.EndsWith(".cs", StringComparison.Ordinal))
			{
				isValueProvider = false;
				return true;
			}

			var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			if(script == null)
			{
				isValueProvider = false;
				return true;
			}

			var type = script.GetClass();
			if(type == null)
			{
				isValueProvider = false;
				// MonoScript.GetClass() result can be temporarily null
				// for value providers when scripts are still being compiled.
				return !EditorApplication.isCompiling && !EditorApplication.isUpdating;
			}

			if(!type.IsAbstract
			&& typeof(ScriptableObject).IsAssignableFrom(type)
			&& (typeof(IValueProvider).IsAssignableFrom(type)
			|| typeof(IValueByTypeProvider).IsAssignableFrom(type)
			|| typeof(IValueByTypeProviderAsync).IsAssignableFrom(type)
			|| typeof(IValueProviderAsync).IsAssignableFrom(type)))
			{
				isValueProvider = true;
				return true;
			}

			isValueProvider = false;
			return true;
		}

		private static bool TryGetValueProviderScript(string path, out MonoScript script)
		{
			script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			if(script == null)
			{
				return false;
			}

			var type = script.GetClass();
			if(type != null
			&& !type.IsAbstract
			&& typeof(ScriptableObject).IsAssignableFrom(type)
			&& (typeof(IValueProvider).IsAssignableFrom(type)
			|| typeof(IValueByTypeProvider).IsAssignableFrom(type)
			|| typeof(IValueByTypeProviderAsync).IsAssignableFrom(type)
			|| typeof(IValueProviderAsync).IsAssignableFrom(type)))
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

			string[] paths = unprocessed.Split('|');
			foreach(string path in paths)
			{
				if(TryGetValueProviderScript(path, out var script))
				{
					if(!startedAssetEditing)
					{
						startedAssetEditing = true;
						AssetDatabase.StartAssetEditing();
					}

					SetValueProviderIcon(script);
				}
			}

			if(startedAssetEditing)
			{
				AssetDatabase.StopAssetEditing();
			}

			EditorPrefs.DeleteKey(EDITOR_PREFS_KEY);
		}

		private static void SetValueProviderIcon(MonoScript script)
		{
			if(valueProviderIcon == null)
			{
				valueProviderIcon = EditorGUIUtility.IconContent(ICON_NAME).image as Texture2D;

				if(valueProviderIcon == null)
				{
					if(EditorApplication.isUpdating)
					{
						EditorApplication.delayCall += ()=> SetValueProviderIcon(script);
						return;
					}

					#if DEV_MODE
					Debug.LogWarning($"Value provider icon '{ICON_NAME}' not found.");
					#endif
					return;
				}
			}

			SetIcon(script, valueProviderIcon);
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
