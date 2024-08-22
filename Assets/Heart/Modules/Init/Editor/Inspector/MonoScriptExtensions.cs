using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
#if DEV_MODE
#endif
#if ODIN_INSPECTOR
#endif

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Extension methods for <see cref="MonoScript"/>.
	/// </summary>
	internal static class MonoScriptExtensions
	{
		/// <summary>
		/// Store boolean user data in the script's meta file.
		/// </summary>
		public static bool TrySetUserData([DisallowNull] this MonoScript script, string userDataKey, bool value, bool defaultValue = false) => TrySetUserData(script, userDataKey, value ? 1 : 0, defaultValue ? 1 : 0);

		/// <summary>
		/// Store enum user data in the script's meta file.
		/// </summary>
		public static bool TrySetUserData<TEnum>([DisallowNull] this MonoScript script, string userDataKey, TEnum value, TEnum defaultValue = default) where TEnum : Enum => TrySetUserData(script, userDataKey, (int)(object)value, (int)(object)defaultValue);

		/// <summary>
		/// Store integer user data in the script's meta file.
		/// </summary>
		public static bool TrySetUserData([DisallowNull] this MonoScript script, string userDataKey, int value, int defaultValue = 0)
		{
			// Use EditorPrefs as fallback, in cases where type is inside a DLL etc.
			if(!AssetDatabase.IsMetaFileOpenForEdit(script)
			|| AssetDatabase.GetAssetPath(script) is not string scriptPath
			|| AssetImporter.GetAtPath(scriptPath) is not AssetImporter assetImporter)
			{
				return false;
			}

			string userData = assetImporter.userData;

			#if UNITY_2023_2 || UNITY_2023_3 || UNITY_6000_0 || INIT_ARGS_DISABLE_WRITE_TO_METADATA
			// Certain Unity versions have a bug where userData always returns an empty string:
			// https://issuetracker.unity3d.com/issues/empty-string-is-returned-when-using-assetimporter-dot-userdata
			if(userData.Length == 0)
			{
				return false;
			}
			#endif

			string[] userDataLines = userData.Length == 0 ? Array.Empty<string>() : userData.Split('\n', StringSplitOptions.None);
			string setUserDataLine = userDataKey + ": " + value;
			bool userDataLinesUpdated = false;
			for(int i = 0; i < userDataLines.Length; i++)
			{
				string userDataLine = userDataLines[i];
				if(!userDataLine.StartsWith(userDataKey + ": "))
				{
					continue;
				}
				
				if(userDataLine == setUserDataLine)
				{
					return true;
				}

				if(value == defaultValue)
				{
					var list = new List<string>(userDataLines);
					list.RemoveAt(i);
					userDataLines = list.ToArray();
				}
				else
				{
					userDataLines[i] = setUserDataLine;
				}

				userDataLinesUpdated = true;
				break;
			}

			if(!userDataLinesUpdated)
			{
				if(value == defaultValue)
				{
					return true;
				}

				if(userData.Length == 0)
				{
					userDataLines = new[] { setUserDataLine };
				}
				else
				{
					int lastIndex = userDataLines.Length;
					Array.Resize(ref userDataLines, lastIndex + 1);
					userDataLines[lastIndex] = setUserDataLine;
				}
			}

			userData = string.Join('\n', userDataLines);
			assetImporter.userData = userData;
			assetImporter.SaveAndReimport();

			#if DEV_MODE
			EditorApplication.delayCall += () => 
			{
				var importer = AssetImporter.GetAtPath(scriptPath);
				if(importer.userData != userData)
				{
					Debug.LogWarning("Failed to serialize user data into '" + scriptPath  +".meta'.");
				}
			};
			#endif

			return true;
		}

		/// <summary>
		/// Get boolean user data from the script's meta file.
		/// </summary>
		public static bool TryGetUserData([DisallowNull] this MonoScript script, string userDataKey, out bool value)
		{
			if(script.TryGetUserData(userDataKey, out int intValue))
			{
				value = intValue != 0;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Get enum user data from the script's meta file.
		/// </summary>
		public static bool TryGetUserData<TEnum>([DisallowNull] this MonoScript script, string userDataKey, out TEnum value) where TEnum : Enum
		{
			if(script.TryGetUserData(userDataKey, out int intValue))
			{
				value = (TEnum)(object)intValue;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Get integer user data from the script's meta file.
		/// </summary>
		public static bool TryGetUserData([DisallowNull] this MonoScript script, string userDataKey, out int value)
		{
			// Use EditorPrefs as fallback, in cases where type is inside a DLL etc.
			if(!AssetDatabase.IsMetaFileOpenForEdit(script)
			|| AssetDatabase.GetAssetPath(script) is not string scriptPath
			|| AssetImporter.GetAtPath(scriptPath) is not MonoImporter assetImporter)
			{
				value = default;
				return false;
			}

			string userData = assetImporter.userData;
			string[] userDataLines = userData.Length == 0 ? Array.Empty<string>() : userData.Split('\n', StringSplitOptions.None);
			string userDataPrefix = userDataKey + ": ";

			for(int i = 0; i < userDataLines.Length; i++)
			{
				string userDataLine = userDataLines[i];
				if(userDataLine.StartsWith(userDataPrefix))
				{
					return int.TryParse(userDataLine.Substring(userDataPrefix.Length), out value);
				}
			}

			value = default;
			return false;
		}
	}
}