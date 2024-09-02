using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Type = System.Type;
using Delegate = System.Delegate;
using Action = System.Action;
using L = Pancake.Linq.L;

// ReSharper disable CognitiveComplexity
// ReSharper disable InconsistentNaming
namespace PancakeEditor.TabEverything
{
    public static class TabEverything
    {
        private static void Update()
        {
            var lastEvent = typeof(Event).GetFieldValue<Event>("s_Current");


            scrollInteractions();
            scrollAnimation();
            createWindowDelayed();
            dragndrop();

            CheckIfFocusedWindowChanged();
            CheckIfWindowWasUnmaximized();
            return;

            void scrollInteractions()
            {
                if (isKeyPressed)
                {
                    sidesscrollPosition = 0;
                    return;
                }

                if (lastEvent.delta == Vector2.zero) return;
                if (lastEvent.type == EventType.MouseMove)
                {
                    sidesscrollPosition = 0;
                    return;
                }

                if (lastEvent.type == EventType.MouseDrag)
                {
                    sidesscrollPosition = 0;
                    return;
                }

                if (lastEvent.type != EventType.ScrollWheel && delayedMousePosition_screenSpace != GUIUtility.GUIToScreenPoint(lastEvent.mousePosition))
                    return; // uncaptured mouse move/drag check
                if (lastEvent.type != EventType.ScrollWheel && Application.platform == RuntimePlatform.OSXEditor &&
                    Mathf.Approximately(lastEvent.delta.x, (int) lastEvent.delta.x))
                    return; // osx uncaptured mouse move/drag in sceneview ang gameview workaround

                shiftscroll();
                sidescroll();
                return;

                void switchTab(int dir)
                {
                    if (!TabMenu.SwitchTabsEnabled) return;
                    if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;
                    if (!hoveredWindow.docked) return;
                    if (hoveredWindow.maximized) return;


                    var tabs = GetTabList(hoveredWindow);

                    int i0 = tabs.IndexOf(hoveredWindow);
                    int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

                    tabs[i1].Focus();

                    UpdateTitle(tabs[i1]);
                }

                void moveTab(int dir)
                {
                    if (!TabMenu.MoveTabsEnabled) return;
                    if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;


                    var tabs = GetTabList(hoveredWindow);

                    int i0 = tabs.IndexOf(hoveredWindow);
                    int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

                    (tabs[i0], tabs[i1]) = (tabs[i1], tabs[i0]);
                    tabs[i1].Focus();
                }

                void shiftscroll()
                {
                    if (!lastEvent.shift) return;

                    float scrollDelta = Application.platform == RuntimePlatform.OSXEditor
                        ? lastEvent.delta.x // osx sends delta.y as delta.x when shift is pressed
                        : lastEvent.delta.x - lastEvent.delta.y; // some software on windows (ex logitech options) may do that too
                    if (TabMenu.ReverseScrollDirectionEnabled) scrollDelta *= -1;

                    if (scrollDelta != 0)
                    {
                        if (lastEvent.control || lastEvent.command) moveTab(scrollDelta > 0 ? 1 : -1);
                        else switchTab(scrollDelta > 0 ? 1 : -1);
                    }
                }

                void sidescroll()
                {
                    if (lastEvent.shift) return;
                    if (lastEvent.delta.x == 0) return;
                    if (lastEvent.delta.x.Abs() <= 0.06f) return;
                    if (lastEvent.delta.x.Abs() * 1.1f < lastEvent.delta.y.Abs())
                    {
                        sidesscrollPosition = 0;
                        return;
                    }

                    if (!TabMenu.SideScrollEnabled) return;

                    const int dampenK = 5; // the larger this k is - the smaller big deltas are, and the less is sidescroll's dependency on scroll speed
                    float a = lastEvent.delta.x.Abs() * dampenK;
                    float deltaDampened = (a < 1 ? a : Mathf.Log(a) + 1) / dampenK * Mathf.Sign(lastEvent.delta.x);

                    const float sensitivityK = .22f;
                    float sidescrollDelta = deltaDampened * TabMenu.SidescrollSensitivity * sensitivityK;

                    if (TabMenu.ReverseScrollDirectionEnabled) sidescrollDelta *= -1;

                    if (sidesscrollPosition.RoundToInt() != (sidesscrollPosition += sidescrollDelta).RoundToInt())
                    {
                        if (lastEvent.control || lastEvent.command) moveTab(sidescrollDelta < 0 ? 1 : -1);
                        else switchTab(sidescrollDelta < 0 ? 1 : -1);
                    }
                }
            }

            void scrollAnimation()
            {
                if (!EditorWindow.focusedWindow) return;
                if (!EditorWindow.focusedWindow.docked) return;
                if (EditorWindow.focusedWindow.maximized) return;

                object dockArea = EditorWindow.focusedWindow.GetMemberValue("m_Parent");

                if (dockArea.GetType() != t_DockArea) return; // happens on 2021.1.28

                var curScroll = dockArea.GetFieldValue<float>("m_ScrollOffset");

                if (!curScroll.Approximately(0)) curScroll -= nonZeroTabScrollOffset;

                if (curScroll == 0 && prevFocusedDockArea == dockArea) curScroll = prevScroll;

                float targScroll = GetOptimalTabScrollerPosition(EditorWindow.focusedWindow);

                const float animationSpeed = 7f;

                float delta = ((float) (EditorApplication.timeSinceStartup - prevTime)).Min(0.03f);

                float newScroll = Mathf.SmoothDamp(curScroll,
                    targScroll,
                    ref scrollDeriv,
                    .5f / animationSpeed,
                    Mathf.Infinity,
                    delta);

                if (newScroll < .5f) newScroll = 0;

                prevScroll = newScroll;
                prevFocusedDockArea = dockArea;
                prevTime = EditorApplication.timeSinceStartup;

                if (newScroll.Approximately(curScroll)) return;

                if (!newScroll.Approximately(0)) newScroll += nonZeroTabScrollOffset;

                dockArea.SetFieldValue("m_ScrollOffset", newScroll);

                EditorWindow.focusedWindow.Repaint();
            }

            void createWindowDelayed()
            {
                if (createWindowDelayedAction == null) return;
                if ((System.DateTime.UtcNow - lastDragndropTime).TotalSeconds < .05f) return;

                createWindowDelayedAction.Invoke();
                createWindowDelayedAction = null;
            }

            void dragndrop()
            {
                if (!TabMenu.DragndropEnabled) return;
                if (lastEvent.type != EventType.DragUpdated && lastEvent.type != EventType.DragPerform) return;
                if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;
                if (!hoveredWindow.position.SetPosition(0, 0)
                        .SetHeight(hoveredWindow.GetType() == t_SceneHierarchyWindow ? 5 : 40)
                        .Contains(lastEvent.mousePosition)) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (lastEvent.type != EventType.DragPerform) return;
                if (lastDragndropPosition == VGUI.currentEvent.MousePosition) return;

                DragAndDrop.AcceptDrag();

                var lockToObject = DragAndDrop.objectReferences.First();
                object dockArea = hoveredWindow.GetMemberValue("m_Parent");

                createWindowDelayedAction =
                    () => new TabInfo(lockToObject).CreateWindow(dockArea, false); // not creating window right away to avoid scroll animation stutter

                lastDragndropPosition = VGUI.currentEvent.MousePosition;
                lastDragndropTime = System.DateTime.UtcNow;
            }
        }

        private static float sidesscrollPosition;

        private static float scrollDeriv;
        private static float prevScroll;
        private static object prevFocusedDockArea;

        private static double deltaTime;
        private static double prevTime;

        private static Vector2 lastDragndropPosition;
        private static System.DateTime lastDragndropTime;
        private static Action createWindowDelayedAction;


        private static void CheckShortcuts() // globalEventHandler
        {
            void addTab()
            {
                if (!VGUI.currentEvent.IsKeyDown) return;
                if (!VGUI.currentEvent.HoldingCmdOnly && !VGUI.currentEvent.HoldingCtrlOnly) return;
                if (VGUI.currentEvent.KeyCode != KeyCode.T) return;

                if (!EditorWindow.mouseOverWindow) return;
                if (!TabMenu.AddTabEnabled) return;

                VGUI.currentEvent.Use();

                addTabMenu_openedOverWindow = EditorWindow.mouseOverWindow;

                List<TabInfo> defaultTabList;
                List<TabInfo> customTabList;

                string customTabListKey = "tabs_customtablist_" + Application.dataPath.GetHashCode();

                loadDefaultTabList();
                loadSavedTabList();

                var menu = new GenericMenu();
                Vector2 menuPosition;

                void set_menuPosition()
                {
                    if (Vector2.Distance(VGUI.currentEvent.MousePositionScreenSpace, addTabMenu_lastClickPosition_screenSpace) < 2)
                        menuPosition = GUIUtility.ScreenToGUIPoint(addTabMenu_lastOpenPosition_screenSpace);
                    else menuPosition = VGUI.currentEvent.MousePosition - Vector2.up * 9;

                    addTabMenu_lastOpenPosition_screenSpace = GUIUtility.GUIToScreenPoint(menuPosition);

#if !UNITY_2021_2_OR_NEWER
                    if (EditorWindow.focusedWindow) menuPosition += EditorWindow.focusedWindow.position.position;
#endif
                }

                set_menuPosition();

                defaultTabs();
                savedTabs();
                remove();
                saveCurrentTab();

                menu.DropDown(Rect.zero.SetPosition(menuPosition));
                return;

                void loadDefaultTabList()
                {
                    defaultTabList = new List<TabInfo>
                    {
                        new TabInfo("SceneView", "Scene"),
                        new TabInfo("GameView", "Game"),
                        new TabInfo("ProjectBrowser", "Project"),
                        new TabInfo("InspectorWindow", "Inspector"),
                        new TabInfo("ConsoleWindow", "Console"),
                        new TabInfo("ProfilerWindow", "Profiler")
                    };
                }

                void loadSavedTabList()
                {
                    string json = EditorPrefs.GetString(customTabListKey);

                    customTabList = JsonUtility.FromJson<TabInfoList>(json)?.list ?? new List<TabInfo>();
                }

                void saveSavedTabsList()
                {
                    string json = JsonUtility.ToJson(new TabInfoList {list = customTabList});

                    EditorPrefs.SetString(customTabListKey, json);
                }

                void rememberClickPosition() { addTabMenu_lastClickPosition_screenSpace = VGUI.currentEvent.MousePositionScreenSpace; }

                void defaultTabs()
                {
                    menu.AddDisabledItem(new GUIContent("Default tabs"));

                    foreach (var tabInfo in defaultTabList)
                        menu.AddItem(new GUIContent(tabInfo.menuItemName),
                            false,
                            () =>
                            {
                                tabInfo.CreateWindow(GetDockArea(addTabMenu_openedOverWindow), false);
                                EditorApplication.delayCall += rememberClickPosition;
                            });
                }

                void savedTabs()
                {
                    if (!customTabList.Any()) return;

                    menu.AddSeparator("");

                    menu.AddDisabledItem(new GUIContent("Saved tabs"));


                    foreach (var tabInfo in customTabList)
                        if (tabInfo.isPropertyEditor && !tabInfo.globalId.GetObject())
                            menu.AddDisabledItem(new GUIContent(tabInfo.menuItemName));
                        else
                            menu.AddItem(new GUIContent(tabInfo.menuItemName),
                                false,
                                () =>
                                {
                                    tabInfo.CreateWindow(GetDockArea(addTabMenu_openedOverWindow), false);
                                    EditorApplication.delayCall += rememberClickPosition;
                                });
                }

                void remove()
                {
                    if (!customTabList.Any()) return;


                    foreach (var tabInfo in customTabList)
                        menu.AddItem(new GUIContent("Remove/" + tabInfo.menuItemName),
                            false,
                            () =>
                            {
                                customTabList.Remove(tabInfo);
                                saveSavedTabsList();
                                EditorApplication.delayCall += rememberClickPosition;
                            });


                    menu.AddSeparator("Remove/");

                    menu.AddItem(new GUIContent("Remove/Remove all"),
                        false,
                        () =>
                        {
                            customTabList.Clear();
                            saveSavedTabsList();
                            EditorApplication.delayCall += rememberClickPosition;
                        });
                }

                void saveCurrentTab()
                {
                    string menuItemName = addTabMenu_openedOverWindow.titleContent.text.Replace("/", " \u2215 ").Trim(' ');

                    if (defaultTabList.Any(r => r.menuItemName == menuItemName)) return;
                    if (customTabList.Any(r => r.menuItemName == menuItemName)) return;

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Save current tab"),
                        false,
                        () =>
                        {
                            customTabList.Add(new TabInfo(addTabMenu_openedOverWindow));
                            saveSavedTabsList();
                            EditorApplication.delayCall += rememberClickPosition;
                        });
                }
            }


            set_isKeyPressed();

            addTab();
            closeTab();
            reopenTab();
            return;

            void set_isKeyPressed()
            {
                if (VGUI.currentEvent.KeyCode == KeyCode.LeftShift) return;
                if (VGUI.currentEvent.KeyCode == KeyCode.LeftControl) return;
                if (VGUI.currentEvent.KeyCode == KeyCode.LeftCommand) return;
                if (VGUI.currentEvent.KeyCode == KeyCode.RightShift) return;
                if (VGUI.currentEvent.KeyCode == KeyCode.RightControl) return;
                if (VGUI.currentEvent.KeyCode == KeyCode.RightCommand) return;

                if (Event.current.type == EventType.KeyDown)
                    isKeyPressed = true;

                if (Event.current.type == EventType.KeyUp)
                    isKeyPressed = false;
            }

            void closeTab()
            {
                if (!VGUI.currentEvent.IsKeyDown) return;
                if (!VGUI.currentEvent.HoldingCmdOnly && !VGUI.currentEvent.HoldingCtrlOnly) return;
                if (VGUI.currentEvent.KeyCode != KeyCode.W) return;

                if (!TabMenu.CloseTabEnabled) return;
                if (!EditorWindow.mouseOverWindow) return;
                if (!EditorWindow.mouseOverWindow.docked) return;
                if (GetTabList(EditorWindow.mouseOverWindow).Count <= 1) return;


                Event.current.Use();

                tabInfosForReopening.Push(new TabInfo(EditorWindow.mouseOverWindow));

                EditorWindow.mouseOverWindow.Close();
            }

            void reopenTab()
            {
                if (!VGUI.currentEvent.IsKeyDown) return;

                if (VGUI.currentEvent.Modifiers != (EventModifiers.Command | EventModifiers.Shift) &&
                    VGUI.currentEvent.Modifiers != (EventModifiers.Control | EventModifiers.Shift)) return;

                if (VGUI.currentEvent.KeyCode != KeyCode.T) return;

                if (!EditorWindow.mouseOverWindow) return;
                if (!TabMenu.ReopenTabEnabled) return;
                if (!tabInfosForReopening.Any()) return;


                Event.current.Use();

                var window = tabInfosForReopening.Pop().CreateWindow();

                UpdateTitle(window);
            }
        }

        private static Vector2 addTabMenu_lastClickPosition_screenSpace;
        private static Vector2 addTabMenu_lastOpenPosition_screenSpace;
        private static EditorWindow addTabMenu_openedOverWindow;

        private static bool isKeyPressed;

        private static readonly Stack<TabInfo> tabInfosForReopening = new();

        [System.Serializable]
        private class TabInfo
        {
            public EditorWindow CreateWindow(object dockArea = null, bool atOriginalTabIndex = true)
            {
                dockArea ??= originalDockArea;
                if (dockArea == null) return null;
                if (dockArea.GetType() != t_DockArea) return null; // happens in 2023.2, no idea why

                object lastInteractedBrowser = t_ProjectBrowser.GetFieldValue("s_LastInteractedProjectBrowser"); // changes on new browser creation // tomove to seutup

                var window = (EditorWindow) ScriptableObject.CreateInstance(typeName);

                addToDockArea();

                setupBrowser();
                setupPropertyEditor();

                setCustomEditorWindowTitle();

                window.Focus();
                return window;

                void addToDockArea()
                {
                    if (atOriginalTabIndex) dockArea.InvokeMethod("AddTab", originalTabIndex, window, true);
                    else dockArea.InvokeMethod("AddTab", window, true);
                }

                void setupBrowser()
                {
                    if (!isBrowser) return;


                    window.InvokeMethod("Init");

                    setSavedGridSize();
                    setLastUsedGridSize();

                    setSavedLayout();
                    setLastUsedLayout();

                    setLastUsedListWidth();

                    lockToFolder_twoColumns();
                    lockToFolder_oneColumn();

                    UpdateBrowserTitle(window);
                    return;

                    void setSavedGridSize()
                    {
                        if (!isGridSizeSaved) return;

                        window.GetFieldValue("m_ListArea")?.SetMemberValue("gridSize", savedGridSize);
                    }

                    void setLastUsedGridSize()
                    {
                        if (isGridSizeSaved) return;
                        if (lastInteractedBrowser == null) return;

                        object listAreaSource = lastInteractedBrowser.GetFieldValue("m_ListArea");
                        object listAreaDest = window.GetFieldValue("m_ListArea");

                        if (listAreaSource != null && listAreaDest != null)
                            listAreaDest.SetPropertyValue("gridSize", listAreaSource.GetPropertyValue("gridSize"));
                    }

                    void setSavedLayout()
                    {
                        if (!isLayoutSaved) return;

                        // ReSharper disable once PossibleNullReferenceException
                        var layoutEnum = System.Enum.ToObject(t_ProjectBrowser.GetField("m_ViewMode", VGUI.maxBindingFlags).FieldType, savedLayout);

                        window.InvokeMethod("SetViewMode", layoutEnum);
                    }

                    void setLastUsedLayout()
                    {
                        if (isLayoutSaved) return;
                        if (lastInteractedBrowser == null) return;

                        window.InvokeMethod("SetViewMode", lastInteractedBrowser.GetMemberValue("m_ViewMode"));
                    }

                    void setLastUsedListWidth()
                    {
                        if (lastInteractedBrowser == null) return;

                        window.SetFieldValue("m_DirectoriesAreaWidth", lastInteractedBrowser.GetFieldValue("m_DirectoriesAreaWidth"));
                    }

                    void lockToFolder_twoColumns()
                    {
                        if (!isLocked) return;
                        if (window.GetMemberValue<int>("m_ViewMode") != 1) return;
                        if (string.IsNullOrEmpty(folderGuid)) return;


                        int iid = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(folderGuid)).GetInstanceID();

                        window.GetFieldValue("m_ListAreaState").SetFieldValue("m_SelectedInstanceIDs", new List<int> {iid});

                        t_ProjectBrowser.InvokeMethod("OpenSelectedFolders");


                        window.SetPropertyValue("isLocked", true);
                    }

                    void lockToFolder_oneColumn()
                    {
                        if (!isLocked) return;
                        if (window.GetMemberValue<int>("m_ViewMode") != 0) return;
                        if (string.IsNullOrEmpty(folderGuid)) return;

                        if (!(window.GetMemberValue("m_AssetTree") is { } m_AssetTree)) return;
                        if (!(m_AssetTree.GetMemberValue("data") is { } data)) return;

                        string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
                        int folderIid = AssetDatabase.LoadAssetAtPath<Object>(folderPath).GetInstanceID();

                        data.SetMemberValue("m_rootInstanceID", folderIid);

                        m_AssetTree.InvokeMethod("ReloadData");

                        window.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {folderPath});


                        window.SetPropertyValue("isLocked", true);
                    }
                }

                void setupPropertyEditor()
                {
                    if (!isPropertyEditor) return;
                    if (globalId.isNull) return;

                    var lockTo = globalId.GetObject();

                    if (lockedPrefabAssetObject)
                        lockTo = lockedPrefabAssetObject; // globalId api doesn't work for prefab asset objects, so we use direct object reference in such cases 

                    if (!lockTo) return;

                    window.GetMemberValue("tracker").InvokeMethod("SetObjectsLockedByThisTracker", new List<Object> {lockTo});

                    window.SetMemberValue("m_GlobalObjectId", globalId.ToString());
                    window.SetMemberValue("m_InspectedObject", lockTo);

                    UpdatePropertyEditorTitle(window);
                }

                void setCustomEditorWindowTitle()
                {
                    if (window.titleContent.text != window.GetType().FullName) return;
                    if (string.IsNullOrEmpty(originalTitle)) return;
                    window.titleContent.text = originalTitle;

                    // custom EditorWindows often have their titles set in EditorWindow.GetWindow
                    // and when such windows are created via ScriptableObject.CreateInstance, their titles default to window type name
                    // so we have to set original window title in such cases
                }
            }


            public TabInfo(EditorWindow window)
            {
                typeName = window.GetType().Name;
                originalDockArea = GetDockArea(window);
                originalTabIndex = GetTabList(window).IndexOf(window);
                originalTitle = window.titleContent.text;
                menuItemName = window.titleContent.text.Replace("/", " \u2215 ").Trim(' ');

                if (isBrowser)
                {
                    isLocked = window.GetPropertyValue<bool>("isLocked");
                    savedGridSize = window.GetFieldValue<int>("m_StartGridSize");
                    isGridSizeSaved = true;
                    savedLayout = window.GetMemberValue<int>("m_ViewMode");
                    isLayoutSaved = true;

                    string folderPath = savedLayout == 0
                        ? window.GetMemberValue("m_SearchFilter")?.GetMemberValue<string[]>("m_Folders")?.FirstOrDefault() ?? "Assets" // one column
                        : window.InvokeMethod<string>("GetActiveFolderPath"); // two columns
                    folderGuid = AssetDatabase.AssetPathToGUID(folderPath);
                }

                if (isPropertyEditor)
                    globalId = new VGUI.GlobalID(window.GetMemberValue<string>("m_GlobalObjectId"));
            }

            public TabInfo(Object lockTo)
            {
                isLocked = true;
                typeName = lockTo is DefaultAsset ? t_ProjectBrowser.Name : t_PropertyEditor.Name;

                if (isBrowser) folderGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(lockTo));

                if (isPropertyEditor) globalId = new VGUI.GlobalID(lockTo);

#if UNITY_2021_2_OR_NEWER
                if (isPropertyEditor)
                {
                    if (StageUtility.GetCurrentStage() is PrefabStage && globalId.ToString().Contains("00000000000000000000000000000000"))
                        lockedPrefabAssetObject = lockTo;
                }
#endif
            }

            public TabInfo(string typeName, string menuItemName)
            {
                this.typeName = typeName;
                this.menuItemName = menuItemName;
            }

            public string typeName;
            public string menuItemName;
            public object originalDockArea;
            public int originalTabIndex;
            public string originalTitle;

            public bool isBrowser => typeName == t_ProjectBrowser.Name;
            public bool isLocked;
            public string folderGuid = "";
            public int savedGridSize;
            public int savedLayout;
            public bool isGridSizeSaved;
            public bool isLayoutSaved;

            public bool isPropertyEditor => typeName == t_PropertyEditor.Name;
            public VGUI.GlobalID globalId;
            public Object lockedPrefabAssetObject;
        }

        [System.Serializable]
        private class TabInfoList
        {
            public List<TabInfo> list = new();
        }

        private static void UpdateTitle(EditorWindow window)
        {
            if (window == null) return;

            bool isPropertyEditor = window.GetType() == t_PropertyEditor;
            bool isBrowser = window.GetType() == t_ProjectBrowser;

            if (!isPropertyEditor && !isBrowser) return;

            if (isPropertyEditor) UpdatePropertyEditorTitle(window);

            if (isBrowser)
            {
                if (window.GetPropertyValue<bool>("isLocked")) UpdateBrowserTitle(window);
            }
        }

        private static void UpdateBrowserTitle(EditorWindow browser)
        {
            var isLocked = browser.GetMemberValue<bool>("isLocked");
            bool isTitleDefault = browser.titleContent.text == "Project";

            setLockedTitle();
            setDefaultTitle();
            return;

            void setLockedTitle()
            {
                if (!isLocked) return;

                bool isOneColumn = browser.GetMemberValue<int>("m_ViewMode") == 0;

                string path = isOneColumn
                    ? browser.GetMemberValue("m_SearchFilter")?.GetMemberValue<string[]>("m_Folders")?.FirstOrDefault() ?? "Assets"
                    : browser.InvokeMethod<string>("GetActiveFolderPath");

                string name = Path.GetFileNameWithoutExtension(path);
                var icon = EditorGUIUtility.FindTexture("Project");

                browser.titleContent = new GUIContent(name, icon);

                t_DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            }

            void setDefaultTitle()
            {
                if (isLocked) return;
                if (isTitleDefault) return;

                var name = "Project";
                var icon = EditorGUIUtility.FindTexture("Project@2x");

                browser.titleContent = new GUIContent(name, icon);

                t_DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            }
        }

        private static void UpdateBrowserTitles() => allBrowsers.ToList().ForEach(UpdateBrowserTitle);

        private static void UpdatePropertyEditorTitle(EditorWindow propertyEditor)
        {
            var obj = propertyEditor.GetMemberValue<Object>("m_InspectedObject");

            if (!obj) return;


            string name = obj is Component component ? GetComponentName(component) : obj.name;
            var sourceIcon = AssetPreview.GetMiniThumbnail(obj);
            Texture2D adjustedIcon;

            getAdjustedIcon();

            propertyEditor.titleContent = new GUIContent(name, adjustedIcon);

            propertyEditor.SetMemberValue("m_InspectedObject", null);

            t_DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            return;

            void getAdjustedIcon()
            {
                if (adjustedObjectIconsBySourceIid.TryGetValue(sourceIcon.GetInstanceID(), out adjustedIcon)) return;

                adjustedIcon = new Texture2D(sourceIcon.width,
                    sourceIcon.height,
                    sourceIcon.format,
                    sourceIcon.mipmapCount,
                    false) {hideFlags = HideFlags.DontSave};
                adjustedIcon.SetPropertyValue("pixelsPerPoint", (sourceIcon.width / 16f).RoundToInt());

                Graphics.CopyTexture(sourceIcon, adjustedIcon);

                adjustedObjectIconsBySourceIid[sourceIcon.GetInstanceID()] = adjustedIcon;
            }
        }

        private static void UpdatePropertyEditorTitles()
        {
            foreach (var item in allPropertyEditors) UpdatePropertyEditorTitle(item);
        }

        private static readonly Dictionary<int, Texture2D> adjustedObjectIconsBySourceIid = new();

        private static void UpdateGUIWrappingForBrowser(EditorWindow browser)
        {
            if (!browser.hasFocus) return;
            var isLocked = browser.GetMemberValue<bool>("isLocked");
            bool isWrapped = browser.GetMemberValue("m_Parent").GetMemberValue<Delegate>("m_OnGUI").Method.Name == nameof(WrappedBrowserOnGUI);

            wrap();
            unwrap();
            return;

            void wrap()
            {
                if (!isLocked) return;
                if (isWrapped) return;

                object hostView = browser.GetMemberValue("m_Parent");

                var newDelegate = typeof(TabEverything).GetMethod(nameof(WrappedBrowserOnGUI), VGUI.maxBindingFlags)?.CreateDelegate(t_EditorWindowDelegate, browser);

                hostView.SetMemberValue("m_OnGUI", newDelegate);

                browser.Repaint();


                browser.SetMemberValue("useTreeViewSelectionInsteadOfMainSelection", false);
            }

            void unwrap()
            {
                if (isLocked) return;
                if (!isWrapped) return;

                object hostView = browser.GetMemberValue("m_Parent");

                object originalDelegate = hostView.InvokeMethod("CreateDelegate", "OnGUI");

                hostView.SetMemberValue("m_OnGUI", originalDelegate);

                browser.Repaint();
            }
        }

        private static void UpdateGUIWrappingForAllBrowsers()
        {
            foreach (var item in allBrowsers) UpdateGUIWrappingForBrowser(item);
        }

        private static void WrappedBrowserOnGUI(EditorWindow browser)
        {
            const int headerHeight = 26;
            const int footerHeight = 21;
            const float breadcrubsYOffset = .5f;

            var headerRect = browser.position.SetPosition(0, 0).SetHeight(headerHeight);
            var footerRect = browser.position.SetPosition(0, 0).SetHeightFromBottom(footerHeight);
            var listAreaRect = browser.position.SetPosition(0, 0).AddHeight(-footerHeight).AddHeightFromBottom(-headerHeight);

            var breadcrumbsRect = headerRect.AddHeightFromBottom(-breadcrubsYOffset * 2);
            var topGapRect = headerRect.SetHeight(breadcrubsYOffset * 2);

            var breadcrumbsTint = EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.05f) : new Color(0, 0, 0, 0.02f);
            var topGapColor = EditorGUIUtility.isProSkin ? new Color(.24f, .24f, .24f, 1) : new Color(.8f, .8f, .8f, 1);

            bool isOneColumn = browser.GetMemberValue<int>("m_ViewMode") == 0;


            setRootForOneColumn();
            handleFolderChange();

            oneColumn();
            twoColumns();
            return;

            void setRootForOneColumn()
            {
                if (!isOneColumn) return;
                if (VGUI.currentEvent.IsRepaint) return;
                if (!(browser.GetMemberValue("m_AssetTree") is { } m_AssetTree)) return;
                if (!(m_AssetTree.GetMemberValue("data") is { } data)) return;

                var m_rootInstanceID = data.GetMemberValue<int>("m_rootInstanceID");

                setInitial();
                update();
                reset();
                return;

                void setInitial()
                {
                    if (m_rootInstanceID != 0) return;

                    string folderPath = browser.GetMemberValue("m_SearchFilter")?.GetMemberValue<string[]>("m_Folders")?.FirstOrDefault() ?? "Assets";
                    int folderIid = AssetDatabase.LoadAssetAtPath<Object>(folderPath).GetInstanceID();

                    data.SetMemberValue("m_rootInstanceID", folderIid);

                    m_AssetTree.InvokeMethod("ReloadData");
                }

                void update()
                {
                    if (m_rootInstanceID == 0) return;

                    int folderIid = m_rootInstanceID;
                    string folderPath = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(folderIid));

                    browser.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {folderPath});
                }

                void reset()
                {
                    if (browser.GetMemberValue<bool>("isLocked")) return;

                    data.SetMemberValue("m_rootInstanceID", 0);
                    browser.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {"Assets"});

                    m_AssetTree.InvokeMethod("ReloadData");

                    // returns the browser to normal state on unlock
                }
            }

            void handleFolderChange()
            {
                if (isOneColumn) return;

                onBreadcrumbsClick();
                onDoubleclick();
                onUndoRedo();
                return;

                void onBreadcrumbsClick()
                {
                    if (!VGUI.currentEvent.IsMouseUp) return;
                    if (!breadcrumbsRect.IsHovered()) return;

                    Undo.RecordObject(browser, "");

                    toCallInGUI += () => UpdateBrowserTitle(browser);
                    toCallInGUI += browser.Repaint;
                }

                void onDoubleclick()
                {
                    if (!VGUI.currentEvent.IsMouseDown) return;
                    if (VGUI.currentEvent.ClickCount != 2) return;

                    Undo.RecordObject(browser, "");

                    EditorApplication.delayCall += () => UpdateBrowserTitle(browser);
                    EditorApplication.delayCall += browser.Repaint;
                }

                void onUndoRedo()
                {
                    if (!VGUI.currentEvent.IsKeyDown) return;
                    if (!VGUI.currentEvent.HoldingCmdOrCtrl) return;
                    if (VGUI.currentEvent.KeyCode != KeyCode.Z) return;

                    string curFolderGuid = AssetDatabase.AssetPathToGUID(browser.InvokeMethod<string>("GetActiveFolderPath"));

                    EditorApplication.delayCall += () =>
                    {
                        string delayedFolderGuid = AssetDatabase.AssetPathToGUID(browser.InvokeMethod<string>("GetActiveFolderPath"));

                        if (delayedFolderGuid == curFolderGuid) return;


                        int folderIid = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(delayedFolderGuid)).GetInstanceID();

                        browser.InvokeMethod("SetFolderSelection", new[] {folderIid}, false);

                        UpdateBrowserTitle(browser);
                    };
                }
            }

            void oneColumn()
            {
                if (!isOneColumn) return;

                if (!browser.InvokeMethod<bool>("Initialized")) browser.InvokeMethod("Init");

                int m_TreeViewKeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

                browser.InvokeMethod("OnEvent");

                if (VGUI.currentEvent.IsMouseDown && browser.position.SetPosition(0, 0).IsHovered())
                    t_ProjectBrowser.SetFieldValue("s_LastInteractedProjectBrowser", browser);

                // header
                browser.SetFieldValue("m_ListHeaderRect", breadcrumbsRect);

                if (VGUI.currentEvent.IsRepaint) browser.InvokeMethod("BreadCrumbBar");

                EditorGUI.DrawRect(breadcrumbsRect, breadcrumbsTint);
                EditorGUI.DrawRect(topGapRect, topGapColor);

                // footer
                browser.SetFieldValue("m_BottomBarRect", footerRect);
                browser.InvokeMethod("BottomBar");

                // tree
                browser.GetMemberValue("m_AssetTree")?.InvokeMethod("OnGUI", listAreaRect, m_TreeViewKeyboardControlID);

                browser.InvokeMethod("HandleCommandEvents");
            }

            void twoColumns()
            {
                if (isOneColumn) return;


                if (!browser.InvokeMethod<bool>("Initialized"))
                    browser.InvokeMethod("Init");


                int m_ListKeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

                object startGridSize = browser.GetFieldValue("m_ListArea")?.GetMemberValue("gridSize");


                browser.InvokeMethod("OnEvent");

                if (VGUI.currentEvent.IsMouseDown && browser.position.SetPosition(0, 0).IsHovered())
                    t_ProjectBrowser.SetFieldValue("s_LastInteractedProjectBrowser", browser);

                // header
                browser.SetFieldValue("m_ListHeaderRect", breadcrumbsRect);

                browser.InvokeMethod("BreadCrumbBar");

                EditorGUI.DrawRect(breadcrumbsRect, breadcrumbsTint);
                EditorGUI.DrawRect(topGapRect, topGapColor);

                // footer
                browser.SetFieldValue("m_BottomBarRect", footerRect);
                browser.InvokeMethod("BottomBar");


                // list area
                browser.GetFieldValue("m_ListArea").InvokeMethod("OnGUI", listAreaRect, m_ListKeyboardControlID);

                // block grid size changes when ctrl-shift-scrolling
                if (VGUI.currentEvent.HoldingCmdOrCtrl) browser.GetFieldValue("m_ListArea").SetMemberValue("gridSize", startGridSize);


                browser.SetFieldValue("m_StartGridSize", browser.GetFieldValue("m_ListArea").GetMemberValue("gridSize"));

                browser.InvokeMethod("HandleContextClickInListArea", listAreaRect);
                browser.InvokeMethod("HandleCommandEvents");
            }
        }


        private static void ReplaceTabScrollerButtonsWithGradients()
        {
            getStyles();
            createTextures();
            assignTextures();
            return;

            void getStyles()
            {
                if (leftScrollerStyle != null && rightScrollerStyle != null) return;

                if (!guiStylesInitialized) TryInitializeGuiStyles();
                if (!guiStylesInitialized) return;

                if (typeof(GUISkin).GetFieldValue("current")?.GetFieldValue<Dictionary<string, GUIStyle>>("m_Styles")?.ContainsKey("dragtab scroller prev") !=
                    true) return;
                if (typeof(GUISkin).GetFieldValue("current")?.GetFieldValue<Dictionary<string, GUIStyle>>("m_Styles")?.ContainsKey("dragtab scroller next") !=
                    true) return;


                var t_Styles = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.DockArea+Styles");

                leftScrollerStyle = t_Styles.GetFieldValue<GUIStyle>("tabScrollerPrevButton");
                rightScrollerStyle = t_Styles.GetFieldValue<GUIStyle>("tabScrollerNextButton");
            }

            void createTextures()
            {
                if (leftScrollerGradient != null && rightScrollerGradient != null && clearTexture != null) return;

                clearTexture = new Texture2D(1, 1) {hideFlags = HideFlags.DontSave};
                clearTexture.SetPixel(0, 0, Color.clear);
                clearTexture.Apply();

                const int res = 16;
                float greyscale = EditorGUIUtility.isProSkin ? .16f : .65f;

                leftScrollerGradient = new Texture2D(res, 1) {hideFlags = HideFlags.DontSave};
                leftScrollerGradient.SetPixels(Enumerable.Range(0, res).Select(r => new Color(greyscale, greyscale, greyscale, r / (res - 1f))).Reverse().ToArray(), 0);
                leftScrollerGradient.Apply();

                rightScrollerGradient = new Texture2D(res, 1) {hideFlags = HideFlags.DontSave};
                rightScrollerGradient.SetPixels(Enumerable.Range(0, res).Select(r => new Color(greyscale, greyscale, greyscale, r / (res - 1f))).ToArray(), 0);
                rightScrollerGradient.Apply();
            }

            void assignTextures()
            {
                if (leftScrollerStyle == null) return;
                if (rightScrollerStyle == null) return;
                leftScrollerStyle.normal.background = leftScrollerGradient;
                rightScrollerStyle.normal.background = rightScrollerGradient;
            }
        }

        private static GUIStyle leftScrollerStyle;
        private static GUIStyle rightScrollerStyle;

        private static Texture2D leftScrollerGradient;
        private static Texture2D rightScrollerGradient;
        private static Texture2D clearTexture;


        private static void ClosePropertyEditorsWithNonLoadableObjects()
        {
            foreach (var propertyEditor in allPropertyEditors)
                if (propertyEditor.GetMemberValue<Object>("m_InspectedObject") == null)
                    propertyEditor.Close();
        }

        private static void LoadPropertyEditorInspectedObjects()
        {
            foreach (var propertyEditor in allPropertyEditors)
                propertyEditor.InvokeMethod("LoadPersistedObject");
        }


        private static void EnsureTabVisibleOnScroller(EditorWindow window)
        {
            float pos = GetOptimalTabScrollerPosition(window);

            if (!pos.Approximately(0)) pos += nonZeroTabScrollOffset;

            GetDockArea(window).SetFieldValue("m_ScrollOffset", pos);
        }

        private static void EnsureActiveTabsVisibleOnScroller()
        {
            var temp = L.Filter(allEditorWindows, r => r.hasFocus && !r.maximized && r.docked);
            foreach (var item in temp) EnsureTabVisibleOnScroller(item);
        }

        private static float GetOptimalTabScrollerPosition(EditorWindow activeTab)
        {
            object dockArea = activeTab.GetMemberValue("m_Parent");
            float tabAreaWidth = dockArea.GetFieldValue<Rect>("m_TabAreaRect").width;

            if (tabAreaWidth == 0)
                tabAreaWidth = activeTab.position.width - 38;

            if (tabStyle == null)
                if (guiStylesInitialized)
                    tabStyle = new GUIStyle("dragtab");
                else return 0;


            var activeTabXMin = 0f;
            var activeTabXMax = 0f;

            var tabWidthSum = 0f;

            var activeTabReached = false;

            foreach (var tab in GetTabList(activeTab))
            {
                var tabWidth = dockArea.InvokeMethod<float>("GetTabWidth", tabStyle, tab);

                tabWidthSum += tabWidth;


                if (activeTabReached) continue;

                activeTabXMin = activeTabXMax;
                activeTabXMax += tabWidth;

                if (tab == activeTab)
                    activeTabReached = true;
            }


            var optimalScrollPos = 0f;

            var visibleAreaPadding = 65f;

            float visibleAreaXMin = activeTabXMin - visibleAreaPadding;
            float visibleAreaXMax = activeTabXMax + visibleAreaPadding;

            optimalScrollPos = Mathf.Max(optimalScrollPos, visibleAreaXMax - tabAreaWidth);
            optimalScrollPos = Mathf.Min(optimalScrollPos, tabWidthSum - tabAreaWidth + 4);

            optimalScrollPos = Mathf.Min(optimalScrollPos, visibleAreaXMin);
            optimalScrollPos = Mathf.Max(optimalScrollPos, 0);


            return optimalScrollPos;
        }

        private static GUIStyle tabStyle;

        private const float nonZeroTabScrollOffset = 3f;

        [UnityEditor.Callbacks.PostProcessBuild]
        private static void OnBuild(BuildTarget _, string __)
        {
            EditorApplication.delayCall += LoadPropertyEditorInspectedObjects;
            EditorApplication.delayCall += UpdatePropertyEditorTitles;
        }

        private static void OnDomainReloaded()
        {
            toCallInGUI += UpdateGUIWrappingForAllBrowsers;
            toCallInGUI += UpdateBrowserTitles;
        }

        private static void OnSceneOpened(Scene _, OpenSceneMode __)
        {
            LoadPropertyEditorInspectedObjects();
            ClosePropertyEditorsWithNonLoadableObjects();
            UpdatePropertyEditorTitles();
        }

        private static void OnProjectLoaded()
        {
            toCallInGUI += EnsureActiveTabsVisibleOnScroller;

            UpdatePropertyEditorTitles();
        }

        private static void OnFocusedWindowChanged()
        {
            if (EditorWindow.focusedWindow?.GetType() == t_ProjectBrowser)
                UpdateGUIWrappingForBrowser(EditorWindow.focusedWindow);
        }

        private static void OnWindowUnmaximized()
        {
            UpdatePropertyEditorTitles();
            UpdateBrowserTitles();

            UpdateGUIWrappingForAllBrowsers();


            EnsureActiveTabsVisibleOnScroller();
        }


        private static void CheckIfFocusedWindowChanged()
        {
            if (prevFocusedWindow != EditorWindow.focusedWindow) OnFocusedWindowChanged();

            prevFocusedWindow = EditorWindow.focusedWindow;
        }

        private static EditorWindow prevFocusedWindow;


        private static void CheckIfWindowWasUnmaximized()
        {
            bool isMaximized = EditorWindow.focusedWindow?.maximized == true;

            if (!isMaximized && wasMaximized) OnWindowUnmaximized();

            wasMaximized = isMaximized;
        }

        private static bool wasMaximized;


        private static void OnSomeGUI()
        {
            toCallInGUI?.Invoke();
            toCallInGUI = null;

            CheckIfFocusedWindowChanged();
        }

        private static void ProjectWindowItemOnGUI(string _, Rect __) => OnSomeGUI();
        private static void HierarchyWindowItemOnGUI(int _, Rect __) => OnSomeGUI();

        private static Action toCallInGUI;


        private static void DelayCallLoop()
        {
            UpdateBrowserTitles();
            UpdateGUIWrappingForAllBrowsers();
            UpdateDelayedMousePosition();
            ReplaceTabScrollerButtonsWithGradients();

            EditorApplication.delayCall -= DelayCallLoop;
            EditorApplication.delayCall += DelayCallLoop;
        }


        private static void UpdateDelayedMousePosition()
        {
            var lastEvent = typeof(Event).GetFieldValue<Event>("s_Current");

            delayedMousePosition_screenSpace = GUIUtility.GUIToScreenPoint(lastEvent.mousePosition);
        }

        private static Vector2 delayedMousePosition_screenSpace;


        private static void ComponentTabHeaderGUI(UnityEditor.Editor editor)
        {
            if (!(editor.target is Component component)) return;

            GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
            var headerRect = GUILayoutUtility.GetLastRect().MoveY(-48).SetHeight(50).AddWidthFromMid(8);
            var nameRect = headerRect.MoveX(43).MoveY(5).SetHeight(20).SetXMax(headerRect.xMax - 50);
            var subtextRect = headerRect.MoveX(43).MoveY(22).SetHeight(20);


            hideName();
            name();
            componentOf();
            goName();
            GUILayout.Space(-4);
            return;

            void hideName()
            {
                var maskRect = headerRect.AddWidthFromRight(-45).AddWidth(-50);

                float brightness = EditorGUIUtility.isProSkin ? .24f : .8f;
                var maskColor = new Color(brightness, brightness, brightness, 1);

                EditorGUI.DrawRect(maskRect, maskColor);
            }

            void name()
            {
                VGUI.SetLabelFontSize(13);

                GUI.Label(nameRect, GetComponentName(component));

                VGUI.ResetLabelStyle();
            }

            void componentOf()
            {
                VGUI.SetGUIEnabled(false);

                GUI.Label(subtextRect, "Component of");

                VGUI.ResetGUIEnabled();
            }

            void goName()
            {
                var goNameRect = subtextRect.MoveX("Component of ".GetLabelWidth() - 3).SetWidth(component.gameObject.name.GetLabelWidth(isBold: true));

                goNameRect.MarkInteractive();

                VGUI.SetGUIEnabled(goNameRect.IsHovered() && !mousePressedOnGoName);
                VGUI.SetLabelBold();

                GUI.Label(goNameRect, component.gameObject.name);

                VGUI.ResetGUIEnabled();
                VGUI.ResetLabelStyle();

                if (VGUI.currentEvent.IsMouseDown && goNameRect.IsHovered())
                {
                    mousePressedOnGoName = true;
                    VGUI.currentEvent.Use();
                }

                if (VGUI.currentEvent.IsMouseUp)
                {
                    if (mousePressedOnGoName) EditorGUIUtility.PingObject(component.gameObject);

                    mousePressedOnGoName = false;
                    VGUI.currentEvent.Use();
                }

                goNameRect.x += 1;
                goNameRect.y += 1;
                goNameRect.width -= 1 * 2;
                goNameRect.height -= 1 * 2;
                if (VGUI.currentEvent.IsMouseLeaveWindow || (!VGUI.currentEvent.IsLayout && !goNameRect.IsHovered())) mousePressedOnGoName = false;
            }
        }

        private static bool mousePressedOnGoName;

        private static string GetComponentName(Component component)
        {
            if (!component) return "";

            string name = new GUIContent(EditorGUIUtility.ObjectContent(component, component.GetType())).text;

            name = name.Substring(name.LastIndexOf('(') + 1);
            name = name.Substring(0, name.Length - 1);

            return name;
        }


        private static void PreventGameViewZoomOnShiftScroll() // called from Update
        {
            if (!VGUI.currentEvent.HoldingShift) return;
            if (Application.isPlaying) return; // zoom by scrolling is disabled in playmode anyway
            if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;
            if (hoveredWindow.GetType() != t_GameView) return;
            if (!(hoveredWindow.GetMemberValue("m_ZoomArea", false) is { } zoomArea)) return;

            bool isScroll = !VGUI.currentEvent.IsMouseMove && VGUI.currentEvent.MouseDelta != Vector2.zero;

            if (isScroll)
            {
                zoomArea.SetMemberValue("m_Scale", lastGameViewScale);
                zoomArea.SetMemberValue("m_Translation", lastGameViewTranslation);
            }
            else
            {
                lastGameViewScale = zoomArea.GetMemberValue<Vector2>("m_Scale");
                lastGameViewTranslation = zoomArea.GetMemberValue<Vector2>("m_Translation");
            }
        }

        private static Vector2 lastGameViewScale = Vector2.one;
        private static Vector2 lastGameViewTranslation = Vector2.zero;


        private static void TryInitializeGuiStyles() => EditorWindow.focusedWindow?.SendEvent(EditorGUIUtility.CommandEvent(""));

        private static bool guiStylesInitialized => typeof(GUI).GetFieldValue("s_Skin") != null;


        private static object GetDockArea(EditorWindow window) => window.GetFieldValue("m_Parent");

        private static List<EditorWindow> GetTabList(EditorWindow window) => GetDockArea(window).GetFieldValue<List<EditorWindow>>("m_Panes");


        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (TabMenu.PluginDisabled) return;


            // dragndrop and scrolling
            EditorApplication.delayCall += () => EditorApplication.update -= Update;
            EditorApplication.delayCall += () => EditorApplication.update += Update;

            EditorApplication.delayCall -= UpdateDelayedMousePosition;
            EditorApplication.delayCall += UpdateDelayedMousePosition;

            EditorApplication.update -= PreventGameViewZoomOnShiftScroll;
            EditorApplication.update += PreventGameViewZoomOnShiftScroll;


            // shortcuts
            var globalEventHandler = typeof(EditorApplication).GetFieldValue<EditorApplication.CallbackFunction>("globalEventHandler");
            typeof(EditorApplication).SetFieldValue("globalEventHandler", globalEventHandler - CheckShortcuts + CheckShortcuts);


            // component tabs
            UnityEditor.Editor.finishedDefaultHeaderGUI -= ComponentTabHeaderGUI;
            UnityEditor.Editor.finishedDefaultHeaderGUI += ComponentTabHeaderGUI;


            // state change detectors
            var projectWasLoaded = typeof(EditorApplication).GetFieldValue<UnityEngine.Events.UnityAction>("projectWasLoaded");
            typeof(EditorApplication).SetFieldValue("projectWasLoaded", projectWasLoaded - OnProjectLoaded + OnProjectLoaded);

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorApplication.projectWindowItemOnGUI -= ProjectWindowItemOnGUI;
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;

            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;

            EditorApplication.delayCall -= DelayCallLoop;
            EditorApplication.delayCall += DelayCallLoop;


            OnDomainReloaded();
        }

        private static IEnumerable<EditorWindow> allBrowsers => _allBrowsers ??= t_ProjectBrowser.GetFieldValue<IList>("s_ProjectBrowsers").Cast<EditorWindow>();
        private static IEnumerable<EditorWindow> _allBrowsers;

        private static IEnumerable<EditorWindow> allPropertyEditors =>
            Resources.FindObjectsOfTypeAll(t_PropertyEditor).Where(r => r.GetType().BaseType == typeof(EditorWindow)).Cast<EditorWindow>();

        private static List<EditorWindow> allEditorWindows => Resources.FindObjectsOfTypeAll<EditorWindow>().ToList();

        private static readonly Type t_DockArea = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.DockArea");
        private static readonly Type t_PropertyEditor = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        private static readonly Type t_ProjectBrowser = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        private static readonly Type t_GameView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        private static readonly Type t_SceneHierarchyWindow = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        private static readonly Type t_HostView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HostView");
        private static readonly Type t_EditorWindowDelegate = t_HostView.GetNestedType("EditorWindowDelegate", VGUI.maxBindingFlags);
    }

    internal static class VGUI
    {
        #region Shortcuts

        public static float GetLabelWidth(this string s) => GUI.skin.label.CalcSize(new GUIContent(s)).x;

        public static float GetLabelWidth(this string s, bool isBold)
        {
            if (isBold) SetLabelBold();

            float r = s.GetLabelWidth();

            if (isBold) ResetLabelStyle();

            return r;
        }

        public static void SetGUIEnabled(bool enabled)
        {
            _prevGuiEnabled = GUI.enabled;
            GUI.enabled = enabled;
        }

        public static void ResetGUIEnabled() => GUI.enabled = _prevGuiEnabled;
        private static bool _prevGuiEnabled = true;

        public static void SetLabelFontSize(int size) => GUI.skin.label.fontSize = size;
        public static void SetLabelBold() => GUI.skin.label.fontStyle = FontStyle.Bold;

        public static void ResetLabelStyle()
        {
            GUI.skin.label.fontSize = 0;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        }


        private static bool _guiColorModified;
        private static Color _defaultGuiColor;

        #endregion

        #region Events

        public readonly struct WrappedEvent
        {
            private readonly Event e;

            public bool IsNull => e == null;
            public bool IsRepaint => IsNull ? default : e.type == EventType.Repaint;
            public bool IsLayout => IsNull ? default : e.type == EventType.Layout;
            public bool IsMouseLeaveWindow => IsNull ? default : e.type == EventType.MouseLeaveWindow;
            public bool IsKeyDown => IsNull ? default : e.type == EventType.KeyDown;
            public KeyCode KeyCode => IsNull ? default : e.keyCode;
            public bool IsMouseDown => IsNull ? default : e.type == EventType.MouseDown;
            public bool IsMouseUp => IsNull ? default : e.type == EventType.MouseUp;
            public bool IsMouseMove => IsNull ? default : e.type == EventType.MouseMove;
            public int ClickCount => IsNull ? default : e.clickCount;
            public Vector2 MousePosition => IsNull ? default : e.mousePosition;
            public Vector2 MousePositionScreenSpace => IsNull ? default : GUIUtility.GUIToScreenPoint(e.mousePosition);
            public Vector2 MouseDelta => IsNull ? default : e.delta;
            public EventModifiers Modifiers => IsNull ? default : e.modifiers;

            public bool HoldingShift => IsNull ? default : e.shift;
            public bool HoldingCmdOrCtrl => IsNull ? default : e.command || e.control;

            public bool HoldingCtrlOnly => IsNull ? default : e.modifiers == EventModifiers.Control;
            public bool HoldingCmdOnly => IsNull ? default : e.modifiers == EventModifiers.Command;
            public void Use() => e?.Use();

            public WrappedEvent(Event e) => this.e = e;

            public override string ToString() => e.ToString();
        }

        private static WrappedEvent Wrap(this Event e) => new(e);
        public static WrappedEvent currentEvent => (Event.current ?? typeof(Event).GetField("s_Current", maxBindingFlags)?.GetValue(null) as Event).Wrap();

        #endregion

        #region Drawing

        public static bool IsHovered(this Rect r) => !currentEvent.IsNull && r.Contains(currentEvent.MousePosition);

        #endregion

        #region Other

        public static void MarkInteractive(this Rect rect)
        {
            if (!currentEvent.IsRepaint) return;

            var unclippedRect = (Rect) guiClipUnclipToWindow.Invoke(null, new object[] {rect});

            object curGuiView = currentGuiView.GetValue(null);

            guiViewMarkHotRegion.Invoke(curGuiView, new object[] {unclippedRect});
        }

        private static readonly PropertyInfo currentGuiView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView").GetProperty("current", maxBindingFlags);

        private static readonly MethodInfo guiViewMarkHotRegion =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GUIView").GetMethod("MarkHotRegion", maxBindingFlags);

        private static readonly MethodInfo guiClipUnclipToWindow = typeof(GUI).Assembly.GetType("UnityEngine.GUIClip")
            .GetMethod("UnclipToWindow",
                maxBindingFlags,
                null,
                new[] {typeof(Rect)},
                null);

        #endregion

        #region Reflection

        public static object GetFieldValue(this object o, string fieldName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetFieldInfo(fieldName) is { } fieldInfo) return fieldInfo.GetValue(target);

            if (exceptionIfNotFound) throw new System.Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static object GetPropertyValue(this object o, string propertyName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is { } propertyInfo) return propertyInfo.GetValue(target);


            if (exceptionIfNotFound) throw new System.Exception($"Property '{propertyName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static object GetMemberValue(this object o, string memberName, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetFieldInfo(memberName) is { } fieldInfo) return fieldInfo.GetValue(target);

            if (type.GetPropertyInfo(memberName) is { } propertyInfo) return propertyInfo.GetValue(target);

            if (exceptionIfNotFound) throw new System.Exception($"Member '{memberName}' not found in type '{type.Name}' and its parent types");

            return null;
        }

        public static void SetFieldValue(this object o, string fieldName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetFieldInfo(fieldName) is { } fieldInfo)
                fieldInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new System.Exception($"Field '{fieldName}' not found in type '{type.Name}' and its parent types");
        }

        public static void SetPropertyValue(this object o, string propertyName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetPropertyInfo(propertyName) is { } propertyInfo)
                propertyInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new System.Exception($"Property '{propertyName}' not found in type '{type.Name}' and its parent types");
        }

        public static void SetMemberValue(this object o, string memberName, object value, bool exceptionIfNotFound = true)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;


            if (type.GetFieldInfo(memberName) is { } fieldInfo)
                fieldInfo.SetValue(target, value);

            else if (type.GetPropertyInfo(memberName) is { } propertyInfo)
                propertyInfo.SetValue(target, value);


            else if (exceptionIfNotFound)
                throw new System.Exception($"Member '{memberName}' not found in type '{type.Name}' and its parent types");
        }

        public static object InvokeMethod(this object o, string methodName, params object[] parameters)
        {
            var type = o as Type ?? o.GetType();
            object target = o is Type ? null : o;

            if (type.GetMethodInfo(methodName, parameters.Select(r => r.GetType()).ToArray()) is { } methodInfo) return methodInfo.Invoke(target, parameters);

            throw new System.Exception($"Method '{methodName}' not found in type '{type.Name}', its parent types and interfaces");
        }

        private static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            if (fieldInfoCache.TryGetValue(type, out var fieldInfosByNames))
            {
                if (fieldInfosByNames.TryGetValue(fieldName, out var fieldInfo)) return fieldInfo;
            }


            if (!fieldInfoCache.ContainsKey(type)) fieldInfoCache[type] = new Dictionary<string, FieldInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetField(fieldName, maxBindingFlags) is { } fieldInfo) return fieldInfoCache[type][fieldName] = fieldInfo;
            }


            return fieldInfoCache[type][fieldName] = null;
        }

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache = new();

        private static PropertyInfo GetPropertyInfo(this Type type, string propertyName)
        {
            if (propertyInfoCache.TryGetValue(type, out var propertyInfosByNames))
            {
                if (propertyInfosByNames.TryGetValue(propertyName, out var propertyInfo)) return propertyInfo;
            }

            if (!propertyInfoCache.ContainsKey(type)) propertyInfoCache[type] = new Dictionary<string, PropertyInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetProperty(propertyName, maxBindingFlags) is { } propertyInfo) return propertyInfoCache[type][propertyName] = propertyInfo;
            }


            return propertyInfoCache[type][propertyName] = null;
        }

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache = new();

        private static MethodInfo GetMethodInfo(this Type type, string methodName, params Type[] argumentTypes)
        {
            int methodHash = methodName.GetHashCode() ^ argumentTypes.Aggregate(0, (hash, r) => hash ^ r.GetHashCode());

            if (methodInfoCache.TryGetValue(type, out var methodInfosByHashes))
            {
                if (methodInfosByHashes.TryGetValue(methodHash, out var methodInfo)) return methodInfo;
            }

            if (!methodInfoCache.ContainsKey(type)) methodInfoCache[type] = new Dictionary<int, MethodInfo>();

            for (var curType = type; curType != null; curType = curType.BaseType)
            {
                if (curType.GetMethod(methodName,
                        maxBindingFlags,
                        null,
                        argumentTypes,
                        null) is { } methodInfo) return methodInfoCache[type][methodHash] = methodInfo;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.GetMethod(methodName,
                        maxBindingFlags,
                        null,
                        argumentTypes,
                        null) is { } methodInfo) return methodInfoCache[type][methodHash] = methodInfo;
            }

            return methodInfoCache[type][methodHash] = null;
        }

        private static readonly Dictionary<Type, Dictionary<int, MethodInfo>> methodInfoCache = new();

        public static T GetFieldValue<T>(this object o, string fieldName, bool exceptionIfNotFound = true) => (T) o.GetFieldValue(fieldName, exceptionIfNotFound);

        public static T GetPropertyValue<T>(this object o, string propertyName, bool exceptionIfNotFound = true) =>
            (T) o.GetPropertyValue(propertyName, exceptionIfNotFound);

        public static T GetMemberValue<T>(this object o, string memberName, bool exceptionIfNotFound = true) => (T) o.GetMemberValue(memberName, exceptionIfNotFound);
        public static T InvokeMethod<T>(this object o, string methodName, params object[] parameters) => (T) o.InvokeMethod(methodName, parameters);

        public const BindingFlags maxBindingFlags = (BindingFlags) 62;

        #endregion

        #region GlobalID

        [System.Serializable]
        public struct GlobalID : System.IEquatable<GlobalID>
        {
            public Object GetObject() => GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
            public int GetObjectInstanceId() => GlobalObjectId.GlobalObjectIdentifierToInstanceIDSlow(globalObjectId);

            public bool isNull => globalObjectId.identifierType == 0;

            public GlobalObjectId globalObjectId =>
                _globalObjectId.Equals(default) && globalObjectIdString != null && GlobalObjectId.TryParse(globalObjectIdString, out var r)
                    ? _globalObjectId = r
                    : _globalObjectId;

            public GlobalObjectId _globalObjectId;

            public GlobalID(Object o) => globalObjectIdString = (_globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(o)).ToString();
            public GlobalID(string s) => globalObjectIdString = GlobalObjectId.TryParse(s, out _globalObjectId) ? _globalObjectId.ToString() : s;

            public string globalObjectIdString;


            public bool Equals(GlobalID other) => globalObjectIdString.Equals(other.globalObjectIdString);

            public static bool operator ==(GlobalID a, GlobalID b) => a.Equals(b);
            public static bool operator !=(GlobalID a, GlobalID b) => !a.Equals(b);

            public override bool Equals(object other) => other is GlobalID otherglobalID && Equals(otherglobalID);
            public override int GetHashCode() => globalObjectIdString == null ? 0 : globalObjectIdString.GetHashCode();

            public override string ToString() => globalObjectIdString;
        }

        #endregion
    }
}