using System.IO;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class GradleVerify : IVerifyBuildProcess
    {
        public bool OnVerify()
        {
            bool customMainGradle = AndroidBuildPipelineSettings.Instance.customMainGradle;
            const string path = "Plugins/Android/mainTemplate.gradle";
            string fullPath = Path.Combine(Application.dataPath, path);
            string fullPathMeta = Path.Combine(Application.dataPath, path + ".meta");
            if (customMainGradle)
            {
                if (!File.Exists(fullPath))
                {
                    if (EditorUtility.DisplayDialog("mainTemplate.gradle Not Found",
                            "mainTemplate not found at path Plugins/Android/mainTemplate.gradle",
                            "Create",
                            "Cancel"))
                    {
                        EditorCreator.CreateTextFile(EditorResources.MainGradleTemplate.text, "mainTemplate.gradle", "Assets/Plugins/Android");
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (File.Exists(fullPath))
                {
                    if (EditorUtility.DisplayDialog("mainTemplate.gradle Exist",
                            "mainTemplate found at path Plugins/Android/mainTemplate.gradle\nBecause customGradle in build setting disabled so you need remove mainTemplate.gradle",
                            "Remove",
                            "Cancel"))
                    {
                        File.Delete(fullPath);
                        File.Delete(fullPathMeta);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            const string pathGradlePoperty = "Plugins/Android/gradleTemplate.properties";
            string fullPathGradlePoperties = Path.Combine(Application.dataPath, pathGradlePoperty);
            if (!File.Exists(fullPathGradlePoperties))
            {
                if (EditorUtility.DisplayDialog("gradleTemplate.properties Not Found",
                        "gradleTemplate not found at path Plugins/Android/gradleTemplate.properties",
                        "Create",
                        "Cancel"))
                {
                    EditorCreator.CreateTextFile(EditorResources.GradlePropertiesTemplate.text, "gradleTemplate.properties", "Assets/Plugins/Android");
                }
                else return false;
            }

            const string pathGradleSetting = "Plugins/Android/settingsTemplate.gradle";
            string fullPathGradleSetting = Path.Combine(Application.dataPath, pathGradleSetting);
            if (!File.Exists(fullPathGradleSetting))
            {
                if (EditorUtility.DisplayDialog("settingsTemplate.gradle Not Found",
                        "settingsTemplate not found at path Plugins/Android/settingsTemplate.gradle",
                        "Create",
                        "Cancel"))
                {
                    EditorCreator.CreateTextFile(EditorResources.GradleSettingsTemplate.text, "settingsTemplate.gradle", "Assets/Plugins/Android");
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public void OnComplete(bool status)
        {
            Debug.Log(status ? "[Gradle] Verify Success".SetColor(Uniform.Success) : "[Gradle] Verify Failure".SetColor(Uniform.Error));
        }
    }
}