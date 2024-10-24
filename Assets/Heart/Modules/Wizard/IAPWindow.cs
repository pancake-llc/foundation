#if PANCAKE_IAP
using System.IO;
using Pancake.IAP;
#endif
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class IAPWindow
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_IAP
            var iapSettings = Resources.Load<IAPSettings>(nameof(IAPSettings));
            if (iapSettings == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink_500;
                if (GUILayout.Button("Create IAP Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<IAPSettings>();
                    if (!Directory.Exists(Common.Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(IAPSettings)}.asset");
                    Debug.Log($"{nameof(IAPSettings).SetColor("f75369")} was created ad {Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(IAPSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.enabled = true;
            }
            else
            {
                var editor = UnityEditor.Editor.CreateEditor(iapSettings);
                editor.OnInspectorGUI();
            }

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Purchasing", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.unity.purchasing", "4.12.2");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}