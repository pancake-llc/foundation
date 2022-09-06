using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal static class Draw
	{
		public static void FilterList(IReadOnlyList<IConsoleFilter> list)
		{
			var any = false;
			if (list.Count > 0)
			{
				EditorGUI.BeginChangeCheck();
				var anySolo = list.Any(f => f.HasAnySolo());
				foreach (var filter in list)
				{
					if (filter.Count <= 0) continue;
					any = true;
					var prevColor = filter.BeforeOnGUI(anySolo);
					filter.OnGUI();
					filter.AfterOnGUI(prevColor);
				}

				if (EditorGUI.EndChangeCheck())
				{
					ConsoleFilter.MarkDirty();
				}
			}

			if (!any)
			{
				ConsoleFilterPreset.DrawHowToFilterHelpBox();
			}
		}
	}
}