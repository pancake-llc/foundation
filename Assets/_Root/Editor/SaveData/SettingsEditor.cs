using Pancake.SaveData;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.SaveData
{
	internal static class SettingsEditor
	{
		public static void Draw(Setting settings)
		{
			var style = EditorStyle.Get;

			settings.Location = (ELocation)EditorGUILayout.EnumPopup("Location", settings.Location);
			// If the location is File, show the Directory.
			if(settings.Location == ELocation.File) settings.eDirectory = (EDirectory)EditorGUILayout.EnumPopup("Directory", settings.eDirectory);

			settings.path = EditorGUILayout.TextField("Default File Path", settings.path);

			EditorGUILayout.Space();

			settings.encryptionType = (EEncryptionType)EditorGUILayout.EnumPopup("Encryption", settings.encryptionType);
			settings.encryptionPassword = EditorGUILayout.TextField("Encryption Password", settings.encryptionPassword);

            EditorGUILayout.Space();

            settings.compressionType = (ECompressionType) EditorGUILayout.EnumPopup("Compression", settings.compressionType);

            EditorGUILayout.Space();

			if(settings.showAdvancedSettings = EditorGUILayout.Foldout(settings.showAdvancedSettings, "Advanced Settings"))
			{
				EditorGUILayout.BeginVertical(style.area);

				settings.format = (EFormat)EditorGUILayout.EnumPopup("Format", settings.format);
                if (settings.format == EFormat.Json)
                    settings.prettyPrint = EditorGUILayout.Toggle(new GUIContent("Pretty print JSON"), settings.prettyPrint);
				settings.bufferSize = EditorGUILayout.IntField("Buffer Size", settings.bufferSize);
				settings.serializationDepthLimit = EditorGUILayout.IntField("Serialisation Depth", settings.serializationDepthLimit);

                EditorGUILayout.Space();

				EditorGUILayout.EndVertical();
			}
		}
    }
}
