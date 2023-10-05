using System;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public static class ProjectWindowUtility
    {
        private class DoCreateScriptAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                UnityEngine.Object o =
                    typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetFromTemplate", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                        .Invoke(null, new object[] {pathName, resourceFile}) as UnityEngine.Object;

                string iconPath = SessionState.GetString("ProjectWindowUtility_icon", string.Empty);
                Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

                if (icon != null)
                {
                    MonoImporter monoImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o)) as MonoImporter;
                    monoImporter.SetIcon(icon);
                    monoImporter.SaveAndReimport();

                    SessionState.EraseString("ProjectWindowUtility_icon");
                }

                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

        private class DoCreateScriptableObject : EndNameEditAction
        {
            public Type scriptableType;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                ScriptableObject scriptableObject = CreateInstance(scriptableType);
                scriptableObject.name = Path.GetFileName(pathName);
                AssetDatabase.CreateAsset(scriptableObject, pathName);
                ProjectWindowUtil.ShowCreatedAsset(scriptableObject);
            }
        }

        public static void CreateScriptAssetFromTemplateFile(string templatePath, string defaultNewFileName, Texture2D icon)
        {
            if (templatePath == null)
            {
                throw new ArgumentNullException("templatePath");
            }

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("The template file \"" + templatePath + "\" could not be found.", templatePath);
            }

            if (string.IsNullOrEmpty(defaultNewFileName))
            {
                defaultNewFileName = Path.GetFileName(templatePath);
            }

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<DoCreateScriptAsset>(),
                $"{defaultNewFileName}.cs",
                icon,
                templatePath);
            SessionState.SetString("ProjectWindowUtility_icon", AssetDatabase.GetAssetPath(icon));
        }

        public static void CreateScriptableObject<T>(string defaultNewFileName, Texture2D icon) where T : ScriptableObject
        {
            if (string.IsNullOrEmpty(defaultNewFileName)) return;

            DoCreateScriptableObject createScriptableObject = ScriptableObject.CreateInstance<DoCreateScriptableObject>();
            createScriptableObject.scriptableType = typeof(T);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                createScriptableObject,
                $"{defaultNewFileName}.asset",
                icon,
                null);
        }
    }
}