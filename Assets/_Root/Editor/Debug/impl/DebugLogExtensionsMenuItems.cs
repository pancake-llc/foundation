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
	}
}