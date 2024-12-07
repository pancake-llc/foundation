using Pancake.Common;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using Editor = UnityEditor.Editor;

namespace PancakeEditor
{
    [CustomEditor(typeof(DefaultNavigatorSetting), true)]
    public class DefaultNavigatorSettingDrawer : Editor
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
        private SerializedProperty _popupBackdropStrategyProperty;
        private SerializedProperty _popupBackdropPrefabProperty;
        private SerializedProperty _assetLoaderProperty;
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
            _popupBackdropStrategyProperty = serializedObject.FindProperty("popupBackdropStrategy");
            _popupBackdropPrefabProperty = serializedObject.FindProperty("popupBackdropPrefab");
            _assetLoaderProperty = serializedObject.FindProperty("assetLoader");
            _enableInteractionInTransitionProperty = serializedObject.FindProperty("enableInteractionInTransition");
            _controlInteractionAllContainerProperty = serializedObject.FindProperty("controlInteractionAllContainer");
            _callCleanupWhenDestroyProperty = serializedObject.FindProperty("callCleanupWhenDestroy");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Label("[Sheet]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            Draw(ref _sheetEnterAnimProperty, "Enter Anim");
            Draw(ref _sheetExitAnimProperty, "Exit Anim");
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Page]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            Draw(ref _pagePushEnterAnimProperty, "Push Enter Anim");
            Draw(ref _pagePushExitAnimProperty, "Push Exit Anim");
            EditorGUILayout.Space();
            Draw(ref _pagePopEnterAnimProperty, "Pop Enter Anim");
            Draw(ref _pagePopExitAnimProperty, "Pop Exit Anim");
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Popup]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            Draw(ref _popupEnterAnimProperty, "Enter Anim");
            Draw(ref _popupExitAnimProperty, "Exit Anim");
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Backdrop]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_popupBackdropStrategyProperty, new GUIContent("Strategy"));
            EditorGUILayout.PropertyField(_popupBackdropPrefabProperty, new GUIContent("Prefab"));
            EditorGUILayout.Space();
            Draw(ref _popupBackdropEnterAnimProperty, "Enter Anim");
            Draw(ref _popupBackdropExitAnimProperty, "Exit Anim");
            EditorGUI.indentLevel--;
            GUILayout.Space(4);

            GUILayout.Label("[Loader]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_assetLoaderProperty, new GUIContent("Asset Loader"));
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

        private void Draw(ref SerializedProperty property, string name)
        {
            if (property.objectReferenceValue == null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property, new GUIContent(name));
                if (GUILayout.Button("Create", GUILayout.Width(70)))
                {
                    string newName = GetFieldName(property).ToSnakeCase();
                    property.objectReferenceValue = EditorCreator.CreateScriptableAt(typeof(SimpleUITransitionAnimationSO),
                        newName,
                        ProjectDatabase.DEFAULT_PATH_SCRIPTABLE_ASSET_GENERATED,
                        true);
                }

                GUILayout.EndHorizontal();
            }
            else EditorGUILayout.PropertyField(property, new GUIContent(name));
        }

        private string GetFieldName(SerializedProperty property)
        {
            string[] pathParts = property.propertyPath.Split('.');
            return pathParts[^1];
        }
    }
}