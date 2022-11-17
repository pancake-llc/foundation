using System.Collections;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Monetization.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private UnityEditor.Editor _editor;

        private void OnGUI()
        {
            if (_editor == null) _editor = UnityEditor.Editor.CreateEditor(AdSettings.Instance);

            if (_editor == null)
            {
                EditorGUILayout.HelpBox("Coundn't create the settings resources editor.", MessageType.Error);
                return;
            }

            SettingsEditor.callFromEditorWindow = true;

            _editor.DrawHeader();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginVertical(new GUIStyle {padding = new RectOffset(6, 3, 3, 3)});
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            SettingsEditor.callFromEditorWindow = false;
        }

        private static SettingsWindow GetWindow()
        {
            var window = GetWindow<SettingsWindow>(InEditor.InspectorWindow);
            window.titleContent = new GUIContent("Ads");

            return window;
        }

        public static void ShowWindow()
        {
            var window = GetWindow();
            if (window == null)
            {
                Debug.LogError("Coundn't open the ads settings window.");
                return;
            }

            window.minSize = new Vector2(275, 0);
            window.Show();
        }

        private void OnEnable()
        {
            SettingManager.downloadPluginProgressCallback = OnDownloadPluginProgress;
            SettingManager.importPackageCompletedCallback = OnImportPackageCompleted;

            SettingManager.importGmaCompletedCallback = OnImportGmaCompleted;

            MaxManager.downloadPluginProgressCallback = OnDownloadMaxPluginProgress;
            MaxManager.importPackageCompletedCallback = OnMaxImportPackageCompleted;

            IronSourceManager.downloadPluginProgressCallback = OnDownloadIronSourcePluginProgress;
            IronSourceManager.importPackageCompletedCallback = OnImportIronSourceCompleted;

            SettingManager.Instance.Load();
            SettingManager.Instance.LoadGma();
            IronSourceManager.Instance.Load();
            MaxManager.Instance.Load();
            MaxManager.Instance.LoadMaxSdkJson();
            Uniform.LoadFoldoutSetting();
        }

        private void OnDisable()
        {
            SettingManager.Instance.webRequest?.Abort();
            IronSourceManager.Instance.webRequest?.Abort();
            IronSourceManager.Instance.webRequest?.Abort();
            EditorUtility.ClearProgressBar();
            Uniform.SaveFoldoutSetting();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Callback method that will be called when package import completed
        /// </summary>
        /// <param name="network"></param>
        private static void OnImportPackageCompleted(Network network)
        {
            SettingManager.SetNetworkUnityVersion(network.name, network.lastVersion.unity);
            SettingManager.Instance.UpdateCurrentVersion(network);
            SettingManager.Instance.RemoveMediationExtras(network);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        private static void OnImportGmaCompleted(Network network)
        {
            SettingManager.Instance.UpdateCurrentVersionGMA(network);

            EditorCoroutine.Start(DelayRefreshGma(1, network));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        private static IEnumerator DelayRefreshGma(float delay, Network network)
        {
            yield return new WaitForSeconds(delay);
            SettingManager.Instance.UpdateCurrentVersionGMA(network);
        }

        /// <summary>
        /// Callback method that will be called with progress updates when the plugin is being downloaded.
        /// </summary>
        private static void OnDownloadPluginProgress(string pluginName, float progress, bool done, int index)
        {
            // Download is complete. Clear progress bar.
            if (done)
            {
                EditorUtility.ClearProgressBar();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar("Ads", string.Format("Downloading {0} plugin...", pluginName), progress))
                {
                    if (index == -1)
                    {
                        SettingManager.Instance.webRequest?.Abort();
                    }
                    else
                    {
                        SettingManager.Instance.brandWidthWebRequest[index]?.Abort();
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// Callback method that will be called when package import completed
        /// </summary>
        /// <param name="network"></param>
        private static void OnMaxImportPackageCompleted(MaxNetwork network)
        {
            string parentDirectory = network.Name.Equals("APPLOVIN_NETWORK") ? MaxManager.PluginParentDirectory : MaxManager.MediationSpecificPluginParentDirectory;
            MaxManager.UpdateCurrentVersions(network, parentDirectory);
        }

        /// <summary>
        /// Callback method that will be called with progress updates when the plugin is being downloaded.
        /// </summary>
        private static void OnDownloadMaxPluginProgress(string pluginName, float progress, bool done, int index)
        {
            // Download is complete. Clear progress bar.
            if (done)
            {
                EditorUtility.ClearProgressBar();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar("Advertisement", string.Format("Downloading {0} plugin...", pluginName), progress))
                {
                    if (index == -1)
                    {
                        MaxManager.Instance.webRequest?.Abort();
                    }
                    else
                    {
                        MaxManager.Instance.branWidthRequest[index]?.Abort();
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// Callback method that will be called with progress updates when the plugin is being downloaded.
        /// </summary>
        private static void OnDownloadIronSourcePluginProgress(string pluginName, float progress, bool done, int index)
        {
            // Download is complete. Clear progress bar.
            if (done)
            {
                EditorUtility.ClearProgressBar();
            }
            // Download is in progress, update progress bar.
            else
            {
                if (EditorUtility.DisplayCancelableProgressBar("Ads", string.Format("Downloading {0} plugin...", pluginName), progress))
                {
                    if (index == -1)
                    {
                        IronSourceManager.Instance.webRequest?.Abort();
                    }
                    else
                    {
                        IronSourceManager.Instance.branWidthRequest[index]?.Abort();
                    }

                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        private static void OnImportIronSourceCompleted(Network network)
        {
            IronSourceManager.Instance.UpdateCurrentVersion(network);
            EditorCoroutine.Start(DelayRefreshIronSource(1, network));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        private static IEnumerator DelayRefreshIronSource(float delay, Network network)
        {
            yield return new WaitForSeconds(delay);
            IronSourceManager.Instance.UpdateCurrentVersion(network);
        }
    }
}