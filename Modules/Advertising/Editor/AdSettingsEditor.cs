using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
#if PANCAKE_APPLOVIN
using AppLovinMax.Scripts.IntegrationManager.Editor;
#endif
#if PANCAKE_ADMOB
using GoogleMobileAds.Editor;
#endif
using Newtonsoft.Json;
using PancakeEditor;

#if PANCAKE_ADVERTISING
using Unity.SharpZipLib.Zip;
#endif

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

            public static readonly Property AutoLoadAdsMode = new Property(new GUIContent("Auto Ad-Loading Mode"));

            public static readonly Property ADCheckingInterval = new Property(new GUIContent("  Checking Interval", "Time (seconds) between 2 ad-availability checks"));

            public static readonly Property ADLoadingInterval = new Property(new GUIContent("  Loading Interval",
                "Minimum time (seconds) between two ad-loading requests, this is to restrict the number of requests sent to ad networks"));

            public static readonly Property PrivacyPolicyUrl = new Property(new GUIContent("  Privacy&Policy Url", "Privacy policy url"));

            public static readonly Property EnableGdpr = new Property(new GUIContent("GDPR",
                "General data protection regulation \nApp requires user consent before these events can be sent, you can delay app measurement until you explicitly initialize the Mobile Ads SDK or load an ad."));

            public static readonly Property EnableMultipleDex = new Property(new GUIContent("MultiDex"));
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
            public static readonly Property Enable = new Property(new GUIContent("Enable", "Enable using applovin ad"));
            public static readonly Property SDKKey = new Property(new GUIContent("Sdk Key", "Sdk of applovin"));
            public static readonly Property BannerAdUnit = new Property(new GUIContent("Banner Ad"));
            public static readonly Property InterstitialAdUnit = new Property(new GUIContent("Interstitial Ad"));
            public static readonly Property RewardedAdUnit = new Property(new GUIContent("Rewarded Ad"));
            public static readonly Property RewardedInterstitialAdUnit = new Property(new GUIContent("Rewarded Interstitial Ad"));
            public static readonly Property AppOpenAdUnit = new Property(new GUIContent("App Open Ad"));
            public static readonly Property EnableAgeRestrictedUser = new Property(new GUIContent("Age Restrictd User"));

            public static readonly Property EnableRequestAdAfterHidden = new Property(new GUIContent("Request Ad After Hidden",
                "Request to add new interstitial and rewarded ad after user finish view ad. Need kick-off request to cache ads as quickly as possible"));

            public static readonly Property EnableMaxAdReview = new Property(new GUIContent("Enable MAX Ad Review"));
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

            AdProperties.main = serializedObject.FindProperty("adCommonSettings");
            AdProperties.AutoLoadAdsMode.property = AdProperties.main.FindPropertyRelative("autoLoadingAd");
            AdProperties.ADCheckingInterval.property = AdProperties.main.FindPropertyRelative("adCheckingInterval");
            AdProperties.ADLoadingInterval.property = AdProperties.main.FindPropertyRelative("adLoadingInterval");
            AdProperties.EnableGdpr.property = AdProperties.main.FindPropertyRelative("enableGdpr");
            AdProperties.PrivacyPolicyUrl.property = AdProperties.main.FindPropertyRelative("privacyUrl");
            AdProperties.EnableMultipleDex.property = AdProperties.main.FindPropertyRelative("multiDex");
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
            ApplovinProperties.Enable.property = ApplovinProperties.main.FindPropertyRelative("enable");
            ApplovinProperties.SDKKey.property = ApplovinProperties.main.FindPropertyRelative("sdkKey");
            ApplovinProperties.BannerAdUnit.property = ApplovinProperties.main.FindPropertyRelative("bannerAdUnit");
            ApplovinProperties.InterstitialAdUnit.property = ApplovinProperties.main.FindPropertyRelative("interstitialAdUnit");
            ApplovinProperties.RewardedAdUnit.property = ApplovinProperties.main.FindPropertyRelative("rewardedAdUnit");
            ApplovinProperties.RewardedInterstitialAdUnit.property = ApplovinProperties.main.FindPropertyRelative("rewardedInterstitialAdUnit");
            ApplovinProperties.AppOpenAdUnit.property = ApplovinProperties.main.FindPropertyRelative("appOpenAdUnit");

            ApplovinProperties.EnableAgeRestrictedUser.property = ApplovinProperties.main.FindPropertyRelative("enableAgeRestrictedUser");
            ApplovinProperties.EnableRequestAdAfterHidden.property = ApplovinProperties.main.FindPropertyRelative("enableRequestAdAfterHidden");
            ApplovinProperties.EnableMaxAdReview.property = ApplovinProperties.main.FindPropertyRelative("enableMaxAdReview");

#if PANCAKE_ADMOB
            if (AdSettings.AdmobSettings.editorListNetwork.IsNullOrEmpty()) LoadAdmobMediation();
            else
            {
                foreach (var n in AdSettings.AdmobSettings.editorListNetwork) UpdateCurrentVersionAdmobMediation(n);
            }
#endif

#if PANCAKE_APPLOVIN
            if (AdSettings.MaxSettings.editorListNetwork.IsNullOrEmpty()) LoadApplovinMediation();
            else
            {
                string p = ParentApplovinDirectory();
                foreach (var n in AdSettings.MaxSettings.editorListNetwork) UpdateCurrentVersionApplovinMediation(n, p);
            }
#endif
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
                    EditorGUILayout.PropertyField(AdProperties.EnableGdpr.property, AdProperties.EnableGdpr.content);
                    if (AdSettings.AdCommonSettings.EnableGdpr)
                        EditorGUILayout.PropertyField(AdProperties.PrivacyPolicyUrl.property, AdProperties.PrivacyPolicyUrl.content);
                    EditorGUILayout.PropertyField(AdProperties.EnableMultipleDex.property, AdProperties.EnableMultipleDex.content);
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
            Uniform.DrawGroupFoldout("monetization_ads_applovin",
                "AppLovin",
                () =>
                {
                    EditorGUILayout.PropertyField(ApplovinProperties.Enable.property, ApplovinProperties.Enable.content);
                    if (AdSettings.MaxSettings.Enable)
                    {
                        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                        if (IsApplovinSdkImported())
                        {
                            if (!Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
                            {
                                Editor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                                AssetDatabase.Refresh();
                            }
                        }
                        else
                        {
                            if (Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
                            {
                                Editor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                                AssetDatabase.Refresh();
                            }

                            // show button install admob sdk
                            GUI.enabled = !EditorApplication.isCompiling;

                            if (GUILayout.Button("Install Applovin SDK", GUILayout.MaxHeight(40f)))
                            {
                                AssetDatabase.ImportPackage(Editor.AssetInPackagePath("Editor/UnityPackages", "applovin.unitypackage"), false);
                            }

                            GUI.enabled = true;
                        }

                        if (IsApplovinSdkImported() && Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
                        {
                            EditorGUILayout.HelpBox("Applovin plugin was imported", MessageType.Info);

                            EditorGUILayout.Space();
                            EditorGUILayout.PropertyField(ApplovinProperties.SDKKey.property, ApplovinProperties.SDKKey.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.EnableAgeRestrictedUser.property, ApplovinProperties.EnableAgeRestrictedUser.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.EnableRequestAdAfterHidden.property, ApplovinProperties.EnableRequestAdAfterHidden.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.EnableMaxAdReview.property, ApplovinProperties.EnableMaxAdReview.content);
#if PANCAKE_APPLOVIN
                            AppLovinSettings.Instance.QualityServiceEnabled = AdSettings.MaxSettings.EnableMaxAdReview;
                            AppLovinSettings.Instance.ConsentFlowEnabled = AdSettings.AdCommonSettings.EnableGdpr;
                            AppLovinSettings.Instance.ConsentFlowPrivacyPolicyUrl = AdSettings.AdCommonSettings.PrivacyUrl;
#endif
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(ApplovinProperties.BannerAdUnit.property, ApplovinProperties.BannerAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.InterstitialAdUnit.property, ApplovinProperties.InterstitialAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.RewardedAdUnit.property, ApplovinProperties.RewardedAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.RewardedInterstitialAdUnit.property, ApplovinProperties.RewardedInterstitialAdUnit.content);
                            EditorGUILayout.PropertyField(ApplovinProperties.AppOpenAdUnit.property, ApplovinProperties.AppOpenAdUnit.content);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();

                            Uniform.DrawGroupFoldout("monetization_ads_applovin_mediation",
                                "Mediation",
                                () =>
                                {
                                    DrawHeaderMediation();
                                    foreach (var network in AdSettings.MaxSettings.editorListNetwork)
                                    {
                                        DrawApplovinNetworkDetailRow(network);
                                    }
                                });
                            EditorGUILayout.Space();
                        }

#if PANCAKE_APPLOVIN
                        if (GUI.changed) AppLovinSettings.Instance.SaveAsync();
#endif
                    }
                    else
                    {
                        var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                        if (Editor.ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
                        {
                            Editor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                            AssetDatabase.Refresh();
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
#if PANCAKE_ADVERTISING
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
                }
            }
#endif
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


            var unityVersionComparison = AdUtility.CompareVersions(network.currentVersion.unity, network.lastVersion.unity);
            var androidVersionComparison = AdUtility.CompareVersions(network.currentVersion.android, network.lastVersion.android);
            var iosVersionComparison = AdUtility.CompareVersions(network.currentVersion.ios, network.lastVersion.ios);

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
                    EditorCoroutine.Start(AdmobDownloadMediation(network));
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

        public IEnumerator AdmobDownloadMediation(Network network)
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

            if (webRequest.result != UnityWebRequest.Result.Success)
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

            var unityVersionComparison = AdUtility.CompareVersions(network.currentVersion.unity, network.lastVersion.unity);
            var androidVersionComparison = AdUtility.CompareVersions(network.currentVersion.android, network.lastVersion.android);
            var iosVersionComparison = AdUtility.CompareVersions(network.currentVersion.ios, network.lastVersion.ios);

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
                if (!Directory.Exists(destinationDirectoryPath)) Directory.CreateDirectory(destinationDirectoryPath);

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

        #region applovin

        private readonly WaitForSeconds _wait = new WaitForSeconds(0.1f);

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
                EditorGUILayout.LabelField(new GUIContent(currentVersion.Equals("Not Installed") ? currentVersion : currentVersion.Split("_")[1]), VersionWidthOption);
                GUILayout.Space(3);
                EditorGUILayout.LabelField(new GUIContent(string.IsNullOrEmpty(latestVersion) ? latestVersion : latestVersion.Split("_")[1]), VersionWidthOption);
                GUILayout.FlexibleSpace();

                if (network.RequiresUpdate)
                {
                    GUILayout.Label(_warningIcon);
                }

                GUI.enabled = isActionEnabled && !EditorApplication.isCompiling;
                if (GUILayout.Button(new GUIContent(status), FieldWidth))
                {
                    // Download the plugin.
                    EditorCoroutine.Start(ApplovinDownloadMediation(network));
                }

                GUI.enabled = !EditorApplication.isCompiling;
                GUILayout.Space(2);

                GUI.enabled = isInstalled && !EditorApplication.isCompiling;
                if (GUILayout.Button(_iconUnintall, FieldWidth))
                {
                    EditorUtility.DisplayProgressBar("Ads", "Deleting " + network.DisplayName + "...", 0.5f);
                    string parentDir = ParentApplovinDirectory();
                    string pluginRoot = !parentDir.StartsWith("Assets") ? "Assets" : parentDir;
                    foreach (var pluginFilePath in network.PluginFilePaths)
                    {
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath));
                        FileUtil.DeleteFileOrDirectory(Path.Combine(pluginRoot, pluginFilePath + ".meta"));
                    }

                    Editor.RemoveAllEmptyFolder(new DirectoryInfo(pluginRoot));
                    UpdateCurrentVersionApplovinMediation(network, pluginRoot);

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
#if PANCAKE_APPLOVIN
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

        private static string GetPluginFileName(MaxNetwork network) { return network.Name.ToLowerInvariant() + "_" + network.LatestVersions.Unity + ".unitypackage"; }

        /// <summary>
        /// Downloads the plugin file for a given network.
        /// </summary>
        /// <param name="network">Network for which to download the current version.</param>
        /// <returns></returns>
        public IEnumerator ApplovinDownloadMediation(MaxNetwork network)
        {
            string path = Path.Combine(Application.temporaryCachePath, GetPluginFileName(network));
            var downloadHandler = new DownloadHandlerFile(path);
            webRequest = new UnityWebRequest(network.DownloadUrl) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                yield return _wait; // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
                downloadMediationProgressCallback?.Invoke(network.DisplayName, operation.progress, operation.isDone);
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                AdSettings.MaxSettings.editorImportingNetwork = network;
                AssetDatabase.ImportPackage(path, false);
            }

            webRequest = null;
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

        private static bool IsApplovinSdkImported() { return AssetDatabase.FindAssets("l:al_max_export_path-MaxSdk/Scripts/MaxSdk.cs").Length >= 1; }

        private string ParentApplovinDirectory()
        {
            string[] guids = AssetDatabase.FindAssets("l:al_max_export_path-MaxSdk/Scripts/MaxSdk.cs");
            if (!guids.IsNullOrEmpty())
            {
                return AssetDatabase.GUIDToAssetPath(guids[0])
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                    .Replace(@"MaxSdk\Scripts\MaxSdk.cs", "")
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return Path.Combine("Assets", "MaxSdk");
        }

        /// <summary>
        /// Updates the CurrentVersion fields for a given network data object.
        /// </summary>
        /// <param name="network">Network for which to update the current versions.</param>
        /// <param name="mediationPluginParentDirectory">The parent directory of where the mediation adapter plugins are imported to.</param>
        public static void UpdateCurrentVersionApplovinMediation(MaxNetwork network, string mediationPluginParentDirectory)
        {
#if PANCAKE_APPLOVIN
            var dependencyFilePath = Path.Combine(mediationPluginParentDirectory, network.DependenciesFilePath);
            MaxVersions currentVersions;
            currentVersions = ApplovinGetCurrentVersions(dependencyFilePath);


            network.CurrentVersions = currentVersions;

            // If AppLovin mediation plugin, get the version from MaxSdk and the latest and current version comparison.
            if (network.Name.Equals("APPLOVIN_NETWORK"))
            {
                network.CurrentVersions.Unity = MaxSdk.Version;

                var unityVersionComparison = (EVersionComparisonResult) (int) MaxSdkUtils.CompareVersions(network.CurrentVersions.Unity, network.LatestVersions.Unity);
                var androidVersionComparison =
                    (EVersionComparisonResult) (int) MaxSdkUtils.CompareVersions(network.CurrentVersions.Android, network.LatestVersions.Android);
                var iosVersionComparison = (EVersionComparisonResult) (int) MaxSdkUtils.CompareVersions(network.CurrentVersions.Ios, network.LatestVersions.Ios);

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
            // For all other mediation adapters, get the version comparison using their Unity versions.
            else
            {
                // If adapter is indeed installed, compare the current (installed) and the latest (from db) versions, so that we can determine if the publisher is on an older, current or a newer version of the adapter.
                // If the publisher is on a newer version of the adapter than the db version, that means they are on a beta version.
                if (!string.IsNullOrEmpty(currentVersions.Unity))
                {
                    network.CurrentToLatestVersionComparisonResult =
                        (EVersionComparisonResult) (int) MaxSdkUtils.CompareUnityMediationVersions(currentVersions.Unity, network.LatestVersions.Unity);
                }

                if (!string.IsNullOrEmpty(network.CurrentVersions.Unity) && AppLovinAutoUpdater.MinAdapterVersions.ContainsKey(network.Name))
                {
                    var comparisonResult = MaxSdkUtils.CompareUnityMediationVersions(network.CurrentVersions.Unity, AppLovinAutoUpdater.MinAdapterVersions[network.Name]);
                    // Requires update if current version is lower than the min required version.
                    network.RequiresUpdate = comparisonResult < 0;
                }
                else
                {
                    // Reset value so that the Integration manager can hide the alert icon once adapter is updated.
                    network.RequiresUpdate = false;
                }
            }
#endif
        }

        /// <summary>
        /// Gets the current versions for a given network's dependency file path.
        /// </summary>
        /// <param name="dependencyPath">A dependency file path that from which to extract current versions.</param>
        /// <returns>Current versions of a given network's dependency file.</returns>
        public static MaxVersions ApplovinGetCurrentVersions(string dependencyPath)
        {
            XDocument dependency;
            try
            {
                dependency = XDocument.Load(dependencyPath);
            }
#pragma warning disable 0168
            catch (IOException exception)
#pragma warning restore 0168
            {
                // Couldn't find the dependencies file. The plugin is not installed.
                return new MaxVersions();
            }

            // <dependencies>
            //  <androidPackages>
            //      <androidPackage spec="com.applovin.mediation:network_name-adapter:1.2.3.4" />
            //  </androidPackages>
            //  <iosPods>
            //      <iosPod name="AppLovinMediationNetworkNameAdapter" version="2.3.4.5" />
            //  </iosPods>
            // </dependencies>
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
                            element.FirstAttribute.Value.StartsWith("com.applovin"));
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
                            element.FirstAttribute.Value.StartsWith("AppLovin"));
                    if (adapterPod != null)
                    {
                        iosVersion = adapterPod.Attributes().First(attribute => attribute.Name.LocalName.Equals("version")).Value;
                    }
                }
            }

            var currentVersions = new MaxVersions();
            if (androidVersion != null && iosVersion != null)
            {
                currentVersions.Unity = string.Format("android_{0}_ios_{1}", androidVersion, iosVersion);
                currentVersions.Android = androidVersion;
                currentVersions.Ios = iosVersion;
            }
            else if (androidVersion != null)
            {
                currentVersions.Unity = string.Format("android_{0}", androidVersion);
                currentVersions.Android = androidVersion;
            }
            else if (iosVersion != null)
            {
                currentVersions.Unity = string.Format("ios_{0}", iosVersion);
                currentVersions.Ios = iosVersion;
            }

            return currentVersions;
        }

#if PANCAKE_APPLOVIN
        public void LoadApplovinMediation()
        {
            EditorCoroutine.Start(LoadApplovinMediation(_ =>
            {
                AdSettings.MaxSettings.editorListNetwork = _.MediatedNetworks;
                foreach (var mediationNetwork in AdSettings.MaxSettings.editorListNetwork.ToList())
                {
                    if (!mediationNetwork.Name.Equals("ADCOLONY_NETWORK") && !mediationNetwork.Name.Equals("CHARTBOOST_NETWORK") &&
                        !mediationNetwork.Name.Equals("FACEBOOK_MEDIATE") && !mediationNetwork.Name.Equals("ADMOB_NETWORK") &&
                        !mediationNetwork.Name.Equals("INMOBI_NETWORK") && !mediationNetwork.Name.Equals("IRONSOURCE_NETWORK") &&
                        !mediationNetwork.Name.Equals("MINTEGRAL_NETWORK") && !mediationNetwork.Name.Equals("TIKTOK_NETWORK") &&
                        !mediationNetwork.Name.Equals("UNITY_NETWORK") && !mediationNetwork.Name.Equals("VUNGLE_NETWORK"))
                    {
                        AdSettings.MaxSettings.editorListNetwork.Remove(mediationNetwork);
                    }
                }
            }));
        }

        /// <summary>
        /// Loads the plugin data to be display by integration manager window.
        /// </summary>
        /// <param name="callback">Callback to be called once the plugin data download completes.</param>
        private IEnumerator LoadApplovinMediation(Action<MaxPluginData> callback)
        {
            var url = string.Format("https://dash.applovin.com/docs/v1/unity_integration_manager?plugin_version={0}", GetPluginVersionForUrl());
            var www = UnityWebRequest.Get(url);
            var operation = www.SendWebRequest();

            while (!operation.isDone) yield return new WaitForSeconds(0.1f); // Just wait till www is done. Our coroutine is pretty rudimentary.

            if (www.result != UnityWebRequest.Result.Success)
            {
                callback(null);
            }
            else
            {
                MaxPluginData pluginData;
                try
                {
                    pluginData = JsonUtility.FromJson<MaxPluginData>(www.downloadHandler.text);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    pluginData = null;
                }

                if (pluginData != null)
                {
                    // Get current version of the plugin
                    var appLovinMax = pluginData.AppLovinMax;
                    string parentDir = ParentApplovinDirectory();
                    UpdateCurrentVersionApplovinMediation(appLovinMax, parentDir);

                    // Get current versions for all the mediation networks.
                    string mediationPluginParentDirectory = !parentDir.StartsWith("Assets") ? "Assets" : parentDir;
                    foreach (var network in pluginData.MediatedNetworks)
                    {
                        UpdateCurrentVersionApplovinMediation(network, mediationPluginParentDirectory);
                    }
                }

                callback(pluginData);
            }
        }
#endif

#if PANCAKE_APPLOVIN
        /// <summary>
        /// Returns a URL friendly version string by replacing periods with underscores.
        /// </summary>
        private static string GetPluginVersionForUrl()
        {
            var version = MaxSdk.Version;
            var versionsSplit = version.Split('.');
            return string.Join("_", versionsSplit);
        }
#endif

        #endregion
    }
}