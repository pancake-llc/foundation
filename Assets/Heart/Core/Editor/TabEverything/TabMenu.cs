using UnityEngine;
using UnityEditor;

namespace PancakeEditor
{
    internal static class TabMenu
    {
        public static bool DragndropEnabled
        {
            get => EditorPrefs.GetBool("tabs_dragndrop_enabled", true);
            private set => EditorPrefs.SetBool("tabs_dragndrop_enabled", value);
        }

        public static bool SwitchTabsEnabled
        {
            get => EditorPrefs.GetBool("tabs_switchtab_enabled", true);
            private set => EditorPrefs.SetBool("tabs_switchtab_enabled", value);
        }

        public static bool MoveTabsEnabled
        {
            get => EditorPrefs.GetBool("tabs_movetab_enabled", true);
            private set => EditorPrefs.SetBool("tabs_movetab_enabled", value);
        }

        public static bool AddTabEnabled { get => EditorPrefs.GetBool("tabs_addtab_enabled", true); private set => EditorPrefs.SetBool("tabs_addtab_enabled", value); }

        public static bool CloseTabEnabled
        {
            get => EditorPrefs.GetBool("tabs_closetab_enabled", true);
            private set => EditorPrefs.SetBool("tabs_closetab_enabled", value);
        }

        public static bool ReopenTabEnabled
        {
            get => EditorPrefs.GetBool("tabs_reopentab_enabled", true);
            private set => EditorPrefs.SetBool("tabs_reopentab_enabled", value);
        }

        public static bool SideScrollEnabled
        {
            get => EditorPrefs.GetBool("tabs_sidescroll_enabled", Application.platform == RuntimePlatform.OSXEditor);
            private set => EditorPrefs.SetBool("tabs_sidescroll_enabled", value);
        }

        public static bool ReverseScrollDirectionEnabled
        {
            get => EditorPrefs.GetBool("tabs_reversescroll_direction_enabled", false);
            private set => EditorPrefs.SetBool("tabs_reversescroll_direction_enabled", value);
        }

        public static float SidescrollSensitivity
        {
            get => EditorPrefs.GetFloat("tabs_sidescroll_sensitivity", 1);
            private set => EditorPrefs.SetFloat("tabs_sidescroll_sensitivity", value);
        }

        public static bool PluginDisabled { get => EditorPrefs.GetBool("tabs_disabled", false); private set => EditorPrefs.SetBool("tabs_disabled", value); }

        private const string DIR = "Tools/Tab Everything/";
#if UNITY_EDITOR_OSX
        private const string CMD = "Cmd";
#else
        private const string CMD = "Ctrl";
#endif

        private const string DRAGNDROP = DIR + "Create tabs with Drag-and-Drop";
        private const string SWITCH_TABS = DIR + "Switch tabs with Shift-Scroll";
        private const string MOVE_TABS = DIR + "Move tabs with " + CMD + "-Shift-Scroll";

        private const string ADD_TAB = DIR + CMD + "-T to add tab";
        private const string CLOSE_TAB = DIR + CMD + "-W to close tab";
        private const string REOPEN_TAB = DIR + CMD + "-Shift-T to reopen closed tab";

        private const string SIDESCROLL = DIR + "Horizontal Scroll as Shift-Scroll";
        private const string REVERSE_SCROLL_DIRECTION = DIR + "Reverse direction";
        private const string INCREASE_SENSITIVITY = DIR + "Increase sensitivity";
        private const string DECREASE_SENSITIVITY = DIR + "Decrease sensitivity";

        private const string DISABLE_TAB_EVERYTHING = DIR + "Disable Tab Everything";


        [MenuItem(DIR + "Features", false, 1)]
        private static void HeaderFeature() { }

        [MenuItem(DIR + "Features", true, 1)]
        private static bool HeaderFeatureValidate() => false;

        [MenuItem(DRAGNDROP, false, 2)]
        private static void CreateTab() => DragndropEnabled = !DragndropEnabled;

        [MenuItem(DRAGNDROP, true, 2)]
        private static bool CreateTabValidate()
        {
            Menu.SetChecked(DRAGNDROP, DragndropEnabled);
            return !PluginDisabled;
        }

        [MenuItem(SWITCH_TABS, false, 3)]
        private static void SwitchTab() => SwitchTabsEnabled = !SwitchTabsEnabled;

        [MenuItem(SWITCH_TABS, true, 3)]
        private static bool SwitchTabValidate()
        {
            Menu.SetChecked(SWITCH_TABS, SwitchTabsEnabled);
            return !PluginDisabled;
        }

        [MenuItem(MOVE_TABS, false, 4)]
        private static void MoveTab() => MoveTabsEnabled = !MoveTabsEnabled;

        [MenuItem(MOVE_TABS, true, 4)]
        private static bool MoveTabValidate()
        {
            Menu.SetChecked(MOVE_TABS, MoveTabsEnabled);
            return !PluginDisabled;
        }


        [MenuItem(DIR + "Shortcuts", false, 101)]
        private static void HeaderShortcut() { }

        [MenuItem(DIR + "Shortcuts", true, 101)]
        private static bool HeaderShortcutValidate() => false;

        [MenuItem(ADD_TAB, false, 102)]
        private static void ShortcutAddTab() => AddTabEnabled = !AddTabEnabled;

        [MenuItem(ADD_TAB, true, 102)]
        private static bool ShortcutAddTabValidate()
        {
            Menu.SetChecked(ADD_TAB, AddTabEnabled);
            return !PluginDisabled;
        }

        [MenuItem(CLOSE_TAB, false, 103)]
        private static void ShortcutCloseTab() => CloseTabEnabled = !CloseTabEnabled;

        [MenuItem(CLOSE_TAB, true, 103)]
        private static bool ShortcutCloseTabValidate()
        {
            Menu.SetChecked(CLOSE_TAB, CloseTabEnabled);
            return !PluginDisabled;
        }

        [MenuItem(REOPEN_TAB, false, 104)]
        private static void ShortcutReopenTab() => ReopenTabEnabled = !ReopenTabEnabled;

        [MenuItem(REOPEN_TAB, true, 104)]
        private static bool ShortcutReopenTabValidate()
        {
            Menu.SetChecked(REOPEN_TAB, ReopenTabEnabled);
            return !PluginDisabled;
        }


        [MenuItem(DIR + "Scrolling", false, 1001)]
        private static void HeaderScrolling() { }

        [MenuItem(DIR + "Scrolling", true, 1001)]
        private static bool HeaderScrollingValidate() => false;

        [MenuItem(SIDESCROLL, false, 1002)]
        private static void SideScroll() => SideScrollEnabled = !SideScrollEnabled;

        [MenuItem(SIDESCROLL, true, 1002)]
        private static bool SideScrollValidate()
        {
            Menu.SetChecked(SIDESCROLL, SideScrollEnabled);
            return !PluginDisabled;
        }

        [MenuItem(REVERSE_SCROLL_DIRECTION, false, 1003)]
        private static void ReverseDirection() => ReverseScrollDirectionEnabled = !ReverseScrollDirectionEnabled;

        [MenuItem(REVERSE_SCROLL_DIRECTION, true, 1003)]
        private static bool ReverseDirectionValidate()
        {
            Menu.SetChecked(REVERSE_SCROLL_DIRECTION, ReverseScrollDirectionEnabled);
            return !PluginDisabled;
        }

        [MenuItem(INCREASE_SENSITIVITY, false, 1004)]
        private static void IncreaseSensitivity()
        {
            SidescrollSensitivity += .2f;
            Debug.Log("Tab Everything: scrolling sensitivity increased to " + SidescrollSensitivity * 100 + "%");
        }

        [MenuItem(INCREASE_SENSITIVITY, true, 1004)]
        private static bool IncreaseSensitivityValidate() => !PluginDisabled;

        [MenuItem(DECREASE_SENSITIVITY, false, 1005)]
        private static void DecreaseSensitivity()
        {
            SidescrollSensitivity -= .2f;
            Debug.Log("Tab Everything: scrolling sensitivity decreased to " + SidescrollSensitivity * 100 + "%");
        }

        [MenuItem(DECREASE_SENSITIVITY, true, 1005)]
        private static bool DecreaseSensitivityValidate() => !PluginDisabled;

        [MenuItem(DISABLE_TAB_EVERYTHING, false, 100001)]
        private static void DisableTabEverything()
        {
            PluginDisabled = !PluginDisabled;
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem(DISABLE_TAB_EVERYTHING, true, 100001)]
        private static bool DisableTabEverythingValidate()
        {
            Menu.SetChecked(DISABLE_TAB_EVERYTHING, PluginDisabled);
            return true;
        }
    }
}