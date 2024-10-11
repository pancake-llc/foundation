using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Type = System.Type;
using Delegate = System.Delegate;
using Action = System.Action;
using Editor = PancakeEditor.Common.Editor;
using L = Pancake.Linq.L;

// ReSharper disable CognitiveComplexity
namespace PancakeEditor
{
    public static class TabEverything
    {
        private static void Update()
        {
            var lastEvent = typeof(Event).GetFieldValue<Event>("s_Current");


            ScrollInteractions();
            ScrollAnimation();
            CreateWindowDelayed();
            Dragndrop();

            CheckIfFocusedWindowChanged();
            CheckIfWindowWasUnmaximized();
            return;

            void ScrollInteractions()
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

                if (lastEvent.type != EventType.ScrollWheel && delayedMousePositionScreenSpace != GUIUtility.GUIToScreenPoint(lastEvent.mousePosition))
                    return; // uncaptured mouse move/drag check
                if (lastEvent.type != EventType.ScrollWheel && Application.platform == RuntimePlatform.OSXEditor &&
                    Mathf.Approximately(lastEvent.delta.x, (int) lastEvent.delta.x))
                    return; // osx uncaptured mouse move/drag in sceneview ang gameview workaround

                Shiftscroll();
                Sidescroll();
                return;

                void SwitchTab(int dir)
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

                void MoveTab(int dir)
                {
                    if (!TabMenu.MoveTabsEnabled) return;
                    if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;


                    var tabs = GetTabList(hoveredWindow);

                    int i0 = tabs.IndexOf(hoveredWindow);
                    int i1 = Mathf.Clamp(i0 + dir, 0, tabs.Count - 1);

                    (tabs[i0], tabs[i1]) = (tabs[i1], tabs[i0]);
                    tabs[i1].Focus();
                }

                void Shiftscroll()
                {
                    if (!lastEvent.shift) return;

                    float scrollDelta = Application.platform == RuntimePlatform.OSXEditor
                        ? lastEvent.delta.x // osx sends delta.y as delta.x when shift is pressed
                        : lastEvent.delta.x - lastEvent.delta.y; // some software on windows (ex logitech options) may do that too
                    if (TabMenu.ReverseScrollDirectionEnabled) scrollDelta *= -1;

                    if (scrollDelta != 0)
                    {
                        if (lastEvent.control || lastEvent.command) MoveTab(scrollDelta > 0 ? 1 : -1);
                        else SwitchTab(scrollDelta > 0 ? 1 : -1);
                    }
                }

                void Sidescroll()
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
                        if (lastEvent.control || lastEvent.command) MoveTab(sidescrollDelta < 0 ? 1 : -1);
                        else SwitchTab(sidescrollDelta < 0 ? 1 : -1);
                    }
                }
            }

            void ScrollAnimation()
            {
                if (!EditorWindow.focusedWindow) return;
                if (!EditorWindow.focusedWindow.docked) return;
                if (EditorWindow.focusedWindow.maximized) return;

                object dockArea = EditorWindow.focusedWindow.GetMemberValue("m_Parent");

                if (dockArea.GetType() != DockArea) return; // happens on 2021.1.28

                var curScroll = dockArea.GetFieldValue<float>("m_ScrollOffset");

                if (!curScroll.Approximately(0)) curScroll -= NON_ZERO_TAB_SCROLL_OFFSET;

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

                if (!newScroll.Approximately(0)) newScroll += NON_ZERO_TAB_SCROLL_OFFSET;

                dockArea.SetFieldValue("m_ScrollOffset", newScroll);

                EditorWindow.focusedWindow.Repaint();
            }

            void CreateWindowDelayed()
            {
                if (createWindowDelayedAction == null) return;
                if ((System.DateTime.UtcNow - lastDragndropTime).TotalSeconds < .05f) return;

                createWindowDelayedAction.Invoke();
                createWindowDelayedAction = null;
            }

            void Dragndrop()
            {
                if (!TabMenu.DragndropEnabled) return;
                if (lastEvent.type != EventType.DragUpdated && lastEvent.type != EventType.DragPerform) return;
                if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;
                if (!hoveredWindow.position.SetPosition(0, 0)
                        .SetHeight(hoveredWindow.GetType() == SceneHierarchyWindow ? 5 : 40)
                        .Contains(lastEvent.mousePosition)) return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (lastEvent.type != EventType.DragPerform) return;
                if (lastDragndropPosition == Editor.CurrentEvent.MousePosition) return;

                DragAndDrop.AcceptDrag();

                var lockToObject = DragAndDrop.objectReferences.First();
                object dockArea = hoveredWindow.GetMemberValue("m_Parent");

                createWindowDelayedAction =
                    () => new TabInfo(lockToObject).CreateWindow(dockArea, false); // not creating window right away to avoid scroll animation stutter

                lastDragndropPosition = Editor.CurrentEvent.MousePosition;
                lastDragndropTime = System.DateTime.UtcNow;
            }
        }

        private const float NON_ZERO_TAB_SCROLL_OFFSET = 3f;
        private static IEnumerable<EditorWindow> allBrowsers;
        private static readonly Type DockArea = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.DockArea");
        private static readonly Type PropertyEditor = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        private static readonly Type ProjectBrowser = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        private static readonly Type GameView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        private static readonly Type SceneHierarchyWindow = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        private static readonly Type HostView = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.HostView");
        private static readonly Type EditorWindowDelegate = HostView.GetNestedType("EditorWindowDelegate", TypeExtensions.MAX_BINDING_FLAGS);
        private static Vector2 lastGameViewScale = Vector2.one;
        private static Vector2 lastGameViewTranslation = Vector2.zero;
        private static bool mousePressedOnGoName;
        private static Vector2 delayedMousePositionScreenSpace;
        private static Action toCallInGUI;
        private static EditorWindow prevFocusedWindow;
        private static bool wasMaximized;
        private static GUIStyle leftScrollerStyle;
        private static GUIStyle rightScrollerStyle;
        private static Texture2D leftScrollerGradient;
        private static Texture2D rightScrollerGradient;
        private static Texture2D clearTexture;
        private static readonly Dictionary<int, Texture2D> AdjustedObjectIconsBySourceIid = new();
        private static Vector2 addTabMenuLastClickPositionScreenSpace;
        private static Vector2 addTabMenuLastOpenPositionScreenSpace;
        private static EditorWindow addTabMenuOpenedOverWindow;
        private static bool isKeyPressed;
        private static readonly Stack<TabInfo> TabInfosForReopening = new();
        private static float sidesscrollPosition;
        private static float scrollDeriv;
        private static float prevScroll;
        private static object prevFocusedDockArea;
        private static double deltaTime;
        private static double prevTime;
        private static Vector2 lastDragndropPosition;
        private static System.DateTime lastDragndropTime;
        private static Action createWindowDelayedAction;
        private static GUIStyle tabStyle;

        private static void CheckShortcuts()
        {
            void AddTab()
            {
                if (!Editor.CurrentEvent.IsKeyDown) return;
                if (!Editor.CurrentEvent.HoldingCmdOnly && !Editor.CurrentEvent.HoldingCtrlOnly) return;
                if (Editor.CurrentEvent.KeyCode != KeyCode.T) return;

                if (!EditorWindow.mouseOverWindow) return;
                if (!TabMenu.AddTabEnabled) return;

                Editor.CurrentEvent.Use();

                addTabMenuOpenedOverWindow = EditorWindow.mouseOverWindow;

                List<TabInfo> defaultTabList;
                List<TabInfo> customTabList;

                string customTabListKey = "tabs_customtablist_" + Application.dataPath.GetHashCode();

                LoadDefaultTabList();
                LoadSavedTabList();

                var menu = new GenericMenu();
                Vector2 menuPosition;

                void SetMenuPosition()
                {
                    if (Vector2.Distance(Editor.CurrentEvent.MousePositionScreenSpace, addTabMenuLastClickPositionScreenSpace) < 2)
                        menuPosition = GUIUtility.ScreenToGUIPoint(addTabMenuLastOpenPositionScreenSpace);
                    else menuPosition = Editor.CurrentEvent.MousePosition - Vector2.up * 9;

                    addTabMenuLastOpenPositionScreenSpace = GUIUtility.GUIToScreenPoint(menuPosition);

#if !UNITY_2021_2_OR_NEWER
                    if (EditorWindow.focusedWindow) menuPosition += EditorWindow.focusedWindow.position.position;
#endif
                }

                SetMenuPosition();

                DefaultTabs();
                SavedTabs();
                Remove();
                SaveCurrentTab();

                menu.DropDown(Rect.zero.SetPosition(menuPosition));
                return;

                void LoadDefaultTabList()
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

                void LoadSavedTabList()
                {
                    string json = EditorPrefs.GetString(customTabListKey);

                    customTabList = JsonUtility.FromJson<TabInfoList>(json)?.list ?? new List<TabInfo>();
                }

                void SaveSavedTabsList()
                {
                    string json = JsonUtility.ToJson(new TabInfoList {list = customTabList});

                    EditorPrefs.SetString(customTabListKey, json);
                }

                void RememberClickPosition() { addTabMenuLastClickPositionScreenSpace = Editor.CurrentEvent.MousePositionScreenSpace; }

                void DefaultTabs()
                {
                    menu.AddDisabledItem(new GUIContent("Default tabs"));

                    foreach (var tabInfo in defaultTabList)
                        menu.AddItem(new GUIContent(tabInfo.menuItemName),
                            false,
                            () =>
                            {
                                tabInfo.CreateWindow(GetDockArea(addTabMenuOpenedOverWindow), false);
                                EditorApplication.delayCall += RememberClickPosition;
                            });
                }

                void SavedTabs()
                {
                    if (!customTabList.Any()) return;

                    menu.AddSeparator("");

                    menu.AddDisabledItem(new GUIContent("Saved tabs"));


                    foreach (var tabInfo in customTabList)
                        if (tabInfo.IsPropertyEditor && !tabInfo.globalId.GetObject())
                            menu.AddDisabledItem(new GUIContent(tabInfo.menuItemName));
                        else
                            menu.AddItem(new GUIContent(tabInfo.menuItemName),
                                false,
                                () =>
                                {
                                    tabInfo.CreateWindow(GetDockArea(addTabMenuOpenedOverWindow), false);
                                    EditorApplication.delayCall += RememberClickPosition;
                                });
                }

                void Remove()
                {
                    if (!customTabList.Any()) return;


                    foreach (var tabInfo in customTabList)
                        menu.AddItem(new GUIContent("Remove/" + tabInfo.menuItemName),
                            false,
                            () =>
                            {
                                customTabList.Remove(tabInfo);
                                SaveSavedTabsList();
                                EditorApplication.delayCall += RememberClickPosition;
                            });


                    menu.AddSeparator("Remove/");

                    menu.AddItem(new GUIContent("Remove/Remove all"),
                        false,
                        () =>
                        {
                            customTabList.Clear();
                            SaveSavedTabsList();
                            EditorApplication.delayCall += RememberClickPosition;
                        });
                }

                void SaveCurrentTab()
                {
                    string menuItemName = addTabMenuOpenedOverWindow.titleContent.text.Replace("/", " \u2215 ").Trim(' ');

                    if (defaultTabList.Any(r => r.menuItemName == menuItemName)) return;
                    if (customTabList.Any(r => r.menuItemName == menuItemName)) return;

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Save current tab"),
                        false,
                        () =>
                        {
                            customTabList.Add(new TabInfo(addTabMenuOpenedOverWindow));
                            SaveSavedTabsList();
                            EditorApplication.delayCall += RememberClickPosition;
                        });
                }
            }

            SetIsKeyPressed();

            AddTab();
            CloseTab();
            ReopenTab();
            return;

            void SetIsKeyPressed()
            {
                if (Editor.CurrentEvent.KeyCode == KeyCode.LeftShift) return;
                if (Editor.CurrentEvent.KeyCode == KeyCode.LeftControl) return;
                if (Editor.CurrentEvent.KeyCode == KeyCode.LeftCommand) return;
                if (Editor.CurrentEvent.KeyCode == KeyCode.RightShift) return;
                if (Editor.CurrentEvent.KeyCode == KeyCode.RightControl) return;
                if (Editor.CurrentEvent.KeyCode == KeyCode.RightCommand) return;

                if (Event.current.type == EventType.KeyDown)
                    isKeyPressed = true;

                if (Event.current.type == EventType.KeyUp)
                    isKeyPressed = false;
            }

            void CloseTab()
            {
                if (!Editor.CurrentEvent.IsKeyDown) return;
                if (!Editor.CurrentEvent.HoldingCmdOnly && !Editor.CurrentEvent.HoldingCtrlOnly) return;
                if (Editor.CurrentEvent.KeyCode != KeyCode.W) return;

                if (!TabMenu.CloseTabEnabled) return;
                if (!EditorWindow.mouseOverWindow) return;
                if (!EditorWindow.mouseOverWindow.docked) return;
                if (GetTabList(EditorWindow.mouseOverWindow).Count <= 1) return;


                Event.current.Use();

                TabInfosForReopening.Push(new TabInfo(EditorWindow.mouseOverWindow));

                EditorWindow.mouseOverWindow.Close();
            }

            void ReopenTab()
            {
                if (!Editor.CurrentEvent.IsKeyDown) return;

                if (Editor.CurrentEvent.Modifiers != (EventModifiers.Command | EventModifiers.Shift) &&
                    Editor.CurrentEvent.Modifiers != (EventModifiers.Control | EventModifiers.Shift)) return;

                if (Editor.CurrentEvent.KeyCode != KeyCode.T) return;

                if (!EditorWindow.mouseOverWindow) return;
                if (!TabMenu.ReopenTabEnabled) return;
                if (!TabInfosForReopening.Any()) return;

                Event.current.Use();

                var window = TabInfosForReopening.Pop().CreateWindow();

                UpdateTitle(window);
            }
        }

        [System.Serializable]
        private class TabInfo
        {
            public EditorWindow CreateWindow(object dockArea = null, bool atOriginalTabIndex = true)
            {
                dockArea ??= originalDockArea;
                if (dockArea == null) return null;
                if (dockArea.GetType() != DockArea) return null; // happens in 2023.2, no idea why

                object lastInteractedBrowser = ProjectBrowser.GetFieldValue("s_LastInteractedProjectBrowser"); // changes on new browser creation // tomove to seutup

                var window = (EditorWindow) ScriptableObject.CreateInstance(typeName);

                AddToDockArea();

                SetupBrowser();
                SetupPropertyEditor();

                SetCustomEditorWindowTitle();

                window.Focus();
                return window;

                void AddToDockArea()
                {
                    if (atOriginalTabIndex) dockArea.InvokeMethod("AddTab", originalTabIndex, window, true);
                    else dockArea.InvokeMethod("AddTab", window, true);
                }

                void SetupBrowser()
                {
                    if (!IsBrowser) return;


                    window.InvokeMethod("Init");

                    SetSavedGridSize();
                    SetLastUsedGridSize();

                    SetSavedLayout();
                    SetLastUsedLayout();

                    SetLastUsedListWidth();

                    LockToFolderTwoColumns();
                    LockToFolderOneColumn();

                    UpdateBrowserTitle(window);
                    return;

                    void SetSavedGridSize()
                    {
                        if (!isGridSizeSaved) return;

                        window.GetFieldValue("m_ListArea")?.SetMemberValue("gridSize", savedGridSize);
                    }

                    void SetLastUsedGridSize()
                    {
                        if (isGridSizeSaved) return;
                        if (lastInteractedBrowser == null) return;

                        object listAreaSource = lastInteractedBrowser.GetFieldValue("m_ListArea");
                        object listAreaDest = window.GetFieldValue("m_ListArea");

                        if (listAreaSource != null && listAreaDest != null)
                            listAreaDest.SetPropertyValue("gridSize", listAreaSource.GetPropertyValue("gridSize"));
                    }

                    void SetSavedLayout()
                    {
                        if (!isLayoutSaved) return;

                        // ReSharper disable once PossibleNullReferenceException
                        var layoutEnum = System.Enum.ToObject(ProjectBrowser.GetField("m_ViewMode", TypeExtensions.MAX_BINDING_FLAGS).FieldType, savedLayout);

                        window.InvokeMethod("SetViewMode", layoutEnum);
                    }

                    void SetLastUsedLayout()
                    {
                        if (isLayoutSaved) return;
                        if (lastInteractedBrowser == null) return;

                        window.InvokeMethod("SetViewMode", lastInteractedBrowser.GetMemberValue("m_ViewMode"));
                    }

                    void SetLastUsedListWidth()
                    {
                        if (lastInteractedBrowser == null) return;

                        window.SetFieldValue("m_DirectoriesAreaWidth", lastInteractedBrowser.GetFieldValue("m_DirectoriesAreaWidth"));
                    }

                    void LockToFolderTwoColumns()
                    {
                        if (!isLocked) return;
                        if (window.GetMemberValue<int>("m_ViewMode") != 1) return;
                        if (string.IsNullOrEmpty(folderGuid)) return;


                        int iid = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(folderGuid)).GetInstanceID();

                        window.GetFieldValue("m_ListAreaState").SetFieldValue("m_SelectedInstanceIDs", new List<int> {iid});

                        ProjectBrowser.InvokeMethod("OpenSelectedFolders");


                        window.SetPropertyValue("isLocked", true);
                    }

                    void LockToFolderOneColumn()
                    {
                        if (!isLocked) return;
                        if (window.GetMemberValue<int>("m_ViewMode") != 0) return;
                        if (string.IsNullOrEmpty(folderGuid)) return;

                        if (!(window.GetMemberValue("m_AssetTree") is { } mAssetTree)) return;
                        if (!(mAssetTree.GetMemberValue("data") is { } data)) return;

                        string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
                        int folderIid = AssetDatabase.LoadAssetAtPath<Object>(folderPath).GetInstanceID();

                        data.SetMemberValue("m_rootInstanceID", folderIid);

                        mAssetTree.InvokeMethod("ReloadData");

                        window.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {folderPath});

                        window.SetPropertyValue("isLocked", true);
                    }
                }

                void SetupPropertyEditor()
                {
                    if (!IsPropertyEditor) return;
                    if (globalId.IsNull) return;

                    var lockTo = globalId.GetObject();

                    if (lockedPrefabAssetObject)
                        lockTo = lockedPrefabAssetObject; // globalId api doesn't work for prefab asset objects, so we use direct object reference in such cases 

                    if (!lockTo) return;

                    window.GetMemberValue("tracker").InvokeMethod("SetObjectsLockedByThisTracker", new List<Object> {lockTo});

                    window.SetMemberValue("m_GlobalObjectId", globalId.ToString());
                    window.SetMemberValue("m_InspectedObject", lockTo);

                    UpdatePropertyEditorTitle(window);
                }

                void SetCustomEditorWindowTitle()
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

                if (IsBrowser)
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

                if (IsPropertyEditor) globalId = new GlobalID(window.GetMemberValue<string>("m_GlobalObjectId"));
            }

            public TabInfo(Object lockTo)
            {
                isLocked = true;
                typeName = lockTo is DefaultAsset ? ProjectBrowser.Name : PropertyEditor.Name;

                if (IsBrowser) folderGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(lockTo));

                if (IsPropertyEditor) globalId = new GlobalID(lockTo);

#if UNITY_2021_2_OR_NEWER
                if (IsPropertyEditor)
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

            public bool IsBrowser => typeName == ProjectBrowser.Name;
            public bool isLocked;
            public string folderGuid = "";
            public int savedGridSize;
            public int savedLayout;
            public bool isGridSizeSaved;
            public bool isLayoutSaved;

            public bool IsPropertyEditor => typeName == PropertyEditor.Name;
            public GlobalID globalId;
            public Object lockedPrefabAssetObject;
        }

        [System.Serializable]
        private class TabInfoList
        {
            public List<TabInfo> list = new();
        }

        private static IEnumerable<EditorWindow> AllBrowsers => allBrowsers ??= ProjectBrowser.GetFieldValue<IList>("s_ProjectBrowsers").Cast<EditorWindow>();

        private static IEnumerable<EditorWindow> AllPropertyEditors =>
            Resources.FindObjectsOfTypeAll(PropertyEditor).Where(r => r.GetType().BaseType == typeof(EditorWindow)).Cast<EditorWindow>();

        private static List<EditorWindow> AllEditorWindows => Resources.FindObjectsOfTypeAll<EditorWindow>().ToList();

        private static void TryInitializeGuiStyles() => EditorWindow.focusedWindow?.SendEvent(EditorGUIUtility.CommandEvent(""));

        private static bool GUIStylesInitialized => typeof(GUI).GetFieldValue("s_Skin") != null;

        private static object GetDockArea(EditorWindow window) => window.GetFieldValue("m_Parent");

        private static List<EditorWindow> GetTabList(EditorWindow window) => GetDockArea(window).GetFieldValue<List<EditorWindow>>("m_Panes");

        private static void UpdateTitle(EditorWindow window)
        {
            if (window == null) return;

            bool isPropertyEditor = window.GetType() == PropertyEditor;
            bool isBrowser = window.GetType() == ProjectBrowser;

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

            SetLockedTitle();
            SetDefaultTitle();
            return;

            void SetLockedTitle()
            {
                if (!isLocked) return;

                bool isOneColumn = browser.GetMemberValue<int>("m_ViewMode") == 0;

                string path = isOneColumn
                    ? browser.GetMemberValue("m_SearchFilter")?.GetMemberValue<string[]>("m_Folders")?.FirstOrDefault() ?? "Assets"
                    : browser.InvokeMethod<string>("GetActiveFolderPath");

                string name = Path.GetFileNameWithoutExtension(path);
                var icon = EditorGUIUtility.FindTexture("Project");

                browser.titleContent = new GUIContent(name, icon);

                DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            }

            void SetDefaultTitle()
            {
                if (isLocked) return;
                if (isTitleDefault) return;

                var name = "Project";
                var icon = EditorGUIUtility.FindTexture("Project@2x");

                browser.titleContent = new GUIContent(name, icon);

                DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            }
        }

        private static void UpdateBrowserTitles() => AllBrowsers.ToList().ForEach(UpdateBrowserTitle);

        private static void UpdatePropertyEditorTitle(EditorWindow propertyEditor)
        {
            var obj = propertyEditor.GetMemberValue<Object>("m_InspectedObject");

            if (!obj) return;


            string name = obj is Component component ? GetComponentName(component) : obj.name;
            var sourceIcon = AssetPreview.GetMiniThumbnail(obj);
            Texture2D adjustedIcon;

            GetAdjustedIcon();

            propertyEditor.titleContent = new GUIContent(name, adjustedIcon);

            propertyEditor.SetMemberValue("m_InspectedObject", null);

            DockArea.GetFieldValue<IDictionary>("s_GUIContents").Clear();
            return;

            void GetAdjustedIcon()
            {
                if (AdjustedObjectIconsBySourceIid.TryGetValue(sourceIcon.GetInstanceID(), out adjustedIcon)) return;

                adjustedIcon = new Texture2D(sourceIcon.width,
                    sourceIcon.height,
                    sourceIcon.format,
                    sourceIcon.mipmapCount,
                    false) {hideFlags = HideFlags.DontSave};
                adjustedIcon.SetPropertyValue("pixelsPerPoint", (sourceIcon.width / 16f).RoundToInt());

                Graphics.CopyTexture(sourceIcon, adjustedIcon);

                AdjustedObjectIconsBySourceIid[sourceIcon.GetInstanceID()] = adjustedIcon;
            }
        }

        private static void UpdatePropertyEditorTitles()
        {
            foreach (var item in AllPropertyEditors) UpdatePropertyEditorTitle(item);
        }

        private static void UpdateGUIWrappingForBrowser(EditorWindow browser)
        {
            if (!browser.hasFocus) return;
            var isLocked = browser.GetMemberValue<bool>("isLocked");
            bool isWrapped = browser.GetMemberValue("m_Parent").GetMemberValue<Delegate>("m_OnGUI").Method.Name == nameof(WrappedBrowserOnGUI);

            Wrap();
            Unwrap();
            return;

            void Wrap()
            {
                if (!isLocked) return;
                if (isWrapped) return;

                object hostView = browser.GetMemberValue("m_Parent");

                var newDelegate = typeof(TabEverything).GetMethod(nameof(WrappedBrowserOnGUI), TypeExtensions.MAX_BINDING_FLAGS)
                    ?.CreateDelegate(EditorWindowDelegate, browser);

                hostView.SetMemberValue("m_OnGUI", newDelegate);

                browser.Repaint();


                browser.SetMemberValue("useTreeViewSelectionInsteadOfMainSelection", false);
            }

            void Unwrap()
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
            foreach (var item in AllBrowsers) UpdateGUIWrappingForBrowser(item);
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


            SetRootForOneColumn();
            HandleFolderChange();

            OneColumn();
            TwoColumns();
            return;

            void SetRootForOneColumn()
            {
                if (!isOneColumn) return;
                if (Editor.CurrentEvent.IsRepaint) return;
                if (!(browser.GetMemberValue("m_AssetTree") is { } mAssetTree)) return;
                if (!(mAssetTree.GetMemberValue("data") is { } data)) return;

                var mRootInstanceID = data.GetMemberValue<int>("m_rootInstanceID");

                SetInitial();
                LocalUpdate();
                Reset();
                return;

                void SetInitial()
                {
                    if (mRootInstanceID != 0) return;

                    string folderPath = browser.GetMemberValue("m_SearchFilter")?.GetMemberValue<string[]>("m_Folders")?.FirstOrDefault() ?? "Assets";
                    int folderIid = AssetDatabase.LoadAssetAtPath<Object>(folderPath).GetInstanceID();

                    data.SetMemberValue("m_rootInstanceID", folderIid);

                    mAssetTree.InvokeMethod("ReloadData");
                }

                void LocalUpdate()
                {
                    if (mRootInstanceID == 0) return;

                    int folderIid = mRootInstanceID;
                    string folderPath = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(folderIid));

                    browser.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {folderPath});
                }

                void Reset()
                {
                    if (browser.GetMemberValue<bool>("isLocked")) return;

                    data.SetMemberValue("m_rootInstanceID", 0);
                    browser.GetMemberValue("m_SearchFilter")?.SetMemberValue("m_Folders", new[] {"Assets"});

                    mAssetTree.InvokeMethod("ReloadData");

                    // returns the browser to normal state on unlock
                }
            }

            void HandleFolderChange()
            {
                if (isOneColumn) return;

                OnBreadcrumbsClick();
                OnDoubleclick();
                OnUndoRedo();
                return;

                void OnBreadcrumbsClick()
                {
                    if (!Editor.CurrentEvent.IsMouseUp) return;
                    if (!breadcrumbsRect.IsHovered()) return;

                    Undo.RecordObject(browser, "");

                    toCallInGUI += () => UpdateBrowserTitle(browser);
                    toCallInGUI += browser.Repaint;
                }

                void OnDoubleclick()
                {
                    if (!Editor.CurrentEvent.IsMouseDown) return;
                    if (Editor.CurrentEvent.ClickCount != 2) return;

                    Undo.RecordObject(browser, "");

                    EditorApplication.delayCall += () => UpdateBrowserTitle(browser);
                    EditorApplication.delayCall += browser.Repaint;
                }

                void OnUndoRedo()
                {
                    if (!Editor.CurrentEvent.IsKeyDown) return;
                    if (!Editor.CurrentEvent.HoldingCmdOrCtrl) return;
                    if (Editor.CurrentEvent.KeyCode != KeyCode.Z) return;

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

            void OneColumn()
            {
                if (!isOneColumn) return;

                if (!browser.InvokeMethod<bool>("Initialized")) browser.InvokeMethod("Init");

                int mTreeViewKeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

                browser.InvokeMethod("OnEvent");

                if (Editor.CurrentEvent.IsMouseDown && browser.position.SetPosition(0, 0).IsHovered())
                    ProjectBrowser.SetFieldValue("s_LastInteractedProjectBrowser", browser);

                // header
                browser.SetFieldValue("m_ListHeaderRect", breadcrumbsRect);

                if (Editor.CurrentEvent.IsRepaint) browser.InvokeMethod("BreadCrumbBar");

                EditorGUI.DrawRect(breadcrumbsRect, breadcrumbsTint);
                EditorGUI.DrawRect(topGapRect, topGapColor);

                // footer
                browser.SetFieldValue("m_BottomBarRect", footerRect);
                browser.InvokeMethod("BottomBar");

                // tree
                browser.GetMemberValue("m_AssetTree")?.InvokeMethod("OnGUI", listAreaRect, mTreeViewKeyboardControlID);

                browser.InvokeMethod("HandleCommandEvents");
            }

            void TwoColumns()
            {
                if (isOneColumn) return;


                if (!browser.InvokeMethod<bool>("Initialized"))
                    browser.InvokeMethod("Init");


                int mListKeyboardControlID = GUIUtility.GetControlID(FocusType.Keyboard);

                object startGridSize = browser.GetFieldValue("m_ListArea")?.GetMemberValue("gridSize");


                browser.InvokeMethod("OnEvent");

                if (Editor.CurrentEvent.IsMouseDown && browser.position.SetPosition(0, 0).IsHovered())
                    ProjectBrowser.SetFieldValue("s_LastInteractedProjectBrowser", browser);

                // header
                browser.SetFieldValue("m_ListHeaderRect", breadcrumbsRect);

                browser.InvokeMethod("BreadCrumbBar");

                EditorGUI.DrawRect(breadcrumbsRect, breadcrumbsTint);
                EditorGUI.DrawRect(topGapRect, topGapColor);

                // footer
                browser.SetFieldValue("m_BottomBarRect", footerRect);
                browser.InvokeMethod("BottomBar");


                // list area
                browser.GetFieldValue("m_ListArea").InvokeMethod("OnGUI", listAreaRect, mListKeyboardControlID);

                // block grid size changes when ctrl-shift-scrolling
                if (Editor.CurrentEvent.HoldingCmdOrCtrl) browser.GetFieldValue("m_ListArea").SetMemberValue("gridSize", startGridSize);


                browser.SetFieldValue("m_StartGridSize", browser.GetFieldValue("m_ListArea").GetMemberValue("gridSize"));

                browser.InvokeMethod("HandleContextClickInListArea", listAreaRect);
                browser.InvokeMethod("HandleCommandEvents");
            }
        }

        private static void ReplaceTabScrollerButtonsWithGradients()
        {
            GetStyles();
            CreateTextures();
            AssignTextures();
            return;

            void GetStyles()
            {
                if (leftScrollerStyle != null && rightScrollerStyle != null) return;

                if (!GUIStylesInitialized) TryInitializeGuiStyles();
                if (!GUIStylesInitialized) return;

                if (typeof(GUISkin).GetFieldValue("current")?.GetFieldValue<Dictionary<string, GUIStyle>>("m_Styles")?.ContainsKey("dragtab scroller prev") !=
                    true) return;
                if (typeof(GUISkin).GetFieldValue("current")?.GetFieldValue<Dictionary<string, GUIStyle>>("m_Styles")?.ContainsKey("dragtab scroller next") !=
                    true) return;


                var tStyles = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.DockArea+Styles");

                leftScrollerStyle = tStyles.GetFieldValue<GUIStyle>("tabScrollerPrevButton");
                rightScrollerStyle = tStyles.GetFieldValue<GUIStyle>("tabScrollerNextButton");
            }

            void CreateTextures()
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

            void AssignTextures()
            {
                if (leftScrollerStyle == null) return;
                if (rightScrollerStyle == null) return;
                leftScrollerStyle.normal.background = leftScrollerGradient;
                rightScrollerStyle.normal.background = rightScrollerGradient;
            }
        }

        private static void ClosePropertyEditorsWithNonLoadableObjects()
        {
            foreach (var propertyEditor in AllPropertyEditors)
                if (propertyEditor.GetMemberValue<Object>("m_InspectedObject") == null)
                    propertyEditor.Close();
        }

        private static void LoadPropertyEditorInspectedObjects()
        {
            foreach (var propertyEditor in AllPropertyEditors)
                propertyEditor.InvokeMethod("LoadPersistedObject");
        }

        private static void EnsureTabVisibleOnScroller(EditorWindow window)
        {
            float pos = GetOptimalTabScrollerPosition(window);

            if (!pos.Approximately(0)) pos += NON_ZERO_TAB_SCROLL_OFFSET;

            GetDockArea(window).SetFieldValue("m_ScrollOffset", pos);
        }

        private static void EnsureActiveTabsVisibleOnScroller()
        {
            var temp = L.Filter(AllEditorWindows, r => r.hasFocus && !r.maximized && r.docked);
            foreach (var item in temp) EnsureTabVisibleOnScroller(item);
        }

        private static float GetOptimalTabScrollerPosition(EditorWindow activeTab)
        {
            object dockArea = activeTab.GetMemberValue("m_Parent");
            float tabAreaWidth = dockArea.GetFieldValue<Rect>("m_TabAreaRect").width;

            if (tabAreaWidth == 0) tabAreaWidth = activeTab.position.width - 38;

            if (tabStyle == null)
            {
                if (GUIStylesInitialized) tabStyle = new GUIStyle("dragtab");
                else return 0;
            }

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

                if (tab == activeTab) activeTabReached = true;
            }


            var optimalScrollPos = 0f;

            const float visibleAreaPadding = 65f;

            float visibleAreaXMin = activeTabXMin - visibleAreaPadding;
            float visibleAreaXMax = activeTabXMax + visibleAreaPadding;

            optimalScrollPos = Mathf.Max(optimalScrollPos, visibleAreaXMax - tabAreaWidth);
            optimalScrollPos = Mathf.Min(optimalScrollPos, tabWidthSum - tabAreaWidth + 4);

            optimalScrollPos = Mathf.Min(optimalScrollPos, visibleAreaXMin);
            optimalScrollPos = Mathf.Max(optimalScrollPos, 0);

            return optimalScrollPos;
        }

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
            if (EditorWindow.focusedWindow?.GetType() == ProjectBrowser)
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

        private static void CheckIfWindowWasUnmaximized()
        {
            bool isMaximized = EditorWindow.focusedWindow?.maximized == true;

            if (!isMaximized && wasMaximized) OnWindowUnmaximized();

            wasMaximized = isMaximized;
        }

        private static void OnSomeGUI()
        {
            toCallInGUI?.Invoke();
            toCallInGUI = null;

            CheckIfFocusedWindowChanged();
        }

        private static void ProjectWindowItemOnGUI(string _, Rect __) => OnSomeGUI();
        private static void HierarchyWindowItemOnGUI(int _, Rect __) => OnSomeGUI();

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

            delayedMousePositionScreenSpace = GUIUtility.GUIToScreenPoint(lastEvent.mousePosition);
        }

        private static void ComponentTabHeaderGUI(UnityEditor.Editor editor)
        {
            if (!(editor.target is Component component)) return;

            GUILayout.Label("", GUILayout.Height(0), GUILayout.ExpandWidth(true));
            var headerRect = GUILayoutUtility.GetLastRect().AddY(-48).SetHeight(50).AddWidthFromMid(8);
            var nameRect = headerRect.AddX(43).AddY(5).SetHeight(20).SetXMax(headerRect.xMax - 50);
            var subtextRect = headerRect.AddX(43).AddY(22).SetHeight(20);


            HideName();
            Name();
            ComponentOf();
            GoName();
            GUILayout.Space(-4);
            return;

            void HideName()
            {
                var maskRect = headerRect.AddWidthFromRight(-45).AddWidth(-50);

                float brightness = EditorGUIUtility.isProSkin ? .24f : .8f;
                var maskColor = new Color(brightness, brightness, brightness, 1);

                EditorGUI.DrawRect(maskRect, maskColor);
            }

            void Name()
            {
                GUI.skin.label.fontSize = 13;

                GUI.Label(nameRect, GetComponentName(component));

                ResetLabelStyle();
            }

            void ComponentOf()
            {
                Uniform.SetGUIEnabled(false);

                GUI.Label(subtextRect, "Component of");

                Uniform.ResetGUIEnabled();
            }

            void GoName()
            {
                var goNameRect = subtextRect.AddX(GUI.skin.label.CalcSize(new GUIContent("Component of ")).x - 3)
                    .SetWidth(component.gameObject.name.GetLabelWidth(isBold: true));

                goNameRect.MarkInteractive();

                Uniform.SetGUIEnabled(goNameRect.IsHovered() && !mousePressedOnGoName);
                GUI.skin.label.fontStyle = FontStyle.Bold;

                GUI.Label(goNameRect, component.gameObject.name);

                Uniform.ResetGUIEnabled();
                ResetLabelStyle();

                if (Editor.CurrentEvent.IsMouseDown && goNameRect.IsHovered())
                {
                    mousePressedOnGoName = true;
                    Editor.CurrentEvent.Use();
                }

                if (Editor.CurrentEvent.IsMouseUp)
                {
                    if (mousePressedOnGoName) EditorGUIUtility.PingObject(component.gameObject);

                    mousePressedOnGoName = false;
                    Editor.CurrentEvent.Use();
                }

                goNameRect.x += 1;
                goNameRect.y += 1;
                goNameRect.width -= 1 * 2;
                goNameRect.height -= 1 * 2;
                if (Editor.CurrentEvent.IsMouseLeaveWindow || (!Editor.CurrentEvent.IsLayout && !goNameRect.IsHovered())) mousePressedOnGoName = false;
            }
        }

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
            if (!Editor.CurrentEvent.HoldingShift) return;
            if (Application.isPlaying) return; // zoom by scrolling is disabled in playmode anyway
            if (!(EditorWindow.mouseOverWindow is { } hoveredWindow)) return;
            if (hoveredWindow.GetType() != GameView) return;
            if (!(hoveredWindow.GetMemberValue("m_ZoomArea", false) is { } zoomArea)) return;

            bool isScroll = !Editor.CurrentEvent.IsMouseMove && Editor.CurrentEvent.MouseDelta != Vector2.zero;

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

        private static void ResetLabelStyle()
        {
            GUI.skin.label.fontSize = 0;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        }

        private static float GetLabelWidth(this string s, bool isBold)
        {
            if (isBold) GUI.skin.label.fontStyle = FontStyle.Bold;

            float r = GUI.skin.label.CalcSize(new GUIContent(s)).x;
            if (isBold) ResetLabelStyle();

            return r;
        }

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
    }
}