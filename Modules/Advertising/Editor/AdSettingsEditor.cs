using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
#if PANCAKE_ADMOB
using GoogleMobileAds.Editor;
#endif
using Newtonsoft.Json;
using PancakeEditor;
using Unity.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Editor = PancakeEditor.Editor;

namespace Pancake.Monetization
{
    [CustomEditor(typeof(AdSettings))]
    public class AdSettingsEditor : UnityEditor.Editor
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

            public static readonly Property AutoInit = new Property(new GUIContent("Auto Initialize",
                "When AutoInitialize is enabled it will automatically create a gameObject in Runtime containing the component Advertising and init it!"));

            public static readonly Property AutoLoadAdsMode = new Property(new GUIContent("Auto Ad-Loading Mode"));

            public static readonly Property ADCheckingInterval = new Property(new GUIContent("  Checking Interval", "Time (seconds) between 2 ad-availability checks"));

            public static readonly Property ADLoadingInterval = new Property(new GUIContent("  Loading Interval",
                "Minimum time (seconds) between two ad-loading requests, this is to restrict the number of requests sent to ad networks"));

            public static readonly Property PrivacyPolicyUrl = new Property(new GUIContent("  Privacy&Policy Url", "Privacy policy url"));

            public static readonly Property EnableGdpr = new Property(new GUIContent("GDPR",
                "General data protection regulation \nApp requires user consent before these events can be sent, you can delay app measurement until you explicitly initialize the Mobile Ads SDK or load an ad."));

            public static readonly Property EnableMultipleDex = new Property(new GUIContent("MultiDex"));
            public static readonly Property HideAppOpenAdStartup = new Property(new GUIContent("Hide AppOpenAd Startup", "Hide App Open Ad When Startup"));
            public static readonly Property CurrentNetwork = new Property(new GUIContent("Current Network", "Current network use show ad"));
        }

        private static class AdmobProperties
        {
            public static SerializedProperty main;
            public static readonly Property Enable = new Property(new GUIContent("Enable", "Enable using admob ad"));
            public static readonly Property DevicesTest = new Property(new GUIContent("Devices Test", "List devices show real ad but mark test user"));
            public static readonly Property BannerAdUnit = new Property(new GUIContent("Banner Ad"));
            public static readonly Property InterstitialAdUnit = new Property(new GUIContent("Interstitial Ad"));
            public static readonly Property RewardedAdUnit = new Property(new GUIContent("Rewarded Ad"));
            public static readonly Property RewardedInterstitialAdUnit = new Property(new GUIContent("Rewarded Interstitial Ad"));
            public static readonly Property AppOpenAdUnit = new Property(new GUIContent("App Open Ad"));
            public static readonly Property EnableTestMode = new Property(new GUIContent("Enable Test Mode", "Enable true when want show test ad"));
        }

        private static class ApplovinProperties
        {
            public static SerializedProperty main;
            public static Property enable = new Property(new GUIContent("Enable", "Enable using applovin ad"));
            public static Property sdkKey = new Property(new GUIContent("Sdk Key", "Sdk of applovin"));
            public static Property bannerAdUnit = new Property(new GUIContent("Banner Ad"));
            public static Property interstitialAdUnit = new Property(new GUIContent("Interstitial Ad"));
            public static Property rewardedAdUnit = new Property(new GUIContent("Rewarded Ad"));
            public static Property rewardedInterstitialAdUnit = new Property(new GUIContent("Rewarded Interstitial Ad"));
            public static Property appOpenAdUnit = new Property(new GUIContent("App Open Ad"));
            public static Property enableAgeRestrictedUser = new Property(new GUIContent("Age Restrictd User"));

            public static Property enableRequestAdAfterHidden = new Property(new GUIContent("Request Ad After Hidden",
                "Request to add new interstitial and rewarded ad after user finish view ad. Need kick-off request to cache ads as quickly as possible"));

            public static Property enableMaxAdReview = new Property(new GUIContent("Enable MAX Ad Review"));
        }

        /// <summary>
        /// Delegate to be called when downloading a plugin with the progress percentage. 
        /// </summary>
        /// <param name="pluginName">The name of the plugin being downloaded.</param>
        /// <param name="progress">Percentage downloaded.</param>
        /// <param name="done">Whether or not the download is complete.</param>
        public delegate void DownloadMediationProgressCallback(string pluginName, float progress, bool done);

        /// <summary>
        /// Delegate to be called when a plugin package is imported.
        /// </summary>
        /// <param name="network">The network data for which the package is imported.</param>
        public delegate void AdmobImportMediationCompleted(Network network);

        #region properties

        //Runtime auto initialization
        private SerializedProperty _autoInitializeProperty;
        public static UnityWebRequest webRequest;
        public static DownloadMediationProgressCallback downloadMediationProgressCallback;
        public static AdmobImportMediationCompleted admobImportMediationCompleted;
        private const float ACTION_FIELD_WIDTH = 65f;
        private const float NETWORK_FIELD_MIN_WIDTH = 100f;
        private const float VERSION_FIELD_MIN_WIDTH = 100f;
        private static readonly GUILayoutOption NetworkWidthOption = GUILayout.Width(NETWORK_FIELD_MIN_WIDTH);
        private static readonly GUILayoutOption VersionWidthOption = GUILayout.Width(VERSION_FIELD_MIN_WIDTH);
        private static readonly GUILayoutOption FieldWidth = GUILayout.Width(ACTION_FIELD_WIDTH);

        private GUIContent _warningIcon;
        private GUIContent _iconUnintall;
        private GUIStyle _headerLabelStyle;

        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly List<string> PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory = new List<string>();

        #endregion

        #region api

        private void OnEnable()
        {
            downloadMediationProgressCallback = OnDownloadMediationProgress;
            admobImportMediationCompleted = OnAdmobImportMediationCompleted;

            AssetDatabase.importPackageCompleted -= OnAdmobMediationPackageImportCompleted;
            AssetDatabase.importPackageCompleted += OnAdmobMediationPackageImportCompleted;
            AssetDatabase.importPackageCancelled -= OnAdmobMediationPackageImportCancelled;
            AssetDatabase.importPackageCancelled += OnAdmobMediationPackageImportCancelled;
            AssetDatabase.importPackageFailed -= OnAdmobMediationPackageImportFailed;
            AssetDatabase.importPackageFailed += OnAdmobMediationPackageImportFailed;
        }


        private void OnDownloadMediationProgress(string pluginName, float progress, bool done)
        {
            // Download is complete. Clear progress bar.
            if (done)
            {
                EditorUtility.ClearProgressBar();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar("Advertising", string.Format("Downloading {0} mediation...", pluginName), progress))
                {
                    webRequest?.Abort();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void Init()
        {
            _warningIcon = Uniform.IconContent("console.warnicon.sml", "Adapter not compatible, please update to the latest version.");
            _iconUnintall = Uniform.IconContent("d_TreeEditor.Trash", "Uninstall");
            _headerLabelStyle = new GUIStyle(EditorStyles.label) {fontSize = 12, fontStyle = FontStyle.Bold, fixedHeight = 18};

            _autoInitializeProperty = serializedObject.FindProperty("runtimeAutoInitialize");

            AdProperties.main = serializedObject.FindProperty("adCommonSettings");
            AdProperties.AutoInit.property = AdProperties.main.FindPropertyRelative("autoInit");
            AdProperties.AutoLoadAdsMode.property = AdProperties.main.FindPropertyRelative("autoLoadingAd");
            AdProperties.ADCheckingInterval.property = AdProperties.main.FindPropertyRelative("adCheckingInterval");
            AdProperties.ADLoadingInterval.property = AdProperties.main.FindPropertyRelative("adLoadingInterval");
            AdProperties.EnableGdpr.property = AdProperties.main.FindPropertyRelative("enableGdpr");
            AdProperties.PrivacyPolicyUrl.property = AdProperties.main.FindPropertyRelative("privacyUrl");
            AdProperties.EnableMultipleDex.property = AdProperties.main.FindPropertyRelative("multiDex");
            AdProperties.HideAppOpenAdStartup.property = AdProperties.main.FindPropertyRelative("hideAppOpenAdStartup");
            AdProperties.CurrentNetwork.property = AdProperties.main.FindPropertyRelative("currentNetwork");

            AdmobProperties.main = serializedObject.FindProperty("admobSettings");
            AdmobProperties.Enable.property = AdmobProperties.main.FindPropertyRelative("enable");
            AdmobProperties.DevicesTest.property = AdmobProperties.main.FindPropertyRelative("devicesTest");
            AdmobProperties.BannerAdUnit.property = AdmobProperties.main.FindPropertyRelative("bannerAdUnit");
            AdmobProperties.InterstitialAdUnit.property = AdmobProperties.main.FindPropertyRelative("interstitialAdUnit");
            AdmobProperties.RewardedAdUnit.property = AdmobProperties.main.FindPropertyRelative("rewardedAdUnit");
            AdmobProperties.RewardedInterstitialAdUnit.property = AdmobProperties.main.FindPropertyRelative("rewardedInterstitialAdUnit");
            AdmobProperties.AppOpenAdUnit.property = AdmobProperties.main.FindPropertyRelative("appOpenAdUnit");
            AdmobProperties.EnableTestMode.property = AdmobProperties.main.FindPropertyRelative("enableTestMode");

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

            if (AdSettings.AdmobSettings.editorListNetwork.IsNullOrEmpty()) LoadAdmobMediation();
            else
            {
                foreach (var n in AdSettings.AdmobSettings.editorListNetwork) UpdateCurrentVersionAdmobMediation(n);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();

            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            #region draw

            Uniform.DrawGroupFoldout("monetization_ads_basic",
                "Setting",
                () =>
                {
                    EditorGUILayout.PropertyField(AdProperties.AutoInit.property, AdProperties.AutoInit.content);
                    EditorGUILayout.PropertyField(AdProperties.EnableGdpr.property, AdProperties.EnableGdpr.content);
                    if (AdSettings.AdCommonSettings.EnableGdpr)
                        EditorGUILayout.PropertyField(AdProperties.PrivacyPolicyUrl.property, AdProperties.PrivacyPolicyUrl.content);
                    EditorGUILayout.PropertyField(AdProperties.EnableMultipleDex.property, AdProperties.EnableMultipleDex.content);
                    EditorGUILayout.PropertyField(AdProperties.HideAppOpenAdStartup.property, AdProperties.HideAppOpenAdStartup.content);
                    EditorGUILayout.PropertyField(AdProperties.CurrentNetwork.property, AdProperties.CurrentNetwork.content);
                    CreateMainTemplateGradle(AdSettings.AdCommonSettings.MultiDex);
                    GUILayout.Space(8);
                    EditorGUILayout.PropertyField(AdProperties.AutoLoadAdsMode.property, AdProperties.AutoLoadAdsMode.content);
                    if (AdSettings.AdCommonSettings.AutoLoadingAd == EAutoLoadingAd.None) return;
                    EditorGUILayout.PropertyField(AdProperties.ADCheckingInterval.property, AdProperties.ADCheckingInterval.content);
                    EditorGUILayout.PropertyField(AdProperties.ADLoadingInterval.property, AdProperties.ADLoadingInterval.content);
                });

            EditorGUILayout.Space();
            Uniform.DrawGroupFoldout("monetization_ads_admob",
                "AdMob",
                () =>
                {
                    EditorGUILayout.PropertyField(AdmobProperties.Enable.property, AdmobProperties.Enable.content);
                    if (AdSettings.AdmobSettings.Enable)
                    {
                        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                        if (IsAdmobSdkImported())
                        {
                            if (!Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
                            {
                                Editor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                                AssetDatabase.Refresh();
                            }
                        }
                        else
                        {
                            if (Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
                            {
                                Editor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                                AssetDatabase.Refresh();
                            }

                            // show button install admob sdk
                            GUI.enabled = !EditorApplication.isCompiling;

                            if (GUILayout.Button("Install Admob SDK", GUILayout.MaxHeight(40f)))
                            {
                                AssetDatabase.ImportPackage(Editor.AssetInPackagePath("Editor/UnityPackages", "admob.unitypackage"), false);
                            }

                            GUI.enabled = true;
                        }

                        if (IsAdmobSdkImported() && Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
                        {
                            EditorGUILayout.HelpBox("Admob plugin was imported", MessageType.Info);
                            if (AdSettings.AdCommonSettings.EnableGdpr)
                            {
                                EditorGUILayout.HelpBox("GDPR is enable so you should turn on Delay app measurement in GoogleMobileAds setting", MessageType.Info);
                            }

                            EditorGUILayout.Space();
#if PANCAKE_ADMOB
                            var googleMobileAdSetting = Resources.Load<GoogleMobileAdsSettings>(nameof(GoogleMobileAdsSettings));
                            if (googleMobileAdSetting == null)
                            {
                                GUI.enabled = !EditorApplication.isCompiling;
                                if (GUILayout.Button("Create GoogleMobileAd Setting", GUILayout.MaxHeight(40f)))
                                {
                                    var setting = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();
                                    const string path = "Assets/GoogleMobileAds/Resources";
                                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(GoogleMobileAdsSettings)}.asset");
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                    Debug.Log($"{nameof(GoogleMobileAdsSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(GoogleMobileAdsSettings)}.asset");
                                }
                            
                                GUI.enabled = true;
                            }
                            else
                            {
                                var editor = UnityEditor.Editor.CreateEditor(googleMobileAdSetting);
                                editor.OnInspectorGUI();
                            }
#endif
                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(AdmobProperties.EnableTestMode.property, AdmobProperties.EnableTestMode.content);
                            if (AdSettings.AdmobSettings.EnableTestMode)
                            {
                                EditorGUILayout.PropertyField(AdmobProperties.DevicesTest.property, AdmobProperties.DevicesTest.content);
                            }
                            
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(AdmobProperties.BannerAdUnit.property, AdmobProperties.BannerAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.InterstitialAdUnit.property, AdmobProperties.InterstitialAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.RewardedAdUnit.property, AdmobProperties.RewardedAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.RewardedInterstitialAdUnit.property, AdmobProperties.RewardedInterstitialAdUnit.content, true);
                            EditorGUILayout.PropertyField(AdmobProperties.AppOpenAdUnit.property, AdmobProperties.AppOpenAdUnit.content, true);
                            EditorGUI.indentLevel--;
                            Uniform.DrawGroupFoldout("monetization_ads_admob_mediation",
                                "Mediation",
                                () =>
                                {
                                    DrawHeaderMediation();
                                    foreach (var network in AdSettings.AdmobSettings.editorListNetwork)
                                    {
                                        DrawAdmobNetworkDetailRow(network);
                                    }
                                });
                        }
                    }
                    else
                    {
                        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                        if (Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
                        {
                            Editor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                            AssetDatabase.Refresh();
                        }
                    }
                });

            EditorGUILayout.Space();
//             Uniform.DrawGroupFoldout("MAX_MODULE",
//                 "MAX",
//                 () =>
//                 {
//                     EditorGUILayout.PropertyField(ApplovinProperties.enable.property, ApplovinProperties.enable.content);
//                     if (AdSettings.MaxSettings.Enable)
//                     {
//                         SettingManager.ValidateApplovinSdkImported();
//                         if (IsApplovinSdkAvaiable)
//                         {
//                             EditorGUILayout.HelpBox("Applovin plugin was imported", MessageType.Info);
//
//                             if (AdSettings.MaxSettings.editorImportingSdk != null && !string.IsNullOrEmpty(AdSettings.MaxSettings.editorImportingSdk.lastVersion.unity) &&
//                                 AdSettings.MaxSettings.editorImportingSdk.CurrentToLatestVersionComparisonResult == EVersionComparisonResult.Lesser)
//                             {
//                                 if (GUILayout.Button("Update MaxSdk Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
//                                 {
//                                     EditorCoroutine.Start(MaxManager.Instance.DownloadMaxSdk(AdSettings.MaxSettings.editorImportingSdk));
//                                 }
//                             }
//
//                             EditorGUILayout.Space();
//                             EditorGUILayout.PropertyField(ApplovinProperties.sdkKey.property, ApplovinProperties.sdkKey.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.enableAgeRestrictedUser.property, ApplovinProperties.enableAgeRestrictedUser.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.enableRequestAdAfterHidden.property, ApplovinProperties.enableRequestAdAfterHidden.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.enableMaxAdReview.property, ApplovinProperties.enableMaxAdReview.content);
// #if PANCAKE_MAX_ENABLE
//                             AppLovinSettings.Instance.QualityServiceEnabled = AdSettings.MaxSettings.EnableMaxAdReview;
//                             AppLovinSettings.Instance.ConsentFlowEnabled = AdSettings.AdCommonSettings.EnableGDPR;
//                             AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl = AdSettings.AdCommonSettings.PrivacyPolicyUrl;
// #endif
//                             EditorGUILayout.Space();
//                             EditorGUI.indentLevel++;
//                             EditorGUILayout.PropertyField(ApplovinProperties.bannerAdUnit.property, ApplovinProperties.bannerAdUnit.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.interstitialAdUnit.property, ApplovinProperties.interstitialAdUnit.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.rewardedAdUnit.property, ApplovinProperties.rewardedAdUnit.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.rewardedInterstitialAdUnit.property, ApplovinProperties.rewardedInterstitialAdUnit.content);
//                             EditorGUILayout.PropertyField(ApplovinProperties.appOpenAdUnit.property, ApplovinProperties.appOpenAdUnit.content);
//                             EditorGUI.indentLevel--;
//                             EditorGUILayout.Space();
//
//                             Uniform.DrawGroupFoldout("APPLOVIN_MODULE_MEDIATION",
//                                 "MEDIATION",
//                                 () =>
//                                 {
//                                     DrawHeaderMediation();
//                                     foreach (var network in AdSettings.MaxSettings.editorListNetwork)
//                                     {
//                                         DrawApplovinNetworkDetailRow(network);
//                                     }
//
//                                     DrawApplovinInstallAllNetwork();
//                                 });
//                             EditorGUILayout.Space();
//                         }
//                         else
//                         {
//                             EditorGUILayout.HelpBox("Max plugin not found. Please import it to show ads from Applovin", MessageType.Warning);
//                             if (GUILayout.Button("Import MAX Plugin", GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f)))
//                             {
//                                 if (AdSettings.MaxSettings.editorImportingSdk != null)
//                                 {
//                                     EditorCoroutine.Start(MaxManager.Instance.DownloadMaxSdk(AdSettings.MaxSettings.editorImportingSdk));
//                                 }
//                                 else
//                                 {
//                                     Application.OpenURL("https://github.com/gamee-studio/ads/releases/tag/1.0.21");
//                                 }
//                             }
//                         }
//
// #if PANCAKE_MAX_ENABLE
//                         if (GUI.changed) AppLovinSettings.Instance.SaveAsync();
// #endif
//                     }
//                 });

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


//         private void DrawApplovinNetworkDetailRow(MaxNetwork network)
//         {
//             string currentVersion = network.CurrentVersions != null ? network.CurrentVersions.Unity : "";
//             string latestVersion = network.LatestVersions.Unity;
//             var status = "";
//             var isActionEnabled = false;
//             var isInstalled = false;
//             ValidateVersionMax(network.CurrentToLatestVersionComparisonResult,
//                 ref currentVersion,
//                 ref status,
//                 ref isActionEnabled,
//                 ref isInstalled);
//
//             GUILayout.Space(4);
//             using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
//             {
//                 GUILayout.Space(5);
//                 EditorGUILayout.LabelField(new GUIContent(network.DisplayName), NetworkWidthOption);
//                 EditorGUILayout.LabelField(new GUIContent(currentVersion), VersionWidthOption);
//                 GUILayout.Space(3);
//                 EditorGUILayout.LabelField(new GUIContent(latestVersion), VersionWidthOption);
//                 GUILayout.FlexibleSpace();
//
//                 if (network.RequiresUpdate)
//                 {
//                     GUILayout.Label(_warningIcon);
//                 }
//
//                 GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
//                 if (GUILayout.Button(new GUIContent(status), FieldWidth))
//                 {
//                     // Download the plugin.
//                     EditorCoroutine.Start(MaxManager.Instance.DownloadPlugin(network));
//                     if (network.Name.Equals("ALGORIX_NETWORK"))
//                     {
//                         AdsEditorUtil.CreateMainTemplateGradle();
//                         AdsEditorUtil.AddSettingProguardFile(new List<string>()
//                         {
//                             "-keep class com.alxad.* {;}",
//                             "-keep class admob.custom.adapter.* {;}",
//                             "-keep class anythink.custom.adapter.* {;}",
//                             "-keep class com.mopub.mobileads.* {;}",
//                             "-keep class com.applovin.mediation.adapters.* {;}"
//                         });
//                         AdsEditorUtil.AddAlgorixSettingGradle(network);
//                     }
//                 }
//
//                 GUI.enabled = !EditorApplication.isCompiling;
//                 GUILayout.Space(2);
//
//                 GUI.enabled = isInstalled && !EditorApplication.isCompiling;
//                 if (GUILayout.Button(_iconUnintall, FieldWidth))
//                 {
//                     EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.DisplayName + "...", 0.5f);
//                     var pluginRoot = SettingManager.MediationSpecificPluginParentDirectory;
//                     foreach (var pluginFilePath in network.PluginFilePaths)
//                     {
//                         FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
//                         FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
//                     }
//
//                     if (network.Name.Equals("ALGORIX_NETWORK"))
//                     {
//                         AdsEditorUtil.RemoveAlgorixSettingGradle();
//                         AdsEditorUtil.DeleteProguardFile();
//                     }
//
//                     SettingManager.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
//                     MaxManager.UpdateCurrentVersions(network, pluginRoot);
//
//                     // Refresh UI
//                     AssetDatabase.Refresh();
//                     EditorUtility.ClearProgressBar();
//                 }
//
//                 GUI.enabled = !EditorApplication.isCompiling;
//                 GUILayout.Space(5);
//             }
//
//             if (isInstalled)
//             {
//                 if (network.Name.Equals("ADMOB_NETWORK"))
//                 {
// #if PANCAKE_MAX_ENABLE
//                     // ReSharper disable once PossibleNullReferenceException
//                     if ((int) MaxSdkUtils.CompareUnityMediationVersions(network.CurrentVersions.Unity, "android_19.0.1.0_ios_7.57.0.0") ==
//                         (int) EVersionComparisonResult.Greater)
//                     {
//                         GUILayout.BeginHorizontal();
//                         GUILayout.Space(20);
//
//                         using (new EditorGUILayout.VerticalScope())
//                         {
//                             AppLovinSettings.Instance.AdMobAndroidAppId =
//                                 Uniform.DrawTextField("App ID (Android)", AppLovinSettings.Instance.AdMobAndroidAppId, NetworkWidthOption);
//                             AppLovinSettings.Instance.AdMobIosAppId = Uniform.DrawTextField("App ID (iOS)", AppLovinSettings.Instance.AdMobIosAppId, NetworkWidthOption);
//                         }
//
//                         GUILayout.EndHorizontal();
//                     }
// #endif
//                 }
//             }
//         }


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


        private static void CreateMainTemplateGradle(bool multiDex)
        {
            const string androidPath = "Assets/Plugins/Android/";
            const string mainTemplatePath = "Assets/Plugins/Android/mainTemplate.gradle";
            if (multiDex)
            {
                if (!Directory.Exists(androidPath)) Directory.CreateDirectory(androidPath);
                if (File.Exists(mainTemplatePath)) return;
                string path = Editor.AssetInPackagePath("Editor/Misc/Templates", "mainTemplate.txt");
                string mainTemplate = (AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset)?.text;
                var writer = new StreamWriter(mainTemplatePath, false);
                writer.Write(mainTemplate);
                writer.Close();
                AssetDatabase.ImportAsset(mainTemplatePath);
            }
            else
            {
                if (!File.Exists(mainTemplatePath)) return;
                FileUtil.DeleteFileOrDirectory(mainTemplatePath);
                FileUtil.DeleteFileOrDirectory(mainTemplatePath + ".meta");
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Write the given bytes data under the given filePath. 
        /// The filePath should be given with its path and filename. (e.g. c:/tmp/test.zip)
        /// </summary>
        private static void UnZip(string filePath, byte[] data)
        {
            using (var s = new ZipInputStream(new MemoryStream(data)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    // create directory
                    if (directoryName?.Length > 0)
                    {
                        var dirPath = Path.Combine(filePath, directoryName);

                        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
                    }

                    if (fileName != string.Empty)
                    {
                        // retrieve directory name only from persistence data path.
                        var entryFilePath = Path.Combine(filePath, theEntry.Name);
                        using (var streamWriter = File.Create(entryFilePath))
                        {
                            var size = 2048;
                            var fdata = new byte[size];
                            while (true)
                            {
                                size = s.Read(fdata, 0, fdata.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(fdata, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                } //end of while
            } //end of using
        }

        #endregion

        #region admob

        public void LoadAdmobMediation()
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            const string url = "https://gist.githubusercontent.com/yenmoc/d79936098344befbd8edfa882c17bf20/raw";
            string json = curl.DownloadString(url);
            AdSettings.AdmobSettings.editorListNetwork = JsonConvert.DeserializeObject<List<Network>>(json);
            foreach (var n in AdSettings.AdmobSettings.editorListNetwork)
            {
                UpdateCurrentVersionAdmobMediation(n);
            }
        }

        public void UpdateCurrentVersionAdmobMediation(Network network)
        {
            var dependencyFilePath = Path.Combine(ParentGoogleMobileAdDirectory(), network.dependenciesFilePath);
            var currentVersion = GetCurrentVersion(dependencyFilePath, network.name);
            network.currentVersion = currentVersion;
            SetNetworkUnityVersion(network.name, network.currentVersion.unity);


            var unityVersionComparison = AdsUtil.CompareVersions(network.currentVersion.unity, network.lastVersion.unity);
            var androidVersionComparison = AdsUtil.CompareVersions(network.currentVersion.android, network.lastVersion.android);
            var iosVersionComparison = AdsUtil.CompareVersions(network.currentVersion.ios, network.lastVersion.ios);

            // Overall version is same if all the current and latest (from db) versions are same.
            if (unityVersionComparison == EVersionComparisonResult.Equal && androidVersionComparison == EVersionComparisonResult.Equal &&
                iosVersionComparison == EVersionComparisonResult.Equal)
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Equal;
            }
            // One of the installed versions is newer than the latest versions which means that the publisher is on a beta version.
            else if (unityVersionComparison == EVersionComparisonResult.Greater || androidVersionComparison == EVersionComparisonResult.Greater ||
                     iosVersionComparison == EVersionComparisonResult.Greater)
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Greater;
            }
            // We have a new version available if all Android, iOS and Unity has a newer version available in db.
            else
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Lesser;
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

        private static bool IsAdmobSdkImported() { return AssetDatabase.FindAssets("l:gvhp_exportpath-GoogleMobileAds/GoogleMobileAds.dll").Length >= 1; }

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
                EditorGUILayout.LabelField(network.displayName.Equals("MetaAudienceNetwork") ? new GUIContent("Meta") : new GUIContent(network.displayName),
                    NetworkWidthOption);

                EditorGUILayout.LabelField(new GUIContent(currentVersion), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(latestVersion), VersionWidthOption);
                GUILayout.FlexibleSpace();

                if (network.requireUpdate) GUILayout.Label(_warningIcon);

                GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent(status), FieldWidth))
                {
                    // Download the plugin.
                    EditorCoroutine.Start(AdmobDownloadPlugin(network));
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = isInstalled && !EditorApplication.isCompiling;
                if (GUILayout.Button(_iconUnintall))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.displayName + "...", 0.5f);
                    string parentDir = ParentGoogleMobileAdDirectory();
                    string pluginRoot = !parentDir.StartsWith("Assets") ? "Assets" : parentDir;
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
                    Editor.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
                    AdmobUpdateCurrentVersion(network);

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

        private string ParentGoogleMobileAdDirectory()
        {
            string[] guids = AssetDatabase.FindAssets("l:gvhp_exportpath-GoogleMobileAds/GoogleMobileAds.dll");
            if (!guids.IsNullOrEmpty())
            {
                return AssetDatabase.GUIDToAssetPath(guids[0])
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    .Replace(@"GoogleMobileAds\GoogleMobileAds.dll", "")
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return Path.Combine("Assets", "GoogleMobileAds");
        }

        public IEnumerator AdmobDownloadPlugin(Network network)
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, $"{network.name.ToLowerInvariant()}_{network.lastVersion.unity}.zip");
            string urlDownload = string.Format(network.path, network.lastVersion.unity);
            var downloadHandler = new DownloadHandlerFile(pathFile);
            webRequest = new UnityWebRequest(urlDownload) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f); // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
                downloadMediationProgressCallback?.Invoke(network.displayName, operation.progress, operation.isDone);
            }

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (webRequest.isNetworkError || webRequest.isHttpError)
#else
            if (webRequest.isError)
#endif
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                AdSettings.AdmobSettings.editorImportingNetwork = network;

                string folderUnZip = Path.Combine(Application.temporaryCachePath, "UnZip");
                UnZip(folderUnZip, File.ReadAllBytes(pathFile));

                AssetDatabase.ImportPackage(Path.Combine(folderUnZip,
                        $"{network.displayName}UnityAdapter-{network.lastVersion.unity}",
                        GetAdmobMediationFileName(network)),
                    false);
            }

            webRequest = null;
        }

        public void AdmobUpdateCurrentVersion(Network network)
        {
            var dependencyFilePath = Path.Combine(ParentGoogleMobileAdDirectory(), network.dependenciesFilePath);
            var currentVersion = GetCurrentVersion(dependencyFilePath, network.name);
            network.currentVersion = currentVersion;
            SetNetworkUnityVersion(network.name, network.currentVersion.unity);

            var unityVersionComparison = AdsUtil.CompareVersions(network.currentVersion.unity, network.lastVersion.unity);
            var androidVersionComparison = AdsUtil.CompareVersions(network.currentVersion.android, network.lastVersion.android);
            var iosVersionComparison = AdsUtil.CompareVersions(network.currentVersion.ios, network.lastVersion.ios);

            // Overall version is same if all the current and latest (from db) versions are same.
            if (unityVersionComparison == EVersionComparisonResult.Equal && androidVersionComparison == EVersionComparisonResult.Equal &&
                iosVersionComparison == EVersionComparisonResult.Equal)
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Equal;
            }
            // One of the installed versions is newer than the latest versions which means that the publisher is on a beta version.
            else if (unityVersionComparison == EVersionComparisonResult.Greater || androidVersionComparison == EVersionComparisonResult.Greater ||
                     iosVersionComparison == EVersionComparisonResult.Greater)
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Greater;
            }
            // We have a new version available if all Android, iOS and Unity has a newer version available in db.
            else
            {
                network.CurrentToLatestVersionComparisonResult = EVersionComparisonResult.Lesser;
            }
        }

        private NetworkVersion GetCurrentVersion(string dependencyFilePath, string nameNetwork)
        {
            XDocument dependency;
            try
            {
                dependency = XDocument.Load(dependencyFilePath);
            }
            catch (Exception)
            {
                return new NetworkVersion();
            }

            string androidVersion = null;
            string iosVersion = null;
            var dependenciesElement = dependency.Element("dependencies");
            if (dependenciesElement != null)
            {
                var androidPackages = dependenciesElement.Element("androidPackages");
                if (androidPackages != null)
                {
                    var adapterPackage = androidPackages.Descendants()
                        .FirstOrDefault(element =>
                            element.Name.LocalName.Equals("androidPackage") && element.FirstAttribute.Name.LocalName.Equals("spec") &&
                            element.FirstAttribute.Value.StartsWith("com.google.ads"));
                    if (adapterPackage != null)
                    {
                        androidVersion = adapterPackage.FirstAttribute.Value.Split(':').Last();
                        // Hack alert: Some Android versions might have square brackets to force a specific version. Remove them if they are detected.
                        if (androidVersion.StartsWith("["))
                        {
                            androidVersion = androidVersion.Trim('[', ']');
                        }
                    }
                }

                var iosPods = dependenciesElement.Element("iosPods");
                if (iosPods != null)
                {
                    var adapterPod = iosPods.Descendants()
                        .FirstOrDefault(element =>
                            element.Name.LocalName.Equals("iosPod") && element.FirstAttribute.Name.LocalName.Equals("name") &&
                            element.FirstAttribute.Value.StartsWith("GoogleMobileAds"));
                    if (adapterPod != null)
                    {
                        iosVersion = adapterPod.Attributes().First(attribute => attribute.Name.LocalName.Equals("version")).Value;
                    }
                }
            }

            var currentVersion = new NetworkVersion();
            if (!string.IsNullOrEmpty(androidVersion) && !string.IsNullOrEmpty(iosVersion))
            {
                currentVersion.android = androidVersion;
                currentVersion.ios = iosVersion;
            }

            currentVersion.unity = GetNetworkUnityVersion(nameNetwork);

            return currentVersion;
        }

        public static void SetNetworkUnityVersion(string name, string version) { EditorPrefs.SetString($"{Application.identifier}_ads_{name}_unity", version); }

        public static string GetNetworkUnityVersion(string name) { return EditorPrefs.GetString($"{Application.identifier}_ads_{name}_unity"); }

        /// <summary>
        /// since MediationExtras class is already added in Admob package
        /// so we don't need MediationExtras class from importing mediation package anymore it will be confusing
        /// </summary>
        public void RemoveMediationExtras(Network network)
        {
            if (network.name.Equals("ADCOLONY_NETWORK") || network.name.Equals("VUNGLE_NETWORK"))
            {
                string parentDir = ParentGoogleMobileAdDirectory();
                string path = parentDir;
                if (!parentDir.StartsWith("Assets")) path = "Assets";
                FileUtil.DeleteFileOrDirectory(Path.Combine(path, "GoogleMobileAds/Api/Mediation/MediationExtras.cs"));
                FileUtil.DeleteFileOrDirectory(Path.Combine(path, "GoogleMobileAds/Api/Mediation/MediationExtras.cs.meta"));
                AssetDatabase.Refresh();
            }
        }

        private bool IsAdmobImportingNetwork(string packageName)
        {
            // Note: The pluginName doesn't have the '.unitypacakge' extension included in its name but the pluginFileName does. So using Contains instead of Equals.
            return AdSettings.AdmobSettings.editorImportingNetwork != null &&
                   GetAdmobMediationFileName(AdSettings.AdmobSettings.editorImportingNetwork).Contains(packageName.Split(@"\").Last());
        }

        private void OnAdmobImportMediationCompleted(Network network)
        {
            SetNetworkUnityVersion(network.name, network.lastVersion.unity);
            UpdateCurrentVersionAdmobMediation(network);
            RemoveMediationExtras(network);
        }

        private string GetAdmobMediationFileName(Network network) { return $"GoogleMobileAds{network.displayName}Mediation.unitypackage"; }

        private void OnAdmobMediationPackageImportFailed(string packageName, string errorMessage) { }

        private void OnAdmobMediationPackageImportCancelled(string packageName) { }

        private void OnAdmobMediationPackageImportCompleted(string packageName)
        {
            if (!IsAdmobImportingNetwork(packageName)) return;

            string pluginParentDir = ParentGoogleMobileAdDirectory();
            bool isPluginOutsideAssetsDir = !pluginParentDir.StartsWith("Assets");
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();
            admobImportMediationCompleted?.Invoke(AdSettings.AdmobSettings.editorImportingNetwork);
            AdSettings.AdmobSettings.editorImportingNetwork = null;
        }

        /// <summary>
        /// Moves the imported plugin files to the GoogleMobileAds directory if the publisher has moved the plugin to a different directory. This is a failsafe for when some plugin files are not imported to the new location.
        /// </summary>
        /// <returns>True if the adapters have been moved.</returns>
        public static bool MovePluginFilesIfNeeded(string pluginParentDirectory, bool isPluginOutsideAssetsDirectory)
        {
            var pluginDir = Path.Combine(pluginParentDirectory, "GoogleMobileAds");
            string defaultPluginExportPath = Path.Combine("Assets", "GoogleMobileAds");
            // Check if the user has moved the Plugin and if new assets have been imported to the default directory.
            if (defaultPluginExportPath.Equals(pluginDir) || !Directory.Exists(defaultPluginExportPath)) return false;

            MovePluginFiles(defaultPluginExportPath, pluginDir, isPluginOutsideAssetsDirectory);
            if (!isPluginOutsideAssetsDirectory)
            {
                FileUtil.DeleteFileOrDirectory(defaultPluginExportPath + ".meta");
            }

            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// A helper function to move all the files recursively from the default plugin dir to a custom location the publisher moved the plugin to.
        /// </summary>
        private static void MovePluginFiles(string fromDirectory, string pluginRoot, bool isPluginOutsideAssetsDirectory)
        {
            string defaultPluginExportPath = Path.Combine("Assets", "GoogleMobileAds");
            var files = Directory.GetFiles(fromDirectory);
            foreach (var file in files)
            {
                // We have to ignore some files, if the plugin is outside the Assets/ directory.
                if (isPluginOutsideAssetsDirectory &&
                    PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory.Any(pluginPathsToIgnore => file.Contains(pluginPathsToIgnore))) continue;

                // Check if the destination folder exists and create it if it doesn't exist
                var parentDirectory = Path.GetDirectoryName(file);
                var destinationDirectoryPath = parentDirectory.Replace(defaultPluginExportPath, pluginRoot);
                if (!Directory.Exists(destinationDirectoryPath))
                {
                    Directory.CreateDirectory(destinationDirectoryPath);
                }

                // If the meta file is of a folder asset and doesn't have labels (it is auto generated by Unity), just delete it.
                if (IsAutoGeneratedFolderMetaFile(file))
                {
                    FileUtil.DeleteFileOrDirectory(file);
                    continue;
                }

                var destinationPath = file.Replace(defaultPluginExportPath, pluginRoot);

                // Check if the file is already present at the destination path and delete it.
                if (File.Exists(destinationPath))
                {
                    FileUtil.DeleteFileOrDirectory(destinationPath);
                }

                FileUtil.MoveFileOrDirectory(file, destinationPath);
            }

            var directories = Directory.GetDirectories(fromDirectory);
            foreach (var directory in directories)
            {
                // We might have to ignore some directories, if the plugin is outside the Assets/ directory.
                if (isPluginOutsideAssetsDirectory &&
                    PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory.Any(pluginPathsToIgnore => directory.Contains(pluginPathsToIgnore))) continue;

                MovePluginFiles(directory, pluginRoot, isPluginOutsideAssetsDirectory);
            }

            if (!isPluginOutsideAssetsDirectory)
            {
                FileUtil.DeleteFileOrDirectory(fromDirectory);
            }
        }

        private static bool IsAutoGeneratedFolderMetaFile(string assetPath)
        {
            // Check if it is a meta file.
            if (!assetPath.EndsWith(".meta")) return false;

            var lines = File.ReadAllLines(assetPath);
            var isFolderAsset = false;
            var hasLabels = false;
            foreach (var line in lines)
            {
                if (line.Contains("folderAsset: yes"))
                {
                    isFolderAsset = true;
                }

                if (line.Contains("labels:"))
                {
                    hasLabels = true;
                }
            }

            // If it is a folder asset and doesn't have a label, the meta file is auto generated by 
            return isFolderAsset && !hasLabels;
        }

        #endregion
    }
}