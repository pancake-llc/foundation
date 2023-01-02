using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Pancake.Editor
{
    internal class Wizard : EditorWindow
    {
        private static GUIStyle styleTitle;

        private static bool enableIap;
        private static bool enableAds;
        private static bool enable = true;
        private static AddRequest request;

        internal static bool Status
        {
            get => EditorPrefs.GetBool($"wizard_{PlayerSettings.productGUID}", false);
            set => EditorPrefs.SetBool($"wizard_{PlayerSettings.productGUID}", value);
        }


        public static void Open()
        {
            enable = true;
            var window = EditorWindow.GetWindow<Wizard>(true, "Wizard", true);
            window.minSize = window.maxSize = new Vector2(400, 200);
            window.ShowUtility();

            Refresh();
            Status = true;
        }

        private static void Refresh()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            enableIap = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_IAP", group);
            enableAds = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADS", group);
        }

        private void OnGUI()
        {
            if (styleTitle == null)
            {
                styleTitle = new GUIStyle(GUI.skin.label) {richText = true, fontSize = 18};
                styleTitle.normal.textColor = styleTitle.active.textColor = styleTitle.focused.textColor = styleTitle.hover.textColor = styleTitle.onNormal.textColor =
                    styleTitle.onActive.textColor = styleTitle.onFocused.textColor = styleTitle.onHover.textColor = new Color(0.58f, 0.87f, 0.35f);
            }

            GUILayout.Label("Add/Remove Modules", styleTitle);

            GUILayout.FlexibleSpace();
            GUI.enabled = !EditorApplication.isCompiling && enable;
            enableAds = EditorGUILayout.Toggle("Advertising", enableAds);
            enableIap = EditorGUILayout.Toggle("In-App-Purchase", enableIap);

            Uniform.SpaceTwoLine();
            Uniform.Horizontal(() =>
            {
                Uniform.Button("Apply",
                    () =>
                    {
                        if (!enableAds || !enableIap)
                        {
                            if (EditorUtility.DisplayDialog("Notification",
                                    "If you disable the Advertising or In-App-Purchase module, IAPSettings or AdSettings won't compile.\nBe sure to remove it after disabling the module.",
                                    "Ok"))
                            {
                                Execute();
                            }
                        }
                        else
                        {
                            Execute();
                        }

                        void Execute()
                        {
                            enable = false;
                            if (enableAds) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADS");
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADS");

                            var flag = false;
                            if (enableIap)
                            {
                                if (!InEditor.IsPackageInstalled("com.unity.purchasing"))
                                {
                                    flag = true;
                                    request = Client.Add("com.unity.purchasing");
                                    EditorApplication.update += Progress;
                                }

                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_IAP");
                            }
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_IAP");

                            if (!flag) InEditor.DelayedCall(2f, Close);
                        }
                    });
                Uniform.Button("Cancel", Close);
            });
            Uniform.SpaceOneLine();
            Uniform.HelpBox("Turn on the module in case you need it\nIt will automatically add or remove Scripting Define Symbols corresponding to each module",
                MessageType.Info);
            Uniform.SpaceOneLine();
            GUI.enabled = true;
        }

        private static void Progress()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
                else if (request.Status >= StatusCode.Failure) Debug.Log(request.Error.message);

                var window = EditorWindow.GetWindow<Wizard>(true, "Wizard", true);
                window.Close();
                EditorApplication.update -= Progress;
            }
        }
    }
}