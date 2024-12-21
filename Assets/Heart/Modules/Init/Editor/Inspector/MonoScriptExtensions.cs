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
			if(!TryGetAssetImporterWithEditableUserData(script, out var assetImporter))
			{
				return false;
			}

			#if DEV_MODE
			var scriptPath = AssetDatabase.GetAssetPath(script);
			#endif

			string userData = assetImporter.userData;
			var userDataLines = userData.Length == 0 ? Array.Empty<string>() : userData.Split(new[] { "\r\n", "\n", ", " }, StringSplitOptions.RemoveEmptyEntries);
			string setUserDataLine = userDataKey + ": " + value;
			bool userDataLinesUpdated = false;
			for(int i = 0; i < userDataLines.Length; i++)
			{
				var userDataLine = userDataLines[i];
				if(!userDataLine.StartsWith(userDataKey + ": "))
				{
					continue;
				}

				if(string.Equals(userDataLine, setUserDataLine))
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

			userData = string.Join(", ", userDataLines);
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
		/// <param name="script"> Scripts whose metadata to read. </param>
		/// <param name="userDataKey"> Key for the metadata entry. </param>
		/// <param name="value">
		/// When this method returns, contains enum value that was read from user data, if it contained an entry with
		/// the given key; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns> <see langword="true"/> if the script's metadata is open for reading and writing; otherwise, <see langword="false"/>. </returns>
		public static bool TryGetUserData([DisallowNull] this MonoScript script, string userDataKey, out bool? value)
		{
			if(script.TryGetUserData(userDataKey, out int? valueFromMetadata))
			{
				value = valueFromMetadata is int and not 0;
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Get enum user data from the script's meta file.
		/// </summary>
		/// <param name="script"> Scripts whose metadata to read. </param>
		/// <param name="userDataKey"> Key for the metadata entry. </param>
		/// <param name="value">
		/// When this method returns, contains enum value that was read from user data, if it contained an entry with
		/// the given key; otherwise, <see langword="null"/>.
		/// </param>
		/// <typeparam name="TEnum"> Type of the enum. </typeparam>
		/// <returns> <see langword="true"/> if the script's metadata is open for reading and writing; otherwise, <see langword="false"/>. </returns>
		public static bool TryGetUserData<TEnum>([DisallowNull] this MonoScript script, string userDataKey, out TEnum? value) where TEnum : struct, Enum
		{
			if(script.TryGetUserData(userDataKey, out int? valueFromMetadata))
			{
				value = valueFromMetadata is int @int ? (TEnum)(object)@int : null;
				return true;
			}

			value = default;
			return false;
		}

		/// <returns> <see langword="true"/> if the script's metadata is open for reading and writing; otherwise, <see langword="false"/>. </returns>
		private static bool TryGetAllUserData([DisallowNull] this MonoScript script, out string userData)
		{
			if(!TryGetAssetImporterWithEditableUserData(script, out var assetImporter))
			{
				userData = default;
				return false;
			}

			userData = assetImporter.userData;
			return true;
		}

		/// <returns> <see langword="true"/> if the script's metadata is open for reading and writing; otherwise, <see langword="false"/>. </returns>
		private static bool TryGetAssetImporterWithEditableUserData([DisallowNull] this MonoScript script, out MonoImporter assetImporter)
		{
			if(!AssetDatabase.IsMetaFileOpenForEdit(script)
			|| AssetDatabase.GetAssetPath(script) is not string scriptPath)
			{
				assetImporter = null;
				return false;
			}

			assetImporter = AssetImporter.GetAtPath(scriptPath) as MonoImporter;
			if(!assetImporter)
			{
				return false;
			}

			#if UNITY_2023_2 || UNITY_2023_3 || UNITY_6000_0 || INIT_ARGS_DISABLE_WRITE_TO_METADATA
			// Certain Unity versions have a bug where userData always returns an empty string:
			// https://issuetracker.unity3d.com/issues/empty-string-is-returned-when-using-assetimporter-dot-userdata
			if(assetImporter.userData.Length == 0)
			{
				assetImporter = null;
				return false;
			}
			#endif

			return true;
		}

		/// <summary>
		/// Get integer user data from the script's meta file.
		/// </summary>
		/// <param name="value">
		/// When this method returns, contains enum value that was read from user data, if it contained an entry with
		/// the given key; otherwise, <see langword="null"/>.
		/// </param>
		/// <typeparam name="TEnum"> Type of the enum. </typeparam>
		/// <returns> <see langword="true"/> if the script's metadata is open for reading and writing; otherwise, <see langword="false"/>. </returns>
		public static bool TryGetUserData([DisallowNull] this MonoScript script, string userDataKey, out int? value)
		{
			if(!TryGetAllUserData(script, out var userData))
			{
				value = null;
				return false;
			}

			string[] userDataLines = userData.Length == 0 ? Array.Empty<string>() : userData.Split(new[] { "\r\n", "\n", ", " }, StringSplitOptions.RemoveEmptyEntries);
			string userDataPrefix = userDataKey + ": ";

			for(int i = 0; i < userDataLines.Length; i++)
			{
				string userDataLine = userDataLines[i];
				if(userDataLine.StartsWith(userDataPrefix))
				{
					if(int.TryParse(userDataLine.Substring(userDataPrefix.Length), out int valueFromLine))
					{
						value = valueFromLine;
						return true;
					}
				}
			}

			value = null;
			return true;
		}
	}
}