using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Needle
{
	public class OpenURLCallback
	{
		[HyperlinkCallback(Priority = -1)]
		// ReSharper disable once UnusedMember.Local
		private static bool OnHyperlinkClicked(string path)
		{
			if (path.StartsWith("www.") || path.StartsWith("http://") || path.StartsWith("https://"))
			{
				Application.OpenURL(path);
				return true;
			}
				
			var result = Uri.TryCreate(path, UriKind.Absolute, out var uriResult) 
			             && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
				
			if (result)
			{
				var url = uriResult.ToString();
				Application.OpenURL(url);
				return true;
			}

			
			// if the path is not an url but some external file path open if with default app
			if (!path.EndsWith(".cs"))
			{
				if (File.Exists(path))
				{
					var absolute = Path.GetFullPath(path);
					var isExternalPath = !absolute.StartsWith(Application.dataPath);
					if (isExternalPath)
					{
						OpenWithDefaultProgram(absolute);
						return true;
					}
				}
				else if (Directory.Exists(path))
				{
					var absolute = Path.GetFullPath(path);
					try
					{
						// any files in here? we want to go directly into the directory
						var firstFile = Directory.GetFiles(path).FirstOrDefault();
						EditorUtility.RevealInFinder(firstFile != null ? firstFile : path);
						return true;
					}
					catch // GetFiles can throw
					{
						// ignored
					}
				}
			}
			

			return false;
		}
		
		private static void OpenWithDefaultProgram(string path)
		{
			var proc = new Process();
			proc.StartInfo.FileName = "explorer";
			proc.StartInfo.Arguments = "\"" + path + "\"";
			proc.Start();
		}
	}
}