using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json;
using Pancake.Editor;
#if PANCAKE_MAX_ENABLE
using AppLovinMax.Scripts.IntegrationManager.Editor;
#endif
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Pancake.Monetization.Editor
{
    /// <summary>
    /// A manager class for MAX integration manager window.
    /// </summary>
    public class MaxManager
    {
        /// <summary>
        /// Delegate to be called when downloading a plugin with the progress percentage. 
        /// </summary>
        /// <param name="pluginName">The name of the plugin being downloaded.</param>
        /// <param name="progress">Percentage downloaded.</param>
        /// <param name="done">Whether or not the download is complete.</param>
        public delegate void DownloadPluginProgressCallback(string pluginName, float progress, bool done, int index);

        /// <summary>
        /// Delegate to be called when a plugin package is imported.
        /// </summary>
        /// <param name="network">The network data for which the package is imported.</param>
        public delegate void ImportPackageCompletedCallback(MaxNetwork network);

        private static readonly MaxManager instance = new MaxManager();

        public static readonly string GradleTemplatePath = Path.Combine("Assets/Plugins/Android", "mainTemplate.gradle");
        public static readonly string DefaultPluginExportPath = Path.Combine("Assets", "MaxSdk");
        private const string DefaultMaxSdkAssetExportPath = "MaxSdk/Scripts/MaxSdk.cs";
        private static readonly string MaxSdkAssetExportPath = Path.Combine("MaxSdk", "Scripts/MaxSdk.cs");

        /// <summary>
        /// Some publishers might re-export our plugin via Unity Package Manager and the plugin will not be under the Assets folder. This means that the mediation adapters, settings files should not be moved to the packages folder,
        /// since they get overridden when the package is updated. These are the files that should not be moved, if the plugin is not under the Assets/ folder.
        /// 
        /// Note: When we distribute the plugin via Unity Package Manager, we need to distribute the adapters as separate packages, and the adapters won't be in the MaxSdk folder. So we need to take that into account.
        /// </summary>
        private static readonly List<string> PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory = new List<string>
        {
            "MaxSdk/Mediation",
            "MaxSdk/Mediation.meta",
            "MaxSdk/Resources.meta",
#if PANCAKE_MAX_ENABLE
            AppLovinSettings.SettingsExportPath,
            AppLovinSettings.SettingsExportPath + ".meta"
#endif
        };

        private static string externalDependencyManagerVersion;

        public static DownloadPluginProgressCallback downloadPluginProgressCallback;
        public static ImportPackageCompletedCallback importPackageCompletedCallback;
        public UnityWebRequest webRequest;
        public UnityWebRequest[] branWidthRequest;
        private readonly WaitForSeconds _wait = new WaitForSeconds(0.1f);

        /// <summary>
        /// An Instance of the Integration manager.
        /// </summary>
        public static MaxManager Instance { get { return instance; } }

        /// <summary>
        /// The parent directory path where the MaxSdk plugin directory is placed.
        /// </summary>
        public static string PluginParentDirectory
        {
            get
            {
#if PANCAKE_MAX_ENABLE
                // Search for the asset with the default exported path first, In most cases, we should be able to find the asset.
                // In some cases where we don't, use the platform specific export path to search for the asset (in case of migrating a project from Windows to Mac or vice versa).
                var maxSdkScriptAssetPath = MaxSdkUtils.GetAssetPathForExportPath(DefaultMaxSdkAssetExportPath);
                if (File.Exists(maxSdkScriptAssetPath))
                {
                    // maxSdkScriptAssetPath will always have AltDirectorySeparatorChar (/) as the path separator. Convert to platform specific path.
                    return maxSdkScriptAssetPath.Replace(DefaultMaxSdkAssetExportPath, "").Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                // We should never reach this line but leaving this in out of paranoia.
                return MaxSdkUtils.GetAssetPathForExportPath(MaxSdkAssetExportPath)
                    .Replace(MaxSdkAssetExportPath, "")
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
#else
                return "Assets";
#endif
            }
        }

        /// <summary>
        /// When the base plugin is outside the <c>Assets/</c> directory, the mediation plugin files are still imported to the default location under <c>Assets/</c>.
        /// Returns the parent directory where the mediation adapter plugins are imported.
        /// </summary>
        public static string MediationSpecificPluginParentDirectory { get { return IsPluginOutsideAssetsDirectory ? "Assets" : PluginParentDirectory; } }

        /// <summary>
        /// Whether or not the plugin is under the Assets/ folder.
        /// </summary>
        public static bool IsPluginOutsideAssetsDirectory { get { return !PluginParentDirectory.StartsWith("Assets"); } }

        /// <summary>
        /// Whether or not gradle build system is enabled.
        /// </summary>
        public static bool GradleBuildEnabled { get { return GetEditorUserBuildSetting("androidBuildSystem", "").ToString().Equals("Gradle"); } }

        /// <summary>
        /// Whether or not Gradle template is enabled.
        /// </summary>
        public static bool GradleTemplateEnabled { get { return GradleBuildEnabled && File.Exists(GradleTemplatePath); } }

        /// <summary>
        /// Whether or not the Quality Service settings can be processed which requires Gradle template enabled or Unity IDE newer than version 2018_2.
        /// </summary>
        public static bool CanProcessAndroidQualityServiceSettings { get { return GradleTemplateEnabled || (GradleBuildEnabled && IsUnity2018_2OrNewer()); } }

        /// <summary>
        /// The External Dependency Manager version obtained dynamically.
        /// </summary>
        public static string ExternalDependencyManagerVersion
        {
            get
            {
                if (!string.IsNullOrEmpty(externalDependencyManagerVersion)) return externalDependencyManagerVersion;

                try
                {
                    var versionHandlerVersionNumberType = Type.GetType("Google.VersionHandlerVersionNumber, Google.VersionHandlerImpl");
                    externalDependencyManagerVersion = versionHandlerVersionNumberType.GetProperty("Value").GetValue(null, null).ToString();
                }
#pragma warning disable 0168
                catch (Exception ignored)
#pragma warning restore 0168
                {
                    externalDependencyManagerVersion = "Failed to get version.";
                }

                return externalDependencyManagerVersion;
            }
        }

        private MaxManager()
        {
            // Add asset import callbacks.
            AssetDatabase.importPackageCompleted += OnAssetDatabaseOnimportPackageCompleted;
            AssetDatabase.importPackageCancelled += OnAssetDatabaseOnimportPackageCancelled;
            AssetDatabase.importPackageFailed += OnAssetDatabaseOnimportPackageFailed;
            
            AssetDatabase.importPackageCompleted += OnAssetDatabaseOnimportAllPackageCompleted;
            AssetDatabase.importPackageCancelled += OnAssetDatabaseOnimportAllPackageCancelled;
            AssetDatabase.importPackageFailed += OnAssetDatabaseOnimportAllPackageFailed;
        }

        private void OnAssetDatabaseOnimportAllPackageCancelled(string packageName)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;
            
            Settings.MaxSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnAssetDatabaseOnimportAllPackageFailed(string packageName, string errormessage)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;
            
            Settings.MaxSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnAssetDatabaseOnimportAllPackageCompleted(string packageName)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;
            string pluginParentDir = PluginParentDirectory;
            bool isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();
            
            CallImportPackageCompletedCallback(Settings.MaxSettings.editorImportingListNetwork[result.Item2]);
            Settings.MaxSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnAssetDatabaseOnimportPackageFailed(string packageName, string errorMessage)
        {
            if (!IsImportingNetwork(packageName)) return;

            Settings.MaxSettings.editorImportingNetwork = null;
        }

        private void OnAssetDatabaseOnimportPackageCancelled(string packageName)
        {
            if (!IsImportingNetwork(packageName)) return;

            Settings.MaxSettings.editorImportingNetwork = null;
        }

        private void OnAssetDatabaseOnimportPackageCompleted(string packageName)
        {
            if (!IsImportingNetwork(packageName)) return;
            string pluginParentDir = PluginParentDirectory;
            bool isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();

            CallImportPackageCompletedCallback(Settings.MaxSettings.editorImportingNetwork);
            Settings.MaxSettings.editorImportingNetwork = null;
        }

        static MaxManager() { }

#if PANCAKE_MAX_ENABLE
        /// <summary>
        /// Loads the plugin data to be display by integration manager window.
        /// </summary>
        /// <param name="callback">Callback to be called once the plugin data download completes.</param>
        public IEnumerator LoadPluginData(Action<MaxPluginData> callback)
        {
            var url = string.Format("https://dash.applovin.com/docs/v1/unity_integration_manager?plugin_version={0}", GetPluginVersionForUrl());
            var www = UnityWebRequest.Get(url);

#if UNITY_2017_2_OR_NEWER
            var operation = www.SendWebRequest();
#else
            var operation = www.Send();
#endif

            while (!operation.isDone) yield return new WaitForSeconds(0.1f); // Just wait till www is done. Our coroutine is pretty rudimentary.

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (www.isNetworkError || www.isHttpError)
#else
            if (www.isError)
#endif
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
                    UpdateCurrentVersions(appLovinMax, PluginParentDirectory);

                    // Get current versions for all the mediation networks.
                    var mediationPluginParentDirectory = MediationSpecificPluginParentDirectory;
                    foreach (var network in pluginData.MediatedNetworks)
                    {
                        UpdateCurrentVersions(network, mediationPluginParentDirectory);
                    }
                }

                callback(pluginData);
            }
        }


        public void LoadPluginDataExtend(Action<List<MaxNetwork>> callback)
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string json = curl.DownloadString("https://gist.githubusercontent.com/yenmoc/a7507066fe5fd1113fce84f06fabe564/raw");
            var convert = JsonConvert.DeserializeObject<List<MaxNetwork>>(json);

            if (convert != null)
            {
                var mediationPluginParentDirectory = MediationSpecificPluginParentDirectory;
                foreach (var network in convert)
                {
                    UpdateCurrentVersions(network, mediationPluginParentDirectory);
                }
            }

            callback(convert);
        }
#endif

        /// <summary>
        /// Updates the CurrentVersion fields for a given network data object.
        /// </summary>
        /// <param name="network">Network for which to update the current versions.</param>
        /// <param name="mediationPluginParentDirectory">The parent directory of where the mediation adapter plugins are imported to.</param>
        public static void UpdateCurrentVersions(MaxNetwork network, string mediationPluginParentDirectory)
        {
#if PANCAKE_MAX_ENABLE
            var dependencyFilePath = Path.Combine(mediationPluginParentDirectory, network.DependenciesFilePath);
            MaxVersions currentVersions;
            if (network.Name.Equals("ALGORIX_NETWORK") || network.Name.Equals("ROULAX_NETWORK"))
            {
                currentVersions = !File.Exists(dependencyFilePath) ? new MaxVersions() : network.LatestVersions;
            }
            else
            {
                currentVersions = GetCurrentVersions(dependencyFilePath);
            }
            

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
        /// Downloads the plugin file for a given network.
        /// </summary>
        /// <param name="network">Network for which to download the current version.</param>
        /// <returns></returns>
        public IEnumerator DownloadPlugin(MaxNetwork network)
        {
            Settings.MaxSettings.editorInstallAllFlag = false;
            string path = Path.Combine(Application.temporaryCachePath, GetPluginFileName(network)); // TODO: Maybe delete plugin file after finishing import.

#if UNITY_2017_2_OR_NEWER
            var downloadHandler = new DownloadHandlerFile(path);
#else
            var downloadHandler = new AppLovinDownloadHandler(path);
#endif
            webRequest = new UnityWebRequest(network.DownloadUrl) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};

#if UNITY_2017_2_OR_NEWER
            var operation = webRequest.SendWebRequest();
#else
            var operation = _webRequest.Send();
#endif

            while (!operation.isDone)
            {
                yield return _wait; // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
                downloadPluginProgressCallback?.Invoke(network.DisplayName, operation.progress, operation.isDone, -1);
            }

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
#else
            if (_webRequest.isError)
#endif
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                Settings.MaxSettings.editorImportingNetwork = network;
                AssetDatabase.ImportPackage(path, true);
            }

            webRequest = null;
        }

        /// <summary>
        /// Downloads the plugin file for a given network.
        /// </summary>
        /// <param name="network">Network for which to download the current version.</param>
        /// <param name="index"></param>
        /// <param name="interactive"></param>
        /// <returns></returns>
        private IEnumerator DownloadPlugin(MaxNetwork network, int index, bool interactive = true)
        {
            string path = Path.Combine(Application.temporaryCachePath, GetPluginFileName(network)); // TODO: Maybe delete plugin file after finishing import.

#if UNITY_2017_2_OR_NEWER
            var downloadHandler = new DownloadHandlerFile(path);
#else
            var downloadHandler = new AppLovinDownloadHandler(path);
#endif
            branWidthRequest[index] = new UnityWebRequest(network.DownloadUrl) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};

#if UNITY_2017_2_OR_NEWER
            var operation = branWidthRequest[index].SendWebRequest();
#else
            var operation = _branWidthRequest[index].Send();
#endif

            while (!operation.isDone)
            {
                yield return _wait; // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
                downloadPluginProgressCallback?.Invoke(network.DisplayName, operation.progress, operation.isDone, index);
            }
            
#if UNITY_2020_1_OR_NEWER
            if (branWidthRequest[index].result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (_branWidthRequest[index].isNetworkError || _branWidthRequest[index].isHttpError)
#else
            if (_branWidthRequest[index].isError)
#endif
            {
                Debug.LogError(branWidthRequest[index].error);
            }
            else
            {
                Settings.MaxSettings.editorImportingListNetwork.Add(network);
                AssetDatabase.ImportPackage(path, interactive);
            }

            branWidthRequest[index] = null;
        }

        /// <summary>
        /// Download all mediation network
        /// </summary>
        /// <param name="networks"></param>
        public void DownloadAllPlugin(List<MaxNetwork> networks)
        {
            branWidthRequest = new UnityWebRequest[networks.Count];
            Settings.MaxSettings.editorImportingListNetwork.Clear();
            Settings.MaxSettings.editorImportingNetwork = null;
            Settings.MaxSettings.editorInstallAllFlag = true;

            for (var i = 0; i < networks.Count; i++)
            {
                EditorCoroutine.Start(DownloadPlugin(networks[i], i, false));
            }
        }

        /// <summary>
        /// Shows a dialog to the user with the given message and logs the error message to console.
        /// </summary>
        /// <param name="message">The failure message to be shown to the user.</param>
        public static void ShowBuildFailureDialog(string message)
        {
#if PANCAKE_MAX_ENABLE
            var openIntegrationManager = EditorUtility.DisplayDialog("AppLovin MAX", message, "Open Integration Manager", "Dismiss");
            if (openIntegrationManager)
            {
                AppLovinIntegrationManagerWindow.ShowManager();
            }

            Debug.LogError(message);
#endif
        }

        #region Utility Methods

        /// <summary>
        /// Gets the current versions for a given network's dependency file path.
        /// </summary>
        /// <param name="dependencyPath">A dependency file path that from which to extract current versions.</param>
        /// <returns>Current versions of a given network's dependency file.</returns>
        public static MaxVersions GetCurrentVersions(string dependencyPath)
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

        /// <summary>
        /// Checks whether or not the given package name is the currently importing package.
        /// </summary>
        /// <param name="packageName">The name of the package that needs to be checked.</param>
        /// <returns>true if the importing package matches the given package name.</returns>
        private bool IsImportingNetwork(string packageName)
        {
            // Note: The pluginName doesn't have the '.unitypacakge' extension included in its name but the pluginFileName does. So using Contains instead of Equals.
            return Settings.MaxSettings.editorImportingNetwork != null && GetPluginFileName(Settings.MaxSettings.editorImportingNetwork).Contains(packageName) && !Settings.MaxSettings.editorInstallAllFlag;
        }

        private (bool, int) IsIncludeImportAllNetwork(string packageName)
        {
            if (packageName.Contains('\\')) packageName = packageName.Split('\\').Last();
          
            var flag = false;
            var index = 0;
            for (var i = 0; i < Settings.MaxSettings.editorImportingListNetwork.Count; i++)
            {
                var importing = Settings.MaxSettings.editorImportingListNetwork[i];
                if (importing != null && GetPluginFileName(importing).Contains(packageName))
                {
                    flag = true;
                    index = i;
                    break;
                }
            }

            return (Settings.MaxSettings.editorInstallAllFlag && flag, index);
        }

#if PANCAKE_MAX_ENABLE
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

        /// <summary>
        /// Adds labels to assets so that they can be easily found.
        /// </summary>
        /// <param name="pluginParentDir">The MAX Unity plugin's parent directory.</param>
        /// <param name="isPluginOutsideAssetsDirectory">Whether or not the plugin is outside the Assets directory.</param>
        public static void AddLabelsToAssetsIfNeeded(string pluginParentDir, bool isPluginOutsideAssetsDirectory)
        {
            if (isPluginOutsideAssetsDirectory)
            {
                var defaultPluginLocation = Path.Combine("Assets", "MaxSdk");
                if (Directory.Exists(defaultPluginLocation))
                {
                    AddLabelsToAssets(defaultPluginLocation, "Assets");
                }
            }

            var pluginDir = Path.Combine(pluginParentDir, "MaxSdk");
            AddLabelsToAssets(pluginDir, pluginParentDir);
        }

        private static void AddLabelsToAssets(string directoryPath, string pluginParentDir)
        {
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;

                UpdateAssetLabelsIfNeeded(file, pluginParentDir);
            }

            var directories = Directory.GetDirectories(directoryPath);
            foreach (var directory in directories)
            {
                // Add labels to this directory asset.
                UpdateAssetLabelsIfNeeded(directory, pluginParentDir);

                // Recursively add labels to all files under this directory.
                AddLabelsToAssets(directory, pluginParentDir);
            }
        }

        private static void UpdateAssetLabelsIfNeeded(string assetPath, string pluginParentDir)
        {
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            var labels = AssetDatabase.GetLabels(asset);

            var labelsToAdd = labels.ToList();
            var didAddLabels = false;
            if (!Enumerable.Contains(labels, "al_max"))
            {
                labelsToAdd.Add("al_max");
                didAddLabels = true;
            }

            var exportPathLabel = "al_max_export_path-" + assetPath.Replace(pluginParentDir, "");
            if (!Enumerable.Contains(labels, exportPathLabel))
            {
                labelsToAdd.Add(exportPathLabel);
                didAddLabels = true;
            }

            // We only need to set the labels if they changed.
            if (!didAddLabels) return;

            AssetDatabase.SetLabels(asset, labelsToAdd.ToArray());
        }

        /// <summary>
        /// Moves the imported plugin files to the MaxSdk directory if the publisher has moved the plugin to a different directory. This is a failsafe for when some plugin files are not imported to the new location.
        /// </summary>
        /// <returns>True if the adapters have been moved.</returns>
        public static bool MovePluginFilesIfNeeded(string pluginParentDirectory, bool isPluginOutsideAssetsDirectory)
        {
            var pluginDir = Path.Combine(pluginParentDirectory, "MaxSdk");

            // Check if the user has moved the Plugin and if new assets have been imported to the default directory.
            if (DefaultPluginExportPath.Equals(pluginDir) || !Directory.Exists(DefaultPluginExportPath)) return false;

            MovePluginFiles(DefaultPluginExportPath, pluginDir, isPluginOutsideAssetsDirectory);
            if (!isPluginOutsideAssetsDirectory)
            {
                FileUtil.DeleteFileOrDirectory(DefaultPluginExportPath + ".meta");
            }

            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// A helper function to move all the files recursively from the default plugin dir to a custom location the publisher moved the plugin to.
        /// </summary>
        private static void MovePluginFiles(string fromDirectory, string pluginRoot, bool isPluginOutsideAssetsDirectory)
        {
            var files = Directory.GetFiles(fromDirectory);
            foreach (var file in files)
            {
                // We have to ignore some files, if the plugin is outside the Assets/ directory.
                if (isPluginOutsideAssetsDirectory &&
                    Enumerable.Any(PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory, pluginPathsToIgnore => file.Contains(pluginPathsToIgnore))) continue;

                // Check if the destination folder exists and create it if it doesn't exist
                var parentDirectory = Path.GetDirectoryName(file);
                var destinationDirectoryPath = parentDirectory.Replace(DefaultPluginExportPath, pluginRoot);
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

                var destinationPath = file.Replace(DefaultPluginExportPath, pluginRoot);

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
                    Enumerable.Any(PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory, pluginPathsToIgnore => directory.Contains(pluginPathsToIgnore))) continue;

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

        private static void CallImportPackageCompletedCallback(MaxNetwork network) { importPackageCompletedCallback?.Invoke(network); }

        private static object GetEditorUserBuildSetting(string name, object defaultValue)
        {
            var editorUserBuildSettingsType = typeof(EditorUserBuildSettings);
            var property = editorUserBuildSettingsType.GetProperty(name);
            if (property != null)
            {
                var value = property.GetValue(null, null);
                if (value != null) return value;
            }

            return defaultValue;
        }

        private static bool IsUnity2018_2OrNewer()
        {
#if UNITY_2018_2_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        private static string GetPluginFileName(MaxNetwork network)
        {
            return network.Name.ToLowerInvariant() + "_" + network.LatestVersions.Unity + ".unitypackage";
        }

        public void Load()
        {
#if PANCAKE_MAX_ENABLE
            EditorCoroutine.Start(Instance.LoadPluginData(_ =>
            {
                Settings.MaxSettings.editorListNetwork = _.MediatedNetworks;
                foreach (var mediationNetwork in Settings.MaxSettings.editorListNetwork.ToList())
                {
                    if (!mediationNetwork.Name.Equals("ADCOLONY_NETWORK") &&
                        !mediationNetwork.Name.Equals("CHARTBOOST_NETWORK") &&
                        !mediationNetwork.Name.Equals("FACEBOOK_MEDIATE") &&
                        !mediationNetwork.Name.Equals("ADMOB_NETWORK") &&
                        !mediationNetwork.Name.Equals("INMOBI_NETWORK") &&
                        !mediationNetwork.Name.Equals("IRONSOURCE_NETWORK") &&
                        !mediationNetwork.Name.Equals("MINTEGRAL_NETWORK") &&
                        !mediationNetwork.Name.Equals("TIKTOK_NETWORK") &&
                        !mediationNetwork.Name.Equals("UNITY_NETWORK") &&
                        !mediationNetwork.Name.Equals("VUNGLE_NETWORK"))
                    {
                        Settings.MaxSettings.editorListNetwork.Remove(mediationNetwork);
                    }
                }

                LoadPluginDataExtend(_ => { Settings.MaxSettings.editorListNetwork.AddRange(_); });
            }));
#endif
        }

        public IEnumerator DownloadMaxSdk(Network network)
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, $"MaxSdk-v{network.lastVersion.unity}.unitypackage");
            string urlDownload = string.Format(network.path, network.lastVersion.unity);
            var downloadHandler = new DownloadHandlerFile(pathFile);
            webRequest = new UnityWebRequest(urlDownload) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f);
                downloadPluginProgressCallback?.Invoke(network.displayName, operation.progress, operation.isDone, -1);
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
                Settings.AdmobSettings.editorImportingSdk = network;
                AssetDatabase.ImportPackage(Path.Combine(Application.temporaryCachePath, $"MaxSdk-v{network.lastVersion.unity}.unitypackage"), true);
            }

            webRequest = null;
        }

        public void LoadMaxSdkJson()
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string json = curl.DownloadString("https://gist.githubusercontent.com/yenmoc/b055344445ef8ee6e7535f895aa9839b/raw");
            Settings.MaxSettings.editorImportingSdk = JsonConvert.DeserializeObject<Network>(json);
            UpdateCurrentVersionMaxSdk(Settings.MaxSettings.editorImportingSdk);
        }
        
        public void UpdateCurrentVersionMaxSdk(Network network)
        {
            var gmaFolder = Path.Combine(PluginParentDirectory, "MaxSdk");
            NetworkVersion currentVersion = new NetworkVersion();
            if (Directory.Exists(gmaFolder))
            {
                var files = Directory.GetFiles(gmaFolder);
                foreach (string s in files)
                {
                    if (s.Contains(network.dependenciesFilePath))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(s);
                        currentVersion.android = currentVersion.ios = currentVersion.unity = fileName.Split('-')[1].Split('_')[0];
                        break;
                    }
                }
            }

            network.currentVersion = currentVersion;

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

        #endregion
    }
}