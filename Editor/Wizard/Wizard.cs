using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
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
        private static bool enablePlayfab;
        private static bool enableAnalytic;
        private static bool enableRemoteConfig;
        private static bool enableCrashlytic;
        private static bool enableCloudMessage;
        private static bool enableInAppReview;
        private static bool enable = true;
        private static Dictionary<string, List<string>> repoTags;
        private static Dictionary<string, int> selectedTags;
        private static EdmModel edmModel;
        private const string URL = "https://api.github.com/repos/{0}/{1}/tags";
        private const string FIREBASE_ANALYTIC_PACKAGE = "com.google.firebase.analytics";
        private const string FIREBASE_REMOTE_CONFIG_PACKAGE = "com.google.firebase.remote-config";
        private const string FIREBASE_CRASHLYTIC_PACKAGE = "com.google.firebase.crashlytics";
        private const string FIREBASE_MESSAGE_PACKAGE = "com.google.firebase.messaging";
        private const string IN_APP_REVIEW_PACKAGE = "com.google.play.review";
        private const int TIME_CACHE_REQUEST = 900;

        internal static bool Status
        {
            get => EditorPrefs.GetBool($"wizard_{PlayerSettings.productGUID}", false);
            private set => EditorPrefs.SetBool($"wizard_{PlayerSettings.productGUID}", value);
        }

        public static void Open()
        {
            enable = true;
            var window = EditorWindow.GetWindow<Wizard>(true, "Wizard", true);
            window.minSize = window.maxSize = new Vector2(400, 440);
            window.ShowUtility();

            selectedTags = new Dictionary<string, int>()
            {
                {FIREBASE_ANALYTIC_PACKAGE, 0},
                {FIREBASE_REMOTE_CONFIG_PACKAGE, 0},
                {FIREBASE_CRASHLYTIC_PACKAGE, 0},
                {FIREBASE_MESSAGE_PACKAGE, 0},
                {IN_APP_REVIEW_PACKAGE, 0}
            };
            string lastTimeFetch = EditorPrefs.GetString(Application.identifier + "_lastime_fetch", "");
            var forceFetchData = false;
            if (string.IsNullOrEmpty(lastTimeFetch) || string.IsNullOrEmpty(EditorPrefs.GetString(Application.identifier + "_cache_repotag")) ||
                string.IsNullOrEmpty(EditorPrefs.GetString(Application.identifier + "_cache_edmmodel")))
            {
                forceFetchData = true;
            }
            else
            {
                var dateTime = DateTime.Parse(lastTimeFetch);
                if ((DateTime.Now - dateTime).TotalSeconds >= TIME_CACHE_REQUEST) forceFetchData = true;
            }

            if (forceFetchData)
            {
                EditorPrefs.SetString(Application.identifier + "_lastime_fetch", DateTime.Now.ToString());
                repoTags = new Dictionary<string, List<string>>
                {
                    {FIREBASE_ANALYTIC_PACKAGE, Fetch("firebase-unity", "firebase-analytics")},
                    {FIREBASE_REMOTE_CONFIG_PACKAGE, Fetch("firebase-unity", "firebase-remote-config")},
                    {FIREBASE_CRASHLYTIC_PACKAGE, Fetch("firebase-unity", "firebase-crashlytics")},
                    {FIREBASE_MESSAGE_PACKAGE, Fetch("firebase-unity", "firebase-messaging")},
                    {IN_APP_REVIEW_PACKAGE, Fetch("google-unity", "in-app-review")}
                };

                edmModel = LoadEdmVersion();

                EditorPrefs.SetString(Application.identifier + "_cache_repotag", JsonConvert.SerializeObject(repoTags));
                EditorPrefs.SetString(Application.identifier + "_cache_edmmodel", JsonConvert.SerializeObject(edmModel));
            }
            else
            {
                repoTags = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(EditorPrefs.GetString(Application.identifier + "_cache_repotag"));
                edmModel = JsonConvert.DeserializeObject<EdmModel>(EditorPrefs.GetString(Application.identifier + "_cache_edmmodel"));
            }

            repoTags[IN_APP_REVIEW_PACKAGE].Remove("1.8.1");
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
            enablePlayfab = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_PLAYFAB", group);
            enableAddressable = RegistryManager.IsInstalled("com.unity.addressables").Item1;
            enableUsingAddressableForPopup = InEditor.ScriptingDefinition.IsSymbolDefined("PANCAKE_ADDRESSABLE_POPUP", group);
            var analyticPackage = RegistryManager.IsInstalled(FIREBASE_ANALYTIC_PACKAGE);
            var remoteConfigPackage = RegistryManager.IsInstalled(FIREBASE_REMOTE_CONFIG_PACKAGE);
            var crashlyticPackage = RegistryManager.IsInstalled(FIREBASE_CRASHLYTIC_PACKAGE);
            var cloudMessagePackage = RegistryManager.IsInstalled(FIREBASE_MESSAGE_PACKAGE);
            var inappreviewPackage = RegistryManager.IsInstalled(IN_APP_REVIEW_PACKAGE);

            enableAnalytic = analyticPackage.Item1;
            enableRemoteConfig = remoteConfigPackage.Item1;
            enableCrashlytic = crashlyticPackage.Item1;
            enableCloudMessage = cloudMessagePackage.Item1;
            enableInAppReview = inappreviewPackage.Item1;

            if (enableAnalytic) FindVersion(analyticPackage, FIREBASE_ANALYTIC_PACKAGE);
            if (enableRemoteConfig) FindVersion(remoteConfigPackage, FIREBASE_REMOTE_CONFIG_PACKAGE);
            if (enableCrashlytic) FindVersion(crashlyticPackage, FIREBASE_CRASHLYTIC_PACKAGE);
            if (enableCloudMessage) FindVersion(cloudMessagePackage, FIREBASE_MESSAGE_PACKAGE);
            if (enableInAppReview) FindVersion(inappreviewPackage, IN_APP_REVIEW_PACKAGE);
        }

        private static EdmModel LoadEdmVersion()
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string raw = client.DownloadString("https://gist.githubusercontent.com/yenmoc/2334e5a93dddc849a557a18882ea406a/raw");
            return JsonConvert.DeserializeObject<EdmModel>(raw);
        }

        private static void FindVersion((bool, string) packs, string key)
        {
            var version = packs.Item2;
            selectedTags[key] = 0;
            if (version.StartsWith("https://"))
            {
                string[] strs = version.Split("#");
                if (strs.Length > 1) version = strs[1];
            }

            if (!string.IsNullOrEmpty(version)) selectedTags[key] = repoTags[key].FindIndex(_ => _ == version);
        }

        private static List<string> Fetch(string userName, string repoName)
        {
            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string result = client.DownloadString(string.Format(URL, userName, repoName));
            var raw = JsonConvert.DeserializeObject<List<GithubTag>>(result);
            return raw.Map(_ => _.name);
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
            Uniform.Toggle(ref enableAtom, "Atom (Scriptable Object Architecture)", 240, highlight: true);
            Uniform.Toggle(ref enableAddressable, "Addressable", 240, highlight: true);
            if (enableAddressable)
            {
                EditorGUI.indentLevel++;
                Uniform.Toggle(ref enableUsingAddressableForPopup, "For Popup", 225, highlight: true);
                EditorGUI.indentLevel--;
            }

            Uniform.Toggle(ref enablePlayfab, "Playfab", 240, highlight: true);
            Uniform.Toggle(ref enableGam, "Game Base Flow", 240, highlight: true);

            Uniform.SpaceThreeLine();
            Uniform.SpaceThreeLine();
            Uniform.Toggle(ref enableAds, "Advertising", 240, highlight: true);
            Uniform.Toggle(ref enableIap, "In-App-Purchase", 240, highlight: true);
            Uniform.Toggle(ref enableNotification, "Local Notification", 240, highlight: true);

            Uniform.SpaceThreeLine();
            Uniform.SpaceThreeLine();
            Uniform.Toggle(ref enableAnalytic,
                "Firebase Analytic",
                240,
                () =>
                {
                    if (!enableAnalytic) return;
                    if (repoTags.IsNullOrEmpty()) return;
                    int index = selectedTags[FIREBASE_ANALYTIC_PACKAGE];
                    Uniform.Dropdown(ref index, repoTags[FIREBASE_ANALYTIC_PACKAGE].ToArray(), GUILayout.Width(90));
                    selectedTags[FIREBASE_ANALYTIC_PACKAGE] = index;
                },
                true);

            Uniform.Toggle(ref enableRemoteConfig,
                "Firebase Remote Config",
                240,
                () =>
                {
                    if (!enableRemoteConfig) return;
                    if (repoTags.IsNullOrEmpty()) return;
                    int index = selectedTags[FIREBASE_REMOTE_CONFIG_PACKAGE];
                    Uniform.Dropdown(ref index, repoTags[FIREBASE_REMOTE_CONFIG_PACKAGE].ToArray(), GUILayout.Width(90));
                    selectedTags[FIREBASE_REMOTE_CONFIG_PACKAGE] = index;
                },
                true);
            Uniform.Toggle(ref enableCrashlytic,
                "Firebase Crashlytic",
                240,
                () =>
                {
                    if (!enableCrashlytic) return;
                    if (repoTags.IsNullOrEmpty()) return;
                    int index = selectedTags[FIREBASE_CRASHLYTIC_PACKAGE];
                    Uniform.Dropdown(ref index, repoTags[FIREBASE_CRASHLYTIC_PACKAGE].ToArray(), GUILayout.Width(90));
                    selectedTags[FIREBASE_CRASHLYTIC_PACKAGE] = index;
                },
                true);
            Uniform.Toggle(ref enableCloudMessage,
                "Firebase Cloud Message",
                240,
                () =>
                {
                    if (!enableCloudMessage) return;
                    if (repoTags.IsNullOrEmpty()) return;
                    int index = selectedTags[FIREBASE_MESSAGE_PACKAGE];
                    Uniform.Dropdown(ref index, repoTags[FIREBASE_MESSAGE_PACKAGE].ToArray(), GUILayout.Width(90));
                    selectedTags[FIREBASE_MESSAGE_PACKAGE] = index;
                },
                true);

            Uniform.SpaceThreeLine();
            Uniform.SpaceThreeLine();
            Uniform.Toggle(ref enableInAppReview,
                "In-app Review",
                240,
                () =>
                {
                    if (!enableInAppReview) return;
                    if (repoTags.IsNullOrEmpty()) return;
                    int index = selectedTags[IN_APP_REVIEW_PACKAGE];
                    Uniform.Dropdown(ref index, repoTags[IN_APP_REVIEW_PACKAGE].ToArray(), GUILayout.Width(90));
                    selectedTags[IN_APP_REVIEW_PACKAGE] = index;
                },
                true);


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

                            if (enablePlayfab)
                            {
                                RegistryManager.Add("com.pancake.playfab", "https://github.com/pancake-llc/playfab.git#2.157.230123");
                                InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_PLAYFAB");
                            }
                            else
                            {
                                RegistryManager.Remove("com.pancake.playfab");
                                InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_PLAYFAB");
                            }

                            if (enableAnalytic) InstallFirebasePackage(FIREBASE_ANALYTIC_PACKAGE, "firebase-analytics");
                            else RegistryManager.Remove(FIREBASE_ANALYTIC_PACKAGE);

                            if (enableRemoteConfig) InstallFirebasePackage(FIREBASE_REMOTE_CONFIG_PACKAGE, "firebase-remote-config");
                            else RegistryManager.Remove(FIREBASE_REMOTE_CONFIG_PACKAGE);

                            if (enableCrashlytic) InstallFirebasePackage(FIREBASE_CRASHLYTIC_PACKAGE, "firebase-crashlytics");
                            else RegistryManager.Remove(FIREBASE_CRASHLYTIC_PACKAGE);

                            if (enableCloudMessage) InstallFirebasePackage(FIREBASE_MESSAGE_PACKAGE, "firebase-messaging");
                            else RegistryManager.Remove(FIREBASE_MESSAGE_PACKAGE);

                            if (enableInAppReview)
                            {
                                string version = repoTags[IN_APP_REVIEW_PACKAGE][selectedTags[IN_APP_REVIEW_PACKAGE]];
                                string versionEdm = edmModel.inappreview.edm[Array.FindIndex(edmModel.inappreview.source, _ => _ == version)];
                                RegistryManager.Add(IN_APP_REVIEW_PACKAGE, $"https://github.com/google-unity/in-app-review.git#{version}");
                                RegistryManager.Add("com.google.play.core", $"https://github.com/google-unity/google-play-core.git#{version}");
                                RegistryManager.Add("com.google.play.common", $"https://github.com/google-unity/google-play-common.git#{version}");
                                RegistryManager.Add("com.google.android.appbundle", $"https://github.com/google-unity/android-app-bundle.git#{version}");
                                RegistryManager.Add("com.google.external-dependency-manager",
                                    $"https://github.com/google-unity/external-dependency-manager.git#{versionEdm}");
                            }
                            else
                            {
                                RegistryManager.Remove("com.google.play.review");
                                RegistryManager.Remove("com.google.play.core");
                                RegistryManager.Remove("com.google.play.common");
                                RegistryManager.Remove("com.google.android.appbundle");
                            }


                            if (!enableAnalytic && !enableRemoteConfig && !enableCrashlytic && !enableCloudMessage)
                            {
                                RegistryManager.Remove("com.google.firebase.app");
                                if (!enableInAppReview && !enableAds) RegistryManager.Remove("com.google.external-dependency-manager");
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

        private static void InstallFirebasePackage(string key, string repoName)
        {
            string version = repoTags[key][selectedTags[key]];
            string versionEdm = edmModel.firebase.edm[Array.FindIndex(edmModel.firebase.source, _ => _ == version)];
            RegistryManager.Add(key, $"https://github.com/firebase-unity/{repoName}.git#{version}");
            RegistryManager.Add("com.google.firebase.app", $"https://github.com/firebase-unity/firebase-app.git#{version}");
            RegistryManager.Remove("com.google.external-dependency-manager"); // alway use edm dependency from firebase
            RegistryManager.Add("com.google.external-dependency-manager", $"https://github.com/google-unity/external-dependency-manager.git#{versionEdm}");
        }
    }

    [Serializable]
    internal class GithubTag
    {
        public string name;
    }

    [Serializable]
    internal class EdmVersion
    {
        public string[] source;
        public string[] edm;
    }

    [Serializable]
    internal class EdmModel
    {
        public EdmVersion firebase;
        public EdmVersion inappreview;
    }
}