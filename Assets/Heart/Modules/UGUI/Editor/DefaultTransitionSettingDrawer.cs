using System.IO;
using Pancake.Common;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PancakeEditor
{
    [CustomEditor(typeof(DefaultTransitionSetting), true)]
    public class DefaultTransitionSettingDrawer : UnityEditor.Editor
    {
        private SerializedProperty _sheetEnterAnimProperty;
        private SerializedProperty _sheetExitAnimProperty;
        private SerializedProperty _pagePushEnterAnimProperty;
        private SerializedProperty _pagePushExitAnimProperty;
        private SerializedProperty _pagePopEnterAnimProperty;
        private SerializedProperty _pagePopExitAnimProperty;
        private SerializedProperty _popupEnterAnimProperty;
        private SerializedProperty _popupExitAnimProperty;
        private SerializedProperty _popupBackdropEnterAnimProperty;
        private SerializedProperty _popupBackdropExitAnimProperty;
        private SerializedProperty _popupBackdropPrefabProperty;
        private SerializedProperty _enableInteractionInTransitionProperty;
        private SerializedProperty _controlInteractionAllContainerProperty;
        private SerializedProperty _callCleanupWhenDestroyProperty;


        private void OnEnable()
        {
            _sheetEnterAnimProperty = serializedObject.FindProperty("sheetEnterAnim");
            _sheetExitAnimProperty = serializedObject.FindProperty("sheetExitAnim");
            _pagePushEnterAnimProperty = serializedObject.FindProperty("pagePushEnterAnim");
            _pagePushExitAnimProperty = serializedObject.FindProperty("pagePushExitAnim");
            _pagePopEnterAnimProperty = serializedObject.FindProperty("pagePopEnterAnim");
            _pagePopExitAnimProperty = serializedObject.FindProperty("pagePopExitAnim");
            _popupEnterAnimProperty = serializedObject.FindProperty("popupEnterAnim");
            _popupExitAnimProperty = serializedObject.FindProperty("popupExitAnim");
            _popupBackdropEnterAnimProperty = serializedObject.FindProperty("popupBackdropEnterAnim");
            _popupBackdropExitAnimProperty = serializedObject.FindProperty("popupBackdropExitAnim");
            _popupBackdropPrefabProperty = serializedObject.FindProperty("popupBackdropPrefab");
            _enableInteractionInTransitionProperty = serializedObject.FindProperty("enableInteractionInTransition");
            _controlInteractionAllContainerProperty = serializedObject.FindProperty("controlInteractionAllContainer");
            _callCleanupWhenDestroyProperty = serializedObject.FindProperty("callCleanupWhenDestroy");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Label("[Sheet]");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_sheetEnterAnimProperty, new GUIContent("Enter Anim"));
            EditorGUILayout.PropertyField(_sheetExitAnimProperty, new GUIContent("Exit Anim"));
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Page]");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_pagePushEnterAnimProperty, new GUIContent("Push Enter Anim"));
            EditorGUILayout.PropertyField(_pagePushExitAnimProperty, new GUIContent("Push Exit Anim"));
            EditorGUILayout.PropertyField(_pagePopEnterAnimProperty, new GUIContent("Pop Enter Anim"));
            EditorGUILayout.PropertyField(_pagePopExitAnimProperty, new GUIContent("Pop Exit Anim"));
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Popup]");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_popupEnterAnimProperty, new GUIContent("Enter Anim"));
            EditorGUILayout.PropertyField(_popupExitAnimProperty, new GUIContent("Exit Anim"));
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Backdrop]");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_popupBackdropEnterAnimProperty, new GUIContent("Enter Anim"));
            EditorGUILayout.PropertyField(_popupBackdropExitAnimProperty, new GUIContent("Exit Anim"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_popupBackdropPrefabProperty, new GUIContent("Prefab"));
            if (_popupBackdropPrefabProperty.objectReferenceValue == null)
            {
                if (GUILayout.Button("Create", GUILayout.Width(80)))
                {
                    var go = new GameObject("DefaultPopupBackdrop",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image),
                        typeof(PopupBackdrop));
                    go.hideFlags = HideFlags.HideInHierarchy;
                    if (go.TryGetComponent<Image>(out var image))
                    {
                        image.rectTransform.Fill();
                        image.color = Color.black;
                        image.color = image.color.ChangeAlpha(0.75f);
                    }

                    if (!Directory.Exists("Assets/_Root/Prefabs/Popup")) Directory.CreateDirectory("Assets/_Root/Prefabs/Popup");
                    const string path = "Assets/_Root/Prefabs/Popup/DefaultPopupBackdrop.prefab";
                    if (File.Exists(path)) AssetDatabase.DeleteAsset(path);

                    PrefabUtility.SaveAsPrefabAsset(go, path);
                    AssetDatabase.SaveAssets();
                    _popupBackdropPrefabProperty.objectReferenceValue = ProjectDatabase.FindAssetWithPath<GameObject>(path, true);
                    
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            EditorGUILayout.PropertyField(_enableInteractionInTransitionProperty, new GUIContent("Interact In Transition"));
            if (!_enableInteractionInTransitionProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_controlInteractionAllContainerProperty, new GUIContent("Control All Interaction"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_callCleanupWhenDestroyProperty, new GUIContent("Clean When Destroy"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}