using System.IO;
using UnityEditor;
using UnityEngine;

namespace Needle.Console
{
	public static class ConsoleUtils
	{
		internal static bool IsCompilerError(this LogEntryInfo entry)
		{
			return ConsoleList.HasMode(entry.mode, ConsoleWindow.Mode.ScriptCompileError | ConsoleWindow.Mode.GraphCompileError);
		}
		
		internal static bool TryPingFile(string file)
		{
			if (!string.IsNullOrEmpty(file) && File.Exists(file) && TryMakeProjectRelative(file, out file))
			{
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(file);
				EditorGUIUtility.PingObject(script);
				return true;
			}
			return false;
		}
		
		internal static bool TryMakeProjectRelative(string path, out string result)
		{
			var fp = Path.GetFullPath(path);
			fp = fp.Replace("\\", "/");
			var project = Path.GetFullPath(Application.dataPath + "/../").Replace("\\", "/");
			if (!fp.StartsWith(project))
			{
				if (PackageFilter.TryGetPackage(fp, out var package))
				{
					var rel = "Packages/" + package.name + "/" + fp.Substring(package.resolvedPath.Length);
					result = rel;
					return File.Exists(result);
				}
			}
			result = fp.Substring(project.Length);
			return File.Exists(result);
		}
	}
}