using System;
using System.IO;
using Pancake.ExLib;
using Pancake.ExLibEditor;
using Pancake.Monetization;
using UnityEditor;
using UnityEngine;

namespace Pancake.MonetizationEditor
{
    [CustomEditor(typeof(AdSettings), true)]
    public class AdSettingsDrawer : Editor
    {
        private SerializedProperty _adCheckingIntervalProperty;
        private SerializedProperty _adLoadingIntervalProperty;
        private SerializedProperty _currentNetworkProperty;
        private SerializedProperty _admobEnableTestModeProperty;
        private SerializedProperty _admobDevicesTestProperty;
        private SerializedProperty _admobClientProperty;
        private SerializedProperty _admobBannerProperty;
        private SerializedProperty _admobInterProperty;
        private SerializedProperty _admobRewardProperty;
        private SerializedProperty _admobRewardInterProperty;
        private SerializedProperty _admobAppOpenProperty;

        private SerializedProperty _sdkKeyPropertyProperty;
        private SerializedProperty _applovinClientProperty;
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
            _admobEnableTestModeProperty = serializedObject.FindProperty("admobEnableTestMode");
            _admobDevicesTestProperty = serializedObject.FindProperty("admobDevicesTest");
            _admobClientProperty = serializedObject.FindProperty("admobClient");
            _admobBannerProperty = serializedObject.FindProperty("admobBanner");
            _admobInterProperty = serializedObject.FindProperty("admobInter");
            _admobRewardProperty = serializedObject.FindProperty("admobReward");
            _admobRewardInterProperty = serializedObject.FindProperty("admobRewardInter");
            _admobAppOpenProperty = serializedObject.FindProperty("admobAppOpen");

            _sdkKeyPropertyProperty = serializedObject.FindProperty("sdkKey");
            _applovinClientProperty = serializedObject.FindProperty("applovinClient");
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

                AssetDatabase.Refresh();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Copy Admob Test AppId"))
            {
                "ca-app-pub-3940256099942544~3347511713".CopyToClipboard();
                DebugEditor.Toast("[Admob] Copy AppId Test Id Success!");
            }
#else
            GUI.backgroundColor = Uniform.Orange;
            EditorGUILayout.HelpBox("Admob plugin not found", MessageType.Warning);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Uniform.Green;
            if (GUILayout.Button("Install Admob SDK (1)", GUILayout.Height(24)))
            {
                DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing admob sdk");
                const string path = "Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string manifest = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                  "\n<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.google.unity.ads\" android:versionName=\"1.0\" android:versionCode=\"1\">" +
                                  "\n<application><uses-library android:required=\"false\" android:name=\"org.apache.http.legacy\"/></application>" + "\n</manifest>";

                string manifestMeta = @"fileFormatVersion: 2
guid: e869b36b657604102a7166a39ee7d9c9
labels:
- gvh
- gvh_version-8.6.0
- gvhp_exportpath-Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
timeCreated: 1427838353
licenseType: Free
TextScriptImporter:
  userData:
  assetBundleName:
  assetBundleVariant:
";

                if (!File.Exists(path + "/AndroidManifest.xml"))
                {
                    var writer = new StreamWriter(path + "/AndroidManifest.xml", false);
                    writer.Write(manifest);
                    writer.Close();
                }

                if (!File.Exists(path + "/AndroidManifest.xml.meta"))
                {
                    var writer = new StreamWriter(path + "/AndroidManifest.xml.meta", false);
                    writer.Write(manifestMeta);
                    writer.Close();
                }

                string properties = "target=android-31" + "\nandroid.library=true";
                string propertiesMeta = @"fileFormatVersion: 2
guid: 1d06ef613dfec4baa88b56aeef69cd9c
labels:
- gvh
- gvh_version-8.6.0
- gvhp_exportpath-Plugins/Android/GoogleMobileAdsPlugin.androidlib/project.properties
timeCreated: 1427838343
licenseType: Free
DefaultImporter:
  userData:
  assetBundleName:
  assetBundleVariant:
";

                if (!File.Exists(path + "/project.properties"))
                {
                    var writer = new StreamWriter(path + "/project.properties", false);
                    writer.Write(properties);
                    writer.Close();
                }

                if (!File.Exists(path + "/project.properties.meta"))
                {
                    var writer = new StreamWriter(path + "/project.properties.meta", false);
                    writer.Write(propertiesMeta);
                    writer.Close();
                }
                
                string packagingOption = @"android {
    packagingOptions {
        pickFirst ""META-INF/kotlinx_coroutines_core.version""
    }
}
";
                string packagingOptionMeta = @"fileFormatVersion: 2
guid: 5b00d19726f74439b9a2fd9accf90795
labels:
- gvh
- gvh_version-8.6.0
- gvhp_exportpath-Plugins/Android/GoogleMobileAdsPlugin.androidlib/packaging_options.gradle
timeCreated: 1480838400
PluginImporter:
  serializedVersion: 1
  iconMap: {}
  executionOrder: {}
  isPreloaded: 0
  platformData:
    Android:
      enabled: 1
      settings:
        CPU: AnyCPU
    Any:
      enabled: 0
      settings: {}
    Editor:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
    Linux:
      enabled: 1
      settings:
        CPU: x86
    Linux64:
      enabled: 1
      settings:
        CPU: x86_64
    LinuxUniversal:
      enabled: 1
      settings:
        CPU: AnyCPU
    OSXIntel:
      enabled: 1
      settings:
        CPU: x86
    OSXIntel64:
      enabled: 1
      settings:
        CPU: x86_64
    OSXUniversal:
      enabled: 1
      settings:
        CPU: AnyCPU
    Web:
      enabled: 0
      settings: {}
    WebStreamed:
      enabled: 0
      settings: {}
    Win:
      enabled: 1
      settings:
        CPU: x86
    Win64:
      enabled: 1
      settings:
        CPU: x86_64
    WindowsStoreApps:
      enabled: 0
      settings:
        CPU: AnyCPU
    iOS:
      enabled: 0
      settings:
        CompileFlags:
        FrameworkDependencies:
    tvOS:
      enabled: 0
      settings:
        CompileFlags:
        FrameworkDependencies:
  userData:
  assetBundleName:
  assetBundleVariant:
";
                if (!File.Exists(path + "/packaging_options.gradle"))
                {
                    var writer = new StreamWriter(path + "/packaging_options.gradle", false);
                    writer.Write(packagingOption);
                    writer.Close();
                }

                if (!File.Exists(path + "/packaging_options.gradle.meta"))
                {
                    var writer = new StreamWriter(path + "/packaging_options.gradle.meta", false);
                    writer.Write(packagingOptionMeta);
                    writer.Close();
                }
                
                string validateDependency = @"// Copyright (C) 2023 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

import groovy.util.XmlSlurper
import groovy.xml.XmlUtil

import java.util.zip.ZipEntry
import java.util.zip.ZipOutputStream

configurations {
    // Configuration used to resolve the artifacts of dependencies.
    aarArtifacts.extendsFrom implementation
}

/**
 * Validates the Unity GMA plugin dependencies.
 * Add the following snippet to Assets/Plugins/Android/mainTemplate.gradle in the Unity Editor or
 * unityLibrary/build.gradle in an Android project to use this script:
 * <pre>{@code
 * gradle.projectsEvaluated {
 *     apply from: 'GoogleMobileAdsPlugin.androidlib/validate_dependencies.gradle'
 * }
 * }</pre>
 */
task validateDependencies {
    def expandedArchiveDirectory
    // List of artifacts resolved from the aarArtifacts configuration.
    project.configurations.aarArtifacts.
            resolvedConfiguration.lenientConfiguration.
            getArtifacts(Specs.satisfyAll()).findResults {
        ResolvedArtifact artifact ->
            File artifactTargetFile = new File(artifact.file.parent , artifact.file.name)
            // Desired artifact - com.google.android.gms:play-services-ads-lite:22.4.0
            // Group ID    - com.google.android.gms
            // Artifact ID - play-services-ads-lite
            // Since Gradle has different naming convention for the same artifact in
            // * modules-2 cache    - play-services-ads-lite-22.4.0.aar
            // * transforms-2 cache - com.google.android.gms.play-services-ads-lite-22.4.0
            // we look for the common segment.
            if (artifact.name.contains(""play-services-ads-lite"")) {
                // Explode the archive to a temporary directory.
                FileTree expandedArchive = project.zipTree(artifactTargetFile)
                expandedArchive.forEach { File androidManifest ->
                    if (androidManifest.getName() == ""AndroidManifest.xml"") {
                        def xml = new XmlSlurper().parse(androidManifest)
                        def propertyNode = xml.depthFirst().find { it.name() == 'property' }
                        if (propertyNode) {
                            // Replace the <property> node with a comment.
                            propertyNode.replaceNode {
                                mkp.comment 'android.adservices.AD_SERVICES_CONFIG property'\
                                + ' removed by GoogleMobileAds Unity plugin - Release notes: '\
                                + 'https://github.com/googleads/googleads-mobile-unity/releases/'\
                                + 'tag/v8.6.0'
                            }
                        }
                        def updatedXml = XmlUtil.serialize(xml)
                        androidManifest.setWritable(true)
                        androidManifest.text = updatedXml
                        expandedArchiveDirectory = androidManifest.parent
                    }
                }
                // Update the artifact archive.
                artifactTargetFile.withOutputStream { outputStream ->
                    def zipStream = new ZipOutputStream(outputStream)
                    file(expandedArchiveDirectory).eachFileRecurse { file ->
                        if (file.isFile()) {
                            def entry = new ZipEntry(file.name)
                            zipStream.putNextEntry(entry)
                            file.withInputStream { zipStream << it }
                            zipStream.closeEntry()
                        }
                    }
                    zipStream.close()
                }
            }
    }
    // Clean up the temporary directory.
    if (expandedArchiveDirectory) delete expandedArchiveDirectory
}

// Run the update task before unityLibrary project is built.
project(':unityLibrary:GoogleMobileAdsPlugin.androidlib') {
    tasks.named('preBuild') {
        dependsOn validateDependencies
    }
}
";
                string validateDependencyMeta = @"fileFormatVersion: 2
guid: 9dfd887a15174d2b91b4e979131a7ac7
labels:
- gvh
- gvh_version-8.6.0
- gvhp_exportpath-Plugins/Android/GoogleMobileAdsPlugin.androidlib/validate_dependencies.gradle
timeCreated: 1480838400
PluginImporter:
  serializedVersion: 1
  iconMap: {}
  executionOrder: {}
  isPreloaded: 0
  platformData:
    Android:
      enabled: 1
      settings:
        CPU: AnyCPU
    Any:
      enabled: 0
      settings: {}
    Editor:
      enabled: 1
      settings:
        CPU: AnyCPU
        DefaultValueInitialized: true
        OS: AnyOS
    Linux:
      enabled: 1
      settings:
        CPU: x86
    Linux64:
      enabled: 1
      settings:
        CPU: x86_64
    LinuxUniversal:
      enabled: 1
      settings:
        CPU: AnyCPU
    OSXIntel:
      enabled: 1
      settings:
        CPU: x86
    OSXIntel64:
      enabled: 1
      settings:
        CPU: x86_64
    OSXUniversal:
      enabled: 1
      settings:
        CPU: AnyCPU
    Web:
      enabled: 0
      settings: {}
    WebStreamed:
      enabled: 0
      settings: {}
    Win:
      enabled: 1
      settings:
        CPU: x86
    Win64:
      enabled: 1
      settings:
        CPU: x86_64
    WindowsStoreApps:
      enabled: 0
      settings:
        CPU: AnyCPU
    iOS:
      enabled: 0
      settings:
        CompileFlags:
        FrameworkDependencies:
    tvOS:
      enabled: 0
      settings:
        CompileFlags:
        FrameworkDependencies:
  userData:
  assetBundleName:
  assetBundleVariant:
";
                
                if (!File.Exists(path + "/validate_dependencies.gradle"))
                {
                    var writer = new StreamWriter(path + "/validate_dependencies.gradle", false);
                    writer.Write(validateDependency);
                    writer.Close();
                }

                if (!File.Exists(path + "/validate_dependencies.gradle.meta"))
                {
                    var writer = new StreamWriter(path + "/validate_dependencies.gradle.meta", false);
                    writer.Write(validateDependencyMeta);
                    writer.Close();
                }

                AssetDatabase.ImportAsset(path + "/AndroidManifest.xml");
                AssetDatabase.ImportAsset(path + "/AndroidManifest.xml.meta");
                AssetDatabase.ImportAsset(path + "/project.properties");
                AssetDatabase.ImportAsset(path + "/project.properties.meta");
                AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/admob.unitypackage"), false);
            }

            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Add Admob Symbol (2)", GUILayout.Height(24)))
            {
                var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
                {
                    ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                    AssetDatabase.Refresh();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
#endif
            EditorGUILayout.PropertyField(_admobEnableTestModeProperty, new GUIContent("Test Mode"));
            if (_admobEnableTestModeProperty.boolValue) EditorGUILayout.PropertyField(_admobDevicesTestProperty, new GUIContent("Devices Test"));

            EditorGUILayout.PropertyField(_admobClientProperty, new GUIContent("Client"));
            EditorGUILayout.PropertyField(_admobBannerProperty, new GUIContent("Banner"));
            EditorGUILayout.PropertyField(_admobInterProperty, new GUIContent("Interstitial"));
            EditorGUILayout.PropertyField(_admobRewardProperty, new GUIContent("Rewarded"));
            EditorGUILayout.PropertyField(_admobRewardInterProperty, new GUIContent("Inter Rewarded"));
            EditorGUILayout.PropertyField(_admobAppOpenProperty, new GUIContent("App Open"));

            GUILayout.Space(20);
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

                AssetDatabase.Refresh();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
#else
            GUI.backgroundColor = Uniform.Orange;
            EditorGUILayout.HelpBox("Applovin plugin not found", MessageType.Warning);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Uniform.Green;
            if (GUILayout.Button("Install AppLovin SDK (1)", GUILayout.Height(24)))
            {
                DebugEditor.Log("<color=#FF77C6>[Ad]</color> importing <color=#FF77C6>applovin</color> sdk");
                AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/applovin.unitypackage"), false);
            }

            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Add AppLovin Symbol (2)", GUILayout.Height(24)))
            {
                var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_APPLOVIN", group))
                {
                    ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_APPLOVIN");
                    AssetDatabase.Refresh();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

#endif
            EditorGUILayout.PropertyField(_sdkKeyPropertyProperty);
            EditorGUILayout.PropertyField(_applovinClientProperty, new GUIContent("Client"));
            EditorGUILayout.PropertyField(_applovinBannerProperty, new GUIContent("Banner"));
            EditorGUILayout.PropertyField(_applovinInterProperty, new GUIContent("Interstitial"));
            EditorGUILayout.PropertyField(_applovinRewardProperty, new GUIContent("Rewarded"));
            EditorGUILayout.PropertyField(_applovinRewardInterProperty, new GUIContent("Inter Rewarded"));
            EditorGUILayout.PropertyField(_applovinAppOpenProperty, new GUIContent("App Open"));
            EditorGUILayout.PropertyField(_applovinEnableAgeRestrictedUserProperty, new GUIContent("Age Restricted"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}