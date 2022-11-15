using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Pancake.Monetization.Editor
{
    /// <summary>
    /// A manager class for IronSource integration manager window.
    /// </summary>
    public class IronSourceManager
    {
        private static readonly IronSourceManager instance = new IronSourceManager();
        public static IronSourceManager Instance => instance;

        public static readonly string DefaultPluginExportPath = Path.Combine("Assets", "IronSource");
        private const string DEFAULT_IRONSOURCE_SDK_ASSET_EXPORT_PATH = "IronSource/Scripts/IronSource.cs";
        private static readonly string IronSourceSdkAssetExportPath = Path.Combine("IronSource", "Scripts/IronSource.cs");
        public static DownloadPluginProgressCallback downloadPluginProgressCallback;
        public static ImportPackageCompletedCallback importPackageCompletedCallback;
        private EditorCoroutine _editorCoroutine;

        public UnityWebRequest webRequest;
        public UnityWebRequest[] branWidthRequest;

        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly List<string> PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory = new List<string>();

        public static bool IsPluginOutsideAssetsDirectory => !PluginParentDirectory.StartsWith("Assets");

        /// <summary>
        /// When the base plugin is outside the <c>Assets/</c> directory, the mediation plugin files are still imported to the default location under <c>Assets/</c>.
        /// Returns the parent directory where the mediation adapter plugins are imported.
        /// </summary>
        public static string MediationSpecificPluginParentDirectory => IsPluginOutsideAssetsDirectory ? "Assets" : PluginParentDirectory;

        public static string PluginParentDirectory
        {
            get
            {
#if PANCAKE_IRONSOURCE_ENABLE
                // Search for the asset with the default exported path first, In most cases, we should be able to find the asset.
                // In some cases where we don't, use the platform specific export path to search for the asset (in case of migrating a project from Windows to Mac or vice versa).
                var admobSdkScriptAssetPath = GetAssetPathForExportPath(DEFAULT_IRONSOURCE_SDK_ASSET_EXPORT_PATH);
                if (File.Exists(admobSdkScriptAssetPath))
                {
                    // admobSdkScriptAssetPath will always have AltDirectorySeparatorChar (/) as the path separator. Convert to platform specific path.
                    return admobSdkScriptAssetPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                        .Replace(DEFAULT_IRONSOURCE_SDK_ASSET_EXPORT_PATH, "")
                        .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                // We should never reach this line but leaving this in out of paranoia.
                return GetAssetPathForExportPath(IronSourceSdkAssetExportPath)
                    .Replace(IronSourceSdkAssetExportPath, "")
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
#else
                return "Assets";
#endif
            }
        }

        public static string GetAssetPathForExportPath(string exportPath)
        {
            var defaultPath = Path.Combine("Assets", exportPath);
            var assetGuids = AssetDatabase.FindAssets("l:pancake_ironsource_export_path-" + exportPath);

            return assetGuids.Length < 1 ? defaultPath : AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        }

        private static void InvokeImportPackageCompletedCallback(Network network)
        {
            if (importPackageCompletedCallback == null) return;

            importPackageCompletedCallback(network);
        }

        public IronSourceManager()
        {
            AssetDatabase.importPackageCompleted += OnAssetDatabaseOnimportPackageCompleted;
            AssetDatabase.importPackageCancelled += OnAssetDatabaseOnimportPackageCancelled;
            AssetDatabase.importPackageFailed += OnAssetDatabaseOnimportPackageFailed;
        }

        private void OnAssetDatabaseOnimportPackageFailed(string packageName, string errorMessage)
        {
            if (!IsImportingSdk(packageName)) return;

            Debug.LogError(errorMessage);
            Settings.IronSourceSettings.editorImportingSdk = null;
        }

        private void OnAssetDatabaseOnimportPackageCancelled(string packageName)
        {
            if (!IsImportingSdk(packageName)) return;

            Settings.IronSourceSettings.editorImportingSdk = null;
        }

        private void OnAssetDatabaseOnimportPackageCompleted(string packageName)
        {
            if (!IsImportingSdk(packageName)) return;

            var pluginParentDir = PluginParentDirectory;
            var isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();

            InvokeImportPackageCompletedCallback(Settings.IronSourceSettings.editorImportingSdk);
            Settings.IronSourceSettings.editorImportingSdk = null;
        }

        private bool IsImportingMedationNetwork(string packageName)
        {
            // Note: The pluginName doesn't have the '.unitypacakge' extension included in its name but the pluginFileName does. So using Contains instead of Equals.
            return Settings.IronSourceSettings.editorImportingNetwork != null && packageName.Contains("Dependencies");
        }

        private bool IsImportingSdk(string packageName)
        {
            return Settings.IronSourceSettings.editorImportingSdk != null && packageName.Contains("IronSource_IntegrationManager_v");
        }

        private static void UpdateAssetLabelsIfNeeded(string assetPath, string pluginParentDir)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var labels = AssetDatabase.GetLabels(asset);

            var labelsToAdd = labels.ToList();
            var didAddLabels = false;
            if (!labels.Contains("pancake_ironsource"))
            {
                labelsToAdd.Add("pancake_ironsource");
                didAddLabels = true;
            }

            string partLabel = assetPath.Replace(pluginParentDir, "");
            if (partLabel[0].Equals('\\')) partLabel = partLabel.Remove(0, 1);
            var exportPathLabel = "pancake_ironsource_export_path-" + partLabel;
            if (!labels.Contains(exportPathLabel))
            {
                labelsToAdd.Add(exportPathLabel);
                didAddLabels = true;
            }

            // We only need to set the labels if they changed.
            if (!didAddLabels) return;

            AssetDatabase.SetLabels(asset, labelsToAdd.ToArray());
        }

        /// <summary>
        /// Moves the imported plugin files to the GoogleMobileAds directory if the publisher has moved the plugin to a different directory. This is a failsafe for when some plugin files are not imported to the new location.
        /// </summary>
        /// <returns>True if the adapters have been moved.</returns>
        private static bool MovePluginFilesIfNeeded(string pluginParentDirectory, bool isPluginOutsideAssetsDirectory)
        {
            var pluginDir = Path.Combine(pluginParentDirectory, "IronSource");

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
                    PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory.Any(pluginPathsToIgnore => file.Contains(pluginPathsToIgnore))) continue;

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
                    PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory.Any(pluginPathsToIgnore => directory.Contains(pluginPathsToIgnore))) continue;

                MovePluginFiles(directory, pluginRoot, isPluginOutsideAssetsDirectory);
            }

            if (!isPluginOutsideAssetsDirectory)
            {
                FileUtil.DeleteFileOrDirectory(fromDirectory);
            }
        }

        /// <summary>
        /// Adds labels to assets so that they can be easily found.
        /// </summary>
        /// <param name="pluginParentDir">The GoogleMobileAds Unity plugin's parent directory.</param>
        /// <param name="isPluginOutsideAssetsDirectory">Whether or not the plugin is outside the Assets directory.</param>
        private static void AddLabelsToAssetsIfNeeded(string pluginParentDir, bool isPluginOutsideAssetsDirectory)
        {
            if (isPluginOutsideAssetsDirectory)
            {
                var defaultPluginLocation = Path.Combine("Assets", "IronSource");
                if (Directory.Exists(defaultPluginLocation))
                {
                    AddLabelsToAssets(defaultPluginLocation, "Assets");
                }
            }

            var pluginDir = Path.Combine(pluginParentDir, "IronSource");
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

        public void Load()
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string json = curl.DownloadString("https://gist.githubusercontent.com/yenmoc/3affa7177f0cd83c1e1aac3e6fbcf33d/raw");
            Settings.IronSourceSettings.editorImportingSdk = JsonConvert.DeserializeObject<Network>(json);

            UpdateCurrentVersion(Settings.IronSourceSettings.editorImportingSdk);

            EditorCoroutine.Start(GetVersions());
        }

        public void UpdateCurrentVersion(Network network)
        {
            var ironSourceFolder = Path.Combine(PluginParentDirectory, "IronSource");
            NetworkVersion currentVersion = new NetworkVersion();
            if (Directory.Exists(ironSourceFolder))
            {
                var files = Directory.GetFiles(ironSourceFolder);
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

        public IEnumerator DownloadPlugin(Network network)
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, $"IronSource_IntegrationManager_v{network.lastVersion.unity}.unitypackage");
            string urlDownload = string.Format(network.path, network.lastVersion.unity);
            var downloadHandler = new DownloadHandlerFile(pathFile);
            webRequest = new UnityWebRequest(urlDownload) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f); // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
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
                Settings.IronSourceSettings.editorImportingSdk = network;
                AssetDatabase.ImportPackage(Path.Combine(Application.temporaryCachePath, $"IronSource_IntegrationManager_v{network.lastVersion.unity}.unitypackage"),
                    true);
            }

            webRequest = null;
        }

        public void DownloadAllPlugin(List<AdapterMediationIronSource> networks)
        {
            branWidthRequest = new UnityWebRequest[networks.Count];

            for (var i = 0; i < networks.Count; i++)
            {
                EditorCoroutine.Start(DownloadFileDependency(networks[i].downloadUrl, i));
            }
        }

        public IEnumerator GetVersions()
        {
            var www = UnityWebRequest.Get("http://ssa.public.s3.amazonaws.com/Ironsource-Integration-Manager/IronSourceSDKInfo.json");
#if UNITY_2017_2_OR_NEWER
            var operation = www.SendWebRequest();
#else
            var operation = www.Send();
#endif

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f);
            }

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (www.isNetworkError || www.isHttpError)
#else
            if (www.isError)
#endif
            {
                // nothing
            }
            else
            {
                string json = www.downloadHandler.text;
                Settings.IronSourceSettings.editorListNetwork.Clear();
                Settings.IronSourceSettings.editorImportingNetwork = new AdapterMediationIronSource();
                if (Json.Deserialize(json) is Dictionary<string, object> dic && dic.Count != 0)
                {
                    if (dic.TryGetValue("SDKSInfo", out object adapterJson))
                    {
                        if (adapterJson == null) yield break;
                        foreach (var item in (Dictionary<string, object>) adapterJson)
                        {
                            var key = item.Key.ToLower();
                            var info = new AdapterMediationIronSource();
                            if (info.GetFromJson(item.Key, item.Value as Dictionary<string, object>))
                            {
                                if (key.Contains("ironsource")) Settings.IronSourceSettings.editorImportingNetwork = info;
                                else
                                {
                                    if (key.Equals("hyprmx") || key.Equals("liftoff") || key.Equals("maio") || key.Equals("mytarget") || key.Equals("smaato") ||
                                        key.Equals("snap") || key.Equals("tapjoy") || key.Equals("yahoo") || key.Equals("tencent"))
                                    {
                                        continue;
                                    }

                                    Settings.IronSourceSettings.editorListNetwork.Add(info);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CancelAdapterDownload()
        {
            webRequest?.Abort();
            webRequest = null;
        }

        public IEnumerator DownloadFileDependency(string url)
        {
            int fileNameIndex = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
            string downloadFileName = url.Substring(fileNameIndex);
            string genericFileName = Regex.Replace(downloadFileName, @"_v+(\d\.\d\.\d\.\d|\d\.\d\.\d)", "");
            string path = Path.Combine(AdapterMediationIronSource.AdapterInstallPath, genericFileName);
            bool isCancelled = false;
            webRequest = new UnityWebRequest(url);
            webRequest.downloadHandler = new DownloadHandlerFile(path);
#if UNITY_2017_2_OR_NEWER
            var operation = webRequest.SendWebRequest();
#else
            var operation = webRequest.Send();
#endif

            if (webRequest.result != UnityWebRequest.Result.ConnectionError)
            {
                while (!operation.isDone)
                {
                    yield return new WaitForSeconds(0.1f);

                    if (EditorUtility.DisplayCancelableProgressBar("Download Manager", $"Downloading {downloadFileName}", webRequest.downloadProgress))
                    {
                        if (webRequest.error != null) Debug.LogError(webRequest.error);

                        CancelAdapterDownload();
                        isCancelled = true;
                    }
                }
            }
            else
            {
                Debug.LogError("Error Downloading " + genericFileName + " : " + webRequest.error);
            }

            EditorUtility.ClearProgressBar();

            if (isCancelled && File.Exists(Path.Combine("Assets/IronSource/Editor", genericFileName)))
            {
                File.Delete(Path.Combine("Assets/IronSource/Editor", genericFileName));
            }

            EditorCoroutine.Start(GetVersions());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public IEnumerator DownloadFileDependency(string url, int index)
        {
            int fileNameIndex = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
            string downloadFileName = url.Substring(fileNameIndex);
            string genericFileName = Regex.Replace(downloadFileName, @"_v+(\d\.\d\.\d\.\d|\d\.\d\.\d)", "");
            string path = Path.Combine(AdapterMediationIronSource.AdapterInstallPath, genericFileName);
            bool isCancelled = false;
            branWidthRequest[index] = new UnityWebRequest(url);
            branWidthRequest[index].downloadHandler = new DownloadHandlerFile(path);
#if UNITY_2017_2_OR_NEWER
            var operation = branWidthRequest[index].SendWebRequest();
#else
            var operation = branWidthRequest[index].Send();
#endif

            if (branWidthRequest[index].result != UnityWebRequest.Result.ConnectionError)
            {
                while (!operation.isDone)
                {
                    yield return new WaitForSeconds(0.1f);

                    if (EditorUtility.DisplayCancelableProgressBar("Download Manager", $"Downloading {downloadFileName}", branWidthRequest[index].downloadProgress))
                    {
                        if (branWidthRequest[index].error != null) Debug.LogError(branWidthRequest[index].error);

                        CancelAdapterDownload();
                        isCancelled = true;
                    }
                }
            }
            else
            {
                Debug.LogError("Error Downloading " + genericFileName + " : " + branWidthRequest[index].error);
            }

            EditorUtility.ClearProgressBar();

            if (isCancelled && File.Exists(Path.Combine("Assets/IronSource/Editor", genericFileName)))
            {
                File.Delete(Path.Combine("Assets/IronSource/Editor", genericFileName));
            }

            EditorCoroutine.Start(GetVersions());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void RefreshAllCurrentVersionAdapter()
        {
            foreach (var source in Settings.IronSourceSettings.editorListNetwork)
            {
                source.RefreshCurrentUnityVersion();
            }
        }
    }
}