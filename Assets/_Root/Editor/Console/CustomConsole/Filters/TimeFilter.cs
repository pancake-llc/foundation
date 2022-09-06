using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;


namespace Needle.Console
{
	[Serializable]
	public struct LogTime
	{
		public string TimeString;

		[NonSerialized] private DateTime _time;
		[NonSerialized] private bool _hasTime;

		public DateTime Time
		{
			get
			{
				if (_hasTime) return _time;
				_hasTime = DateTime.TryParse(TimeString, out _time);
				return _time;
			}
		}


		public override string ToString()
		{
			return Time.ToString(CultureInfo.CurrentCulture);
		}
	}

	public class TimeFilter : FilterBase<LogTime>
	{
		public TimeFilter(ref List<FilterEntry> entries) : base(ref entries)
		{
		}

		public override string GetLabel(int index)
		{
			return this[index].Time.ToString("HH:mm:ss");
		}

		private readonly Dictionary<string, DateTime> logTimes = new Dictionary<string, DateTime>();

		protected override bool MatchFilter(LogTime entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			if (!logTimes.ContainsKey(message))
			{
				TryGetTime(message, out var logTime, out _);
				logTimes.Add(message, logTime);
			}

			var log = logTimes[message];

			if (IsSoloAtIndex(index))
				return log == entry.Time;
			return log <= entry.Time;
		}

		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (TryGetTime(preview, out _, out var timeString))
			{
				var lt = new LogTime {TimeString = timeString};
				AddContextMenuItem_Hide(menu, HideMenuItemPrefix + "Until " + timeString, lt);
				AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + "From " + timeString, lt);
			}
		}

		private static bool TryGetTime(string str, out DateTime dt, out string timeString)
		{
			var start = str.IndexOf("[", StringComparison.InvariantCulture);
			if (start < 0)
			{
				dt = DateTime.MaxValue;
				timeString = null;
				return false;
			}

			var end = str.IndexOf("]", StringComparison.InvariantCulture);
			if (end < 0 || end <= start)
			{
				dt = DateTime.MaxValue;
				timeString = null;
				return false;
			}

			timeString = str.Substring(start + 1, end - 1);
			if (string.IsNullOrEmpty(timeString))
			{
				dt = DateTime.MaxValue;
				timeString = null;
				return false;
			}

			return DateTime.TryParse(timeString, out dt);
		}
	}
}