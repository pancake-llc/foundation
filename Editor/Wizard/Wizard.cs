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
        private static bool enableGam;
        private static bool enableNotification;
        private static bool enable = true;
        private static int countInstall;
        private static AddRequest requestPurchasing;
        private static AddRequest requestAdsIosSupport;
        private static AddRequest requestZipLib;
        private static AddRequest requestNewtonSoftJson;
        private static AddRequest requestNotification;

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
            enableGam = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_GAM", group);
            enableNotification = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_NOTIFICATION", group);
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
            enableNotification = EditorGUILayout.Toggle("Local Notification", enableNotification);
            enableGam = EditorGUILayout.Toggle("Game Base Flow", enableGam);

            Uniform.SpaceTwoLine();
            Uniform.Horizontal(() =>
            {
                Uniform.Button("Apply",
                    () =>
                    {
                        if (!enableAds || !enableIap)
                        {
                            string str;
                            if (!enableAds && !enableIap)
                            {
                                str =
                                    "If you disable the Advertising or In-App-Purchase module, IAPSettings or AdSettings won't compile.\nBe sure to remove it after disabling the module.";
                            }
                            else if (!enableAds)
                            {
                                str = "If you disable the Advertising module, AdSettings won't compile.\nBe sure to remove AdSettings after disabling the module.";
                            }
                            else
                            {
                                str = "If you disable the In-App-Purchase module, IAPSettings won't compile.\nBe sure to remove it after disabling the module.";
                            }

                            if (EditorUtility.DisplayDialog("Notification", str, "Ok"))
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
                            countInstall = 0;
                            enable = false;
                            var flag = false;
                            var time = 0;
                            if (enableAds)
                            {
                                if (!InEditor.IsPackageInstalled("com.unity.ads.ios-support"))
                                {
                                    time += 2;
                                    flag = true;
                                    countInstall++;
                                    countInstall++;
                                    countInstall++;
                                    requestPurchasing = Client.Add("com.unity.ads.ios-support");
                                    requestZipLib = Client.Add("com.unity.sharp-zip-lib");
                                    requestNewtonSoftJson = Client.Add("com.unity.nuget.newtonsoft-json");
                                    EditorApplication.update += ProgressAdsIosSupport;
                                    EditorApplication.update += ProgressZipLib;
                                    EditorApplication.update += ProgressNewtonSoftJson;
                                }

                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADS");
                            }
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADS");


                            if (enableIap)
                            {
                                if (!InEditor.IsPackageInstalled("com.unity.purchasing"))
                                {
                                    time += 2;
                                    flag = true;
                                    countInstall++;
                                    requestPurchasing = Client.Add("com.unity.purchasing");
                                    EditorApplication.update += ProgressPurchasing;
                                }

                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_IAP");
                            }
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_IAP");

                            if (enableGam) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_GAM");
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_GAM");

                            if (enableNotification)
                            {
                                if (!InEditor.IsPackageInstalled("com.unity.mobile.notifications"))
                                {
                                    time += 2;
                                    flag = true;
                                    countInstall++;
                                    requestNotification = Client.Add("com.unity.mobile.notifications");
                                    EditorApplication.update += ProgressNotification;
                                }

                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_NOTIFICATION");
                            }
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_NOTIFICATION");

                            if (!flag) InEditor.DelayedCall(time, Close);
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

        private void ProgressNewtonSoftJson()
        {
            if (requestNewtonSoftJson.IsCompleted)
            {
                if (requestNewtonSoftJson.Status == StatusCode.Success) Debug.Log("Installed: " + requestNewtonSoftJson.Result.packageId);
                else if (requestNewtonSoftJson.Status >= StatusCode.Failure) Debug.Log(requestNewtonSoftJson.Error.message);

                countInstall--;
                if (countInstall == 0) EditorWindow.GetWindow<Wizard>(true, "Wizard", false).Close();
                EditorApplication.update -= ProgressNewtonSoftJson;
            }
        }

        private void ProgressAdsIosSupport()
        {
            if (requestAdsIosSupport.IsCompleted)
            {
                if (requestAdsIosSupport.Status == StatusCode.Success) Debug.Log("Installed: " + requestAdsIosSupport.Result.packageId);
                else if (requestAdsIosSupport.Status >= StatusCode.Failure) Debug.Log(requestAdsIosSupport.Error.message);

                countInstall--;
                if (countInstall == 0) EditorWindow.GetWindow<Wizard>(true, "Wizard", false).Close();
                EditorApplication.update -= ProgressAdsIosSupport;
            }
        }

        private void ProgressZipLib()
        {
            if (requestZipLib.IsCompleted)
            {
                if (requestZipLib.Status == StatusCode.Success) Debug.Log("Installed: " + requestZipLib.Result.packageId);
                else if (requestZipLib.Status >= StatusCode.Failure) Debug.Log(requestZipLib.Error.message);

                countInstall--;
                if (countInstall == 0) EditorWindow.GetWindow<Wizard>(true, "Wizard", false).Close();
                EditorApplication.update -= ProgressZipLib;
            }
        }

        private static void ProgressPurchasing()
        {
            if (requestPurchasing.IsCompleted)
            {
                if (requestPurchasing.Status == StatusCode.Success) Debug.Log("Installed: " + requestPurchasing.Result.packageId);
                else if (requestPurchasing.Status >= StatusCode.Failure) Debug.Log(requestPurchasing.Error.message);

                countInstall--;
                if (countInstall == 0) EditorWindow.GetWindow<Wizard>(true, "Wizard", false).Close();
                EditorApplication.update -= ProgressPurchasing;
            }
        }

        private static void ProgressNotification()
        {
            if (requestNotification.IsCompleted)
            {
                if (requestNotification.Status == StatusCode.Success) Debug.Log("Installed: " + requestNotification.Result.packageId);
                else if (requestNotification.Status >= StatusCode.Failure) Debug.Log(requestNotification.Error.message);

                countInstall--;
                if (countInstall == 0) EditorWindow.GetWindow<Wizard>(true, "Wizard", false).Close();
                EditorApplication.update -= ProgressNotification;
            }
        }
    }
}