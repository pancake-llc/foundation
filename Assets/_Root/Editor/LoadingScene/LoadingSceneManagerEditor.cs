#if UNITY_EDITOR
using System.Collections.Generic;
using Pancake.Core.Tasks;
using Pancake.Loader;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Pancake.Editor
{
    [CustomEditor(typeof(LoadingSceneManager))]
    public class LoadingSceneManagerEditor : UnityEditor.Editor
    {
        private LoadingSceneManager _loadingSceneManager;
        private readonly List<string> _prefabLoadingNames = new List<string>();
        private int _selectedLoadingIndex = 0;

        private void OnEnable() { Load(); }

        private async void Load()
        {
            _loadingSceneManager = (LoadingSceneManager) target;
            _prefabLoadingNames.Clear();
            var results = await Addressables.LoadResourceLocationsAsync(LoadingScene.LABEL);
            
            for (int i = 0; i < results.Count; i++)
            {
                _prefabLoadingNames.Add(results[i].PrimaryKey);
                if (!string.IsNullOrEmpty(_loadingSceneManager.prefabLoadingName) && _loadingSceneManager.prefabLoadingName.Equals(results[i].PrimaryKey))
                {
                    _selectedLoadingIndex = i;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (_loadingSceneManager == null || _prefabLoadingNames.Count == 0) Load();

            var dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            var prefabLoadingName = serializedObject.FindProperty("prefabLoadingName");
            var unityEventOnBegin = serializedObject.FindProperty("onBeginEvents");
            var unityEventOnFinish = serializedObject.FindProperty("onFinishEvents");

            if (_prefabLoadingNames.Count == 1 || _prefabLoadingNames.Count >= 1)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Selected Template"), GUILayout.Width(120));
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

            else EditorGUILayout.HelpBox("There isn't any loading screen prefab Addressable!", MessageType.Warning);

            GUILayout.Space(6);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif