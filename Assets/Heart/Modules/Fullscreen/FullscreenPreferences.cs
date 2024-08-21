using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Pancake.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace FullscreenEditor
{
    /// <summary>Define a source mode to get a fullscreen rect.</summary>
    public enum RectSourceMode
    {
        /// <summary>The bounds of the main display.</summary>
        MainDisplay,

        /// <summary>Open on the display that the target window is located.</summary>
        WindowDisplay,

        /// <summary>The bounds of the display where the mouse pointer is.</summary>
        AtMousePosition,

        /// <summary>A rect that spans across all the displays. (Windows only)</summary>
        Span,

        /// <summary>A custom rect defined by <see cref="FullscreenPreferences.CustomRect"/>.</summary>
        Custom,
        Display1 = 0x100,
        Display2 = 0x101,
        Display3 = 0x102,
        Display4 = 0x103,
        Display5 = 0x104,
        Display6 = 0x105,
        Display7 = 0x106,
        Display8 = 0x107
    }

    /// <summary>Contains preferences for the Fullscreen Editor plugin.</summary>
    [InitializeOnLoad]
    public static class FullscreenPreferences
    {
        private const float LABEL_WIDTH = 300f;

        /// <summary>Current version of the Fullscreen Editor plugin.</summary>
        public static readonly Version pluginVersion = new(2, 2, 8);

        /// <summary>Release date of this version.</summary>
        public static readonly DateTime pluginDate = new(2023, 09, 07);

        private static readonly GUIContent resetSettingsContent = new("Use Defaults", "Reset all settings to default ones");
        private static readonly GUIContent versionContent = new(string.Format("Version: {0} ({1:d})", pluginVersion, pluginDate));

        private static readonly string[] mosaicDropDownOptions =
        {
            "nothing", "virtual display 1", "virtual display 2", "virtual display 3", "virtual display 4", "virtual display 5", "virtual display 6",
            "virtual display 7", "virtual display 8",
        };

        internal static Action onLoadDefaults = () => { };
        internal static readonly List<GUIContent> contents = new();

        private static readonly PrefItem<Vector2> scroll = new("Scroll", Vector2.zero, string.Empty, string.Empty);

        /// <summary>Is the window toolbar currently visible?</summary>
        public static readonly PrefItem<bool> ToolbarVisible;

        /// <summary>Is Fullscreen on Play currently enabled?</summary>
        public static readonly PrefItem<bool> FullscreenOnPlayEnabled;

        /// <summary>Defines a source to get a fullscreen rect.</summary>
        public static readonly PrefItem<RectSourceMode> RectSource;

        /// <summary>Custom rect to be used when <see cref="RectSource"/> is set to <see cref="RectSourceMode.Custom"/>.</summary>
        public static readonly PrefItem<Rect> CustomRect;

        /// <summary>Disable notifications when opening fullscreen windows.</summary>
        public static readonly PrefItem<bool> DisableNotifications;

        /// <summary>Keep fullscreen views below other modal and utility windows.</summary>
        public static readonly PrefItem<bool> KeepFullscreenBelow;

        /// <summary>Disable background SceneView rendering when there are open fullscreen views to increase performance.</summary>
        public static readonly PrefItem<bool> DisableSceneViewRendering;

        /// <summary>Hide toolbars of all windows, not only the fullscreened (issue #80).</summary>
        public static readonly PrefItem<bool> UseGlobalToolbarHiding;

        /// <summary>Defines which display renders on each screen when using Mosaic.</summary>
        public static readonly PrefItem<int[]> MosaicMapping;

        /// <summary>Do not attempt to use wmctrl's fullscreen on Linux environments.</summary>
        public static readonly PrefItem<bool> DoNotUseWmctrl;

        /// <summary>Restore cursor lock and hide state after going in and out of fullscreen.</summary>
        public static readonly PrefItem<bool> RestoreCursorLockAndHideState;

        [RuntimeInitializeOnLoadMethod]
        private static void SyncVersion()
        {
            // Sync for automation
            PlayerPrefs.SetString("PLUGIN_VERSION", pluginVersion.ToString());
        }

        static FullscreenPreferences()
        {
            var rectSourceTooltip = string.Empty;

            rectSourceTooltip += "Controls where Fullscreen views opens.\n\n";
            rectSourceTooltip += "Main Screen: Fullscreen opens on the primary screen;\n\n";
            rectSourceTooltip += "Window Display: Open on the display that the target window is located (Windows only);\n\n";
            rectSourceTooltip += "At Mouse Position: Fullscreen opens on the screen where the mouse pointer is;\n\n";
            rectSourceTooltip += "Span: Fullscreen spans across all screens (Windows only);\n\n";
            rectSourceTooltip += "Custom Rect: Fullscreen opens on the given custom Rect.";

            ToolbarVisible = new PrefItem<bool>("Toolbar",
                false,
                "Toolbar Visible",
                "Show and hide the toolbar on the top of some windows, like the Game View and Scene View.");
            FullscreenOnPlayEnabled = new PrefItem<bool>("FullscreenOnPlay",
                false,
                "Fullscreen On Play",
                "Override the \"Maximize on Play\" option of the game view to \"Fullscreen on Play\"");
            RectSource = new PrefItem<RectSourceMode>("RectSource", RectSourceMode.MainDisplay, "Placement source", rectSourceTooltip);
            CustomRect = new PrefItem<Rect>("CustomRect", FullscreenRects.GetMainDisplayRect(), "Custom Rect", string.Empty);
            DisableNotifications = new PrefItem<bool>("DisableNotifications",
                false,
                "Disable Notifications",
                "Disable the notifications that shows up when opening a new fullscreen view.");
            KeepFullscreenBelow = new PrefItem<bool>("KeepFullscreenBelow",
                true,
                "Keep Utility Views Above",
                "Keep utility views on top of fullscreen views.\nThis is useful to integrate with assets that need to keep windows open, such as Peek by Ludiq.");
            DisableSceneViewRendering = new PrefItem<bool>("DisableSceneViewRendering",
                true,
                "Disable Scene View Rendering",
                "Increase Fullscreen Editor performance by not rendering SceneViews while there are open fullscreen views.");
            UseGlobalToolbarHiding = new PrefItem<bool>("UseGlobalToolbarHiding",
                FullscreenUtility.IsMacOS,
                "Use global toolbar hiding",
                "Changes toolbars of all windows at once. This option fixes the gray bar bug on MacOS.");
            MosaicMapping = new PrefItem<int[]>("MosaicMapping",
                new[] {0, 1, 2, 3, 4, 5, 6, 7},
                "Mosaic Screen Mapping",
                "Defines which display renders on each screen when using Mosaic.");
            DoNotUseWmctrl = new PrefItem<bool>("DoNotUseWmctrl", false, "Do not use wmctrl", "Avoid using 'wmctrl' helper when opening fullscreen windows");
            RestoreCursorLockAndHideState = new PrefItem<bool>("RestoreCursorLockAndHideState",
                true,
                "Restore Cursor Lock and Hide State",
                "Restore cursor lock and hide state after going in and out of fullscreen.");

            onLoadDefaults += () => // Array won't revert automaticaly because it is changed as reference
                MosaicMapping.Value = new[] {0, 1, 2, 3, 4, 5, 6, 7};

            if (FullscreenUtility.MenuItemHasShortcut(Shortcut.TOOLBAR_PATH))
                ToolbarVisible.Content.text += string.Format(" ({0})", FullscreenUtility.TextifyMenuItemShortcut(Shortcut.TOOLBAR_PATH));
            if (FullscreenUtility.MenuItemHasShortcut(Shortcut.FULLSCREEN_ON_PLAY_PATH))
                FullscreenOnPlayEnabled.Content.text += string.Format(" ({0})", FullscreenUtility.TextifyMenuItemShortcut(Shortcut.FULLSCREEN_ON_PLAY_PATH));
        }

#if UNITY_2018_3_OR_NEWER
        [SettingsProvider]
        private static SettingsProvider RetrieveSettingsProvider()
        {
            var sp = new SettingsProvider("Preferences/Fullscreen Editor", SettingsScope.User, contents.Select(c => c.text));
            sp.footerBarGuiHandler = OnFooterGUI;
            sp.guiHandler = (search) =>
            {
                EditorGUIUtility.labelWidth = LABEL_WIDTH;
                OnPreferencesGUI(search);
            };
            return sp;
        }

        [SettingsProvider]
        private static SettingsProvider RetrieveSettingsProviderShortcuts()
        {
            var sp = new SettingsProvider("Preferences/Fullscreen Editor/Shortcuts", SettingsScope.User, contents.Select(c => c.text));
            sp.footerBarGuiHandler = OnFooterGUI;
            sp.guiHandler = (search) =>
            {
                EditorGUIUtility.labelWidth = LABEL_WIDTH;
                Shortcut.DoShortcutsGUI();
            };
            return sp;
        }

#else
        [PreferenceItem("Fullscreen")]
        private static void OnPreferencesGUI() {
            scroll.Value = EditorGUILayout.BeginScrollView(scroll);
            OnPreferencesGUI(string.Empty);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            Shortcut.DoShortcutsGUI();
            EditorGUI.indentLevel--;

            EditorGUILayout.EndScrollView();
            OnFooterGUI();
        }
#endif

        private static void OnPreferencesGUI(string search)
        {
            ToolbarVisible.DoGUI();
            FullscreenOnPlayEnabled.DoGUI();

            EditorGUILayout.Separator();
            RectSource.DoGUI();

            if (RectSource.Value == RectSourceMode.AtMousePosition)
                EditorGUILayout.HelpBox("\'At mouse position\' can cause slowdowns on large projects", MessageType.Warning);

            if (!IsRectModeSupported(RectSource))
                EditorGUILayout.HelpBox("The selected placement source mode is not supported on this platform", MessageType.Warning);

            // Custom Rect
            switch (RectSource.Value)
            {
                case RectSourceMode.Custom:
                    EditorGUI.indentLevel++;
                    CustomRect.DoGUI();

                    var customRect = CustomRect.Value;

                    if (customRect.width < 300f) customRect.width = 300f;
                    if (customRect.height < 300f) customRect.height = 300f;

                    CustomRect.Value = customRect;

                    EditorGUI.indentLevel--;
                    break;
            }

            DisableNotifications.DoGUI();
            KeepFullscreenBelow.DoGUI();

            if (Patcher.IsSupported()) DisableSceneViewRendering.DoGUI();

            UseGlobalToolbarHiding.DoGUI();
            RestoreCursorLockAndHideState.DoGUI();

            if (FullscreenUtility.IsLinux)
            {
                using (new EditorGUI.DisabledGroupScope(!Linux.wmctrl.IsInstalled))
                {
                    DoNotUseWmctrl.DoGUI();
                }

                if (!Linux.wmctrl.IsInstalled)
                {
                    EditorGUILayout.HelpBox("'wmctrl' not found. Try installing it with 'sudo apt-get install wmctrl'.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox("Try enabling the option above if you're experiencing any kind of toolbars or offsets " +
                                            "while in fullscreen mode.\nDisabling 'wmctrl' can fix issues on some Linux environments when the window manager " +
                                            "does not handle fullscreen windows properly (I'm looking at you Ubuntu).",
                        MessageType.Info);
                }
            }

            // Mosaic
            if (FullscreenRects.ScreenCount > 1)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField(MosaicMapping.Content, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                GUI.changed = false;
                var mosaicMapping = MosaicMapping.Value;

                for (var i = 0; i < mosaicMapping.Length && i < FullscreenRects.ScreenCount; i++)
                {
                    var val = EditorGUILayout.IntPopup(string.Format("Physical display {0} renders", i + 1),
                        mosaicMapping[i],
                        mosaicDropDownOptions,
                        new[] {-1, 0, 1, 2, 3, 4, 5, 6, 7});
                    mosaicMapping[i] = val;
                }

                if (GUI.changed) MosaicMapping.SaveValue();
                EditorGUI.indentLevel--;
            }
        }

        private static void OnFooterGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(resetSettingsContent, GUILayout.Width(120f))) onLoadDefaults();

                using (new EditorGUI.DisabledGroupScope(EditorApplication.isCompiling))
                {
                    GUI.changed = false;

                    bool enable = GUILayout.Toggle(ScriptingDefinition.IsSymbolDefined("FULLSCREEN_DEBUG"), "Debug", "Button");
                    if (GUI.changed)
                    {
                        if (enable) ScriptingDefinition.AddDefineSymbolOnAllPlatforms("FULLSCREEN_DEBUG");
                        else ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("FULLSCREEN_DEBUG");
                    }
                }

                if (GUILayout.Button("Copy debug data", GUILayout.Width(120f))) CopyDisplayDebugInfo();

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(versionContent, GUILayout.Width(170f));
            }

            EditorGUILayout.Separator();
        }

        private static void CopyDisplayDebugInfo()
        {
            var str = new StringBuilder();

            str.Append("Fullscreen Editor");
            str.AppendFormat("\nVersion: {0}", pluginVersion.ToString(3));
            str.AppendFormat("\nUnity {0}", InternalEditorUtility.GetFullUnityVersion());
            str.AppendFormat("\n{0}", SystemInfo.operatingSystem);
            str.AppendFormat("\nScaling {0:p}", FullscreenUtility.GetDisplayScaling());

            foreach (var display in DisplayInfo.GetDisplays())
            {
                str.AppendFormat("\n----------- DISPLAY -----------");
                str.AppendFormat("\nDeviceName: {0} ({1})", display.FriendlyName, display.DeviceName);
                str.AppendFormat("\nDpiCorrectedArea: {0}", display.DpiCorrectedArea);
                str.AppendFormat("\nUnityCorrectedArea: {0}", display.UnityCorrectedArea);
                str.AppendFormat("\nMonitorArea: {0}", display.MonitorArea);
                str.AppendFormat("\nPhysicalArea: {0}", display.PhysicalArea);
                str.AppendFormat("\nWorkArea: {0}", display.WorkArea);
                str.AppendFormat("\nLogicalScreenHeight: {0}", display.LogicalScreenHeight);
                str.AppendFormat("\nPhysicalScreenHeight: {0}", display.PhysicalScreenHeight);
                str.AppendFormat("\nScreenWidth: {0}", display.ScreenWidth);
                str.AppendFormat("\nScreenHeight: {0}", display.ScreenHeight);
                str.AppendFormat("\nPrimaryDisplay: {0}", display.PrimaryDisplay);
                str.AppendFormat("\nscaleFactor: {0}", display.scaleFactor);
                str.AppendFormat("\nscaleFactor2: {0}", display.scaleFactor2);
                str.AppendFormat("\ndevMode: {0}\n", JsonUtility.ToJson(display.devMode, true));
            }

            EditorGUIUtility.systemCopyBuffer = str.ToString();
            EditorUtility.DisplayDialog("Debug", "Display debug data was copied to the clipboard", "OK");
        }

        private static string GetFilePath(string file)
        {
            var stack = new StackFrame(0, true);
            var currentFile = stack.GetFileName();
            var currentPath = Path.GetDirectoryName(currentFile);

            return Path.Combine(currentPath, "../" + file);
        }

        internal static bool IsRectModeSupported(RectSourceMode mode)
        {
            switch (mode)
            {
                case RectSourceMode.Display1:
                case RectSourceMode.Display2:
                case RectSourceMode.Display3:
                case RectSourceMode.Display4:
                case RectSourceMode.Display5:
                case RectSourceMode.Display6:
                case RectSourceMode.Display7:
                case RectSourceMode.Display8:
                case RectSourceMode.Span:
                case RectSourceMode.WindowDisplay:
                    return FullscreenUtility.IsWindows;

                case RectSourceMode.MainDisplay:
                case RectSourceMode.AtMousePosition:
                    return true;

                case RectSourceMode.Custom:
                    // Custom rect is not supported on Linux
                    // since we're using native fullscreen
                    return !FullscreenUtility.IsLinux;

                default:
                    return false;
            }
        }
    }
}