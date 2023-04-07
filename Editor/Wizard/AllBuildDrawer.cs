using System;
using System.Collections.Generic;
using System.IO;
using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class AllBuildDrawer
    {
        private static List<bool> allStatus;
        public static List<string> keys;
        public static List<string> reasons;
        private static int countPass;
        private static bool isHasBuildResult;

        private static void SyncSetting()
        {

#if UNITY_ANDROID
            switch (PlayerSettings.Android.targetArchitectures)
            {
                case AndroidArchitecture.ARMv7:
                    EditorPreBuildSettings.Architecture = EditorPreBuildSettings.EAndroidArchitecture.ARMv7;
                    break;
                case AndroidArchitecture.ARM64:
                    EditorPreBuildSettings.Architecture = EditorPreBuildSettings.EAndroidArchitecture.ARM64;
                    break;
                case AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64:
                    EditorPreBuildSettings.Architecture = EditorPreBuildSettings.EAndroidArchitecture.ARMv7_ARM64;
                    break;
            }

            switch (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android))
            {
                case ScriptingImplementation.Mono2x:
                    EditorPreBuildSettings.ScriptingBackend = EditorPreBuildSettings.EScriptingBackend.Mono;
                    break;
                case ScriptingImplementation.IL2CPP:
                    EditorPreBuildSettings.ScriptingBackend = EditorPreBuildSettings.EScriptingBackend.IL2CPP;
                    break;
            }

            EditorPreBuildSettings.AppBundle = EditorUserBuildSettings.buildAppBundle;
            EditorPreBuildSettings.CompanyName = PlayerSettings.companyName;
            EditorPreBuildSettings.ProductName = PlayerSettings.productName;
            EditorPreBuildSettings.PackageName = PlayerSettings.applicationIdentifier;
            EditorPreBuildSettings.Version = PlayerSettings.bundleVersion;
            EditorPreBuildSettings.VersionCode = PlayerSettings.Android.bundleVersionCode;
            EditorPreBuildSettings.KeyaliasPass = PlayerSettings.Android.keyaliasPass;
            EditorPreBuildSettings.KeystorePass = PlayerSettings.Android.keystorePass;
            EditorPreBuildSettings.PathKeystore = PlayerSettings.Android.keystoreName;
            EditorPreBuildSettings.KeyaliasName = PlayerSettings.Android.keyaliasName;
#endif
        }

        public static void OnInspectorGUI()
        {
#if UNITY_ANDROID
            var buildSetting = Resources.Load<EditorPreBuildSettings>(nameof(EditorPreBuildSettings));
            if (buildSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create PreBuild Setting", GUILayout.Height(40f)))
                {
                    var setting = ScriptableObject.CreateInstance<EditorPreBuildSettings>();
                    const string path = "Assets/_Root/Editor/Resources";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    AssetDatabase.CreateAsset(setting, $"{path}/{nameof(EditorPreBuildSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(EditorPreBuildSettings).TextColor("#52D5F2")} was created ad {path}/{nameof(EditorPreBuildSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (!Wizard.buildFetchSettingFlag)
                {
                    SyncSetting();
                    Wizard.buildFetchSettingFlag = true;
                }

                var editor = UnityEditor.Editor.CreateEditor(buildSetting);
                editor.OnInspectorGUI();
                EditorGUILayout.Space();

                GUILayout.FlexibleSpace();
                if (isHasBuildResult)
                {
                    GUILayout.Label($"Pass: {countPass} / {EditorPreBuildSettings.DefaultConditions.Count + EditorPreBuildSettings.ExtendConditions.Count}");

                    for (var i = 0; i < allStatus.Count; i++)
                    {
                        bool status = allStatus[i];
                        EditorGUILayout.BeginHorizontal();
                        var label = $"{keys[i].Replace("[editor]-prebuild-", "")} ";
                        GUILayout.Label(label);
                        var lastRect = GUILayoutUtility.GetLastRect();
                        var iconRect = new Rect(lastRect.x + label.Length * 6f, lastRect.y, 10, lastRect.height);
                        if (status)
                        {
                            GUI.Label(iconRect, Uniform.IconContent("winbtn_mac_max"), Uniform.InstalledIcon);
                        }
                        else
                        {
                            GUI.Label(iconRect, Uniform.IconContent("winbtn_mac_close"), Uniform.InstalledIcon);
                        }

                        EditorGUILayout.EndHorizontal();
                        if (!string.IsNullOrEmpty(reasons[i]))
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.HelpBox(reasons[i], MessageType.Error);
                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.Space();
                    }
                }

                GUI.backgroundColor = Uniform.Green;
                if (GUILayout.Button("Build", GUILayout.Height(40f)))
                {
                    isHasBuildResult = false;
                    keys = new List<string>();
                    reasons = new List<string>();
                    allStatus = new List<bool>();
                    countPass = 0;
                    int totalTest = EditorPreBuildSettings.DefaultConditions.Count + EditorPreBuildSettings.ExtendConditions.Count;

                    for (int i = 0; i < EditorPreBuildSettings.DefaultConditions.Count; i++)
                    {
                        var defaultCondition = EditorPreBuildSettings.DefaultConditions[i];
                        var result = defaultCondition.Validate();
                        if (result.Item1)
                        {
                            countPass++;
                        }

                        allStatus.Add(result.Item1);
                        keys.Add(defaultCondition.name);
                        reasons.Add(result.Item2);
                    }

                    for (var i = 0; i < EditorPreBuildSettings.ExtendConditions.Count; i++)
                    {
                        var extend = EditorPreBuildSettings.ExtendConditions[i];
                        var result = extend.Validate();
                        if (result.Item1)
                        {
                            countPass++;
                        }

                        keys.Add(extend.name);
                        reasons.Add(result.Item2);
                    }

                    isHasBuildResult = true;

                    if (countPass < totalTest)
                    {
                        EditorUtility.DisplayDialog("Build Report",
                            $"{totalTest - countPass}/{totalTest} of the pre-build tests didn't pass!. Please fix them then build again",
                            "Ok");
                    }
                    else
                    {
                        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
                        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                        {
                            scenePaths[i] = EditorBuildSettings.scenes[i].path;
                        }

                        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
                        buildPlayerOptions.scenes = scenePaths;
                        string nameFile = EditorPreBuildSettings.ProductName.Replace(' ', '_');
                        if (!EditorPreBuildSettings.AppBundle)
                        {
                            buildPlayerOptions.locationPathName = $"{EditorPreBuildSettings.OutputFolder}/{nameFile}.apk";
                        }
                        else
                        {
                            buildPlayerOptions.locationPathName = $"{EditorPreBuildSettings.OutputFolder}/.{nameFile}aab";
                        }

                        buildPlayerOptions.target = BuildTarget.Android;
                        switch (EditorPreBuildSettings.Architecture)
                        {
                            case EditorPreBuildSettings.EAndroidArchitecture.ARMv7:
                                PlayerSettings.SetArchitecture(BuildTargetGroup.Android, (int) AndroidArchitecture.ARMv7);
                                break;
                            case EditorPreBuildSettings.EAndroidArchitecture.ARM64:
                                PlayerSettings.SetArchitecture(BuildTargetGroup.Android, (int) AndroidArchitecture.ARM64);
                                break;
                            case EditorPreBuildSettings.EAndroidArchitecture.ARMv7_ARM64:
                                const AndroidArchitecture androidArchitecture = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                                PlayerSettings.SetArchitecture(BuildTargetGroup.Android, (int) androidArchitecture);
                                break;
                        }

                        switch (EditorPreBuildSettings.ScriptingBackend)
                        {
                            case EditorPreBuildSettings.EScriptingBackend.Mono:
                                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
                                break;
                            case EditorPreBuildSettings.EScriptingBackend.IL2CPP:
                                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                                break;
                        }

                        EditorUserBuildSettings.buildAppBundle = EditorPreBuildSettings.AppBundle;
                        PlayerSettings.companyName = EditorPreBuildSettings.CompanyName;
                        PlayerSettings.productName = EditorPreBuildSettings.ProductName;
                        PlayerSettings.applicationIdentifier = EditorPreBuildSettings.PackageName;
                        PlayerSettings.bundleVersion = EditorPreBuildSettings.Version;
                        PlayerSettings.Android.bundleVersionCode = EditorPreBuildSettings.VersionCode;
                        PlayerSettings.Android.keyaliasPass = EditorPreBuildSettings.KeyaliasPass;
                        PlayerSettings.Android.keystorePass = EditorPreBuildSettings.KeystorePass;
                        PlayerSettings.Android.keystoreName = EditorPreBuildSettings.PathKeystore;
                        PlayerSettings.Android.keyaliasName = EditorPreBuildSettings.KeyaliasName;
                        buildPlayerOptions.options = BuildOptions.None;

                        BuildPipeline.BuildPlayer(buildPlayerOptions);
                    }
                }

                GUI.backgroundColor = Color.white;
            }
#endif
        }
    }
}