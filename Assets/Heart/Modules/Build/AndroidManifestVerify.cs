using System.IO;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace PancakeEditor
{
    public class AndroidManifestVerify : IVerifyBuildProcess
    {
        public bool OnVerify()
        {
            const string path = "Plugins/Android/AndroidManifest.xml";
            string fullPath = Path.Combine(Application.dataPath, path);
            if (!File.Exists(fullPath))
            {
                if (EditorUtility.DisplayDialog("AndroidManifest Not Found", "AndroidManifest not found at path Plugins/Android/AndroidManifest.xml", "Create", "Cancel"))
                {
                    EditorCreator.CreateTextFile(EditorResources.AndroidManifestTemplate.text, "AndroidManifest.xml", "Assets/Plugins/Android");
                    return true;
                }

                return false;
            }

            string content = File.ReadAllText(fullPath);

            if (!content.Contains("android:allowNativeHeapPointerTagging=\"false\""))
            {
                if (EditorUtility.DisplayDialog("AndroidManifest Issue",
                        "AndroidManifest has not declared allowNativeHeapPointerTagging inside the application tag",
                        "Add",
                        "Cancel"))
                {
                    content = content.Replace("<application", "<application android:allowNativeHeapPointerTagging=\"false\"");
                    File.WriteAllText(fullPath, content);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return true;
                }

                return false;
            }

            if (!content.Contains("android:debuggable=\"false\""))
            {
                if (EditorUtility.DisplayDialog("AndroidManifest Issue",
                        "AndroidManifest has not declared android:debuggable=false inside the application tag",
                        "Add",
                        "Cancel"))
                {
                    content = content.Replace("<application", "<application android:debuggable=\"false\"");
                    File.WriteAllText(fullPath, content);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return true;
                }

                return false;
            }

            if (content.Contains("android:debuggable=\"true\""))
            {
                if (EditorUtility.DisplayDialog("AndroidManifest Issue",
                        "AndroidManifest value of android:debuggable needs to be set to false inside application tag",
                        "Update",
                        "Cancel"))
                {
                    content = content.Replace("android:debuggable=\"true\"", "android:debuggable=\"false\"");
                    File.WriteAllText(fullPath, content);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    return true;
                }

                return false;
            }

            return true;
        }

        public void OnComplete(bool status)
        {
            Debug.Log(status ? "[AndroidManifest] Verify Success".SetColor(Uniform.Success) : "[AndroidManifest] Verify Failure".SetColor(Uniform.Error));
        }
    }
}