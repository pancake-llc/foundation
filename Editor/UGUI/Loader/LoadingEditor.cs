#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Pancake.Linq;
using Pancake.Loader;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [CustomEditor(typeof(Loading))]
    public class LoadingEditor : UnityEditor.Editor
    {
        private Loading _loading;
        private List<LoadingComponent> _loadingScenePrefab = new List<LoadingComponent>();
        private LoadingComponent _selectedLoading;
        private int _selectedLoadingIndex;

        private void OnEnable() { Load(); }

        private void Load()
        {
            _loading = (Loading) target;
            _loadingScenePrefab = InEditor.FindAllAssets<GameObject>().Map(_=>_.GetComponent<LoadingComponent>());
            _loadingScenePrefab.RemoveAll(_ => _ == null);
        }

        public override void OnInspectorGUI()
        {
            if (_loading == null || _loadingScenePrefab.Count == 0) Load();

            var dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            var prefabLoadingName = serializedObject.FindProperty("prefabLoading");
            var unityEventOnBegin = serializedObject.FindProperty("onBeginEvents");
            var unityEventOnFinish = serializedObject.FindProperty("onFinishEvents");

            if (_loadingScenePrefab.Count >= 1)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Selected Template"), GUILayout.Width(120));
                _selectedLoadingIndex = EditorGUILayout.Popup(_selectedLoadingIndex, _loadingScenePrefab.Select(_=>_.name).ToArray());
                prefabLoadingName.objectReferenceValue = _loadingScenePrefab[_selectedLoadingIndex];
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

            else EditorGUILayout.HelpBox("Did not find any prefab loading in the project!", MessageType.Warning);

            GUILayout.Space(6);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif