using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;

namespace Needle.Console
{
	
	[Serializable]
	public class WarningFilter : FilterBase<string>
	{
		public WarningFilter(ref List<FilterEntry> list) : base(ref list){}
		
		public override string GetLabel(int index)
		{
			return this[index];
		}

		protected override bool MatchFilter(string entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			var warning = TryGetWarning(message);
			if (warning == null) return false;
			return entry == warning;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (string.IsNullOrEmpty(clickedLog.file)) return;
			if (clickedLog.line <= 0) return;
			var warning = TryGetWarning(preview);
			if (warning == null) return;
			// var fl = default(string);
			var text = warning.Substring(0, warning.Length-1);
			text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
			AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, warning);
			AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, warning);
		}

		private string TryGetWarning(string text)
		{
			const string marker = "warning CS"; 
			var begin = text.IndexOf(marker, StringComparison.Ordinal);
			if (begin <= 0) return null;
			var end = text.IndexOf(" ", begin+marker.Length, StringComparison.Ordinal);
			return text.Substring(begin, end - begin); 
		}
	}
}