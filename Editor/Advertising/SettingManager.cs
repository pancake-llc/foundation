using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json;
using Pancake.Editor;
using Unity.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


namespace Pancake.Monetization.Editor
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
    public delegate void ImportPackageCompletedCallback(Network network);

    public class SettingManager
    {
        // ReSharper disable once InconsistentNaming
        private static readonly SettingManager instance = new SettingManager();
        public static SettingManager Instance => instance;
        public UnityWebRequest webRequest;
        public UnityWebRequest[] brandWidthWebRequest;
        public static readonly string DefaultPluginExportPath = Path.Combine("Assets", "GoogleMobileAds");
        public const string DEFAULT_ADMOB_SDK_ASSET_EXPORT_PATH = @"GoogleMobileAds\GoogleMobileAds.dll";
        private static readonly string AdmobSdkAssetExportPath = Path.Combine("GoogleMobileAds", "GoogleMobileAds.dll");
        public static DownloadPluginProgressCallback downloadPluginProgressCallback;
        public static ImportPackageCompletedCallback importPackageCompletedCallback;
        public static ImportPackageCompletedCallback importGmaCompletedCallback;

        // ReSharper disable once CollectionNeverUpdated.Local
        private static readonly List<string> PluginPathsToIgnoreMoveWhenPluginOutsideAssetsDirectory = new List<string>();

        public static bool IsPluginOutsideAssetsDirectory => !PluginParentDirectory.StartsWith("Assets");

        public static string PluginParentDirectory
        {
            get
            {
                // Search for the asset with the default exported path first, In most cases, we should be able to find the asset.
                // In some cases where we don't, use the platform specific export path to search for the asset (in case of migrating a project from Windows to Mac or vice versa).
                var admobSdkScriptAssetPath = GetAssetPathForExportPath(DEFAULT_ADMOB_SDK_ASSET_EXPORT_PATH);
                if (File.Exists(admobSdkScriptAssetPath))
                {
                    // admobSdkScriptAssetPath will always have AltDirectorySeparatorChar (/) as the path separator. Convert to platform specific path.
                    return admobSdkScriptAssetPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                        .Replace(DEFAULT_ADMOB_SDK_ASSET_EXPORT_PATH, "")
                        .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                // We should never reach this line but leaving this in out of paranoia.
                return GetAssetPathForExportPath(AdmobSdkAssetExportPath)
                    .Replace(AdmobSdkAssetExportPath, "")
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
        }

        /// <summary>
        /// When the base plugin is outside the <c>Assets/</c> directory, the mediation plugin files are still imported to the default location under <c>Assets/</c>.
        /// Returns the parent directory where the mediation adapter plugins are imported.
        /// </summary>
        public static string MediationSpecificPluginParentDirectory => IsPluginOutsideAssetsDirectory ? "Assets" : PluginParentDirectory;

        public SettingManager()
        {
            AssetDatabase.importPackageCompleted += OnAssetDatabaseOnimportPackageCompleted;
            AssetDatabase.importPackageCancelled += OnAssetDatabaseOnimportPackageCancelled;
            AssetDatabase.importPackageFailed += OnAssetDatabaseOnimportPackageFailed;

            AssetDatabase.importPackageCompleted += OnAssetDatabaseOnimportAllPackageCompleted;
            AssetDatabase.importPackageCancelled += OnAssetDatabaseOnimportAllPackageCancelled;
            AssetDatabase.importPackageFailed += OnAssetDatabaseOnimportAllPackageFailed;


            #region gma

            AssetDatabase.importPackageCompleted += OnImportSdkCompletedCallback;
            AssetDatabase.importPackageCancelled += OnImportSdkCancelledCallback;
            AssetDatabase.importPackageFailed += OnImportSdkFailedCallback;

            #endregion
        }

        private void OnAssetDatabaseOnimportAllPackageFailed(string packageName, string errorMessage)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;

            Debug.LogError(errorMessage);
            Settings.AdmobSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnAssetDatabaseOnimportAllPackageCancelled(string packageName)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;

            Settings.AdmobSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnAssetDatabaseOnimportAllPackageCompleted(string packageName)
        {
            var result = IsIncludeImportAllNetwork(packageName);
            if (!result.Item1) return;

            var pluginParentDir = PluginParentDirectory;
            var isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();

            Debug.Log("Import completed : " + packageName);
            CallImportPackageCompletedCallback(Settings.AdmobSettings.editorImportingListNetwork[result.Item2]);
            Settings.AdmobSettings.editorImportingListNetwork[result.Item2] = null;
        }

        private void OnImportSdkFailedCallback(string packageName, string errorMessage)
        {
            if (!IsImportingGMA(packageName)) return;

            Debug.LogError(errorMessage);
            Settings.AdmobSettings.editorImportingSdk = null;
        }

        private void OnImportSdkCancelledCallback(string packageName)
        {
            if (!IsImportingGMA(packageName)) return;

            Settings.AdmobSettings.editorImportingSdk = null;
        }

        private void OnImportSdkCompletedCallback(string packageName)
        {
            if (!IsImportingGMA(packageName)) return;

            //var pluginParentDir = PluginParentDirectory;
            //var isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            //MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            //AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();

            CallImportGmaCompletedCallback(Settings.AdmobSettings.editorImportingSdk);
            Settings.AdmobSettings.editorImportingSdk = null;
        }

        private void OnAssetDatabaseOnimportPackageFailed(string packageName, string errorMessage)
        {
            if (!IsImportingNetwork(packageName)) return;

            Debug.LogError(errorMessage);
            Settings.AdmobSettings.editorImportingNetwork = null;
        }

        private void OnAssetDatabaseOnimportPackageCancelled(string packageName)
        {
            if (!IsImportingNetwork(packageName)) return;

            Settings.AdmobSettings.editorImportingNetwork = null;
        }

        private void OnAssetDatabaseOnimportPackageCompleted(string packageName)
        {
            if (!IsImportingNetwork(packageName)) return;

            var pluginParentDir = PluginParentDirectory;
            var isPluginOutsideAssetsDir = IsPluginOutsideAssetsDirectory;
            MovePluginFilesIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AddLabelsToAssetsIfNeeded(pluginParentDir, isPluginOutsideAssetsDir);
            AssetDatabase.Refresh();

            CallImportPackageCompletedCallback(Settings.AdmobSettings.editorImportingNetwork);
            Settings.AdmobSettings.editorImportingNetwork = null;
        }

        private static void CallImportPackageCompletedCallback(Network network) { importPackageCompletedCallback?.Invoke(network); }

        private static void CallImportGmaCompletedCallback(Network network) { importGmaCompletedCallback?.Invoke(network); }

        /// <summary>
        /// Adds labels to assets so that they can be easily found.
        /// </summary>
        /// <param name="pluginParentDir">The GoogleMobileAds Unity plugin's parent directory.</param>
        /// <param name="isPluginOutsideAssetsDirectory">Whether or not the plugin is outside the Assets directory.</param>
        public static void AddLabelsToAssetsIfNeeded(string pluginParentDir, bool isPluginOutsideAssetsDirectory)
        {
            if (isPluginOutsideAssetsDirectory)
            {
                var defaultPluginLocation = Path.Combine("Assets", "GoogleMobileAds");
                if (Directory.Exists(defaultPluginLocation))
                {
                    AddLabelsToAssets(defaultPluginLocation, "Assets");
                }
            }

            var pluginDir = Path.Combine(pluginParentDir, "GoogleMobileAds");
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
            if (!labels.Contains("pancake_admob"))
            {
                labelsToAdd.Add("pancake_admob");
                didAddLabels = true;
            }

            var exportPathLabel = "pancake_admob_export_path-" + assetPath.Replace(pluginParentDir, "");
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
        public static bool MovePluginFilesIfNeeded(string pluginParentDirectory, bool isPluginOutsideAssetsDirectory)
        {
            var pluginDir = Path.Combine(pluginParentDirectory, "GoogleMobileAds");

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
        /// Gets the path of the asset in the project for a given GoogleMobileAds plugin export path.
        /// ex : pancake_admob_export_path-GoogleMobileAds\GoogleMobileAds.dll
        /// </summary>
        /// <param name="exportPath">The actual exported path of the asset.</param>
        /// <returns>The exported path of the MAX plugin asset or the default export path if the asset is not found.</returns>
        public static string GetAssetPathForExportPath(string exportPath)
        {
            var defaultPath = Path.Combine("Assets", exportPath);
            var assetGuids = AssetDatabase.FindAssets("l:pancake_admob_export_path-" + exportPath);

            return assetGuids.Length < 1 ? defaultPath : AssetDatabase.GUIDToAssetPath(assetGuids[0]);
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

        private bool IsImportingNetwork(string packageName)
        {
            // Note: The pluginName doesn't have the '.unitypacakge' extension included in its name but the pluginFileName does. So using Contains instead of Equals.
            return Settings.AdmobSettings.editorImportingNetwork != null && GetPluginFileName(Settings.AdmobSettings.editorImportingNetwork).Contains(packageName) &&
                   !Settings.AdmobSettings.editorInstallAllFlag;
        }

        private (bool, int) IsIncludeImportAllNetwork(string packageName)
        {
            if (packageName.Contains('\\')) packageName = packageName.Split('\\').Last();

            var flag = false;
            var index = 0;
            for (var i = 0; i < Settings.AdmobSettings.editorImportingListNetwork.Count; i++)
            {
                var importing = Settings.AdmobSettings.editorImportingListNetwork[i];
                if (importing != null && GetPluginFileName(importing).Contains(packageName))
                {
                    flag = true;
                    index = i;
                    break;
                }
            }

            return (Settings.AdmobSettings.editorInstallAllFlag && flag, index);
        }

        private bool IsImportingGMA(string packageName) { return Settings.AdmobSettings.editorImportingSdk != null && packageName.Contains("GoogleMobileAds-v"); }

        private string GetPluginFileName(Network network) { return $"GoogleMobileAds{network.displayName}Mediation.unitypackage"; }

        public void Load()
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            const string url = "https://gist.githubusercontent.com/yenmoc/d79936098344befbd8edfa882c17bf20/raw";
            string json = curl.DownloadString(url);
            Settings.AdmobSettings.editorListNetwork = JsonConvert.DeserializeObject<List<Network>>(json);
            foreach (var n in Settings.AdmobSettings.editorListNetwork)
            {
                UpdateCurrentVersion(n);
            }
        }

        public IEnumerator DownloadPlugin(Network network)
        {
            Settings.AdmobSettings.editorInstallAllFlag = false;
            string pathFile = Path.Combine(Application.temporaryCachePath, $"{network.name.ToLowerInvariant()}_{network.lastVersion.unity}.zip");
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
                Settings.AdmobSettings.editorImportingNetwork = network;

                string folderUnZip = Path.Combine(Application.temporaryCachePath, "UnZip");
                UnZip(folderUnZip, File.ReadAllBytes(pathFile));

                AssetDatabase.ImportPackage(Path.Combine(folderUnZip, $"{network.displayName}UnityAdapter-{network.lastVersion.unity}", GetPluginFileName(network)),
                    true);
            }

            webRequest = null;
        }

        private IEnumerator DownloadPlugin(Network network, int index)
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, $"{network.name.ToLowerInvariant()}_{network.lastVersion.unity}.zip");
            string urlDownload = string.Format(network.path, network.lastVersion.unity);
            var downloadHandler = new DownloadHandlerFile(pathFile);
            brandWidthWebRequest[index] = new UnityWebRequest(urlDownload) {method = UnityWebRequest.kHttpVerbGET, downloadHandler = downloadHandler};
            var operation = brandWidthWebRequest[index].SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSeconds(0.1f); // Just wait till webRequest is completed. Our coroutine is pretty rudimentary.
                downloadPluginProgressCallback?.Invoke(network.displayName, operation.progress, operation.isDone, index);
            }

#if UNITY_2020_1_OR_NEWER
            if (brandWidthWebRequest[index].result != UnityWebRequest.Result.Success)
#elif UNITY_2017_2_OR_NEWER
            if (brandWidthWebRequest[index].isNetworkError || brandWidthWebRequest[index].isHttpError)
#else
            if (brandWidthWebRequest[index].isError)
#endif
            {
                Debug.LogError(brandWidthWebRequest[index].error);
            }
            else
            {
                Settings.AdmobSettings.editorImportingListNetwork.Add(network);
                string folderUnZip = Path.Combine(Application.temporaryCachePath, "UnZip");
                UnZip(folderUnZip, File.ReadAllBytes(pathFile));

                AssetDatabase.ImportPackage(Path.Combine(folderUnZip, $"{network.displayName}UnityAdapter-{network.lastVersion.unity}", GetPluginFileName(network)),
                    false);
            }

            brandWidthWebRequest[index] = null;
        }

        public void DownloadAllPlugin(List<Network> networks)
        {
            brandWidthWebRequest = new UnityWebRequest[networks.Count];
            Settings.AdmobSettings.editorImportingListNetwork.Clear();
            Settings.AdmobSettings.editorImportingNetwork = null;
            Settings.AdmobSettings.editorInstallAllFlag = true;

            for (var i = 0; i < networks.Count; i++)
            {
                EditorCoroutine.Start(DownloadPlugin(networks[i], i));
            }
        }

        public IEnumerator DownloadGma(Network network)
        {
            string pathFile = Path.Combine(Application.temporaryCachePath, $"GoogleMobileAds-v{network.lastVersion.unity}.unitypackage");
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
                Settings.AdmobSettings.editorImportingSdk = network;
                AssetDatabase.ImportPackage(Path.Combine(Application.temporaryCachePath, $"GoogleMobileAds-v{network.lastVersion.unity}.unitypackage"), true);
            }

            webRequest = null;
        }

        public void LoadGma()
        {
            using var curl = new WebClient();
            curl.Headers.Add(HttpRequestHeader.UserAgent, "request");
            string json = curl.DownloadString("https://gist.githubusercontent.com/yenmoc/df91d875eb78556b8644a2c5a7dc8a03/raw");
            Settings.AdmobSettings.editorImportingSdk = JsonConvert.DeserializeObject<Network>(json);

            UpdateCurrentVersionGMA(Settings.AdmobSettings.editorImportingSdk);
        }

        public void UpdateCurrentVersionGMA(Network network)
        {
            var gmaFolder = Path.Combine(PluginParentDirectory, "GoogleMobileAds");
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
                    if (directoryName.Length > 0)
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

        public void UpdateCurrentVersion(Network network)
        {
            var dependencyFilePath = Path.Combine(PluginParentDirectory, network.dependenciesFilePath);
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

        /// <summary>
        /// since MediationExtras class is already added in Admob package
        /// so we don't need MediationExtras class from importing mediation package anymore it will be confusing
        /// </summary>
        public void RemoveMediationExtras(Network network)
        {
            if (network.name.Equals("ADCOLONY_NETWORK") || network.name.Equals("VUNGLE_NETWORK"))
            {
                FileUtil.DeleteFileOrDirectory(Path.Combine(MediationSpecificPluginParentDirectory, "GoogleMobileAds/Api/Mediation/MediationExtras.cs"));
                FileUtil.DeleteFileOrDirectory(Path.Combine(MediationSpecificPluginParentDirectory, "GoogleMobileAds/Api/Mediation/MediationExtras.cs.meta"));
                AssetDatabase.Refresh();
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

        private static bool GetEmptyDirectories(DirectoryInfo dir, List<DirectoryInfo> results)
        {
            var isEmpty = true;
            try
            {
                isEmpty = dir.GetDirectories().Count(x => !GetEmptyDirectories(x, results)) == 0 // Are sub directories empty?
                          && dir.GetFiles("*.*").All(x => x.Extension == ".meta"); // No file exist?
            }
            catch
            {
            }

            // Store empty directory to results.
            if (isEmpty) results.Add(dir);

            return isEmpty;
        }

        public static void RemoveAllEmptyFolder(DirectoryInfo dir)
        {
            var result = new List<DirectoryInfo>();
            GetEmptyDirectories(dir, result);

            if (result.Count > 0)
            {
                foreach (var d in result)
                {
                    FileUtil.DeleteFileOrDirectory(d.FullName);
                    FileUtil.DeleteFileOrDirectory(d.Parent + "\\" + d.Name + ".meta"); // unity 2020.2 need to delete the meta too
                }

                AssetDatabase.Refresh();
            }
        }

        public static bool IsAdmobSdkImported()
        {
            if (AssetDatabase.FindAssets(AdsEditorUtil.DEFAULT_FILTER_ADMOB_DLL).Length >= 1 ||
                AssetDatabase.FindAssets(@"l:pancake_exportpath-" + PluginParentDirectory).Length >= 1)
            {
                return true;
            }

            return false;
        }

        public static void ValidateAdmobSdkImported()
        {
            if (IsAdmobSdkImported())
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_ADMOB);
            }
            else
            {
                ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_ADMOB);
            }
        }

        public static bool IsMaxSdkImported()
        {
            if (AssetDatabase.FindAssets(AdsEditorUtil.DEFAULT_FILTER_MAX_MAXSDK).Length >= 1 ||
                AssetDatabase.FindAssets(AdsEditorUtil.DEFAULT_FILTER_MAX_MAXSDK.Replace("/", "\\")).Length >= 1)
            {
                return true;
            }

            return false;
        }

        public static void ValidateApplovinSdkImported()
        {
            if (IsMaxSdkImported())
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_APPLOVIN);
            }
            else
            {
                ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_APPLOVIN);
            }
        }

        public static bool IsIronSourceSdkImported()
        {
            return AssetDatabase.FindAssets(AdsEditorUtil.DEFAULT_FILTER_IRONSOURCE_SDK).Length >= 1 ||
                   AssetDatabase.FindAssets(AdsEditorUtil.DEFAULT_FILTER_IRONSOURCE_SDK.Replace("/", "\\")).Length >= 1;
        }

        public static void ValidateIronSourceSdkImported()
        {
            if (IsIronSourceSdkImported())
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_IRONSOURCE);
            }
            else
            {
                ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(AdsEditorUtil.SCRIPTING_DEFINITION_IRONSOURCE);
            }
        }

        public static void SetNetworkUnityVersion(string name, string version) { EditorPrefs.SetString($"{Application.identifier}_ads_{name}_unity", version); }
        public static string GetNetworkUnityVersion(string name) { return EditorPrefs.GetString($"{Application.identifier}_ads_{name}_unity"); }
    }
}