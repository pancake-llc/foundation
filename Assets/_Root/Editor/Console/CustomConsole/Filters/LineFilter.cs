using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	[Serializable]
	public struct FileLine
	{
		public string file;
		public int line;

		public override string ToString()
		{
			return file + ":" + line;
		}
	}
	
	[Serializable]
	public class LineFilter : FilterBase<FileLine>
	{
		public LineFilter(ref List<FilterEntry> list) : base(ref list){}
		
		public override string GetLabel(int index) 
		{
			var e = this[index];
			return Path.GetFileName(e.file) + ":" + e.line;
		}

		protected override bool MatchFilter(FileLine entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			return entry.line == info.line && entry.file == info.file;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (string.IsNullOrEmpty(clickedLog.file)) return;
			if (clickedLog.line <= 0) return;
			var fileName = Path.GetFileName(clickedLog.file);
			var fl = new FileLine {file = clickedLog.file, line = clickedLog.line};
			var text = "Line " + fileName + ":" + clickedLog.line;
			AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, fl);
			AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, fl);
		}
	}
}