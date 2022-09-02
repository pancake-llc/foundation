using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using JetBrains.Annotations;

namespace Pancake.Debugging
{
    [InitializeOnLoad]
    public static class DebugLogExtensionsProjectSettingsProvider
    {
        private const int UsedNamespaceUndetermined = 0;
        private const int GlobalNamespace = 1;
        private const int UniqueNamespace = 2;
        
        private static readonly GUIContent BuildStrippingLabel = new GUIContent("Strip Log Calls From Builds",
            "If true all calls to logging methods in the Debug class will be completely removed from release builds. Critical.Log calls will still remain in use.");

        private static readonly GUIContent UnlistedEnabledByDefaultLabel = new GUIContent("Unlisted Channels Enabled By Default",
            "If true all messages not found on the Channels list are shown to all users by default.\n\nIf false all messages not found on the Channels list are not shown to any users by default.\n\nThis only applies to messages from channels not found in the Channels list or in the user's whitelist or blacklist.");

        private static readonly GUIContent IgnoreUnlistedChannelPrefixesLabel = new GUIContent("Ignore Unlisted Channel Prefixes",
            "When this is disabled Debug.Log Extensions automatically generates new channels from prefixes wrapped inside the '[' and ']' character found at the beginning of logged messages.");

        private static readonly GUIContent AutoAddDevUniqueChannelsLabel = new GUIContent("Auto-Register Dev Unique Channels",
            "If true Dev.UniqueChannel from all users will be automatically added to the Channels list and and set to not be enabled by default.\n\nThis makes it easier for devs to log messages that only they will see.");

        private static readonly GUIContent ChannelsHeaderLabel =
            new GUIContent("Channels", "List of channels, their colors, and whether or not they are enabled for all users by default.");

        private static readonly GUIContent SideStackTraceLabel = new GUIContent("Hide Stack Trace",
            "Top level stack trace rows are hidden in the Console+ window's detail when their namespace, class name and/or method name match one of the following entries.");

        private static readonly GUIContent StackTraceRowsColumn1Header = new GUIContent("Namespace",
            "Hide top level stack trace rows from classes whose namespace equals.\n\nAdd a '*' character at the end to match with namespaces that begin with this string instead.");

        private static readonly GUIContent StackTraceRowsColumn2Header = new GUIContent("Class",
            "Hide top level stack trace rows from classes.\n\nAdd a '*' character at the end to match with class names that begin with this string instead.");

        private static readonly GUIContent StackTraceRowsColumn3Header = new GUIContent("Method",
            "Hide top level stack trace rows from methods.\n\nAdd a '*' character at the end to match with method names that begin with this string instead.");

        private static DebugLogExtensionsProjectSettingsAsset asset;

        private static SerializedObject settingsSerializedObject;
        private static ReorderableList channelsList;
        private static ReorderableList hideStackTraceList;
        private static bool channelsUnappliedChanges;

        private static GUIStyle subtitleStyle;

        [UsedImplicitly]
        static DebugLogExtensionsProjectSettingsProvider() { Apply(); }

        private static void Apply()
        {
            if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            {
#if DEV_MODE
				Debug.Log("DebugLogExtensionsProjectSettingsProvider.Apply - delaying...");
#endif

                EditorApplication.delayCall += Apply;
                return;
            }

#if DEV_MODE
			Debug.Log("DebugLogExtensionsProjectSettingsProvider.Apply now");
#endif

            if (asset == null)
            {
                asset = DebugLogExtensionsProjectSettingsAsset.Get();
                if (asset == null)
                {
#if DEV_MODE
					Debug.LogWarning("DebugLogExtensionsProjectSettingsProvider.Apply - aborting because DebugLogExtensionsProjectSettings.Get() returned null.");
#endif
                    return;
                }

                // Handle initializating Debug class when RuntimeInitializeOnLoadMethod can not
                if (!Application.isPlaying)
                {
                    typeof(Debug).GetMethod("Initialize",
                            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        .Invoke(null, null);
                }

                settingsSerializedObject = new SerializedObject(asset);

                channelsList = new ReorderableList(settingsSerializedObject,
                    settingsSerializedObject.FindProperty("channels"),
                    true,
                    true,
                    true,
                    true)
                {
                    drawHeaderCallback = DrawChannelsHeader,
                    drawElementCallback = DrawChannelsElement,
                    onAddCallback = AddChannel,
                    onRemoveCallback = RemoveChannel,
                    elementHeight = 20f
                };

                hideStackTraceList = new ReorderableList(settingsSerializedObject,
                    settingsSerializedObject.FindProperty("hideStackTraceRows"),
                    true,
                    true,
                    true,
                    true)
                {
                    drawHeaderCallback = DrawHideStackTraceRowsHeader,
                    drawElementCallback = DrawHideStackTraceRowsElement,
                    onAddCallback = AddHideStackTraceRow,
                    onRemoveCallback = RemoveHideStackTraceRow,
                    elementHeight = 20f
                };

                // disable all unique dev channels by default in settings
                if (asset.autoAddDevUniqueChannels)
                {
                    var devUniqueChannel = Dev.PersonalChannelName;
                    bool devUniqueChannelFound = false;
                    var channels = asset.channels;
                    for (int n = channels.Length - 1; n >= 0; n--)
                    {
                        if (string.Equals(channels[n].id, devUniqueChannel, StringComparison.OrdinalIgnoreCase))
                        {
                            devUniqueChannelFound = true;
                            break;
                        }
                    }

#if DEV_MODE
					devUniqueChannelFound = true;
#endif

                    if (!devUniqueChannelFound)
                    {
                        var list = new System.Collections.Generic.List<DebugChannelInfo>(channels);
                        list.Insert(0, new DebugChannelInfo() {id = devUniqueChannel, color = Color.grey, enabledByDefault = false});
                        Undo.RecordObject(asset, "Disable Unique Dev Channel By Default");
                        asset.channels = list.ToArray();
                    }
                }
            }

            var packageRootDir = new StackTrace(true).GetFrame(0).GetFileName();
            for (int n = 1; n <= 2; n++)
            {
                packageRootDir = Path.GetDirectoryName(packageRootDir);

                if (packageRootDir == null)
                {
                    Debug.LogError("\"Debug.Log Extensions\" directory root not found. Please don't modify the directory structure.");
                    packageRootDir = Application.dataPath;
                    break;
                }
            }

            // don't edit assets in play mode
            if (!Application.isPlaying)
            {
                // handle user changing used namespace
                bool usingGlobalNamespace = string.IsNullOrEmpty(typeof(Debug).Namespace);
                bool usingBuildStripping = !(bool) typeof(DebugLogExtensionsProjectSettings).GetField(nameof(DebugLogExtensionsProjectSettings.LogEnabledInBuilds),
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .GetValue(null);
                bool shouldUseBuildStripping = asset.stripAllCallsFromBuilds;

                if (usingBuildStripping != shouldUseBuildStripping)
                {
                    // If Debug class does not exist inside a DLL found in the Assets folder, then don't modify dll import settings.
                    // This would result in compile errors when conflicting classes would exist both inside an imported dll and as c# script files.
                    if (typeof(Debug).Assembly.Location.StartsWith(Application.dataPath.Replace("/", "\\")))
                    {
                        string installerPath;
                        if (shouldUseBuildStripping)
                        {
                            installerPath = AssetUtility.FindByNameAndExtension("debug_dll_stripping", ".unitypackage");
                        }
                        else
                        {
                            installerPath = AssetUtility.FindByNameAndExtension("debug_dll", ".unitypackage");
                        }

                        if (!File.Exists(installerPath))
                        {
#if DEV_MODE
							Debug.LogWarning("Installer not found: "+installerPath);
#endif
                            return;
                        }

#if DEV_MODE
						Debug.Log("Installing "+installerPath);
#endif

                        AssetDatabase.ImportPackage(installerPath, false);
                    }
#if DEV_MODE
					else { Debug.LogWarning("Won't import dlls because Debug dll " + typeof(Debug).Assembly.Location + " was not found inside Assets folder " + Application.dataPath.Replace("/", "\\")); }
#endif
                }

                // UPDATE: Changed rebuilding of Channels.cs to only happen when user triggers it manually,
                // because there were issues with the class constantly being rebuilt.
                // However if script compilation failed we'll still want to do this, since it's possible
                // for the Channel prefix to get reset to its default value during udates.
                channelsUnappliedChanges = !ChannelClassBuilder.ChannelClassContentsMatch(asset.channels);
                if (EditorUtility.scriptCompilationFailed && channelsUnappliedChanges)
                {
                    channelsUnappliedChanges = false;
                    EditorApplication.delayCall += () => ChannelClassBuilder.BuildClass(asset.channels);
                }
            }

            asset.Apply(DebugLogExtensionsProjectSettings.Get());
            DebugLogExtensionsPreferences.Apply();
        }

        [SettingsProvider, UsedImplicitly]
        private static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/Heart/Debug Log", SettingsScope.Project)
            {
                label = "Debug Log",
                guiHandler = DrawSettingsGUI,

                // Populate the search keywords to enable smart search filtering and label highlighting
                keywords = new System.Collections.Generic.HashSet<string>(new[]
                {
                    "Formatting", "List Display Style", "Colorize", "Single Line Max Char Count", "Channels", "Enabled Channels", "Disabled Channels"
                })
            };

            return provider;
        }

        private static void DrawSettingsGUI(string searchContext) { DrawSettingsGUI(); }

        private static void DrawSettingsGUI()
        {
            if (subtitleStyle == null)
            {
                subtitleStyle = new GUIStyle(EditorStyles.largeLabel);
                subtitleStyle.wordWrap = true;
            }

            GUILayout.Label("These settings will affect all users across the project.", subtitleStyle);

            GUILayout.Space(10f);

            if (asset == null)
            {
                GUILayout.Label("Project Settings asset not found!", EditorStyles.helpBox);
                return;
            }

            settingsSerializedObject.Update();

            int indentLevelWas = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float labelWidthWas = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 180f;

            GUILayout.Label("Namespace", EditorStyles.boldLabel);

            var buildStripping = asset.stripAllCallsFromBuilds;
            var setBuildStripping = EditorGUILayout.Toggle(BuildStrippingLabel, buildStripping);
            if (setBuildStripping != buildStripping)
            {
                asset.stripAllCallsFromBuilds = setBuildStripping;
                OnSettingChanged();
            }

            GUILayout.Space(10f);

            channelsList.DoLayoutList();

#if UNITY_2019_3_OR_NEWER
            GUILayout.Space(-20f);
#else
			GUILayout.Space(-15f);
#endif

            var allChannelsEnabledByDefault = asset.unlistedChannelsEnabledByDefault;
            var allEnabledByDefaultRect = EditorGUILayout.GetControlRect();
            allEnabledByDefaultRect.width = 240f;
            var setAllChannelsEnabledByDefault = EditorGUI.ToggleLeft(allEnabledByDefaultRect, UnlistedEnabledByDefaultLabel, allChannelsEnabledByDefault);
            if (setAllChannelsEnabledByDefault != allChannelsEnabledByDefault)
            {
                Undo.RecordObject(asset, "Set Unlisted Channels Enabled By Default");
                asset.unlistedChannelsEnabledByDefault = setAllChannelsEnabledByDefault;
                OnSettingChanged();
            }

            var ignoreUnlistedChannelPrefixes = asset.ignoreUnlistedChannelPrefixes;
            var setIgnoreUnlistedChannelPrefixes = EditorGUILayout.ToggleLeft(IgnoreUnlistedChannelPrefixesLabel, ignoreUnlistedChannelPrefixes);
            if (setIgnoreUnlistedChannelPrefixes != ignoreUnlistedChannelPrefixes)
            {
                Undo.RecordObject(asset, "Set Ignore Unlisted Channel Prefixes");
                asset.ignoreUnlistedChannelPrefixes = setIgnoreUnlistedChannelPrefixes;
                OnSettingChanged();
            }

            var autoAddDevUniqueChannels = asset.autoAddDevUniqueChannels;
            var setAutoAddDevUniqueChannels = EditorGUILayout.ToggleLeft(AutoAddDevUniqueChannelsLabel, autoAddDevUniqueChannels);
            if (setAutoAddDevUniqueChannels != autoAddDevUniqueChannels)
            {
                Undo.RecordObject(asset, "Set Auto-Register Dev Unique Channels");
                asset.autoAddDevUniqueChannels = setAutoAddDevUniqueChannels;
                OnSettingChanged();
            }

            GUILayout.Space(10f);

            GUILayout.Label(SideStackTraceLabel, EditorStyles.boldLabel);

            hideStackTraceList.DoLayoutList();

            GUILayout.Label("Shortcuts", EditorStyles.boldLabel);

            DrawKeyConfigGUI();

            GUILayout.Space(20f);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            if (GUILayout.Button("Reset All To Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset All To Defaults?",
                        "Are you sure you want to reset all project settings to default values?\n\nThis will affect all users of the project.",
                        "Reset All",
                        "Cancel"))
                {
                    EditorPrefs.DeleteKey("DebugLogExtensions.UseNamespace");
                    var freshInstance = ScriptableObject.CreateInstance<DebugLogExtensionsProjectSettingsAsset>();
                    var json = EditorJsonUtility.ToJson(freshInstance);
                    EditorJsonUtility.FromJsonOverwrite(json, asset);
                }
            }

            GUILayout.Space(20f);
            GUILayout.EndHorizontal();

            settingsSerializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = labelWidthWas;
            EditorGUI.indentLevel = indentLevelWas;
        }

        public static void DrawKeyConfigGUI()
        {
            var toggleView = asset.toggleView;

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Toggle GUI");

                int indentLevelWas = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                var setKey = (KeyCode) EditorGUILayout.EnumPopup(toggleView.keyCode);
                if (!toggleView.KeyCode.Equals(setKey))
                {
                    toggleView.keyCode = setKey;
                    OnSettingChanged();
                }

                GUILayout.Label(new GUIContent("Ctrl", "Require Control modifier?"), GUILayout.Width(30f));

                bool controlWas = toggleView.control;
                bool setControl = EditorGUILayout.Toggle(controlWas);
                if (setControl != controlWas)
                {
                    toggleView.control = setControl;
                    OnSettingChanged();
                }

                GUILayout.Label(new GUIContent("Alt", "Require Alt modifier?"), GUILayout.Width(30f));

                bool altWas = toggleView.alt;
                bool setAlt = EditorGUILayout.Toggle(altWas);
                if (setAlt != altWas)
                {
                    toggleView.alt = setAlt;
                    OnSettingChanged();
                }

                GUILayout.Label(new GUIContent("Shift", "Require Shift modifier?"), GUILayout.Width(30f));

                bool shiftWas = toggleView.shift;
                bool setShift = EditorGUILayout.Toggle(shiftWas);
                if (setShift != shiftWas)
                {
                    toggleView.shift = setShift;
                    OnSettingChanged();
                }

                EditorGUI.indentLevel = indentLevelWas;
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawChannelsHeader(Rect rect)
        {
            var buttonRect = rect;
            buttonRect.x += rect.width - 140f;
            buttonRect.width = 140f;

            var titleRect = rect;
            titleRect.width -= buttonRect.width;
            GUI.Label(titleRect, ChannelsHeaderLabel);

            if (!channelsUnappliedChanges)
            {
                return;
            }

            if (!GUI.Button(buttonRect, new GUIContent("Rebuild Channel Class", "Rebuild Channel.cs based on the current channels listed below.")))
            {
                return;
            }

            // Unfocus current control so that changes made in delayed text fields aren't lost.
            GUI.FocusControl("");

            if (asset == null)
            {
                asset = DebugLogExtensionsProjectSettingsAsset.Get();
            }

            ChannelClassBuilder.BuildClass(asset.channels);
        }

        private static void DrawHideStackTraceRowsHeader(Rect rect)
        {
            const float leftOffset = 16f;
            rect.width -= leftOffset;
            rect.x += leftOffset;
            rect.width *= 0.33333333f;
            GUI.Label(rect, StackTraceRowsColumn1Header);
            rect.x += rect.width;
            GUI.Label(rect, StackTraceRowsColumn2Header);
            rect.x += rect.width;
            GUI.Label(rect, StackTraceRowsColumn3Header);
        }

        private static void DrawChannelsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            rect.height -= 4f;

            EditorGUI.indentLevel = 0;

            const float minLabelWidth = 50f;
            float totalWidth = rect.width;
            if (totalWidth <= minLabelWidth)
            {
                return;
            }

            const float colorRectWidth = 50f;
            const float enabledRectWidth = 18f;
            const float offset = 5f;
            const float minWidthToShowAllControls = minLabelWidth + offset + enabledRectWidth + offset + colorRectWidth;
            bool onlyDrawIdField = totalWidth < minWidthToShowAllControls;
            var idRect = rect;
            if (!onlyDrawIdField)
            {
                idRect.width = rect.width - offset - enabledRectWidth - offset - colorRectWidth;
            }

            GUI.Label(idRect,
                new GUIContent("",
                    "Tag which should be used as a prefix wrapped in square brackets for any messages that should be tied in the channel.\n\nUsage Example:\nDebug.Log(\"[AUDIO]Play(SFX_Hit)\");"));

            string idWas = asset.channels[index].id;
            string setId = EditorGUI.DelayedTextField(idRect, idWas);
            if (!string.Equals(idWas, setId))
            {
                setId = setId.Replace("[", "").Replace("]", "");

                if (!string.Equals(idWas, setId))
                {
                    Undo.RecordObject(asset, "Set Channel Id");
                    asset.channels[index].id = setId;
                    OnSettingChanged();
                }
            }

            if (onlyDrawIdField)
            {
                return;
            }

            var colorRect = rect;
            colorRect.x += rect.width - enabledRectWidth - offset - colorRectWidth;
            colorRect.width = colorRectWidth;

            GUI.Label(colorRect, new GUIContent("", "Color for channel tag in console."));

            var colorWas = asset.channels[index].color;
            var setColor = EditorGUI.ColorField(colorRect,
                GUIContent.none,
                colorWas,
                false,
                false,
                false);
            if (colorWas != setColor)
            {
                setColor.a = 1f;
                if (colorWas != setColor)
                {
                    Undo.RecordObject(asset, "Set Channel Color");
                    asset.channels[index].color = setColor;
                    OnSettingChanged();
                }
            }

            var enabledRect = rect;
            enabledRect.x += rect.width - enabledRectWidth;
            enabledRect.width = enabledRectWidth;

            GUI.Label(enabledRect,
                new GUIContent("",
                    "Is channel enabled by default for all users?\n\nIf true users will need to blacklist the channel to not see its messages.\n\nIf false users will need to whitelist the channel to see its messages."));

            var wasEnabled = asset.channels[index].enabledByDefault;
            var setEnabled = EditorGUI.Toggle(enabledRect, wasEnabled);
            if (wasEnabled != setEnabled)
            {
                Undo.RecordObject(asset, "Set Channel Enabled");
                asset.channels[index].enabledByDefault = setEnabled;
                OnSettingChanged();
            }
        }

        private static void DrawHideStackTraceRowsElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            rect.height -= 4f;
            rect.width *= 0.33333333f;

            EditorGUI.indentLevel = 0;

            string namespaceWas = asset.hideStackTraceRows[index].namespaceName;
            string setNamespace = EditorGUI.TextField(rect, namespaceWas);
            if (!string.Equals(namespaceWas, setNamespace))
            {
                Undo.RecordObject(asset, "Set Hide Stack Trace Row Namespace");
                asset.hideStackTraceRows[index].namespaceName = setNamespace;
                OnSettingChanged();
            }

            rect.x += rect.width;
            string classWas = asset.hideStackTraceRows[index].className;
            string setClass = EditorGUI.TextField(rect, classWas);
            if (!string.Equals(classWas, setClass))
            {
                Undo.RecordObject(asset, "Set Hide Stack Trace Row Class");
                asset.hideStackTraceRows[index].className = setClass;
                OnSettingChanged();
            }

            rect.x += rect.width;
            string methodWas = asset.hideStackTraceRows[index].methodName;
            string setMethod = EditorGUI.TextField(rect, methodWas);
            if (!string.Equals(methodWas, setMethod))
            {
                Undo.RecordObject(asset, "Set Hide Stack Trace Row Method");
                asset.hideStackTraceRows[index].methodName = setMethod;
                OnSettingChanged();
            }
        }

        private static void AddChannel(ReorderableList list)
        {
            Undo.RecordObject(asset, "Add Channel");

            var channels = asset.channels;
            int countWas = channels.Length;
            int setCount = countWas + 1;
            var setChannels = new DebugChannelInfo[setCount];
            Array.Copy(channels,
                0,
                setChannels,
                0,
                countWas);
            var setColor = UnityEngine.Random.ColorHSV();
            setColor.a = 1f;
            setChannels[countWas] = new DebugChannelInfo() {enabledByDefault = asset.unlistedChannelsEnabledByDefault, color = setColor};
            asset.channels = setChannels;

            OnSettingChanged();
        }

        private static void AddHideStackTraceRow(ReorderableList list)
        {
            Undo.RecordObject(asset, "Add Hide Stack Trace Row");

            var oldArray = asset.hideStackTraceRows;
            int countWas = oldArray.Length;
            int setCount = countWas + 1;
            var newArray = new IgnoredStackTraceInfo[setCount];
            Array.Copy(oldArray,
                0,
                newArray,
                0,
                countWas);
            newArray[countWas] = new IgnoredStackTraceInfo();
            asset.hideStackTraceRows = newArray;

            OnSettingChanged();
        }

        private static void RemoveChannel(ReorderableList list)
        {
            Undo.RecordObject(asset, "Remove Channel");

            var channels = asset.channels;
            int countWas = channels.Length;
            if (countWas == 0)
            {
                return;
            }

            int setCount = countWas - 1;
            var setChannels = new DebugChannelInfo[setCount];

            int selected = list.index;
            if (selected >= 0 && selected < countWas)
            {
                Array.Copy(channels,
                    0,
                    setChannels,
                    0,
                    selected);
                Array.Copy(channels,
                    selected + 1,
                    setChannels,
                    selected,
                    setCount - selected);
            }
            else
            {
                Array.Copy(channels,
                    0,
                    setChannels,
                    0,
                    countWas);
            }

            asset.channels = setChannels;

            OnSettingChanged();
        }

        private static void RemoveHideStackTraceRow(ReorderableList list)
        {
            Undo.RecordObject(asset, "Remove Hide Stack Trace Row");

            var oldArray = asset.hideStackTraceRows;
            int countWas = oldArray.Length;
            if (countWas == 0)
            {
                return;
            }

            int setCount = countWas - 1;
            var newArray = new IgnoredStackTraceInfo[setCount];

            int selected = list.index;
            if (selected >= 0 && selected < countWas)
            {
                Array.Copy(oldArray,
                    0,
                    newArray,
                    0,
                    selected);
                Array.Copy(oldArray,
                    selected + 1,
                    newArray,
                    selected,
                    setCount - selected);
            }
            else
            {
                Array.Copy(oldArray,
                    0,
                    newArray,
                    0,
                    countWas);
            }

            asset.hideStackTraceRows = newArray;

            OnSettingChanged();
        }

        private static void OnSettingChanged()
        {
            Apply();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            channelsUnappliedChanges = !ChannelClassBuilder.ChannelClassContentsMatch(asset.channels);

            if (Event.current != null)
            {
                GUIUtility.ExitGUI();
            }
        }
    }
}