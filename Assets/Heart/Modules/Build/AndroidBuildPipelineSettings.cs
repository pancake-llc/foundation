using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PancakeEditor
{
    [EditorIcon("so_dark_setting")]
    [Serializable]
    public class AndroidBuildPipelineSettings : ScriptableSettings<AndroidBuildPipelineSettings>
    {
        public enum Environment
        {
            Development,
            Production
        }

        public enum CompressOption
        {
            [InspectorName("LZ4")] LZ4,
            [InspectorName("LZ4HC")] LZ4HC
        }

        public enum BuildType
        {
            [InspectorName("APK")] APK,
            [InspectorName("AAB")] AAB
        }

        public enum BuildSystem
        {
            Gradle
        }

        public enum AndroidAPITarget
        {
            [InspectorName("Android 5   API Level 22")] Android5_1 = 22,
            [InspectorName("Android 6   API Level 23")] Android6 = 23,
            [InspectorName("Android 7   API Level 24")] Android7 = 24,
            [InspectorName("Android 7.1 API Level 25")] Android7_1 = 25,
            [InspectorName("Android 8   API Level 26")] Android8 = 26,
            [InspectorName("Android 8.1 API Level 27")] Android8_1 = 27,
            [InspectorName("Android 9   API Level 28")] Android9 = 28,
            [InspectorName("Android 10  API Level 29")] Android10 = 29,
            [InspectorName("Android 11  API Level 30")] Android11 = 30,
            [InspectorName("Android 12  API Level 31")] Android12 = 31,
            [InspectorName("Android 12L API Level 32")] Android12_1 = 32,
            [InspectorName("Android 13  API Level 33")] Android13 = 33,
            [InspectorName("Android 14  API Level 34")] Android14 = 34,
            [InspectorName("Android 15  API Level 35")] Android15 = 35,
        }

        public Environment environment = Environment.Development;
        public CompressOption compressOption = CompressOption.LZ4;
        public BuildType buildType = BuildType.APK;
        public AndroidCreateSymbols createSymbol = AndroidCreateSymbols.Debugging;
        public BuildSystem buildSystem = BuildSystem.Gradle;
        public bool customMainGradle = true;
        public AndroidAPITarget minAPITarget = AndroidAPITarget.Android6;
        public AndroidAPITarget maxAPITarget = AndroidAPITarget.Android14;
        public bool optimizedFramePacing;
        public bool customKeystore = false;
        public string keystorePath;
        public string password = "";
        public string alias = "";
        public string aliasPassword = "";
        public string versionNumber = "1.0.0";
        public int buildNumber = 1;
        public List<string> allVerifyProcesses;
    }

    [CustomEditor(typeof(AndroidBuildPipelineSettings), true)]
    public class EditorAndroidBuildPipelineSettingDrawer : UnityEditor.Editor
    {
        private SerializedProperty _environmentProperty;
        private SerializedProperty _compressOptionProperty;
        private SerializedProperty _buildTypeProperty;
        private SerializedProperty _buildSystemProperty;
        private SerializedProperty _customMainGradleProperty;
        private SerializedProperty _optimizedFramePacingProperty;
        private SerializedProperty _minAPITargetProperty;
        private SerializedProperty _maxAPITargetProperty;
        private SerializedProperty _versionNumberProperty;
        private SerializedProperty _buildNumberProperty;
        private SerializedProperty _createSymbolProperty;
        private SerializedProperty _customKeystoreProperty;
        private SerializedProperty _passwordProperty;
        private SerializedProperty _aliasProperty;
        private SerializedProperty _aliasPasswordProperty;
        private SerializedProperty _keystorePathProperty;
        private SerializedProperty _allVerifyProcessesProperty;

        private void OnEnable()
        {
            _environmentProperty = serializedObject.FindProperty("environment");
            _compressOptionProperty = serializedObject.FindProperty("compressOption");
            _buildTypeProperty = serializedObject.FindProperty("buildType");
            _buildSystemProperty = serializedObject.FindProperty("buildSystem");
            _customMainGradleProperty = serializedObject.FindProperty("customMainGradle");
            _optimizedFramePacingProperty = serializedObject.FindProperty("optimizedFramePacing");
            _minAPITargetProperty = serializedObject.FindProperty("minAPITarget");
            _maxAPITargetProperty = serializedObject.FindProperty("maxAPITarget");
            _versionNumberProperty = serializedObject.FindProperty("versionNumber");
            _buildNumberProperty = serializedObject.FindProperty("buildNumber");
            _createSymbolProperty = serializedObject.FindProperty("createSymbol");
            _customKeystoreProperty = serializedObject.FindProperty("customKeystore");
            _passwordProperty = serializedObject.FindProperty("password");
            _aliasProperty = serializedObject.FindProperty("alias");
            _aliasPasswordProperty = serializedObject.FindProperty("aliasPassword");
            _keystorePathProperty = serializedObject.FindProperty("keystorePath");
            _allVerifyProcessesProperty = serializedObject.FindProperty("allVerifyProcesses");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Space(8);

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                GUILayout.Label("Switch Platform To Android To Use This Pipeline Setting".SetColor(Uniform.Notice).SetSize(18), Uniform.CenterRichLabel);
                GUILayout.Space(8);
                if (GUILayout.Button("Switch Android", GUILayout.MaxHeight(30f)))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }

                return;
            }

            EditorGUI.BeginChangeCheck();

            var color = _environmentProperty.intValue switch
            {
                (int) AndroidBuildPipelineSettings.Environment.Production => Uniform.Emerald_500,
                (int) AndroidBuildPipelineSettings.Environment.Development => Uniform.Yellow_500,
                _ => Color.white
            };
            GUILayout.Label(("BUILD ".SetColor(Color.white) +
                             ((AndroidBuildPipelineSettings.Environment) _environmentProperty.enumValueIndex).ToString().ToUpper().SetColor(color)).SetSize(25),
                Uniform.CenterRichLabel);
            GUILayout.Space(4);
            EditorGUILayout.PropertyField(_environmentProperty);
            GUILayout.Space(4);
            EditorGUILayout.PropertyField(_compressOptionProperty);

            EditorGUILayout.PropertyField(_buildTypeProperty);
            if (_environmentProperty.enumValueIndex == (int) AndroidBuildPipelineSettings.Environment.Production &&
                _buildTypeProperty.enumValueIndex == (int) AndroidBuildPipelineSettings.BuildType.AAB)
            {
                EditorGUILayout.PropertyField(_createSymbolProperty);
            }

            EditorGUILayout.PropertyField(_buildSystemProperty);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_customMainGradleProperty);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(_optimizedFramePacingProperty);
            EditorGUILayout.PropertyField(_minAPITargetProperty);
            EditorGUILayout.PropertyField(_maxAPITargetProperty);

            GUILayout.Space(4);
            EditorGUILayout.HelpBox("Semantic Version : <major>.<minor>.<patch>" + "\nMajor increases when the API breaks backward compatibility." +
                                    "\nMinor increases when the API adds new features without breaking backward compatibility." +
                                    "\nPatch increases when the API changes small things like fixing bugs or refactoring.",
                MessageType.Info);
            EditorGUILayout.PropertyField(_versionNumberProperty);
            EditorGUILayout.PropertyField(_buildNumberProperty);

            GUILayout.Space(4);

            if (_environmentProperty.enumValueIndex == (int) AndroidBuildPipelineSettings.Environment.Development)
            {
                EditorGUILayout.PropertyField(_customKeystoreProperty);
            }

            if (_customKeystoreProperty.boolValue || _environmentProperty.enumValueIndex == (int) AndroidBuildPipelineSettings.Environment.Production)
            {
                EditorGUILayout.BeginHorizontal();
                var buttonContent = new GUIContent(EditorGUIUtility.IconContent("TextAsset Icon").image, "Select File Keystore");
                var buttonStyle = new GUIStyle(GUI.skin.button) {margin = new RectOffset(2, 2, 0, 0)};
                if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Height(20f), GUILayout.MaxWidth(40)))
                {
                    _keystorePathProperty.stringValue = EditorUtility.OpenFilePanel("Select Keystore", "Assets", "keystore");

                    string rootFolder = Application.dataPath.Replace("/Assets", "");

                    if (_keystorePathProperty.stringValue.Contains(rootFolder))
                        _keystorePathProperty.stringValue = _keystorePathProperty.stringValue.Replace(rootFolder, "");
                }

                EditorGUILayout.LabelField(_keystorePathProperty.stringValue, GUI.skin.textField);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(_passwordProperty);
                EditorGUILayout.PropertyField(_aliasProperty);
                EditorGUILayout.PropertyField(_aliasPasswordProperty);
            }

            GUILayout.Space(4);

            EditorGUILayout.HelpBox("Since Android Gradle Plugin 7.0 always uses R8 so don't need turn on R8", MessageType.Info);

            GUILayout.Space(4);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_allVerifyProcessesProperty);
            GUI.enabled = true;
            if (EditorGUI.EndChangeCheck()) SessionState.SetBool("build_verify", false);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            bool status = SessionState.GetBool("build_verify", false);
            var content = "Verify";
            var previousColor = GUI.color;
            if (status)
            {
                GUI.color = Uniform.Green_500;
                content = "Verify Success";
            }

            if (GUILayout.Button(content, GUILayout.Height(30)))
            {
                SessionState.SetBool("build_verify", false);
                var type = typeof(IVerifyBuildProcess);
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => type.IsAssignableFrom(p) && type != p).ToList();

                _allVerifyProcessesProperty.arraySize = types.Count;
                bool check = false;
                for (var index = 0; index < types.Count; index++)
                {
                    _allVerifyProcessesProperty.GetArrayElementAtIndex(index).stringValue = types[index].Name;
                    var instance = (IVerifyBuildProcess) Activator.CreateInstance(types[index]);
                    check = instance.OnVerify();
                    instance.OnComplete(check);
                    if (!check) break;
                }

                if (check) Debug.Log("ALL VERIFY SUCCESS! YOU CAN BUILD NOW".SetColor(Uniform.Success).ToBold());

                SessionState.SetBool("build_verify", check);
            }

            GUI.enabled = status;
            if (status) GUI.color = Uniform.Green_500;
            GUILayout.Label(" =====> ", GUILayout.Width(52), GUILayout.Height(30));
            GUI.color = previousColor;
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("BUILD", GUILayout.Height(30)))
            {
                PlayerSettings.Android.optimizedFramePacing = _optimizedFramePacingProperty.boolValue;
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions) _minAPITargetProperty.intValue;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions) _maxAPITargetProperty.intValue;
                PlayerSettings.Android.minifyRelease = false;
                PlayerSettings.Android.minifyDebug = false;
                PlayerSettings.Android.useCustomKeystore = _customKeystoreProperty.boolValue;

                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                if (_customKeystoreProperty.boolValue || _environmentProperty.intValue == (int) AndroidBuildPipelineSettings.Environment.Production)
                {
                    string keystorePath = _keystorePathProperty.stringValue;
                    if (keystorePath.StartsWith("/")) keystorePath = Path.Combine(Application.dataPath, $"..{keystorePath}");

                    PlayerSettings.Android.keystoreName = keystorePath;
                    PlayerSettings.Android.keyaliasName = _aliasProperty.stringValue;
                    PlayerSettings.Android.keyaliasPass = _aliasPasswordProperty.stringValue;
                    PlayerSettings.Android.keystorePass = _passwordProperty.stringValue;
                }

                AssetDatabase.Refresh();
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                EditorUserBuildSettings.buildAppBundle = _buildTypeProperty.intValue == (int) AndroidBuildPipelineSettings.BuildType.AAB;
                PlayerSettings.Android.bundleVersionCode = _buildNumberProperty.intValue;
                PlayerSettings.bundleVersion = _versionNumberProperty.stringValue;

                string[] scenes = EditorBuildSettings.scenes.Where(x => x.enabled).Select(scene => scene.path).ToArray();
                string path = GetFilePath(((AndroidBuildPipelineSettings.Environment) _environmentProperty.intValue).ToString().ToLower(),
                    _buildTypeProperty.intValue == (int) AndroidBuildPipelineSettings.BuildType.APK);
                var buildOptions = _compressOptionProperty.intValue == (int) AndroidBuildPipelineSettings.CompressOption.LZ4
                    ? BuildOptions.CompressWithLz4
                    : BuildOptions.CompressWithLz4HC;
                var report = BuildPipeline.BuildPlayer(scenes, path, BuildTarget.Android, buildOptions);
                Process.Start(Path.GetDirectoryName(report.summary.outputPath) ?? Application.dataPath);
                GUIUtility.ExitGUI();
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            serializedObject.ApplyModifiedProperties();
        }

        private static string GetFilePath(string prefix, bool buildApk)
        {
            string outputPath = GetCommandLineArgs("-outputPath");
            string productName = PlayerSettings.productName;
            string gameName = GetValidFileName(productName);
            string extension = buildApk ? ".apk" : ".aab";
            if (outputPath != null)
                return
                    $"{outputPath}/{gameName}_{prefix}_v{PlayerSettings.bundleVersion}_code{PlayerSettings.Android.bundleVersionCode}_{DateTime.Now:ddMM_hhmmtt}{extension}";
            return Path.Combine(Application.dataPath,
                $"../Builds/{gameName}_{prefix}_v{PlayerSettings.bundleVersion}_code{PlayerSettings.Android.bundleVersionCode}_{DateTime.Now:ddMM_hhmmtt}{extension}");
        }

        private static string GetCommandLineArgs(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1) return args[i + 1];
            }

            return null;
        }

        private static string GetValidFileName(string fileName)
        {
            foreach (char @char in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(@char, '_');
            }

            return fileName;
        }
    }
}