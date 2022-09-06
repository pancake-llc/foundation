using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Needle.Console
{
	internal static class Filepaths
	{
		// pattern: match absolute disc path for cs files
		private const string Pattern = @" \(at (?<filepath>\w{1}\:\/.*\.cs)\:\d{1,}\)";
		private static readonly Regex Regex = new Regex(Pattern, RegexOptions.Compiled);

		private static readonly Dictionary<string, string> pathsCache = new Dictionary<string, string>();

		public static void TryMakeRelative(ref string line)
		{
			// unity sometimes fails to make paths relative to the project (e.g. when logging from local packages)
			try
			{
				var match = Regex.Match(line);
				if (!match.Success) return;
				var pathGroup = match.Groups["filepath"];
				if (!pathGroup.Success) return;
				var path = pathGroup.Value;
				if (pathsCache.TryGetValue(path, out var rel))
				{
					if (rel != null)
						line = line.Replace(pathGroup.Value, rel);
					return;
				}
				var filePath = new Uri(path, UriKind.RelativeOrAbsolute);
				if (filePath.IsAbsoluteUri)
				{
					var appPath = new Uri(Application.dataPath, UriKind.Absolute);
					// TODO: MakeRelativeUri produces quite a lot of garbage
					var relativePath = appPath.MakeRelativeUri(filePath).ToString();
					relativePath = WebUtility.UrlDecode(relativePath);
					// relativePath = relativePath.Replace("%20", " ");
					// if (makeHyperlink) relativePath = "<a href=\"" + pathGroup.Value + "\">" + relativePath + "</a>";
					line = line.Replace(path, relativePath);
					pathsCache.Add(path, relativePath);
					return;
				}
				pathsCache[path] = null;
			}
			catch
				// (Exception e)
			{
				// ignore
			}
		}
	}
}