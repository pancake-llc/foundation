using System;
using System.IO;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.Monetization;
using UnityEditor;
using UnityEngine;

namespace Pancake.MonetizationEditor
{
    [CustomEditor(typeof(AdSettings), true)]
    public class AdSettingsDrawer : UnityEditor.Editor
    {
        private SerializedProperty _adCheckingIntervalProperty;
        private SerializedProperty _adLoadingIntervalProperty;
        private SerializedProperty _currentNetworkProperty;
        private SerializedProperty _gdprProperty;
        private SerializedProperty _gdprTestModeProperty;
        private SerializedProperty _admobEnableTestModeProperty;
        private SerializedProperty _admobDevicesTestProperty;
        private SerializedProperty _admobBannerProperty;
        private SerializedProperty _admobInterProperty;
        private SerializedProperty _admobRewardProperty;
        private SerializedProperty _admobRewardInterProperty;
        private SerializedProperty _admobAppOpenProperty;

        private SerializedProperty _sdkKeyPropertyProperty;
        private SerializedProperty _applovinBannerProperty;
        private SerializedProperty _applovinInterProperty;
        private SerializedProperty _applovinRewardProperty;
        private SerializedProperty _applovinRewardInterProperty;
        private SerializedProperty _applovinAppOpenProperty;
        private SerializedProperty _applovinEnableAgeRestrictedUserProperty;


        private void Init()
        {
            _adCheckingIntervalProperty = serializedObject.FindProperty("adCheckingInterval");
            _adLoadingIntervalProperty = serializedObject.FindProperty("adLoadingInterval");
            _currentNetworkProperty = serializedObject.FindProperty("currentNetwork");
            _gdprProperty = serializedObject.FindProperty("gdpr");
            _gdprTestModeProperty = serializedObject.FindProperty("gdprTestMode");
            _admobEnableTestModeProperty = serializedObject.FindProperty("admobEnableTestMode");
            _admobDevicesTestProperty = serializedObject.FindProperty("admobDevicesTest");
            _admobBannerProperty = serializedObject.FindProperty("admobBanner");
            _admobInterProperty = serializedObject.FindProperty("admobInter");
            _admobRewardProperty = serializedObject.FindProperty("admobReward");
            _admobRewardInterProperty = serializedObject.FindProperty("admobRewardInter");
            _admobAppOpenProperty = serializedObject.FindProperty("admobAppOpen");

            _sdkKeyPropertyProperty = serializedObject.FindProperty("sdkKey");
            _applovinBannerProperty = serializedObject.FindProperty("applovinBanner");
            _applovinInterProperty = serializedObject.FindProperty("applovinInter");
            _applovinRewardProperty = serializedObject.FindProperty("applovinReward");
            _applovinRewardInterProperty = serializedObject.FindProperty("applovinRewardInter");
            _applovinAppOpenProperty = serializedObject.FindProperty("applovinAppOpen");
            _applovinEnableAgeRestrictedUserProperty = serializedObject.FindProperty("applovinEnableAgeRestrictedUser");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();
            EditorGUILayout.HelpBox("Default, Ads will be auto loading", MessageType.Info);
            EditorGUILayout.PropertyField(_adCheckingIntervalProperty);
            EditorGUILayout.PropertyField(_adLoadingIntervalProperty);
            EditorGUILayout.PropertyField(_currentNetworkProperty);
            EditorGUILayout.PropertyField(_gdprProperty, new GUIContent("GDPR"));
            EditorGUILayout.PropertyField(_gdprTestModeProperty, new GUIContent("GDPR Test Mode"));
            if (_gdprProperty.boolValue)
            {
                bool umpSdkInstalled = File.Exists("Assets/GoogleMobileAds/GoogleMobileAds.Ump.dll");
                if (!umpSdkInstalled)
                {
                    EditorGUILayout.HelpBox(
                        "GDPR is currently implemented using the UMP SDK provided in the Admob SDK so if you use GDPR you need to install the Admob SDK. you need to leave admob client blank to avoid admob init if you only use Applovin to show ads.\\nRemember to enable Delay app measurement in GoogleMobileAds settings.",
                        MessageType.Warning);
                }
            }

            GUILayout.Space(4);

            if (_currentNetworkProperty.enumValueIndex == (int) EAdNetwork.Admob)
            {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
                GUI.backgroundColor = Uniform.Green;
                EditorGUILayout.HelpBox("Admob plugin was imported", MessageType.Info);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Open GoogleAdmobSetting", GUILayout.Height(24)))
                {
                    EditorApplication.ExecuteMenuItem("Assets/Google Mobile Ads/Settings...");
                }

                GUI.backgroundColor = Uniform.Red;
                if (GUILayout.Button("Uninstall Admob SDK", GUILayout.Height(24)))
                {
                    var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                    if (ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group)) ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GoogleMobileAds"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GoogleMobileAds.meta"));

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "googlemobileads-unity.aar"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "googlemobileads-unity.aar.meta"));

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GoogleMobileAdsPlugin.androidlib"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GoogleMobileAdsPlugin.androidlib.meta"));

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "GADUAdNetworkExtras.h"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "GADUAdNetworkExtras.h.meta"));

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "unity-plugin-library.a"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "unity-plugin-library.a.meta"));

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "NativeTemplates"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/iOS", "NativeTemplates.meta"));

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Import Mediation", GUILayout.Height(24)))
                {
                    DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing admob mediation");
                    AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Editor/UnityPackages/admob-mediation.unitypackage"), false);
                }

                if (GUILayout.Button("Get Test AppId", GUILayout.Height(24)))
                {
                    "ca-app-pub-3940256099942544~3347511713".CopyToClipboard();
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy AppId Test Id Success!");
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Get Test BannerId", GUILayout.Height(24)))
                {
#if UNITY_ANDROID
                    "ca-app-pub-3940256099942544/6300978111".CopyToClipboard();
#elif UNITY_IOS
                    "ca-app-pub-3940256099942544/2934735716".CopyToClipboard();
#endif
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy Banner Test Unit Id Success!");
                }

                if (GUILayout.Button("Get Test InterId", GUILayout.Height(24)))
                {
#if UNITY_ANDROID
                    "ca-app-pub-3940256099942544/1033173712".CopyToClipboard();
#elif UNITY_IOS
                    "ca-app-pub-3940256099942544/4411468910".CopyToClipboard();
#endif
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy Interstitial Test Unit Id Success!");
                }

                if (GUILayout.Button("Get Test RewardId", GUILayout.Height(24)))
                {
#if UNITY_ANDROID
                    "ca-app-pub-3940256099942544/5224354917".CopyToClipboard();
#elif UNITY_IOS
                    "ca-app-pub-3940256099942544/1712485313".CopyToClipboard();
#endif
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy Rewarded Test Unit Id Success!");
                }

                if (GUILayout.Button("Get Test RewardInterId", GUILayout.Height(24)))
                {
#if UNITY_ANDROID
                    "ca-app-pub-3940256099942544/5354046379".CopyToClipboard();
#elif UNITY_IOS
                    "ca-app-pub-3940256099942544/6978759866".CopyToClipboard();
#endif
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy Rewarded Interstitial Test Unit Id Success!");
                }

                if (GUILayout.Button("Get Test AppOpenId", GUILayout.Height(24)))
                {
#if UNITY_ANDROID
                    "ca-app-pub-3940256099942544/9257395921".CopyToClipboard();
#elif UNITY_IOS
                    "ca-app-pub-3940256099942544/5575463023".CopyToClipboard();
#endif
                    DebugEditor.Log("<color=#FF77C6>[Admob]</color> Copy App Open Test Unit Id Success!");
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_admobEnableTestModeProperty, new GUIContent("Test Mode"));
                if (_admobEnableTestModeProperty.boolValue) EditorGUILayout.PropertyField(_admobDevicesTestProperty, new GUIContent("Devices Test"));

                EditorGUILayout.PropertyField(_admobBannerProperty, new GUIContent("Banner"));
                EditorGUILayout.PropertyField(_admobInterProperty, new GUIContent("Interstitial"));
                EditorGUILayout.PropertyField(_admobRewardProperty, new GUIContent("Rewarded"));
                EditorGUILayout.PropertyField(_admobRewardInterProperty, new GUIContent("Inter Rewarded"));
                EditorGUILayout.PropertyField(_admobAppOpenProperty, new GUIContent("App Open"));
#else
                GUI.backgroundColor = Uniform.Orange;
                EditorGUILayout.HelpBox("Admob plugin not found", MessageType.Warning);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                bool googleMobileAdsInstalled = File.Exists("Assets/GoogleMobileAds/GoogleMobileAds.dll");
                var contentInstallLabel = "Install Admob SDK v9.1.0 (1)";
                if (googleMobileAdsInstalled)
                {
                    GUI.backgroundColor = Uniform.Green;
                    contentInstallLabel = "Admob SDK v9.1.0 Installed (1)";
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(contentInstallLabel, GUILayout.Height(24)))
                {
                    DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing admob sdk");
                    AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Editor/UnityPackages/admob.unitypackage"), false);
                }

                var previousColor = GUI.color;
                if (googleMobileAdsInstalled) GUI.color = Uniform.Green;

                GUI.enabled = googleMobileAdsInstalled;
                GUILayout.Label(" =====> ", new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, -10, 0)}, GUILayout.Width(52));
                GUI.color = previousColor;
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Add Admob Symbol (2)", GUILayout.Height(24)))
                {
                    if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)))
                    {
                        ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
#endif
            }
            else if (_currentNetworkProperty.enumValueIndex == (int) EAdNetwork.Applovin)
            {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
                GUI.backgroundColor = Uniform.Green;
                EditorGUILayout.HelpBox("Applovin plugin was imported", MessageType.Info);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Open AppLovin Integration", GUILayout.Height(24)))
                {
                    EditorApplication.ExecuteMenuItem("AppLovin/Integration Manager");
                }

                GUI.backgroundColor = Uniform.Red;
                if (GUILayout.Button("Uninstall AppLovin SDK", GUILayout.Height(24)))
                {
                    var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                    if (ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group)) ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");

                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "MaxSdk"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "MaxSdk.meta"));

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_sdkKeyPropertyProperty);
                EditorGUILayout.PropertyField(_applovinEnableAgeRestrictedUserProperty, new GUIContent("Age Restricted"));
                EditorGUILayout.PropertyField(_applovinBannerProperty, new GUIContent("Banner"));
                EditorGUILayout.PropertyField(_applovinInterProperty, new GUIContent("Interstitial"));
                EditorGUILayout.PropertyField(_applovinRewardProperty, new GUIContent("Rewarded"));
                EditorGUILayout.PropertyField(_applovinRewardInterProperty, new GUIContent("Inter Rewarded"));
                EditorGUILayout.PropertyField(_applovinAppOpenProperty, new GUIContent("App Open"));
#else
                GUI.backgroundColor = Uniform.Orange;
                EditorGUILayout.HelpBox("Applovin plugin not found", MessageType.Warning);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                bool applovinInstalled = File.Exists("Assets/MaxSdk/Scripts/MaxSdk.cs");
                var contentInstallLabel = "Install Applovin v6.4.4 (1)";
                if (applovinInstalled)
                {
                    GUI.backgroundColor = Uniform.Green;
                    contentInstallLabel = "Applovin SDK v6.4.4 Installed (1)";
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }

                if (GUILayout.Button(contentInstallLabel, GUILayout.Height(24)))
                {
                    DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing <color=#FF77C6>applovin</color> sdk");
                    AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Editor/UnityPackages/applovin.unitypackage"), false);
                }

                var previousColor = GUI.color;
                if (applovinInstalled) GUI.color = Uniform.Green;
                GUI.enabled = applovinInstalled;
                GUILayout.Label(" =====> ", new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, -10, 0)}, GUILayout.Width(52));
                GUI.color = previousColor;
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Add AppLovin Symbol (2)", GUILayout.Height(24)))
                {
                    if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)))
                    {
                        ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }

                GUI.enabled = true;
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

#endif
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}