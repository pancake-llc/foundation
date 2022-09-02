using UnityEditor;
using JetBrains.Annotations;

namespace Pancake.Debugging.Console
{
	internal static class DebugLogExtensionsMenuItems
	{
		[MenuItem("Tools/Pancake/Debug/Console+ %#C", priority = 101), UsedImplicitly]
		private static void Open()
		{
			ConsoleWindowPlusExperimental.Open();
		}

		[MenuItem("Tools/Pancake/Debug/Console+ (Legacy)", priority = 102), UsedImplicitly]
		private static void OpenConsoleWindowPlus()
		{
			ConsoleWindowPlus.Open();
		}
	}
}