using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal class Wizard : EditorWindow
    {
        private static GUIStyle styleTitle;

        private static bool enableAtom;
        private static bool enableIap;
        private static bool enableAds;
        private static bool enableGam;
        private static bool enableNotification;
        private static bool enableAddressable;
        private static bool enableUsingAddressableForPopup;
        private static bool enable = true;

        internal static bool Status
        {
            get => EditorPrefs.GetBool($"wizard_{PlayerSettings.productGUID}", false);
            set => EditorPrefs.SetBool($"wizard_{PlayerSettings.productGUID}", value);
        }


        public static void Open()
        {
            enable = true;
            var window = EditorWindow.GetWindow<Wizard>(true, "Wizard", true);
            window.minSize = window.maxSize = new Vector2(400, 250);
            window.ShowUtility();

            Refresh();
            Status = true;
        }

        private static void Refresh()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            enableAtom = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ATOM", group);
            enableIap = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_IAP", group);
            enableAds = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADS", group);
            enableGam = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_GAM", group);
            enableNotification = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_NOTIFICATION", group);
            enableAddressable = RegistryManager.IsInstalled("com.unity.addressables");
            enableUsingAddressableForPopup = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADDRESSABLE_POPUP", group);
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
            enableAtom = EditorGUILayout.Toggle("Atom", enableAtom);
            enableAds = EditorGUILayout.Toggle("Advertising", enableAds);
            enableIap = EditorGUILayout.Toggle("In-App-Purchase", enableIap);
            enableNotification = EditorGUILayout.Toggle("Local Notification", enableNotification);
            enableAddressable = EditorGUILayout.Toggle("Addressable", enableAddressable);
            if (enableAddressable)
            {
                EditorGUI.indentLevel++;
                enableUsingAddressableForPopup = EditorGUILayout.Toggle("For Popup", enableUsingAddressableForPopup);
                EditorGUI.indentLevel--;
            }

            enableGam = EditorGUILayout.Toggle("Game Base Flow", enableGam);

            Uniform.SpaceTwoLine();
            Uniform.Horizontal(() =>
            {
                Uniform.Button("Apply",
                    () =>
                    {
                        if (!enableAds || !enableIap)
                        {
                            string str = "";
                            const string s1 =
                                "If you disable the Advertising or In-App-Purchase module, IAPSettings or AdSettings won't compile.\nBe sure to remove it after disabling the module.";
                            const string s2 =
                                "If you disable the Advertising module, AdSettings won't compile.\nBe sure to remove AdSettings after disabling the module.";
                            const string s3 = "If you disable the In-App-Purchase module, IAPSettings won't compile.\nBe sure to remove it after disabling the module.";
                            var adSettings = InEditor.FindAllAssets<ScriptableObject>().Filter(_ => _.name.Equals("AdSettings"));
                            var iapSettings = InEditor.FindAllAssets<ScriptableObject>().Filter(_ => _.name.Equals("IAPSettings"));
                            var foundAdSetting = adSettings.Count > 0;
                            var foundIapSetting = iapSettings.Count > 0;

                            if (!enableAds && !enableIap)
                            {
                                if (foundAdSetting && foundIapSetting)
                                {
                                    str = s1 + $"\nFound at: {AssetDatabase.GetAssetPath(adSettings[0])}" + $"\nFound at: {AssetDatabase.GetAssetPath(iapSettings[0])}";
                                }
                                else if (foundAdSetting)
                                {
                                    str = s2 + $"\nFound at: {AssetDatabase.GetAssetPath(adSettings[0])}";
                                }
                                else if (foundIapSetting)
                                {
                                    str = s3 + $"\nFound at: {AssetDatabase.GetAssetPath(iapSettings[0])}";
                                }
                            }
                            else if (!enableAds)
                            {
                                if (foundAdSetting) str = s2 + $"\nFound at: {AssetDatabase.GetAssetPath(adSettings[0])}";
                            }
                            else if (!enableIap)
                            {
                                if (foundIapSetting) str = s3 + $"\nFound at: {AssetDatabase.GetAssetPath(iapSettings[0])}";
                            }

                            if (!string.IsNullOrEmpty(str))
                            {
                                if (EditorUtility.DisplayDialog("Notification", str, "Ok")) Execute();
                            }
                            else
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

                            if (enableAtom) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ATOM");
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ATOM");

                            if (enableAds)
                            {
                                RegistryManager.Add("com.unity.ads.ios-support", "1.2.0");
                                RegistryManager.Add("com.unity.sharp-zip-lib", "1.3.3-preview");
                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADS");
                            }
                            else
                            {
                                RegistryManager.Remove("com.unity.ads.ios-support");
                                RegistryManager.Remove("com.unity.sharp-zip-lib");
                                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADS");
                            }

                            if (enableIap)
                            {
                                RegistryManager.Add("com.unity.purchasing", "4.5.2");
                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_IAP");
                            }
                            else
                            {
                                RegistryManager.Remove("com.unity.purchasing");
                                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_IAP");
                            }

                            if (enableGam) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_GAM");
                            else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_GAM");

                            if (enableNotification)
                            {
                                RegistryManager.Add("com.unity.mobile.notifications", "2.1.1");
                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_NOTIFICATION");
                            }
                            else
                            {
                                RegistryManager.Remove("com.unity.mobile.notifications");
                                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_NOTIFICATION");
                            }

                            if (enableAddressable)
                            {
                                RegistryManager.Add("com.unity.addressables", "1.19.19");
                                if (enableUsingAddressableForPopup)
                                {
                                    InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADDRESSABLE_POPUP");
                                }
                                else
                                {
                                    InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADDRESSABLE_POPUP");
                                }
                            }
                            else
                            {
                                RegistryManager.Remove("com.unity.addressables");
                                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADDRESSABLE_POPUP");
                            }

                            RegistryManager.Resolve();
                            Close();
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
    }
}