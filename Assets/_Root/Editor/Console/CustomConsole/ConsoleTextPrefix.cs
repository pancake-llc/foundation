using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Needle.Console
{
	internal static class ConsoleTextPrefix
	{
		[InitializeOnLoadMethod]
		private static void Init()
		{
			ClearCaches();

			// clear cache when colors change
			NeedleConsoleProjectSettings.ColorSettingsChanged += ClearCaches;
			NeedleConsoleSettings.Changed += ClearCaches;
			EditorApplication.playModeStateChanged += mode => ClearCaches();
		}

		private static void ClearCaches()
		{
			cachedInfo.Clear();
			cachedPrefix.Clear();
		}

		private static readonly LogEntry tempEntry = new LogEntry();


		private static readonly string[] onlyUseMethodNameFromLinesWithout = new[]
		{
			"UnityEngine.UnitySynchronizationContext",
			"UnityEngine.Debug",
			"UnityEngine.Logger",
			"UnityEngine.DebugLogHandler",
			"System.Runtime.CompilerServices"
		};

		private static readonly Dictionary<string, string> cachedInfo = new Dictionary<string, string>();
		private static readonly Dictionary<string, string> cachedPrefix = new Dictionary<string, string>();
		private static readonly StringBuilder keyBuilder = new StringBuilder();

		// called from console list with current list view element and console text

		internal static void ModifyText(ListViewElement element, ref string text)
		{
			// var rect = element.position;
			// GUI.DrawTexture(rect, Texture2D.whiteTexture);//, ScaleMode.StretchToFill, true, 1, Color.red, Vector4.one, Vector4.zero);

			using (new ProfilerMarker("ConsoleList.ModifyText").Auto())
			{
				var settings = NeedleConsoleSettings.instance;
				if (!settings.ShowLogPrefix && (string.IsNullOrWhiteSpace(settings.ColorMarker) || !settings.UseColorMarker))
				{
					return;
				}

				if (!LogEntries.GetEntryInternal(element.row, tempEntry))
				{
					return;
				}
				keyBuilder.Clear();
				keyBuilder.Append(tempEntry.file).Append(tempEntry.line).Append(tempEntry.column).Append(tempEntry.mode);
				
#if UNITY_2021_2_OR_NEWER
				if(string.IsNullOrWhiteSpace(tempEntry.file))
					keyBuilder.Append(tempEntry.identifier).Append(tempEntry.globalLineIndex);
#else
				if (tempEntry.file == "" && tempEntry.line == 0 && tempEntry.column == -1)
				{
					keyBuilder.Append(element.row);
				}
#endif
				
				var key = keyBuilder.Append(text).ToString();
				var isSelected = ConsoleList.IsSelectedRow(element.row);
				var cacheEntry = !isSelected;
				var isInCache = cachedInfo.ContainsKey(key);
				if (cacheEntry && isInCache)
				{
					using (new ProfilerMarker("ConsoleList.ModifyText cached").Auto())
					{
						text = cachedInfo[key];
						if (NeedleConsoleSettings.DevelopmentMode)
						{
							text += " \t<color=#ff99ff>CacheKey: " + key + "</color>";
						}
						return;
					}
				}

				using (new ProfilerMarker("ConsoleList.ModifyText (Not in cache)").Auto())
				{
					try
					{
						var filePath = tempEntry.file;
						var fileName = Path.GetFileNameWithoutExtension(filePath);
						const string colorPrefixDefault = "<color=#888888>";
						const string colorPrefixSelected = "<color=#ffffff>";
						// const string colorPrefixSelected = "<color=#111122>";
						var colorPrefix = isInCache && isSelected ? colorPrefixSelected : colorPrefixDefault;
						const string colorPostfix = "</color>";

						string GetPrefix()
						{
							if (fileName == "Debug.bindings") return "";
							if (!NeedleConsoleSettings.instance.ShowLogPrefix) return string.Empty;
							keyBuilder.Clear();
							keyBuilder.Append(tempEntry.file).Append(tempEntry.line).Append(tempEntry.column).Append(tempEntry.mode);
							keyBuilder.Append(tempEntry.message);
#if UNITY_2021_2_OR_NEWER
							if(string.IsNullOrWhiteSpace(tempEntry.file))
								keyBuilder.Append(tempEntry.identifier).Append(tempEntry.globalLineIndex);
#else
							if (tempEntry.file == "" && tempEntry.line == 0 && tempEntry.column == -1)
							{
								keyBuilder.Append(element.row);
							}
#endif
							var key2 = keyBuilder.ToString();
							if (!isSelected && cachedPrefix.TryGetValue(key2, out var cached))
							{
								return cached;
							}

							var str = default(string);
							if (TryGetMethodName(tempEntry.message, out var typeName, out var methodName))
							{
								if (string.IsNullOrWhiteSpace(typeName))
									str = fileName;
								else str = typeName;
								str += "." + methodName;
							}
							else if (!isSelected && cachedPrefix.TryGetValue(key2, out cached))
							{
								return cached;
							}

							if (string.IsNullOrWhiteSpace(str))
							{
								if (string.IsNullOrEmpty(fileName)) return string.Empty;
								str = fileName;
							}

							if (tempEntry.line > 0)
								str += ":" + tempEntry.line;


							// str = colorPrefix + "[" + str + "]" + colorPostfix;
							// str = "<b>" + str + "</b>";
							// str = "\t" + str;
							str = Prefix(str); // + " |";
							if (cacheEntry)
							{
								if (!cachedPrefix.ContainsKey(key2))
									cachedPrefix.Add(key2, str);
								else cachedPrefix[key2] = str;
							}
							return str;

							string Prefix(string s) => $"{colorPrefix} {s} {colorPostfix}";
						}


						// text = element.row.ToString();
						var endTimeIndex = text.IndexOf("] ", StringComparison.InvariantCulture);
						var prefix = GetPrefix();

						var colorKey = string.IsNullOrWhiteSpace(fileName) ? prefix : fileName;
						var colorMarker = settings.UseColorMarker ? NeedleConsoleSettings.instance.ColorMarker : string.Empty; // " ▍";
						if (settings.UseColorMarker && !string.IsNullOrWhiteSpace(colorMarker))
							LogColor.CalcLogColor(colorKey, ref colorMarker);

						// no time:
						if (endTimeIndex == -1)
						{
							// LogColor.AddColor(colorKey, ref text);
							RemoveFilePathInCompilerErrorMessages(ref text);
							text = $"{colorMarker}{prefix}{text}";
						}
						// contains time:
						else
						{
							var message = text.Substring(endTimeIndex + 1);
							RemoveFilePathInCompilerErrorMessages(ref message);
							text = $"{colorPrefix}{text.Substring(1, endTimeIndex - 1)}{colorPostfix} {colorMarker}{prefix}{message}";
						}

						if (cacheEntry)
						{
							if (!cachedInfo.ContainsKey(key))
								cachedInfo.Add(key, text);
							else cachedInfo[key] = text;
						}
					}
					catch (ArgumentException)
					{
						// sometimes filepath contains illegal characters and is not actually a path
						if (cacheEntry)
							cachedInfo.Add(key, text);
					}
					catch (Exception e)
					{
						Debug.LogException(e);
						if (cacheEntry)
							cachedInfo.Add(key, text);
					}
				}
			}
		}

		private static readonly Regex findEndOfFilePathRegex = new Regex(@"(\(\d{1,4},\d{1,3}\):)", RegexOptions.Compiled);

		private static void RemoveFilePathInCompilerErrorMessages(ref string str)
		{
			const ConsoleWindow.Mode modestoRemovePath = ConsoleWindow.Mode.ScriptCompileError
			                                         | ConsoleWindow.Mode.GraphCompileError
			                                         | ConsoleWindow.Mode.ScriptingWarning
			                                         | ConsoleWindow.Mode.ScriptCompileWarning 
			                                         | ConsoleWindow.Mode.AssetImportWarning
				;

			if (ConsoleList.HasMode(tempEntry.mode, modestoRemovePath))
			{
				var match = findEndOfFilePathRegex.Match(str);
				if (match.Success)
				{
					str = str.Substring(match.Index + match.Length).TrimStart();
				}
			}
		}

		private static bool TryGetMethodName(string message, out string typeName, out string methodName)
		{
			using (new ProfilerMarker("ConsoleList.ParseMethodName").Auto())
			{
				typeName = null;
				using (var rd = new StringReader(message))
				{
					var linesRead = 0;
					while (true)
					{
						var line = rd.ReadLine();
						if (line == null) break;
						if (onlyUseMethodNameFromLinesWithout.Any(line.Contains)) continue;
						if (!line.Contains(".cs")) continue;
						Match match;
						// https://regex101.com/r/qZ0cIT/2
						using (new ProfilerMarker("Regex").Auto())
							match = Regex.Match(line, @"([\. ](?<type_name>[\w\+]+?)){0,}[\.\:](?<method_name>\w+?)\(.+\.cs(:\d{1,})?",
								RegexOptions.Compiled | RegexOptions.ExplicitCapture);
						using (new ProfilerMarker("Handle Match").Auto())
						{
							// var match = matches[i];
							var type = match.Groups["type_name"];
							if (type.Success)
							{
								typeName = type.Value.Trim();
							}

							var group = match.Groups["method_name"];
							if (group.Success)
							{
								methodName = group.Value.Trim();

								// nicify local function names
								const string localPrefix = "g__";
								var localStart = methodName.IndexOf(localPrefix, StringComparison.InvariantCulture);
								if (localStart > 0)
								{
									var sub = methodName.Substring(localStart + localPrefix.Length);
									var localEnd = sub.IndexOf("|", StringComparison.InvariantCulture);
									if (localEnd > 0)
									{
										sub = sub.Substring(0, localEnd);
										if (!string.IsNullOrEmpty(sub))
											methodName = sub;
									}
								}

								return true;
							}
						}

						linesRead += 1;
						if (linesRead > 15) break;
					}
				}

				methodName = null;
				return false;
			}
		}
	}
}