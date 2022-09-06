using UnityEngine;

namespace Needle.Console
{
	public static class ConsoleFilterExtensions
	{
		public static Color DisabledColor =>  new Color(.7f, .7f, .7f, .5f);

		public static Color BeforeOnGUI(this IConsoleFilter fil, bool anySolo)
		{
			var prevColor = UnityEngine.GUI.color;
			if (anySolo && !fil.HasAnySolo()) UnityEngine.GUI.color = DisabledColor;
			return prevColor;
		}
		
		public static void AfterOnGUI(this IConsoleFilter fil, Color prevColor)
		{
			UnityEngine.GUI.color = prevColor;
		}
	}
}