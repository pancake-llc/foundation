using System;
using System.Collections.Generic;
using Pancake;
using Pancake.Apex;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public enum TypeSetting
    {
        TreeMapShow = 0,
        TreeMapColor = 77,
        TreeMapEnhanced = 78,
        TreeMapTransparentBackground = 60,

        SeparatorShow = 8,
        SeparatorShowRowShading = 50,
        SeparatorColor = 80,
        SeparatorEvenRowShadingColor = 79,
        SeparatorOddRowShadingColor = 81,

        VisibilityShow = 1,
        VisibilityShowDuringPlayMode = 15,

        LockShow = 2,
        LockShowDuringPlayMode = 16,
        LockPreventSelectionOfLockedObjects = 41,

        ErrorShow = 6,
        ErrorShowDuringPlayMode = 20,
        ErrorShowIconOnParent = 27,
        ErrorShowScriptIsMissing = 28,
        ErrorShowReferenceIsNull = 29,
        ErrorShowReferenceIsMissing = 58,
        ErrorShowStringIsEmpty = 30,
        ErrorShowMissingEventMethod = 31,
        ErrorShowWhenTagOrLayerIsUndefined = 32,
        ErrorIgnoreString = 33,
        ErrorShowForDisabledComponents = 44,
        ErrorShowForDisabledGameObjects = 59,


        AdditionalIdentation = 39,
        AdditionalShowHiddenQHierarchyObjectList = 42,
        AdditionalShowModifierWarning = 43,
        AdditionalShowObjectListContent = 49,
        AdditionalHideIconsIfNotFit = 52,
        AdditionalBackgroundColor = 73,
        AdditionalActiveColor = 74,
        AdditionalInactiveColor = 75,
        AdditionalSpecialColor = 76,
    }

    public enum TypeHierarchyComponent
    {
        Lock = 0,
        Visibility = 1,
        Error = 4,
        Separator = 1000,
        TreeMap = 1001,
    }

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

        [Header("LOCK")] [SerializeField] private bool enabledLock = true;
        [SerializeField] private bool lockShowDuringPlayMode = true;
        [SerializeField] private bool lockPreventSelectionOfLockedObjects;

        [Header("ERROR")] [SerializeField] private bool enabledError = true;
        [SerializeField] private bool errorShowDuringPlayMode;
        [SerializeField] private bool showIconOnParent;
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
        [SerializeField] private EditorThemeColor additionalSpecialColor = new() {dark = new Color(0.17f, 0.66f, 0.79f), light = new Color(0.11f, 0.47f, 0.84f)};
        [SerializeField] private bool additionalHideIconIfNotFit;

        private Dictionary<int, Action> _settingHandlerCollection = new Dictionary<int, Action>();

        public static void AddEventListener(TypeSetting setting, Action action)
        {
            var id = (int) setting;
            Instance._settingHandlerCollection.TryAdd(id, null);

            if (Instance._settingHandlerCollection[id] == null)
            {
                Instance._settingHandlerCollection[id] = action;
            }
            else
            {
                Instance._settingHandlerCollection[id] += action;
            }
        }


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
        public static bool EnabledLock { get => Instance.enabledLock; set => Instance.enabledLock = value; }
        public static bool LockShowDuringPlayMode { get => Instance.lockShowDuringPlayMode; set => Instance.lockShowDuringPlayMode = value; }

        public static bool LockPreventSelectionOfLockedObjects
        {
            get => Instance.lockPreventSelectionOfLockedObjects;
            set => Instance.lockPreventSelectionOfLockedObjects = value;
        }

        public static bool EnabledError { get => Instance.enabledError; set => Instance.enabledError = value; }
        public static bool ErrorShowDuringPlayMode { get => Instance.errorShowDuringPlayMode; set => Instance.errorShowDuringPlayMode = value; }
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
        public static EditorThemeColor AdditionalBackgroundColor { get => Instance.additionalBackgroundColor; set => Instance.additionalBackgroundColor = value; }
        public static EditorThemeColor AdditionalActiveColor { get => Instance.additionalActiveColor; set => Instance.additionalActiveColor = value; }
        public static EditorThemeColor AdditionalInactiveColor { get => Instance.additionalInactiveColor; set => Instance.additionalInactiveColor = value; }
        public static EditorThemeColor AdditionalSpecialColor { get => Instance.additionalSpecialColor; set => Instance.additionalSpecialColor = value; }
    }
}