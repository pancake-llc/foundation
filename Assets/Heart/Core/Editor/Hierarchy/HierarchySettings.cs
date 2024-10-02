using Pancake;
using PancakeEditor.Common;
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
        public override void OnInspectorGUI()
        {
            string[] propertiesToHide = {"m_Script"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);
        }
    }
}