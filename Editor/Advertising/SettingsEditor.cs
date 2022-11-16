using System.Collections.Generic;
using System.IO;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Monetization.Editor
{
    [CustomEditor(typeof(Settings))]
    internal class SettingsEditor : UnityEditor.Editor
    {
        private class Property
        {
            public SerializedProperty property;
            public GUIContent content;

            public Property(SerializedProperty property, GUIContent content)
            {
                this.property = property;
                this.content = content;
            }

            public Property(GUIContent content) { this.content = content; }
        }

        private static class AdProperties
        {
            public static SerializedProperty main;

            public static Property autoInit = new Property(null, new GUIContent("Auto Init", "Whether the ads should automatically initialize itself"));
            public static Property autoLoadAdsMode = new Property(null, new GUIContent("Auto Ad-Loading Mode"));
            public static Property adCheckingInterval = new Property(null, new GUIContent("Ad Checking Interval", "Time (seconds) between 2 ad-availability checks"));

            public static Property adLoadingInterval = new Property(null,
                new GUIContent("Ad Loading Interval",
                    "Minimum time (seconds) between two ad-loading requests, this is to restrict the number of requests sent to ad networks"));

            public static Property privacyPolicyUrl = new Property(null, new GUIContent("Privacy&Policy Url", "Privacy policy url"));

            public static Property enableGDPR = new Property(null,
                new GUIContent("GDPR",
                    "General data protection regulation \nApp requires user consent before these events can be sent, you can delay app measurement until you explicitly initialize the Mobile Ads SDK or load an ad."));

            public static Property enableMultipleDex = new Property(null, new GUIContent("Multiple Dex"));
            public static Property currentNetwork = new Property(null, new GUIContent("Current Network", "Current network use show ad"));
        }

        private static class AdmobProperties
        {
            public static SerializedProperty main;
            public static Property enable = new Property(null, new GUIContent("Enable", "Enable using admob ad"));
            public static Property devicesTest = new Property(null, new GUIContent("Devices Test", "List devices show real ad but mark test user"));
            public static Property bannerAdUnit = new Property(null, new GUIContent("Banner Ad"));
            public static Property interstitialAdUnit = new Property(null, new GUIContent("Interstitial Ad"));
            public static Property rewardedAdUnit = new Property(null, new GUIContent("Rewarded Ad"));
            public static Property rewardedInterstitialAdUnit = new Property(null, new GUIContent("Rewarded interstitial Ad"));
            public static Property appOpenAdUnit = new Property(null, new GUIContent("App Open Ad"));
            public static Property enableTestMode = new Property(null, new GUIContent("Enable Test Mode", "Enable true when want show test ad"));
            public static Property useAdaptiveBanner = new Property(null, new GUIContent("Use Adaptive Banner", "Use adaptive banner ad when use smart banner"));
        }

        private static class ApplovinProperties
        {
            public static SerializedProperty main;
            public static Property enable = new Property(null, new GUIContent("Enable", "Enable using applovin ad"));
            public static Property sdkKey = new Property(null, new GUIContent("Sdk Key", "Sdk of applovin"));
            public static Property bannerAdUnit = new Property(null, new GUIContent("Banner Ad"));
            public static Property interstitialAdUnit = new Property(null, new GUIContent("Interstitial Ad"));
            public static Property rewardedAdUnit = new Property(null, new GUIContent("Rewarded Ad"));
            public static Property rewardedInterstitialAdUnit = new Property(null, new GUIContent("Rewarded Interstitial Ad"));
            public static Property appOpenAdUnit = new Property(null, new GUIContent("App Open Ad"));
            public static Property enableAgeRestrictedUser = new Property(null, new GUIContent("Age Restrictd User"));

            public static Property enableRequestAdAfterHidden = new Property(null,
                new GUIContent("Request Ad After Hidden",
                    "Request to add new interstitial and rewarded ad after user finish view ad. Need kick-off request to cache ads as quickly as possible"));

            public static Property enableMaxAdReview = new Property(null, new GUIContent("Enable MAX Ad Review"));
        }

        private static class IronSourceProperties
        {
            public static SerializedProperty main;
            public static Property enable = new Property(null, new GUIContent("Enable", "Enable using ironSource ad"));
            public static Property appKey = new Property(null, new GUIContent("Sdk Key", "Sdk of ironSource"));
            public static Property bannerAdUnit = new Property(null, new GUIContent("Banner Ad"));

            public static Property useAdaptiveBanner = new Property(null,
                new GUIContent("Use Adaptive Banner", "Use adaptive banner ad when use smart banner affect for admob ad of ironsouce mediation"));
        }

        #region properties

        //Runtime auto initialization
        private SerializedProperty _autoInitializeProperty;
        public static bool callFromEditorWindow = false;

        private const float ACTION_FIELD_WIDTH = 65f;
        private const float NETWORK_FIELD_MIN_WIDTH = 130f;
        private const float VERSION_FIELD_MIN_WIDTH = 180f;
        private static readonly GUILayoutOption NetworkWidthOption = GUILayout.Width(NETWORK_FIELD_MIN_WIDTH);
        private static readonly GUILayoutOption VersionWidthOption = GUILayout.Width(VERSION_FIELD_MIN_WIDTH);
        private static readonly GUILayoutOption FieldWidth = GUILayout.Width(ACTION_FIELD_WIDTH);

        private GUIContent _warningIcon;
        private GUIContent _iconUnintall;
        private GUIStyle _headerLabelStyle;

        private bool IsAdmobSdkAvaiable
        {
            get
            {
#if PANCAKE_ADMOB_ENABLE
                return true;
#else
                return false;
#endif
            }
        }

        private bool IsApplovinSdkAvaiable
        {
            get
            {
#if PANCAKE_MAX_ENABLE
                return true;
#else
                return false;
#endif
            }
        }

        private bool IsIronSourceSdkAvaiable
        {
            get
            {
#if PANCAKE_IRONSOURCE_ENABLE
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region api

        private void Init()
        {
            _warningIcon = Uniform.IconContent("console.warnicon.sml", "Adapter not compatible, please update to the latest version.");
            _iconUnintall = Uniform.IconContent("d_TreeEditor.Trash", "Uninstall");
            _headerLabelStyle = new GUIStyle(EditorStyles.label) {fontSize = 12, fontStyle = FontStyle.Bold, fixedHeight = 18};

            _autoInitializeProperty = serializedObject.FindProperty("runtimeAutoInitialize");

            AdProperties.main = serializedObject.FindProperty("adSettings");
            AdProperties.autoInit.property = AdProperties.main.FindPropertyRelative("autoInit");
            AdProperties.autoLoadAdsMode.property = AdProperties.main.FindPropertyRelative("autoLoadingAd");
            AdProperties.adCheckingInterval.property = AdProperties.main.FindPropertyRelative("adCheckingInterval");
            AdProperties.adLoadingInterval.property = AdProperties.main.FindPropertyRelative("adLoadingInterval");
            AdProperties.enableGDPR.property = AdProperties.main.FindPropertyRelative("enableGDPR");
            AdProperties.privacyPolicyUrl.property = AdProperties.main.FindPropertyRelative("privacyPolicyUrl");
            AdProperties.enableMultipleDex.property = AdProperties.main.FindPropertyRelative("enableMultipleDex");
            AdProperties.currentNetwork.property = AdProperties.main.FindPropertyRelative("currentNetwork");

            AdmobProperties.main = serializedObject.FindProperty("admobSettings");
            AdmobProperties.enable.property = AdmobProperties.main.FindPropertyRelative("enable");
            AdmobProperties.devicesTest.property = AdmobProperties.main.FindPropertyRelative("devicesTest");
            AdmobProperties.bannerAdUnit.property = AdmobProperties.main.FindPropertyRelative("bannerAdUnit");
            AdmobProperties.interstitialAdUnit.property = AdmobProperties.main.FindPropertyRelative("interstitialAdUnit");
            AdmobProperties.rewardedAdUnit.property = AdmobProperties.main.FindPropertyRelative("rewardedAdUnit");
            AdmobProperties.rewardedInterstitialAdUnit.property = AdmobProperties.main.FindPropertyRelative("rewardedInterstitialAdUnit");
            AdmobProperties.appOpenAdUnit.property = AdmobProperties.main.FindPropertyRelative("appOpenAdUnit");
            AdmobProperties.enableTestMode.property = AdmobProperties.main.FindPropertyRelative("enableTestMode");
            AdmobProperties.useAdaptiveBanner.property = AdmobProperties.main.FindPropertyRelative("useAdaptiveBanner");

            ApplovinProperties.main = serializedObject.FindProperty("maxSettings");
            ApplovinProperties.enable.property = ApplovinProperties.main.FindPropertyRelative("enable");
            ApplovinProperties.sdkKey.property = ApplovinProperties.main.FindPropertyRelative("sdkKey");
            ApplovinProperties.bannerAdUnit.property = ApplovinProperties.main.FindPropertyRelative("bannerAdUnit");
            ApplovinProperties.interstitialAdUnit.property = ApplovinProperties.main.FindPropertyRelative("interstitialAdUnit");
            ApplovinProperties.rewardedAdUnit.property = ApplovinProperties.main.FindPropertyRelative("rewardedAdUnit");
            ApplovinProperties.rewardedInterstitialAdUnit.property = ApplovinProperties.main.FindPropertyRelative("rewardedInterstitialAdUnit");
            ApplovinProperties.appOpenAdUnit.property = ApplovinProperties.main.FindPropertyRelative("appOpenAdUnit");

            ApplovinProperties.enableAgeRestrictedUser.property = ApplovinProperties.main.FindPropertyRelative("enableAgeRestrictedUser");
            ApplovinProperties.enableRequestAdAfterHidden.property = ApplovinProperties.main.FindPropertyRelative("enableRequestAdAfterHidden");
            ApplovinProperties.enableMaxAdReview.property = ApplovinProperties.main.FindPropertyRelative("enableMaxAdReview");

            IronSourceProperties.main = serializedObject.FindProperty("ironSourceSettings");
            IronSourceProperties.enable.property = IronSourceProperties.main.FindPropertyRelative("enable");
            IronSourceProperties.appKey.property = IronSourceProperties.main.FindPropertyRelative("appKey");
            IronSourceProperties.bannerAdUnit.property = IronSourceProperties.main.FindPropertyRelative("bannerAdUnit");
            IronSourceProperties.useAdaptiveBanner.property = IronSourceProperties.main.FindPropertyRelative("useAdaptiveBanner");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();

            if (!callFromEditorWindow)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "This ScriptableObject holds all the settings of Ads. Please go to menu Tools > Snorlax > Ads or click the button below to edit it.",
                    MessageType.Info);
                if (GUILayout.Button("Edit")) SettingsWindow.ShowWindow();
                return;
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            #region draw

            Uniform.DrawGroupFoldout("AUTO_INITIALIZE_FOLDOUT_KEY",
                "BASIC",
                () =>
                {
                    EditorGUILayout.PropertyField(AdProperties.autoInit.property, AdProperties.autoInit.content);
                    EditorGUILayout.PropertyField(AdProperties.enableGDPR.property, AdProperties.enableGDPR.content);
                    EditorGUILayout.PropertyField(AdProperties.enableMultipleDex.property, AdProperties.enableMultipleDex.content);

                    EditorGUILayout.PropertyField(AdProperties.currentNetwork.property, AdProperties.currentNetwork.content);

                    if (Settings.AdSettings.EnableGDPR) EditorGUILayout.PropertyField(AdProperties.privacyPolicyUrl.property, AdProperties.privacyPolicyUrl.content);

                    if (Settings.AdSettings.EnableMultipleDex)
                    {
                        AdsEditorUtil.CreateMainTemplateGradle();
#if UNITY_2020_3_OR_NEWER
                        AdsEditorUtil.CreateGradleTemplateProperties();
#endif
                        ScriptingDefinition.AddDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_MULTIPLE_DEX);
                        AdsEditorUtil.SetDeleteGradleState(true);
                    }
                    else
                    {
                        if (AdsEditorUtil.StateDeleteGradle())
                        {
                            AdsEditorUtil.SetDeleteGradleState(false);
                            AdsEditorUtil.DeleteMainTemplateGradle();
#if UNITY_2020_3_OR_NEWER
                            AdsEditorUtil.DeleteGradleTemplateProperties();
#endif
                            ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_MULTIPLE_DEX);
                        }
                    }
                });

            EditorGUILayout.Space();
            Uniform.DrawGroupFoldout("AUTO_AD_LOADING_CONFIG_FOLDOUT_KEY",
                "AUTO AD-LOADING",
                () =>
                {
                    EditorGUILayout.PropertyField(AdProperties.autoLoadAdsMode.property, AdProperties.autoLoadAdsMode.content);
                    if (Settings.AdSettings.AutoLoadingAd != EAutoLoadingAd.None)
                    {
                        EditorGUILayout.PropertyField(AdProperties.adCheckingInterval.property, AdProperties.adCheckingInterval.content);
                        EditorGUILayout.PropertyField(AdProperties.adLoadingInterval.property, AdProperties.adLoadingInterval.content);
                    }
                });

            EditorGUILayout.Space();
            Uniform.DrawGroupFoldout("ADMOB_MODULE",
                "ADMOB",
                () =>
                {
                    EditorGUILayout.PropertyField(AdmobProperties.enable.property, AdmobProperties.enable.content);
                    if (Settings.AdmobSettings.Enable)
                    {
                        SettingManager.ValidateAdmobSdkImported();
                        if (IsAdmobSdkAvaiable)
                        {
                            EditorGUILayout.HelpBox("Admob plugin was imported", MessageType.Info);
                            if (Settings.AdmobSettings.editorImportingSdk != null && !string.IsNullOrEmpty(Settings.AdmobSettings.editorImportingSdk.lastVersion.unity) &&
                                Settings.AdmobSettings.editorImportingSdk.CurrentToLatestVersionComparisonResult == EVersionComparisonResult.Lesser)
                            {
                                if (GUILayout.Button("Update Admob Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                                {
                                    EditorCoroutine.Start(SettingManager.Instance.DownloadGma(Settings.AdmobSettings.editorImportingSdk));
                                }
                            }

                            if (Settings.AdSettings.EnableGDPR)
                            {
                                EditorGUILayout.HelpBox("GDPR is enable so you should turn on Delay app measurement in GoogleMobileAds setting", MessageType.Info);
                            }

                            EditorGUILayout.Space();
                            if (GUILayout.Button("Open GoogleMobileAds Setting", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                            {
#if PANCAKE_ADMOB_ENABLE
                                GoogleMobileAds.Editor.GoogleMobileAdsSettingsEditor.OpenInspector();
                                EditorWindow.GetWindow(InEditor.InspectorWindow).Focus();
#endif
                            }

                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(AdmobProperties.bannerAdUnit.property, AdmobProperties.bannerAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.interstitialAdUnit.property, AdmobProperties.interstitialAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.rewardedAdUnit.property, AdmobProperties.rewardedAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.rewardedInterstitialAdUnit.property, AdmobProperties.rewardedInterstitialAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.appOpenAdUnit.property, AdmobProperties.appOpenAdUnit.content, true);
                            EditorGUI.indentLevel--;
                            Uniform.DrawGroupFoldout("ADMOB_MODULE_MEDIATION",
                                "MEDIATION",
                                () =>
                                {
                                    DrawHeaderMediation();
                                    foreach (var network in Settings.AdmobSettings.editorListNetwork)
                                    {
                                        DrawAdmobNetworkDetailRow(network);
                                    }

                                    DrawAdmobInstallAllNetwork();
                                });

                            EditorGUILayout.Space();
                            if (Settings.AdmobSettings.BannerAdUnit.size == EBannerSize.SmartBanner)
                                EditorGUILayout.PropertyField(AdmobProperties.useAdaptiveBanner.property, AdmobProperties.useAdaptiveBanner.content);
                            EditorGUILayout.PropertyField(AdmobProperties.enableTestMode.property, AdmobProperties.enableTestMode.content);
                            if (Settings.AdmobSettings.EnableTestMode)
                            {
                                EditorGUILayout.PropertyField(AdmobProperties.devicesTest.property, AdmobProperties.devicesTest.content);
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Admob plugin not found. Please import it to show ads from Admob", MessageType.Warning);
                            if (GUILayout.Button("Import Admob Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                            {
                                if (Settings.AdmobSettings.editorImportingSdk != null)
                                {
                                    EditorCoroutine.Start(SettingManager.Instance.DownloadGma(Settings.AdmobSettings.editorImportingSdk));
                                }
                                else
                                {
                                    Application.OpenURL("https://github.com/googleads/googleads-mobile-unity/releases");
                                }
                            }
                        }
                    }
                });

            EditorGUILayout.Space();
            Uniform.DrawGroupFoldout("MAX_MODULE",
                "MAX",
                () =>
                {
                    EditorGUILayout.PropertyField(ApplovinProperties.enable.property, ApplovinProperties.enable.content);
                    if (Settings.MaxSettings.Enable)
                    {
                        SettingManager.ValidateApplovinSdkImported();
                        if (IsApplovinSdkAvaiable)
                        {
                            EditorGUILayout.HelpBox("Applovin plugin was imported", MessageType.Info);
                            
                            if (Settings.MaxSettings.editorImportingSdk != null && !string.IsNullOrEmpty(Settings.MaxSettings.editorImportingSdk.lastVersion.unity) &&
                                Settings.MaxSettings.editorImportingSdk.CurrentToLatestVersionComparisonResult == EVersionComparisonResult.Lesser)
                            {
                                if (GUILayout.Button("Update MaxSdk Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                                {
                                    EditorCoroutine.Start(MaxManager.Instance.DownloadMaxSdk(Settings.MaxSettings.editorImportingSdk));
                                }
                            }
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(ApplovinProperties.sdkKey.property, ApplovinProperties.sdkKey.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.enableAgeRestrictedUser.property, ApplovinProperties.enableAgeRestrictedUser.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.enableRequestAdAfterHidden.property, ApplovinProperties.enableRequestAdAfterHidden.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.enableMaxAdReview.property, ApplovinProperties.enableMaxAdReview.content);
#if PANCAKE_MAX_ENABLE
                            AppLovinSettings.Instance.QualityServiceEnabled = Settings.MaxSettings.EnableMaxAdReview;
                            AppLovinSettings.Instance.ConsentFlowEnabled = Settings.AdSettings.EnableGDPR;
                            AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl = Settings.AdSettings.PrivacyPolicyUrl;
#endif
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(ApplovinProperties.bannerAdUnit.property, ApplovinProperties.bannerAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.interstitialAdUnit.property, ApplovinProperties.interstitialAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.rewardedAdUnit.property, ApplovinProperties.rewardedAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.rewardedInterstitialAdUnit.property, ApplovinProperties.rewardedInterstitialAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.appOpenAdUnit.property, ApplovinProperties.appOpenAdUnit.content);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();

                            Uniform.DrawGroupFoldout("APPLOVIN_MODULE_MEDIATION",
                                "MEDIATION",
                                () =>
                                {
                                    DrawHeaderMediation();
                                    foreach (var network in Settings.MaxSettings.editorListNetwork)
                                    {
                                        DrawApplovinNetworkDetailRow(network);
                                    }

                                    DrawApplovinInstallAllNetwork();
                                });
                            EditorGUILayout.Space();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Max plugin not found. Please import it to show ads from Applovin", MessageType.Warning);
                            if (GUILayout.Button("Import MAX Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                            {
                                if (Settings.MaxSettings.editorImportingSdk != null)
                                {
                                    EditorCoroutine.Start(MaxManager.Instance.DownloadMaxSdk(Settings.MaxSettings.editorImportingSdk));
                                }
                                else
                                {
                                    Application.OpenURL("https://github.com/gamee-studio/ads/releases/tag/1.0.21");
                                }
                            }
                        }

#if PANCAKE_MAX_ENABLE
                        if (GUI.changed) AppLovinSettings.Instance.SaveAsync();
#endif
                    }
                });


            EditorGUILayout.Space();
            Uniform.DrawGroupFoldout("IRONSOURCE_MODULE",
                "IRONSOURCE",
                () =>
                {
                    EditorGUILayout.PropertyField(IronSourceProperties.enable.property, IronSourceProperties.enable.content);
                    if (Settings.IronSourceSettings.Enable)
                    {
                        SettingManager.ValidateIronSourceSdkImported();
                        if (IsIronSourceSdkAvaiable)
                        {
                            EditorGUILayout.HelpBox("IronSource plugin was imported", MessageType.Info);
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(IronSourceProperties.appKey.property, IronSourceProperties.appKey.content);
                            EditorGUILayout.PropertyField(IronSourceProperties.bannerAdUnit.property, IronSourceProperties.bannerAdUnit.content);
                            EditorGUILayout.PropertyField(IronSourceProperties.useAdaptiveBanner.property, IronSourceProperties.useAdaptiveBanner.content);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();

                            Uniform.DrawGroupFoldout("IRONSOURCE_MODULE_MEDIATION",
                                "MEDIATION",
                                () =>
                                {
                                    DrawHeaderMediation();
                                    foreach (var network in Settings.IronSourceSettings.editorListNetwork)
                                    {
                                        DrawIronSourceNetworkDetailRow(network);
                                    }

                                    DrawIronsourceInstallAllNetwork();
                                });
                            EditorGUILayout.Space();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("IronSource plugin not found. Please import it to show ads from IronSource", MessageType.Warning);
                            if (GUILayout.Button("Import IronSource Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
                            {
                                if (Settings.IronSourceSettings.editorImportingSdk != null)
                                {
                                    EditorCoroutine.Start(IronSourceManager.Instance.DownloadPlugin(Settings.IronSourceSettings.editorImportingSdk));
                                }
                                else
                                {
                                    Application.OpenURL("https://developers.is.com/ironsource-mobile/unity/unity-plugin/");
                                }
                            }
                        }
                    }
                });

            #endregion

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderMediation()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("Network", _headerLabelStyle, NetworkWidthOption);
                EditorGUILayout.LabelField("Current Version", _headerLabelStyle, VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField("Latest Version", _headerLabelStyle, VersionWidthOption);
                GUILayout.FlexibleSpace();
                GUILayout.Button("Actions", _headerLabelStyle, FieldWidth);
                GUILayout.Space(5);
            }
        }

        private void DrawAdmobNetworkDetailRow(Network network)
        {
            string currentVersion = network.currentVersion != null ? network.currentVersion.unity : "";
            string latestVersion = network.lastVersion.unity;
            var status = "";
            var isActionEnabled = false;
            var isInstalled = false;
            ValidateVersionAdmob(network.CurrentToLatestVersionComparisonResult,
                ref currentVersion,
                ref status,
                ref isActionEnabled,
                ref isInstalled);

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(network.displayName), NetworkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(currentVersion), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(latestVersion), VersionWidthOption);
                GUILayout.FlexibleSpace();

                if (network.requireUpdate)
                {
                    GUILayout.Label(_warningIcon);
                }

                GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent(status), FieldWidth))
                {
                    // Download the plugin.
                    EditorCoroutine.Start(SettingManager.Instance.DownloadPlugin(network));
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = isInstalled && !EditorApplication.isCompiling;
                if (GUILayout.Button(_iconUnintall))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.displayName + "...", 0.5f);
                    var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;
                    foreach (var pluginFilePath in network.pluginFilePath)
                    {
                        if (pluginFilePath.StartsWith("Plugins"))
                        {
                            FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", pluginFilePath));
                            FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", pluginFilePath + ".meta"));
                        }
                        else
                        {
                            FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
                            FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
                        }
                    }

                    network.currentVersion = new NetworkVersion();
                    SettingManager.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
                    SettingManager.Instance.UpdateCurrentVersion(network);

                    // Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(5);
            }

            if (isInstalled)
            {
            }
        }

        private void DrawApplovinNetworkDetailRow(MaxNetwork network)
        {
            string currentVersion = network.CurrentVersions != null ? network.CurrentVersions.Unity : "";
            string latestVersion = network.LatestVersions.Unity;
            var status = "";
            var isActionEnabled = false;
            var isInstalled = false;
            ValidateVersionMax(network.CurrentToLatestVersionComparisonResult,
                ref currentVersion,
                ref status,
                ref isActionEnabled,
                ref isInstalled);

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(network.DisplayName), NetworkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(currentVersion), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(latestVersion), VersionWidthOption);
                GUILayout.FlexibleSpace();

                if (network.RequiresUpdate)
                {
                    GUILayout.Label(_warningIcon);
                }

                GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent(status), FieldWidth))
                {
                    // Download the plugin.
                    EditorCoroutine.Start(MaxManager.Instance.DownloadPlugin(network));
                    if (network.Name.Equals("ALGORIX_NETWORK"))
                    {
                        AdsEditorUtil.CreateMainTemplateGradle();
                        AdsEditorUtil.AddSettingProguardFile(new List<string>()
                        {
                            "-keep class com.alxad.* {;}",
                            "-keep class admob.custom.adapter.* {;}",
                            "-keep class anythink.custom.adapter.* {;}",
                            "-keep class com.mopub.mobileads.* {;}",
                            "-keep class com.applovin.mediation.adapters.* {;}"
                        });
                        AdsEditorUtil.AddAlgorixSettingGradle(network);
                    }
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = isInstalled && !EditorApplication.isCompiling;
                if (GUILayout.Button(_iconUnintall, FieldWidth))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.DisplayName + "...", 0.5f);
                    var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;
                    foreach (var pluginFilePath in network.PluginFilePaths)
                    {
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
                    }

                    if (network.Name.Equals("ALGORIX_NETWORK"))
                    {
                        AdsEditorUtil.RemoveAlgorixSettingGradle();
                        AdsEditorUtil.DeleteProguardFile();
                    }

                    SettingManager.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
                    MaxManager.UpdateCurrentVersions(network, pluginRoot);

                    // Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(5);
            }

            if (isInstalled)
            {
                if (network.Name.Equals("ADMOB_NETWORK"))
                {
#if PANCAKE_MAX_ENABLE
                    // ReSharper disable once PossibleNullReferenceException
                    if ((int) MaxSdkUtils.CompareUnityMediationVersions(network.CurrentVersions.Unity, "android_19.0.1.0_ios_7.57.0.0") ==
                        (int) EVersionComparisonResult.Greater)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(20);

                        using (new EditorGUILayout.VerticalScope())
                        {
                            AppLovinSettings.Instance.AdMobAndroidAppId =
                                Uniform.DrawTextField("App ID (Android)", AppLovinSettings.Instance.AdMobAndroidAppId, NetworkWidthOption);
                            AppLovinSettings.Instance.AdMobIosAppId = Uniform.DrawTextField("App ID (iOS)", AppLovinSettings.Instance.AdMobIosAppId, NetworkWidthOption);
                        }

                        GUILayout.EndHorizontal();
                    }
#endif
                }
            }
        }

        private void DrawIronSourceNetworkDetailRow(AdapterMediationIronSource network)
        {
            if (!network.Equals(default(AdapterMediationIronSource)))
            {
                string currentVersion = network.currentUnityVersion;
                string latestVersion = network.latestUnityVersion;
                var status = "";
                var isActionEnabled = false;
                var isInstalled = false;

                ValidateVersionIronSource(network.currentStatus,
                    ref currentVersion,
                    ref status,
                    ref isActionEnabled,
                    ref isInstalled);

                GUILayout.Space(4);
                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
                {
                    GUILayout.Space(5);
                    string displayName = network.displayAdapterName;
                    if (displayName.Equals("Google (AdMob and Ad Manager)")) displayName = "Admob";
                    EditorGUILayout.LabelField(new GUIContent(displayName), NetworkWidthOption);
                    EditorGUILayout.LabelField(new GUIContent(currentVersion), VersionWidthOption);
                    GUILayout.Space(3);
                    EditorGUILayout.LabelField(new GUIContent(latestVersion), VersionWidthOption);
                    GUILayout.Space(3);
                    GUILayout.FlexibleSpace();

                    GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
                    if (GUILayout.Button(new GUIContent(status), FieldWidth))
                    {
                        // Download the plugin.
                        EditorCoroutine.Start(IronSourceManager.Instance.DownloadFileDependency(network.downloadUrl));
                    }

                    GUI.enabled = !EditorApplication.isCompiling;
                    GUILayout.Space(2);

                    GUI.enabled = isInstalled && !EditorApplication.isCompiling;
                    if (GUILayout.Button(_iconUnintall))
                    {
                        EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.displayAdapterName + "...", 0.5f);
                        string pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, "IronSource", "Editor", network.fileName));
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, "IronSource", "Editor", network.fileName + ".meta"));

                        IronSourceManager.RefreshAllCurrentVersionAdapter();
                        // Refresh UI
                        AssetDatabase.Refresh();
                        EditorUtility.ClearProgressBar();
                    }

                    GUI.enabled = !EditorApplication.isCompiling;
                    GUILayout.Space(5);
                }
            }
        }

        /// <summary>
        /// Use Install All Network to import package => AssetDatabase.importPackageCompleted not called
        /// </summary>
        private void DrawAdmobInstallAllNetwork()
        {
            var showInstallAll = false;
            var showUninstallAll = false;
            for (int i = 0; i < Settings.AdmobSettings.editorListNetwork.Count; i++)
            {
                var network = Settings.AdmobSettings.editorListNetwork[i];
                string currentVersion = network.currentVersion != null ? network.currentVersion.unity : "";
                var status = "";
                var isActionEnabled = false;
                var isInstalled = false;
                ValidateVersionAdmob(network.CurrentToLatestVersionComparisonResult,
                    ref currentVersion,
                    ref status,
                    ref isActionEnabled,
                    ref isInstalled);

                if (isActionEnabled) showInstallAll = true;
                if (isInstalled) showUninstallAll = true;
            }

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(), NetworkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                GUILayout.FlexibleSpace();

                GUI.enabled = showInstallAll && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent("Install All"), FieldWidth))
                {
                    SettingManager.Instance.DownloadAllPlugin(Settings.AdmobSettings.editorListNetwork);
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = showUninstallAll && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent("Unistall All"), GUILayout.Width(ACTION_FIELD_WIDTH + 10)))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting All Network...", 0.5f);
                    var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;

                    foreach (var network in Settings.AdmobSettings.editorListNetwork)
                    {
                        foreach (var pluginFilePath in network.pluginFilePath)
                        {
                            if (pluginFilePath.StartsWith("Plugins"))
                            {
                                FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", pluginFilePath));
                                FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", pluginFilePath + ".meta"));
                            }
                            else
                            {
                                FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
                                FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
                            }
                        }

                        network.currentVersion = new NetworkVersion();
                        network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Lesser;
                        SettingManager.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
                        SettingManager.Instance.UpdateCurrentVersion(network);
                    }

                    // Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(5);
            }
        }

        /// <summary>
        /// AssetDatabase.importPackageCompleted not called
        /// </summary>
        private void DrawApplovinInstallAllNetwork()
        {
            var showInstallAll = false;
            var showUninstallAll = false;
            for (int i = 0; i < Settings.MaxSettings.editorListNetwork.Count; i++)
            {
                var network = Settings.MaxSettings.editorListNetwork[i];
                var status = "";
                string currentVersion = network.CurrentVersions != null ? network.CurrentVersions.Unity : "";
                var isActionEnabled = false;
                var isInstalled = false;
                ValidateVersionMax(network.CurrentToLatestVersionComparisonResult,
                    ref currentVersion,
                    ref status,
                    ref isActionEnabled,
                    ref isInstalled);

                if (isActionEnabled) showInstallAll = true;
                if (isInstalled) showUninstallAll = true;
            }

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(), NetworkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                GUILayout.FlexibleSpace();

                GUI.enabled = showInstallAll && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent("Install All"), FieldWidth))
                {
                    MaxManager.Instance.DownloadAllPlugin(Settings.MaxSettings.editorListNetwork);
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = showUninstallAll && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent("Unistall All"), GUILayout.Width(ACTION_FIELD_WIDTH + 10)))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting All Network...", 0.5f);
                    var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;

                    foreach (var network in Settings.MaxSettings.editorListNetwork)
                    {
                        var status = "";
                        var isActionEnabled = false;
                        var isInstalled = false;
                        string currentVersion = network.CurrentVersions != null ? network.CurrentVersions.Unity : "";

                        if (!ValidateVersionMax(network.CurrentToLatestVersionComparisonResult,
                                ref currentVersion,
                                ref status,
                                ref isActionEnabled,
                                ref isInstalled))
                        {
                            foreach (var pluginFilePath in network.PluginFilePaths)
                            {
                                FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
                                FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
                            }

                            MaxManager.UpdateCurrentVersions(network, pluginRoot);
                        }
                    }

                    // Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(5);
            }
        }

        /// <summary>
        /// AssetDatabase.importPackageCompleted not called
        /// </summary>
        private void DrawIronsourceInstallAllNetwork()
        {
            var showInstallAll = false;
            var showUninstallAll = false;
            for (int i = 0; i < Settings.IronSourceSettings.editorListNetwork.Count; i++)
            {
                var network = Settings.IronSourceSettings.editorListNetwork[i];
                var status = "";
                string currentVersion = network.currentUnityVersion;
                var isActionEnabled = false;
                var isInstalled = false;
                ValidateVersionIronSource(network.currentStatus,
                    ref currentVersion,
                    ref status,
                    ref isActionEnabled,
                    ref isInstalled);

                if (isActionEnabled) showInstallAll = true;
                if (isInstalled) showUninstallAll = true;
            }

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(new GUIContent(), NetworkWidthOption);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(), VersionWidthOption);
                GUILayout.Space(3);
                GUILayout.FlexibleSpace();

                const string ironsourceKeyInstallAll = "IronSource_InstallAll";
                GUI.enabled = showInstallAll && !EditorApplication.isCompiling && EditorPrefs.GetBool(ironsourceKeyInstallAll);
                if (GUILayout.Button(new GUIContent("Install All"), FieldWidth))
                {
                    EditorPrefs.SetBool(ironsourceKeyInstallAll, false);
                    InEditor.DelayedCall(2f, () => EditorPrefs.SetBool(ironsourceKeyInstallAll, true));
                    IronSourceManager.Instance.DownloadAllPlugin(Settings.IronSourceSettings.editorListNetwork);
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = showUninstallAll && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent("Unistall All"), GUILayout.Width(ACTION_FIELD_WIDTH + 10)))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting All Network...", 0.5f);
                    var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;

                    foreach (var network in Settings.IronSourceSettings.editorListNetwork)
                    {
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, "IronSource", "Editor", network.fileName));
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, "IronSource", "Editor", network.fileName + ".meta"));

                        IronSourceManager.RefreshAllCurrentVersionAdapter();
                    }

                    // Refresh UI
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(5);
            }
        }

        private bool ValidateVersionAdmob(
            EVersionComparisonResult comparison,
            ref string currentVersion,
            // ReSharper disable once RedundantAssignment
            ref string status,
            // ReSharper disable once RedundantAssignment
            ref bool isActionEnabled,
            // ReSharper disable once RedundantAssignment
            ref bool isInstalled)
        {
            if (string.IsNullOrEmpty(currentVersion))
            {
                status = "Install";
                currentVersion = "Not Installed";
                isActionEnabled = true;
                isInstalled = false;
            }
            else
            {
                isInstalled = true;

                // A newer version is available
                if (comparison == EVersionComparisonResult.Lesser)
                {
                    status = "Upgrade";
                    isActionEnabled = true;
                }
                // Current installed version is newer than latest version from DB (beta version)
                else if (comparison == EVersionComparisonResult.Greater)
                {
                    status = "Installed";
                    isActionEnabled = false;
                }
                // Already on the latest version
                else
                {
                    status = "Installed";
                    isActionEnabled = false;
                }
            }

            return isActionEnabled;
        }

        private bool ValidateVersionMax(
            EVersionComparisonResult comparisonResult,
            ref string currentVersion,
            // ReSharper disable once RedundantAssignment
            ref string status,
            // ReSharper disable once RedundantAssignment
            ref bool isActionEnabled,
            // ReSharper disable once RedundantAssignment
            ref bool isInstalled)
        {
            if (string.IsNullOrEmpty(currentVersion))
            {
                status = "Install";
                currentVersion = "Not Installed";
                isActionEnabled = true;
                isInstalled = false;
            }
            else
            {
                isInstalled = true;

                // A newer version is available
                if (comparisonResult == EVersionComparisonResult.Lesser)
                {
                    status = "Upgrade";
                    isActionEnabled = true;
                }
                // Current installed version is newer than latest version from DB (beta version)
                else if (comparisonResult == EVersionComparisonResult.Greater)
                {
                    status = "Installed";
                    isActionEnabled = false;
                }
                // Already on the latest version
                else
                {
                    status = "Installed";
                    isActionEnabled = false;
                }
            }

            return isActionEnabled;
        }

        private bool ValidateVersionIronSource(
            EAdapterStatus currentStatus,
            ref string currentVersion,
            // ReSharper disable once RedundantAssignment
            ref string status,
            // ReSharper disable once RedundantAssignment
            ref bool isActionEnabled,
            // ReSharper disable once RedundantAssignment
            ref bool isInstalled)
        {
            if (string.IsNullOrEmpty(currentVersion))
            {
                status = "Install";
                currentVersion = "Not Installed";
                isActionEnabled = true;
                isInstalled = false;
            }
            else
            {
                isInstalled = true;
                switch (currentStatus)
                {
                    // A newer version is available
                    case EAdapterStatus.Upgrade:
                        status = "Upgrade";
                        isActionEnabled = true;
                        break;
                    // Current installed version is newer than latest version from DB (beta version)
                    case EAdapterStatus.Installed:
                        status = "Installed";
                        isActionEnabled = false;
                        break;
                    // Already on the latest version
                    default:
                        status = "Installed";
                        isActionEnabled = false;
                        break;
                }
            }

            return isActionEnabled;
        }

        #endregion
    }
}