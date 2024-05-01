using System;
using System.Collections.Generic;
using Pancake;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    [EditorIcon("so_dark_setting")]
    public class HierarchyEditorSetting : ScriptableSettings<HierarchyEditorSetting>
    {
        [Header("TREE MAP")] [SerializeField] private bool enabledTreeMap = true;
        [SerializeField] private EditorThemeColor treeMapColor = new() {dark = new Color(1f, 1f, 1f, 0.22f), light = new Color(0.36f, 0.36f, 0.36f, 0.56f)};
        [SerializeField] private bool treeMapEnhanced = true;
        [SerializeField] private bool treeMapTransparentBackground = true;

        [SerializeField] private EditorThemeColor additionalBackgroundColor =
            new() {dark = new Color(0.22f, 0.22f, 0.22f, 0f), light = new Color(0.81f, 0.81f, 0.81f, 0f)};

        [Header("SEPARATOR")] [SerializeField] private bool enabledSeparator = true;
        [SerializeField] private bool showRowShading = true;
        [SerializeField] private EditorThemeColor separatorColor = new() {dark = new Color(0.19f, 0.19f, 0.19f, 0f), light = new Color(0.4f, 0.4f, 0.4f, 0.28f)};
        [SerializeField] private EditorThemeColor evenRowColor = new() {dark = new Color(0f, 0f, 0f, 0.07f), light = new Color(1f, 1f, 1f, 0.03f)};
        [SerializeField] private EditorThemeColor oddRowColor = new() {dark = Color.clear, light = new Color(1f, 1f, 1f, 0f)};

        public static bool EnabledTreeMap { get => Instance.enabledTreeMap; set => Instance.enabledTreeMap = value; }
        public static EditorThemeColor TreeMapColor { get => Instance.treeMapColor; set => Instance.treeMapColor = value; }
        public static bool TreeMapEnhanced { get => Instance.treeMapEnhanced; set => Instance.treeMapEnhanced = value; }
        public static bool TreeMapTransparentBackground { get => Instance.treeMapTransparentBackground; set => Instance.treeMapTransparentBackground = value; }
        public static bool EnabledSeparator { get => Instance.enabledSeparator; set => Instance.enabledSeparator = value; }
        public static bool ShowRowShading { get => Instance.showRowShading; set => Instance.showRowShading = value; }
        public static EditorThemeColor SeperatorColor { get => Instance.separatorColor; set => Instance.separatorColor = value; }
        public static EditorThemeColor EvenRowColor { get => Instance.evenRowColor; set => Instance.evenRowColor = value; }
        public static EditorThemeColor OddRowColor { get => Instance.oddRowColor; set => Instance.oddRowColor = value; }
        public static EditorThemeColor AdditionalBackgroundColor { get => Instance.additionalBackgroundColor; set => Instance.additionalBackgroundColor = value; }
    }
}