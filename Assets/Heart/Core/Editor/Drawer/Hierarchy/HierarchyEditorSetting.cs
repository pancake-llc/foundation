using System;
using System.Collections.Generic;
using Pancake;
using Pancake.Apex;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    [EditorIcon("scriptable_editor_setting")]
    public class HierarchyEditorSetting : ScriptableSettings<HierarchyEditorSetting>
    {
        [Header("TREE MAP")] [SerializeField] private bool enabledTreeMap = true;
        [SerializeField] private EditorThemeColor treeMapColor = new() {dark = new Color(1f, 1f, 1f, 0.22f), light = new Color(0.36f, 0.36f, 0.36f, 0.56f)};
        [SerializeField] private bool treeMapEnhanced = true;
        [SerializeField] private bool treeMapTransparentBackground = true;

        [Header("SEPARATOR")] [SerializeField] private bool enabledSeparator = true;
        [SerializeField] private bool showRowShading = true;
        [SerializeField] private EditorThemeColor separatorColor = new() {dark = new Color(0.19f, 0.19f, 0.19f, 0f), light = new Color(0.4f, 0.4f, 0.4f, 0.28f)};
        [SerializeField] private EditorThemeColor evenRowColor = new() {dark = new Color(0f, 0f, 0f, 0.07f), light = new Color(1f, 1f, 1f, 0.03f)};
        [SerializeField] private EditorThemeColor oddRowColor = new() {dark = Color.clear, light = new Color(1f, 1f, 1f, 0f)};

        [Header("VISIBILITY")] [SerializeField] private bool enabledVisibility = true;
        [SerializeField] private bool visibilityShowDuringPlayMode = true;

        [Header("ERROR")] [SerializeField] private bool enabledError = true;
        [SerializeField] private bool showIconOnParent = true;
        [SerializeField] private bool showScriptMissing = true;
        [SerializeField] private bool showReferenceNull;
        [SerializeField] private bool showReferenceIsMissing = true;
        [SerializeField] private bool showMissingEventMethod = true;
        [SerializeField] private bool showWhenTagOrLayerIsUndefined = true;
        [SerializeField] private bool showForDisabledComponents = true;
        [SerializeField] private bool showForDisabledGameObjects = true;

        [Header("OTHER")] [SerializeField, Range(0f, 500f)] private float additionalIndent;

        [SerializeField] private EditorThemeColor additionalBackgroundColor =
            new() {dark = new Color(0.22f, 0.22f, 0.22f, 0f), light = new Color(0.81f, 0.81f, 0.81f, 0f)};

        [SerializeField] private EditorThemeColor additionalActiveColor = new() {dark = new Color(1f, 1f, 0.5f), light = new Color(0.21f, 0.21f, 0.21f, 0.81f)};
        [SerializeField] private EditorThemeColor additionalInactiveColor = new() {dark = new Color(0.31f, 0.31f, 0.31f), light = new Color(0f, 0f, 0f, 0.12f)};
        [SerializeField] private bool additionalHideIconIfNotFit;
        [SerializeField] private bool additionalShowModifierWarning = true;

        public static bool EnabledTreeMap { get => Instance.enabledTreeMap; set => Instance.enabledTreeMap = value; }
        public static EditorThemeColor TreeMapColor { get => Instance.treeMapColor; set => Instance.treeMapColor = value; }
        public static bool TreeMapEnhanced { get => Instance.treeMapEnhanced; set => Instance.treeMapEnhanced = value; }
        public static bool TreeMapTransparentBackground { get => Instance.treeMapTransparentBackground; set => Instance.treeMapTransparentBackground = value; }
        public static bool EnabledSeparator { get => Instance.enabledSeparator; set => Instance.enabledSeparator = value; }
        public static bool ShowRowShading { get => Instance.showRowShading; set => Instance.showRowShading = value; }
        public static EditorThemeColor SeperatorColor { get => Instance.separatorColor; set => Instance.separatorColor = value; }
        public static EditorThemeColor EvenRowColor { get => Instance.evenRowColor; set => Instance.evenRowColor = value; }
        public static EditorThemeColor OddRowColor { get => Instance.oddRowColor; set => Instance.oddRowColor = value; }
        public static bool EnabledVisibility { get => Instance.enabledVisibility; set => Instance.enabledVisibility = value; }
        public static bool VisibilityShowDuringPlayMode { get => Instance.visibilityShowDuringPlayMode; set => Instance.visibilityShowDuringPlayMode = value; }

        public static bool EnabledError { get => Instance.enabledError; set => Instance.enabledError = value; }
        public static bool ShowIconOnParent { get => Instance.showIconOnParent; set => Instance.showIconOnParent = value; }
        public static bool ShowScriptMissing { get => Instance.showScriptMissing; set => Instance.showScriptMissing = value; }
        public static bool ShowReferenceNull { get => Instance.showReferenceNull; set => Instance.showReferenceNull = value; }
        public static bool ShowReferenceIsMissing { get => Instance.showReferenceIsMissing; set => Instance.showReferenceIsMissing = value; }
        public static bool ShowMissingEventMethod { get => Instance.showMissingEventMethod; set => Instance.showMissingEventMethod = value; }
        public static bool ShowWhenTagOrLayerIsUndefined { get => Instance.showWhenTagOrLayerIsUndefined; set => Instance.showWhenTagOrLayerIsUndefined = value; }
        public static bool ShowForDisabledComponents { get => Instance.showForDisabledComponents; set => Instance.showForDisabledComponents = value; }
        public static bool ShowForDisabledGameObjects { get => Instance.showForDisabledGameObjects; set => Instance.showForDisabledGameObjects = value; }

        public static float AdditionalIndent { get => Instance.additionalIndent; set => Instance.additionalIndent = value; }
        public static bool AdditionalHideIconIfNotFit { get => Instance.additionalHideIconIfNotFit; set => Instance.additionalHideIconIfNotFit = value; }
        public static bool AdditionalShowModifierWarning { get => Instance.additionalShowModifierWarning; set => Instance.additionalShowModifierWarning = value; }
        public static EditorThemeColor AdditionalBackgroundColor { get => Instance.additionalBackgroundColor; set => Instance.additionalBackgroundColor = value; }
        public static EditorThemeColor AdditionalActiveColor { get => Instance.additionalActiveColor; set => Instance.additionalActiveColor = value; }
        public static EditorThemeColor AdditionalInactiveColor { get => Instance.additionalInactiveColor; set => Instance.additionalInactiveColor = value; }
    }
}