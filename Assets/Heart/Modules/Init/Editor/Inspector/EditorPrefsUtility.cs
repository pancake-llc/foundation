using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;

namespace Sisus.Init.EditorOnly.Internal
{
	/// <summary>
	/// Utility class that can be used to record and retrieve script-specific user data
	/// via EditorPrefs, using the pattern "InitArgs." + classType.Name + "." + userDataKey
	/// for the key.
	/// </summary>
	internal static class EditorPrefsUtility
	{
		public static void SetUserData([DisallowNull] Type classType, [DisallowNull] string userDataKey, bool value, bool defaultValue = false)
		{
			string editorPrefsKey = GetEditorPrefsKey(classType, userDataKey);

			if(value != defaultValue)
			{
				EditorPrefs.SetBool(editorPrefsKey, value);
			}
			else if(EditorPrefs.HasKey(editorPrefsKey))
			{
				EditorPrefs.DeleteKey(editorPrefsKey);
			}
		}

		public static void SetUserData([DisallowNull] string userDataKey, bool value, bool defaultValue = false)
		{
			string editorPrefsKey = GetEditorPrefsKey(userDataKey);

			if(value != defaultValue)
			{
				EditorPrefs.SetBool(editorPrefsKey, value);
			}
			else if(EditorPrefs.HasKey(editorPrefsKey))
			{
				EditorPrefs.DeleteKey(editorPrefsKey);
			}
		}

		public static void SetUserData([DisallowNull] Type classType, string userDataKey, int value, int defaultValue = 0)
		{
			string editorPrefsKey = GetEditorPrefsKey(classType, userDataKey);

			if(value != defaultValue)
			{
				EditorPrefs.SetInt(editorPrefsKey, value);
			}
			else if(EditorPrefs.HasKey(editorPrefsKey))
			{
				EditorPrefs.DeleteKey(editorPrefsKey);
			}
		}

		public static void SetUserData<TEnum>([DisallowNull] Type classType, string userDataKey, TEnum value, TEnum defaultValue = default) where TEnum : Enum
		{
			string editorPrefsKey = GetEditorPrefsKey(classType, userDataKey);

			if(!EqualityComparer<TEnum>.Default.Equals(value, defaultValue))
			{
				EditorPrefs.SetInt(editorPrefsKey, (int)(object)value);
			}
			else if(EditorPrefs.HasKey(editorPrefsKey))
			{
				EditorPrefs.DeleteKey(editorPrefsKey);
			}
		}

		public static bool GetBoolUserData([DisallowNull] Type classType, string userDataKey, bool defaultValue = false) => EditorPrefs.GetBool(GetEditorPrefsKey(classType, userDataKey), defaultValue);
		public static bool GetBoolUserData(string userDataKey, bool defaultValue = false) => EditorPrefs.GetBool(GetEditorPrefsKey(userDataKey), defaultValue);
		public static int GetIntUserData([DisallowNull] Type classType, string userDataKey, int defaultValue = 0) => EditorPrefs.GetInt(GetEditorPrefsKey(classType, userDataKey), defaultValue);
		public static TEnum GetEnumUserData<TEnum>([DisallowNull] Type classType, string userDataKey, TEnum defaultValue = default) where TEnum : Enum => (TEnum)(object)EditorPrefs.GetInt(GetEditorPrefsKey(classType, userDataKey), (int)(object)defaultValue);

		private static string GetEditorPrefsKey([DisallowNull] Type classType, [DisallowNull] string userDataKey) => "InitArgs." + classType.Name + "." + userDataKey;
		private static string GetEditorPrefsKey(string userDataKey) => "InitArgs." + userDataKey;
	}
}