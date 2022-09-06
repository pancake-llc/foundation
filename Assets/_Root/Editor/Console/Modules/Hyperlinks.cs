using System;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Needle.Console
{
	internal static class Hyperlinks
	{
		private static readonly Regex hyperlinks = new Regex(@"((?<brackets>\))?(?<prefix> in) (?<file>.*?):line (?<line>\d+)(?<post>.*))",
			RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly StringBuilder stacktraceBuilder = new StringBuilder();
		private static readonly StringBuilder fixStacktraceBuilder = new StringBuilder();

		public static void FixStacktrace(ref string stacktrace)
		{
			var lines = stacktrace.Split(new []{'\n'}, StringSplitOptions.RemoveEmptyEntries);
			
			stacktraceBuilder.Clear();
			
			foreach (var t in lines)
			{
				fixStacktraceBuilder.Clear();
				var line = t;
				// hyperlinks capture 
				var path = Fix(line, fixStacktraceBuilder, out var lineNumber);
				if (!string.IsNullOrEmpty(path))
				{
					// dont append path to editor only lines to force unity to open the previous file path
					if (line.Contains("DebugEditor.Log"))
					{
					}
					else
					{
						// path = path.Replace("\n", "");
						fixStacktraceBuilder.Append(path).Append(lineNumber).Append(")");
						line = fixStacktraceBuilder.ToString();
						Filepaths.TryMakeRelative(ref line);
					}
					
				}
				
				stacktraceBuilder.Append(line).Append("\n");
			}

			stacktrace = stacktraceBuilder.ToString();
		}

		private static readonly StringBuilder lineBuilder = new StringBuilder();
		
		/// <summary>
		/// parse demystify path format and reformat to unity hyperlink format
		/// </summary>
		/// <param name="line"></param>
		/// <returns>hyperlink that must be appended to line once further processing is done</returns>
		private static string Fix(string line, StringBuilder lineBuilder, out string lineNumber)
		{
			lineNumber = string.Empty;
			
			const string START = " in ";
			const string END = "line ";
			var begin = line.LastIndexOf(START, StringComparison.Ordinal);
			var end = line.LastIndexOf(END, StringComparison.Ordinal);
			if (begin < 0 || end < 0) return string.Empty;
			begin += START.Length;
			var pathLength = end - begin;
			var lineStart = end + END.Length;
			lineNumber = line.Substring(lineStart, line.Length - lineStart).TrimEnd('\r');
			var path = line.Substring(begin, pathLength);
			line = line.Substring(0, begin - START.Length);
			lineBuilder.Append(line).Append(" (at ");
			return path;
			
			
			// var match = hyperlinks.Match(line);
			// Hyperlinks.lineBuilder.Clear();
			// if (match.Success)
			// {
			// 	var file = match.Groups["file"];
			// 	var lineNr = match.Groups["line"];
			// 	var brackets = match.Groups["brackets"].ToString();
			// 	if (string.IsNullOrEmpty(brackets)) brackets = ")";
			// 	var post = match.Groups["post"];
			//
			// 	// var end = line.Substring(match.Index, line.Length - match.Index);
			// 	// line = line.Remove(match.Index, end.Length) + brackets;
			// 	// var newString = " (at " + file + ":" + lineNr + ")\n";
			// 	// if (post.Success)
			// 	// 	newString += post;
			// 	// return newString;
			//
			// 	var end = line.Substring(match.Index, line.Length - match.Index);
			// 	line = line.Remove(match.Index, end.Length);
			// 	lineBuilder.Append(line).Append(brackets);
			// 	
			// 	Hyperlinks.lineBuilder.Append(" (at ").Append(file).Append(":").Append(lineNr).Append(")\n");
			// 	if (post.Success) 
			// 		Hyperlinks.lineBuilder.Append(post);
			// 	return Hyperlinks.lineBuilder.ToString();
			// }
			//
			// return string.Empty;
		}


		public static void ApplyHyperlinkColor(ref string stacktrace)
		{
			if (string.IsNullOrEmpty(stacktrace)) return;
			var str = stacktrace;
			const string pattern = @"(?<open>\(at )(?<pre><a	href=.*?>)(?<path>.*)(?<post><\/a>)(?<close>\))";
			str = Regex.Replace(str, pattern, m =>
				{
					if (m.Success && SyntaxHighlighting.CurrentTheme.TryGetValue("link", out var col))
					{
						var open = "in ";// m.Groups["open"].Value;
						var close = string.Empty;// m.Groups["close"].Value;
						var pre = m.Groups["pre"].Value;
						var post = m.Groups["post"].Value;
						var path = m.Groups["path"].Value;
						ModifyFilePath(ref path);
						var col_0 = $"<color={col}>";
						var col_1 = "</color>";
						var res = $"{col_0}{open}{col_1}{pre}{col_0}{path}{col_1}{post}{col_0}{close}{col_1}";
						return res;
					}

					return m.Value;
				},
				RegexOptions.Compiled);
			stacktrace = str;
		}

		private static readonly Regex capturePackageNameInPath = new Regex(@".+[/\\](?<packageName>\w+\...+?)[/\\](.*)?[/\\](?<fileName>.*)$", RegexOptions.Compiled);
		
		private static void ModifyFilePath(ref string path)
		{
			if (NeedleConsoleSettings.instance.ShortenFilePaths == false) return;
			// Debug.Log(path);
			// var lineNumberStart = path.LastIndexOf(':');
			// if(lineNumberStart > 0)
			// 	path = path.Substring(0, lineNumberStart);
			// var fileNameIndexStart = path.LastIndexOf('/');
			// if (fileNameIndexStart > 0)
			// 	path = path.Substring(fileNameIndexStart + 1);

			var isPackageCache = path.Contains("PackageCache");
			var match = capturePackageNameInPath.Match(path);
			if (match.Success)
			{
				var package = match.Groups["packageName"].Value;
				var file = match.Groups["fileName"].Value;
				if (!string.IsNullOrEmpty(package) && !string.IsNullOrEmpty(file))
				{
					path = package + "/" + file;
					if (isPackageCache) path = "PackageCache/" + path;
				}
			}

		}
	}
}