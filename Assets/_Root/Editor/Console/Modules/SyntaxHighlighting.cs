using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Profiling;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	internal enum Highlighting
	{
		None = 0,
		Simple = 1,
		Complex = 2,
		TypesOnly = 3,
	}

	internal static class SyntaxHighlighting
	{
		private static readonly Dictionary<Highlighting, List<string>> regexPatterns = new Dictionary<Highlighting, List<string>>()
		{
			{Highlighting.None, null},
			{
				// Simple: https://regex101.com/r/sWR1X1/2 
				Highlighting.Simple, new List<string>()
				{
					@"(?<return_type>.+).*?[\. ](?<class>.*)\.(?<method_name>.+?)(?<params>\(.*?\))\+?",
					@"(?<exception>.*?\w*Exception:.+)",
				}
			},
			{
				// Complex: https://regex101.com/r/HegsIG/2
				Highlighting.Complex, new List<string>()
				{
					@"((?<new>new)|(((?<return_tuple>\(.*\))|((?<async>async)?( ?(?<return_type>(.*<.*>)|(?#return non generic)(.*?)))))) )?(?#capture namespaces)(?<namespace>.*(\.|\+))?(?<class>.*?)\.(?<method_name>.+?)(?<params>\(.*?\))\+?((?<func>\((?<func_params>.*?)\) => { })|((?<local_func>.*?)\((?<local_func_params>.*)\)))?",
					@"(?<exception>.*?\w*Exception:.+)",
				}
			},
			{
				// https://regex101.com/r/3Bc9EI/1
				Highlighting.TypesOnly, new List<string>()
				{
					@"(?<keywords>string |bool |int |long |uint |float |double |object |Action |async |Object |byte |in |out |ref |null )",
					@"(?<keywords><string>|<bool>|<int>|<long>|<uint>|<float>|<double>|<object>|<Action>|<async>|<Object>|<byte>|<in>|<out>|<ref>|<null>|<static )",
					@"(?<exception>.*?\w*Exception:.+)",
				}
			}
		};

		internal static IEnumerable<string> GetCodeSyntaxHighlightingPatterns()
		{
			yield return @"(?<comment>\/\/.*)";
			yield return "\"(?<string_literal>.+)\"";
			yield return @"(?<define>\s{0,}\#(if|define|elif) \w+)";
			yield return @"(?<define>\s{0,}\#(endif)\s{0,})";
			yield return @"(?<exception> throw new \w*Exception)";
			yield return @"(?<exception>throw )";
			yield return @"(?<keywords>new )";
			yield return @" (?<keywords>this|new|base)[\. \(]";
			yield return @"(?<keywords>var|string|bool|int|long|uint|float|double|object|Action|async|Object|byte|in|out|ref|null)[ \,\)\>]";
			yield return @"(?<keywords>return|void||await|class|struct|public|private|internal|static|readonly)[ \,\)\>]";
			yield return @"(?<keywords>(if|while|foreach|for) ?)\(";
		}

		internal static List<string> CurrentPatternsList
		{
			get
			{
				regexPatterns.TryGetValue(NeedleConsoleSettings.instance.SyntaxHighlighting, out var patterns);
				return patterns;
			}
		}

		internal static void OnSyntaxHighlightingModeHasChanged() => _currentPattern = null;

		private static string _currentPattern;

		internal static string CurrentPattern
		{
			get
			{
				if (_currentPattern == null)
				{
					_currentPattern = string.Join("|", CurrentPatternsList);

					if (NeedleConsoleSettings.DevelopmentMode)
					{
						// this is just to give patching time to being loaded to add syntax highlighting to this call too :)
						void NextFrame()
						{
							Debug.Log("<b>Patterns</b>: " + _currentPattern);
							EditorApplication.update -= NextFrame;
						}

						EditorApplication.update += NextFrame;
					}
				}

				return _currentPattern;
			}
		}

		public static string GetPattern(Highlighting highlighting)
		{
			return regexPatterns.TryGetValue(highlighting, out var patterns) ? string.Join("|", patterns) : null;
		}


		internal static readonly Dictionary<string, string> CurrentTheme = new Dictionary<string, string>();

		public static void AddSyntaxHighlighting(ref string line, Dictionary<string, string> colorDict = null)
		{
			var pattern = CurrentPattern;
			if (colorDict == null) colorDict = CurrentTheme;
			AddSyntaxHighlighting(pattern, colorDict, ref line);
		}

		private const string linkPrefix = " (at ";
		private static readonly Regex hyperlink = new Regex(@" \(at (?<hyperlink>.+)\:(?<line>.+)\)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private static readonly Dictionary<string, Regex> highlight = new Dictionary<string, Regex>();
		
		public static void AddSyntaxHighlighting(string pattern, Dictionary<string, string> colorDict, ref string line, bool trim = true)
		{
			using (new ProfilerMarker("Demystify.AddSyntaxHighlighting").Auto())
			{
				if (string.IsNullOrEmpty(pattern)) return;

				var evalMarker = new ProfilerMarker("EvalMatch");
				string Eval(Match m)
				{
					using (evalMarker.Auto())
					{
						if (m.Groups.Count <= 1) return m.Value;
						var str = m.Value;
						var separators = new string[1];
						for (var index = m.Groups.Count - 1; index >= 1; index--)
						{
							var @group = m.Groups[index];
							if (string.IsNullOrWhiteSpace(@group.Value) || string.IsNullOrEmpty(@group.Name)) continue;
							if (colorDict.TryGetValue(@group.Name, out var col))
							{
								// check if we have to use regex to replace it
								separators[0] = group.Value;
								var occ = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
								if (occ.Length >= 2 && occ[0] != "\"" && occ[occ.Length-1] != "\"")
								{
									var replaced = false;
									using (new ProfilerMarker("Regex.Replace").Auto())
									{
										var _pattern = @group.Value;
										using (new ProfilerMarker("Regex.Escape").Auto())
											_pattern = Regex.Escape(_pattern);
										str = Regex.Replace(@str, _pattern,
											m1 =>
											{
												if (replaced) return m1.Value;
												if (m1.Index != group.Index)
													return m1.Value;
												replaced = true;
												
												// if (group.Name == "namespace")
												// {
												// 	return string.Empty;
												// }
												
												using (new ProfilerMarker("Concat").Auto())
													return "<color=" + col + ">" + @group.Value + "</color>";
											}, RegexOptions.Compiled);
									}
								}
								else
								{
									using(new ProfilerMarker("String.Replace").Auto())
										str = str.Replace(group.Value, "<color=" + col + ">" + @group.Value + "</color>");
								}
							}
							// else if()
							// {
							// 	if (int.TryParse(@group.Name, out _)) continue;
							// 	if (!string.IsNullOrWhiteSpace(@group.Name) && @group.Name.Length > 1)
							// 		Debug.LogWarning("Missing color entry for " + @group.Name + ", matched for " + @group);
							// }
						}

						return str;
					}
				}


				var link = hyperlink.Match(line);
				if (link.Success)
				{
					line = line.Remove(link.Groups["hyperlink"].Index - linkPrefix.Length);
				}

				if (trim) line = line.TrimStart();

				if (!highlight.ContainsKey(pattern))
					highlight.Add(pattern, new Regex(pattern, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture));
				line = highlight[pattern].Replace(line, Eval);
				if (link.Success)
				{
					// https://github.com/Unity-Technologies/UnityCsReference/blob/98cc8a97afc8cb990bc0c89165bdb276cbcc8ec4/Editor/Mono/ConsoleWindow.cs#L864
					if (colorDict.TryGetValue("link", out var col))
					{
						var displayUrl = link.Value.Substring(linkPrefix.Length).TrimEnd(')');
						line += $"<a href=\"{link.Groups["hyperlink"].Value}\" line=\"{link.Groups["line"].Value}\"><color={col}> in {displayUrl}</color></a>";
					}
					else
						line += link.Value;
				}
			}
			
		}
	}
}
