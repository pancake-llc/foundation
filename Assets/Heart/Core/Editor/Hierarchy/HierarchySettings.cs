using Pancake;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [EditorIcon("so_dark_setting")]
    public class HierarchySettings : ScriptableSettings<HierarchySettings>
    {
        [Header("Hierarchy")] [SerializeField] private HierarchyObjectMode hierarchyObjectMode = HierarchyObjectMode.RemoveInBuild;
        [SerializeField] private bool showHierarchyToggles;
        [SerializeField] private bool showComponentIcons;
        [SerializeField] private bool showTreeMap;
        [SerializeField] private Color treeMapColor = new(0.53f, 0.53f, 0.53f, 0.45f);
        [SerializeField] private bool showSeparator;
        [SerializeField] private bool showRowShading;
        [SerializeField] private Color separatorColor = new(0.19f, 0.19f, 0.19f, 0f);
        [SerializeField] private Color evenRowColor = new(0f, 0f, 0f, 0.07f);
        [SerializeField] private Color oddRowColor = Color.clear;

        public static HierarchyObjectMode HierarchyObjectMode => Instance.hierarchyObjectMode;
        public static bool ShowHierarchyToggles => Instance.showHierarchyToggles;
        public static bool ShowComponentIcons => Instance.showComponentIcons;
        public static bool ShowTreeMap => Instance.showTreeMap;
        public static Color TreeMapColor => Instance.treeMapColor;
        public static bool ShowSeparator => Instance.showSeparator;
        public static bool ShowRowShading => Instance.showRowShading;
        public static Color SeparatorColor => Instance.separatorColor;
        public static Color EvenRowColor => Instance.evenRowColor;
        public static Color OddRowColor => Instance.oddRowColor;
    }


    [CustomEditor(typeof(HierarchySettings), true)]
    public class HierarchySettingDrawer : UnityEditor.Editor
    {
        private SerializedProperty _hierarchyObjectModeProperty;
        private SerializedProperty _showHierarchyTogglesProperty;
        private SerializedProperty _showComponentIconsProperty;
        private SerializedProperty _showTreeMapProperty;
        private SerializedProperty _treeMapColorProperty;
        private SerializedProperty _showSeparatorProperty;
        private SerializedProperty _showRowShadingProperty;
        private SerializedProperty _separatorColorProperty;
        private SerializedProperty _evenRowColorProperty;
        private SerializedProperty _oddRowColorProperty;

        private void OnEnable()
        {
            _hierarchyObjectModeProperty = serializedObject.FindProperty("hierarchyObjectMode");
            _showHierarchyTogglesProperty = serializedObject.FindProperty("showHierarchyToggles");
            _showComponentIconsProperty = serializedObject.FindProperty("showComponentIcons");
            _showTreeMapProperty = serializedObject.FindProperty("showTreeMap");
            _treeMapColorProperty = serializedObject.FindProperty("treeMapColor");
            _showSeparatorProperty = serializedObject.FindProperty("showSeparator");
            _showRowShadingProperty = serializedObject.FindProperty("showRowShading");
            _separatorColorProperty = serializedObject.FindProperty("separatorColor");
            _evenRowColorProperty = serializedObject.FindProperty("evenRowColor");
            _oddRowColorProperty = serializedObject.FindProperty("oddRowColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_hierarchyObjectModeProperty);
            EditorGUILayout.PropertyField(_showHierarchyTogglesProperty);
            EditorGUILayout.PropertyField(_showComponentIconsProperty);
            EditorGUILayout.PropertyField(_showTreeMapProperty);
            if (_showTreeMapProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_treeMapColorProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_showSeparatorProperty);
            if (_showSeparatorProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_separatorColorProperty);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_showRowShadingProperty);
            if (_showRowShadingProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_evenRowColorProperty);
                EditorGUILayout.PropertyField(_oddRowColorProperty);
                EditorGUI.indentLevel--;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}