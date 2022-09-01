using UnityEngine;
using UnityEditor;
using JetBrains.Annotations;
using UnityEditor.SceneManagement;

namespace Pancake.Debugging.Console
{
	internal static class DebugLogExtensionsMenuItems
	{
		[MenuItem("Window/Debugging/Console+ %#C", priority = 101), UsedImplicitly]
		private static void Open()
		{
			ConsoleWindowPlusExperimental.Open();
		}

		[MenuItem("Window/Debugging/Console+ (Legacy)", priority = 102), UsedImplicitly]
		private static void OpenConsoleWindowPlus()
		{
			ConsoleWindowPlus.Open();
		}

		[MenuItem("Window/Debugging/Debug.Log Extensions/Project Settings"), UsedImplicitly]
		private static void OpenSettings()
		{
			#if UNITY_2019_1_OR_NEWER
			SettingsService.OpenProjectSettings("Project/Console");
			#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
			#endif
		}

		[MenuItem("Window/Debugging/Debug.Log Extensions/User Preferences"), UsedImplicitly]
		private static void OpenPreferences()
		{
			#if UNITY_2019_1_OR_NEWER
			SettingsService.OpenUserPreferences("Preferences/Console");
			#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
			#endif
		}

		[MenuItem("Window/Debugging/Debug.Log Extensions/Demo"), UsedImplicitly]
		private static void OpenDemo()
		{
			var sceneGuids = AssetDatabase.FindAssets("Debug.Log Extensions Demo t:SceneAsset");
			if(sceneGuids.Length == 0)
			{
				if(EditorUtility.DisplayDialog("Demo Package Not Found", "Debug.Log Extensions Demo scene was not found at path\nSisus/Debug.Log Extensions/Demo/Debug.Log Extensions Demo.unity.\n\nWould you like to visit the Asset Store page from where you can reinstall Debug.Log Extensions along with the demo scene?", "Open Store Page", "Cancel"))
				{
					Application.OpenURL("http://u3d.as/1Lcj");
				}
				return;
			}

			var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);
			EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		}
	}
}