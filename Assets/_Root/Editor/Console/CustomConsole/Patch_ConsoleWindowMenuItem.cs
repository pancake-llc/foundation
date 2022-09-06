using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{	
	internal class Patch_ConsoleWindowMenuItem : PatchBase
	{
		protected override IEnumerable<MethodBase> GetPatches()
		{
			var method = Patch_Console.ConsoleWindowType.GetMethod("AddItemsToMenu", BindingFlags.Public | BindingFlags.Instance);
			yield return method;
		}

		private static void Postfix(GenericMenu menu)
		{
			const string prefix = "Needle Console/";
			menu.AddItem(new GUIContent(prefix + "Code Preview"), NeedleConsoleSettings.instance.AllowCodePreview,
				data => { NeedleConsoleSettings.instance.AllowCodePreview = !NeedleConsoleSettings.instance.AllowCodePreview; }, null);
			menu.AddItem(new GUIContent(prefix + "Short Hyperlinks"), NeedleConsoleSettings.instance.ShortenFilePaths,
				data => { NeedleConsoleSettings.instance.ShortenFilePaths = !NeedleConsoleSettings.instance.ShortenFilePaths; }, null);
			menu.AddItem(new GUIContent(prefix + "Show Filename"), NeedleConsoleSettings.instance.ShowLogPrefix,
				data => { NeedleConsoleSettings.instance.ShowLogPrefix = !NeedleConsoleSettings.instance.ShowLogPrefix; }, null);
			menu.AddItem(new GUIContent(prefix + "Custom Console"), NeedleConsoleSettings.instance.CustomConsole,
				data => { NeedleConsoleSettings.instance.CustomConsole = !NeedleConsoleSettings.instance.CustomConsole; }, null);
			menu.AddItem(new GUIContent(prefix + "Individual Collapse"), NeedleConsoleSettings.instance.IndividualCollapse,
				data =>
				{
					NeedleConsoleSettings.instance.IndividualCollapse = !NeedleConsoleSettings.instance.IndividualCollapse;
					ConsoleFilter.MarkDirty();
				}, null);
		}
	}
}