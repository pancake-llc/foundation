#if UNITY_EDITOR
using System.Collections.Generic;
using Pancake.Core.Tasks;
using Pancake.Loader;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.LoaderEditor
{
    [CustomEditor(typeof(LoadingScreenManager))]
    public class LoadingScreenManagerEditor : UnityEditor.Editor
    {
        private LoadingScreenManager _loadingScreenManager;
        private readonly List<string> _prefabLoadingNames = new List<string>();
        private int _selectedLoadingIndex = 0;

        private void OnEnable() { Load(); }

        private async void Load()
        {
            _loadingScreenManager = (LoadingScreenManager) target;
            _prefabLoadingNames.Clear();
            var results = await Addressables.LoadResourceLocationsAsync("loader");
            
            for (int i = 0; i < results.Count; i++)
            {
                _prefabLoadingNames.Add(results[i].PrimaryKey);
                if (!string.IsNullOrEmpty(_loadingScreenManager.prefabLoadingName) && _loadingScreenManager.prefabLoadingName.Equals(results[i].PrimaryKey))
                {
                    _selectedLoadingIndex = i;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (_loadingScreenManager == null || _prefabLoadingNames.Count == 0) Load();

            var customSkin = (GUISkin) Resources.Load("loader-dark-skin");

            var dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            var prefabLoadingName = serializedObject.FindProperty("prefabLoadingName");
            var unityEventOnBegin = serializedObject.FindProperty("onBeginEvents");
            var unityEventOnFinish = serializedObject.FindProperty("onFinishEvents");

            if (_prefabLoadingNames.Count == 1 || _prefabLoadingNames.Count >= 1)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Selected Template"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                _selectedLoadingIndex = EditorGUILayout.Popup(_selectedLoadingIndex, _prefabLoadingNames.ToArray());
                prefabLoadingName.stringValue = _prefabLoadingNames[_selectedLoadingIndex];
                GUILayout.EndHorizontal();
                GUILayout.Space(6);
                EditorGUI.indentLevel = 1;
                EditorGUILayout.PropertyField(dontDestroyOnLoad, new GUIContent("Don't Destroy On Load"), true);
                EditorGUI.indentLevel = 0;
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(unityEventOnBegin, new GUIContent("OnBegin"), true);
                GUILayout.Space(4);
                EditorGUILayout.PropertyField(unityEventOnFinish, new GUIContent("OnFinish"), true);
                GUILayout.Space(4);
                GUILayout.EndVertical();
            }

            else EditorGUILayout.HelpBox("There isn't any loading screen prefab in Resources > [Loader] > Prefabs folder!", MessageType.Warning);

            GUILayout.Space(6);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif