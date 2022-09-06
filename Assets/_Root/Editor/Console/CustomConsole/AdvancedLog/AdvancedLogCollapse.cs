using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	internal class AdvancedLogCollapse : ICustomLogCollapser
	{
		internal static IReadOnlyDictionary<int, AdvancedLogData> LogsData => logsData;

		private readonly List<AdvancedLogEntry> selectedLogs;

		public AdvancedLogCollapse(List<AdvancedLogEntry> selectedLogs)
		{
			this.selectedLogs = selectedLogs;
		}

		private static readonly Dictionary<string, int> logs = new Dictionary<string, int>();
		private static readonly Dictionary<int, AdvancedLogData> logsData = new Dictionary<int, AdvancedLogData>();

		private static readonly StringBuilder builder = new StringBuilder();

		internal void ClearCache()
		{
			logs.Clear();
			logsData.Clear();
		}

		// private static readonly Dictionary<string, string> fileContent = new Dictionary<string, string>();
		// private static readonly List<int> notFoundCandidates = new List<int>();

		private string lastKey;
		private int messageCounter;

		public bool OnHandleLog(LogEntry entry, int row, string preview, List<CachedConsoleInfo> entries)
		{
			if (selectedLogs.Count <= 0) return false;

			builder.Clear();
			builder.Append(entry.file).Append("::").Append(entry.line);
			if (entry.instanceID != 0) builder.Append("@").Append(entry.instanceID);
			var key = builder.ToString();
			if (NeedleConsoleSettings.instance.IndividualCollapsePreserveContext)
			{
				if (!string.IsNullOrEmpty(lastKey) && key != lastKey) messageCounter += 1;
				lastKey = key;
				builder.Append("-").Append(messageCounter);
				key = builder.ToString();
			}

			// notFoundCandidates.Clear();
			for (var i = 0; i < selectedLogs.Count; i++)
			{
				var selected = selectedLogs[i];
				if (!selected.Active) continue;
				var matches = selected.Line == entry.line && selected.File == entry.file;

				if (!matches)
				{
					// if (selected.File == entry.file)
					// {
					// notFoundCandidates.Add(i);
					// }

					continue;
				}

				// entry.message += "\n" + UnityDemystify.DemystifyEndMarker;
				var newEntry = new CachedConsoleInfo()
				{
					entry = new LogEntryInfo(entry),
					row = row,
					str = preview,
					collapseCount = 1
				};

				AdvancedLogData data;
				if (logs.TryGetValue(key, out var index))
				{
					var ex = entries[index];
					newEntry.row = ex.row;
					newEntry.collapseCount = ex.collapseCount + 1;
					entries[index] = newEntry;
					data = logsData[index];
				}
				else
				{
					var newIndex = entries.Count;
					logs.Add(key, newIndex);
					entries.Add(newEntry);

					data = new AdvancedLogData();
					logsData.Add(newIndex, data);
				}

				// var e = entries[logs[key]];
				// e.str += collapseCounter;
				// entries[logs[key]] = e;
				
				var id = 0;
				data.AddData(preview, id++);

				// parse data
				// const string timestampStart = "[";
				// const string timestampEnd = "] ";
				// var timestampIndex = preview.IndexOf(timestampEnd, StringComparison.Ordinal);
				// var messageStart = timestampIndex > 0 ? (timestampIndex + timestampEnd.Length) : 0;
				// ParseLogData(data, preview, messageStart, id);
				return true;
			}

			// if 
			// if (notFoundCandidates.Count > 0)
			// {
			// }

			return false;
		}

		// number matcher https://regex101.com/r/D0dFIj/1/ -> [-\d.]+
		// non number matcher https://regex101.com/r/VRXwpC/1/
		// grouped numbers, separated by ,s https://regex101.com/r/GJVz7t/1 -> [,-.\d ]+
		//TODO: capture arrays

		private static readonly Regex numberMatcher = new Regex(@"[,-.\d ]+", RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly StringBuilder valueBuilder = new StringBuilder();
		private static readonly List<float> vectorValues = new List<float>(4);

		private void ParseLogData(AdvancedLogData data, string text, int startIndex, int id)
		{
			var numbers = numberMatcher.Matches(text, startIndex);
			// id marks the # of value fields in a log message

			foreach (Match match in numbers)
			{
				var str = match.Value;

				if (str.Length == 0) continue;
				// ignore empty spaces
				if (str.Length == 1 && str == " ") continue;

				// vector types:
				const char vectorSeparator = ',';
				if (str.Contains(vectorSeparator))
				{
					// TODO: test format of matrices if we want to support that at some point 
					valueBuilder.Clear();
					vectorValues.Clear();

					void TryParseCollectedValue()
					{
						if (valueBuilder.Length <= 0) return;
						var parsed = valueBuilder.ToString();
						valueBuilder.Clear();
						if (float.TryParse(parsed, out var val))
							vectorValues.Add(val);
					}

					for (var i = 0; i < str.Length; i++)
					{
						var c = str[i];
						switch (c)
						{
							case vectorSeparator:
								TryParseCollectedValue();
								break;
							case ' ':
								continue;
							default:
								valueBuilder.Append(c);
								break;
						}
					}

					TryParseCollectedValue();
					switch (vectorValues.Count)
					{
						case 1:
							data.AddData(vectorValues[0], id);
							break;
						case 2:
							data.AddData(new Vector2(vectorValues[0], vectorValues[1]), id);
							break;
						case 3:
							data.AddData(new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]), id);
							break;
						case 4:
							data.AddData(new Vector4(vectorValues[0], vectorValues[1], vectorValues[2], vectorValues[3]), id);
							break;
					}
				}
				// float types:
				else if (str.Contains("."))
				{
					if (float.TryParse(str, out var fl))
					{
						data.AddData(fl, id);
					}
				}

				// TODO integer or string types


				id += 1;
			}
		}
	}
}