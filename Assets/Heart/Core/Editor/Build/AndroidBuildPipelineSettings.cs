using System;
using System.Collections.Generic;
using System.Linq;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [EditorIcon("scriptable_editor_setting")]
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

        public Environment environment = Environment.Development;
        public CompressOption compressOption = CompressOption.LZ4;
        public BuildType buildType = BuildType.APK;
        public AndroidCreateSymbols createSymbol = AndroidCreateSymbols.Public;
        public BuildSystem buildSystem = BuildSystem.Gradle;
        public bool customMainGradle = true;
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

            var color = _environmentProperty.intValue switch
            {
                (int) AndroidBuildPipelineSettings.Environment.Production => Uniform.FluorescentBlue,
                (int) AndroidBuildPipelineSettings.Environment.Development => Uniform.Yellow,
                _ => Color.white
            };
            GUILayout.Label(("BUILD ".TextColor(Color.white) +
                             ((AndroidBuildPipelineSettings.Environment) _environmentProperty.enumValueIndex).ToString().ToUpper().TextColor(color)).TextSize(25),
                Uniform.RichCenterLabel);
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
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_allVerifyProcessesProperty);
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            bool status = SessionState.GetBool("build_verify", false);
            var content = "Verify";
            var previousColor = GUI.color;
            if (status)
            {
                GUI.color = Uniform.Green;
                content = "Verify Success";
            }

            if (GUILayout.Button(content, GUILayout.Height(30)))
            {
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

                SessionState.SetBool("build_verify", check);
            }

            GUI.enabled = status;
            if (status) GUI.color = Uniform.Green;
            GUILayout.Label(" =====> ", new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, 5, 0)}, GUILayout.Width(52));
            GUI.color = previousColor;
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button("BUILD", GUILayout.Height(30)))
            {
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            serializedObject.ApplyModifiedProperties();
        }
    }
}