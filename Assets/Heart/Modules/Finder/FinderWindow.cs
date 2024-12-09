using System;
using System.Collections.Generic;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;


namespace PancakeEditor.Finder
{
    [Serializable]
    public class SelectHistory
    {
        public bool isSceneAssets;
        public UnityObject[] selection;

        public bool IsTheSame(UnityObject[] objects)
        {
            if (objects.Length != selection.Length) return false;
            var j = 0;
            for (; j < objects.Length; j++)
            {
                if (selection[j] != objects[j]) break;
            }

            return j == objects.Length;
        }
    }

    [Serializable]
    internal class PanelSettings
    {
        public bool selection;
        public bool horzLayout;
        public bool scene = true;
        public bool asset = true;
        public bool details;
        public bool bookmark;
        public bool toolMode;
        public bool showFullPath = true;
        public bool showFileSize;
        public bool showFileExtension;
        public bool showUsageType = true;
        public FindRefDrawer.Mode toolGroupMode = FindRefDrawer.Mode.Type;
        public FindRefDrawer.Mode groupMode = FindRefDrawer.Mode.Dependency;
        public FindRefDrawer.Sort sortMode = FindRefDrawer.Sort.Path;

        public int historyIndex;
        public List<SelectHistory> history = new();
    }

    public class FinderWindow : FinderWindowBase, IHasCustomMenu
    {
        [SerializeField] internal PanelSettings settings = new();

        public static void ShowWindow()
        {
            var window = GetWindow<FinderWindow>("Finder", true, TypeExtensions.InspectorWindow);
            if (window != null)
            {
                window.InitIfNeeded();
                window.Show();
            }
        }

        [NonSerialized] private AssetBookmark _bookmark;
        [NonSerialized] internal FindSelection selection;
        [NonSerialized] private AssetUsedInBuild _usedInBuild;
        [NonSerialized] private AssetDuplicateTree2 _duplicated;
        [NonSerialized] private FindRefDrawer _refUnUse;

        [NonSerialized] private FindRefDrawer _usesDrawer;
        [NonSerialized] private FindRefDrawer _usedByDrawer;
        [NonSerialized] private FindRefDrawer _sceneToAssetDrawer;
        [NonSerialized] private FindAddressableDrawer _addressableDrawer;

        [NonSerialized] private FindRefDrawer _refInScene;
        [NonSerialized] private FindRefDrawer _sceneUsesDrawer;
        [NonSerialized] private FindRefDrawer _refSceneInScene;


        internal int level;
        private Vector2 _scrollPos;
        private string _tempGuid;
        private string _tempFileID;
        private UnityObject _tempObject;

        protected bool LockSelection => selection is {isLock: true};

        private void OnEnable() { Repaint(); }

        protected void InitIfNeeded()
        {
            if (_usesDrawer != null) return;

            _usesDrawer = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected Assets] are not [USING] (depends on / contains reference to) any other assets!"
            };

            _usedByDrawer = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected Assets] are not [USED BY] any other assets!"
            };

            _addressableDrawer = new FindAddressableDrawer(this, () => settings.sortMode, () => settings.groupMode);

            _duplicated = new AssetDuplicateTree2(this, () => settings.sortMode, () => settings.toolGroupMode);
            _sceneToAssetDrawer = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected GameObjects] (in current open scenes) are not [USING] any assets!"
            };

            _refUnUse = new FindRefDrawer(this, () => settings.sortMode, () => settings.toolGroupMode) {groupDrawer = {hideGroupIfPossible = true}};

            _usedInBuild = new AssetUsedInBuild(this, () => settings.sortMode, () => settings.toolGroupMode);
            _bookmark = new AssetBookmark(this, () => settings.sortMode, () => settings.groupMode);
            selection = new FindSelection(this, () => settings.sortMode, () => settings.groupMode);

            _sceneUsesDrawer = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected GameObjects] are not [USING] any other GameObjects in scenes"
            };

            _refInScene = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected Assets] are not [USED BY] any GameObjects in opening scenes!"
            };

            _refSceneInScene = new FindRefDrawer(this, () => settings.sortMode, () => settings.groupMode)
            {
                messageEmpty = "[Selected GameObjects] are not [USED BY] by any GameObjects in opening scenes!"
            };

#if UNITY_2018_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#elif UNITY_2017_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged += OnSceneChanged;
#endif

            AssetCache.onCacheReady -= OnReady;
            AssetCache.onCacheReady += OnReady;

            FinderSetting.onIgnoreChange -= OnIgnoreChanged;
            FinderSetting.onIgnoreChange += OnIgnoreChanged;

            int idx = settings.historyIndex;
            if (idx != -1 && settings.history.Count > idx)
            {
                var h = settings.history[idx];
                Selection.objects = h.selection;
                settings.historyIndex = idx;
                RefreshOnSelectionChange();
                Repaint();
            }

            RefreshShowFullPath();
            RefreshShowFileSize();
            RefreshShowFileExtension();
            RefreshShowUsageType();
            Repaint();
        }

#if UNITY_2018_OR_NEWER
        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            if (IsFocusingFindInScene || IsFocusingSceneToAsset || IsFocusingSceneInScene)
            {
                OnSelectionChange();
            }
        }
#endif
        protected void OnIgnoreChanged()
        {
            _refUnUse.ResetUnusedAsset();
            _usedInBuild.SetDirty();
            OnSelectionChange();
        }

        protected void OnReady() { OnSelectionChange(); }

        private void AddHistory()
        {
            var objects = Selection.objects;

            // Check if the same set of selection has already existed
            RefreshHistoryIndex(objects);
            if (settings.historyIndex != -1) return;

            // Add newly selected objects to the selection
            const int maxHistoryLength = 10;
            settings.history.Add(new SelectHistory {selection = Selection.objects});
            settings.historyIndex = settings.history.Count - 1;
            if (settings.history.Count > maxHistoryLength)
            {
                settings.history.RemoveRange(0, settings.history.Count - maxHistoryLength);
            }

            EditorUtility.SetDirty(this);
        }

        private void RefreshHistoryIndex(UnityObject[] objects)
        {
            if (this == null) return;
            settings.historyIndex = -1;
            if (objects == null || objects.Length == 0) return;
            var history = settings.history;
            for (var i = 0; i < history.Count; i++)
            {
                var h = history[i];
                if (!h.IsTheSame(objects)) continue;
                settings.historyIndex = i;
            }

            try
            {
                EditorUtility.SetDirty(this);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private bool IsScenePanelVisible
        {
            get
            {
                if (IsFocusingAddressable) return false;
                if (selection.IsSelectingAsset && IsFocusingUses) return false;
                if (!selection.IsSelectingAsset && IsFocusingUsedBy) return true;
                return settings.scene;
            }
        }

        private bool IsAssetPanelVisible
        {
            get
            {
                if (IsFocusingAddressable) return false;
                if (selection.IsSelectingAsset && IsFocusingUses) return true;
                if (!selection.IsSelectingAsset && IsFocusingUsedBy) return false;
                return settings.asset;
            }
        }

        private void RefreshPanelVisible()
        {
            if (sp1 == null || sp2 == null) return;
            sp2.splits[0].visible = IsScenePanelVisible;
            sp2.splits[1].visible = IsAssetPanelVisible;
            sp2.splits[2].visible = IsFocusingAddressable;
            sp2.CalculateWeight();
        }

        private void RefreshOnSelectionChange()
        {
            _ids = FinderUtility.SelectionAssetGUIDs;
            selection.Clear();

            var gameObjects = Selection.gameObjects;

            //ignore selection on asset when selected any object in scene
            if (gameObjects.Length > 0 && !FinderUtility.IsInAsset(gameObjects[0]))
            {
                _ids = Array.Empty<string>();
                selection.AddRange(gameObjects);
            }
            else
            {
                selection.AddRange(_ids);
            }

            level = 0;
            RefreshPanelVisible();

            if (selection.IsSelectingAsset)
            {
                _usesDrawer.Reset(_ids, true);
                _usedByDrawer.Reset(_ids, false);
                _refInScene.Reset(_ids, this as IWindow);
                _addressableDrawer.RefreshView();
            }
            else
            {
                _refSceneInScene.ResetSceneInScene(gameObjects);
                _sceneToAssetDrawer.Reset(gameObjects, true, true);
                _sceneUsesDrawer.ResetSceneUseSceneObjects(gameObjects);
            }
        }

        public override void OnSelectionChange()
        {
            Repaint();

            if (!IsCacheReady) return;

            if (focusedWindow == null) return;
            if (_sceneUsesDrawer == null) InitIfNeeded();
            if (_usesDrawer == null) InitIfNeeded();

            if (!LockSelection)
            {
                RefreshOnSelectionChange();
                RefreshHistoryIndex(Selection.objects);
            }

            if (IsFocusingGUIDs)
            {
                _guidObjs = new Dictionary<string, UnityObject>();
                var objects = Selection.objects;
                for (var i = 0; i < objects.Length; i++)
                {
                    var item = objects[i];
                    try
                    {
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(item, out string guid, out long fileid)) _guidObjs.Add(guid + "/" + fileid, objects[i]);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            if (IsFocusingUnused) _refUnUse.ResetUnusedAsset();

            if (FindSceneCache.Api.Dirty && !Application.isPlaying) FindSceneCache.Api.RefreshCache(this);

            EditorApplication.delayCall -= Repaint;
            EditorApplication.delayCall += Repaint;
        }


        [NonSerialized] public SplitView sp1; // container : Selection / sp2 / Bookmark 
        [NonSerialized] public SplitView sp2; // Scene / Assets
        [NonSerialized] public SplitView sp3; // Addressable

        private void DrawHistory(Rect rect)
        {
            var c = GUI.backgroundColor;
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();

            for (var i = 0; i < settings.history.Count; i++)
            {
                var h = settings.history[i];
                int idx = i;
                GUI.backgroundColor = i == settings.historyIndex ? GUI2.darkBlue : c;

                var content = new GUIContent($"{i + 1}", "RightClick to delete!");
                if (GUILayout.Button(content, EditorStyles.miniButton, GUILayout.Width(24f)))
                {
                    if (Event.current.button == 0) // left click
                    {
                        Selection.objects = h.selection;
                        settings.historyIndex = idx;
                        RefreshOnSelectionChange();
                        Repaint();
                    }

                    if (Event.current.button == 1) // right click
                    {
                        bool isActive = i == settings.historyIndex;
                        settings.history.RemoveAt(idx);

                        if (isActive && settings.history.Count > 0)
                        {
                            int idx2 = settings.history.Count - 1;
                            Selection.objects = settings.history[idx2].selection;
                            settings.historyIndex = idx2;
                            RefreshOnSelectionChange();
                            Repaint();
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.backgroundColor = c;
        }

        private void InitPanes()
        {
            sp2 = new SplitView(this)
            {
                isHorz = false,
                splits = new List<SplitView.Info>
                {
                    new() {title = new GUIContent("Scene", Uniform.IconContent("SceneAsset Icon").image), draw = DrawScene, visible = settings.scene},
                    new() {title = new GUIContent("Assets", Uniform.IconContent("Folder Icon").image), draw = DrawAsset, visible = settings.asset},
                    new() {title = null, draw = rect => _addressableDrawer.Draw(rect), visible = false}
                }
            };

            sp2.CalculateWeight();

            sp1 = new SplitView(this)
            {
                isHorz = true,
                splits = new List<SplitView.Info>
                {
                    new()
                    {
                        title = new GUIContent("Selection", Uniform.IconContent("d_Selectable Icon").image),
                        weight = 0.4f,
                        visible = settings.selection,
                        draw = rect =>
                        {
                            var historyRect = rect;
                            historyRect.yMin = historyRect.yMax - 16f;

                            rect.yMax -= 16f;
                            selection.Draw(rect);
                            DrawHistory(historyRect);
                        }
                    },
                    new() {draw = r => { sp2.Draw(r); }},
                    new()
                    {
                        title = new GUIContent("Asset Detail", Uniform.IconContent("d_UnityEditor.SceneHierarchyWindow").image),
                        weight = 0.4f,
                        visible = settings.details,
                        draw = rect =>
                        {
                            var assetDrawer = GetAssetDrawer();
                            assetDrawer?.DrawDetails(rect);
                        }
                    },
                    new()
                    {
                        title = new GUIContent("Bookmark", Uniform.IconContent("d_Favorite").image),
                        weight = 0.4f,
                        visible = settings.bookmark,
                        draw = rect => _bookmark.Draw(rect)
                    }
                }
            };

            sp1.CalculateWeight();
        }

        private TabView _tabs;
        private TabView _toolTabs;
        private TabView _bottomTabs;
        private SearchView _search;

        private void DrawScene(Rect rect)
        {
            var drawer = IsFocusingUses ? selection.IsSelectingAsset ? null : _sceneUsesDrawer : selection.IsSelectingAsset ? _refInScene : _refSceneInScene;
            if (drawer == null) return;

            if (!FindSceneCache.ready)
            {
                var rr = rect;
                rr.height = 16f;

                int cur = FindSceneCache.Api.current, total = FindSceneCache.Api.total;
                EditorGUI.ProgressBar(rr, cur * 1f / total, $"{cur} / {total}");
                WillRepaint = true;
                return;
            }

            drawer.Draw(rect);

            var refreshRect = new Rect(rect.xMax - 16f, rect.yMin - 14f, 18f, 18f);
            if (GUI2.ColorIconButton(refreshRect, Uniform.IconContent("d_Refresh@2x").image, FindSceneCache.Api.Dirty ? GUI2.lightRed : (Color?) null))
                FindSceneCache.Api.RefreshCache(drawer.Window);
        }


        private FindRefDrawer GetAssetDrawer()
        {
            if (IsFocusingUses) return selection.IsSelectingAsset ? _usesDrawer : _sceneToAssetDrawer;
            if (IsFocusingUsedBy) return selection.IsSelectingAsset ? _usedByDrawer : null;
            if (IsFocusingAddressable) return _addressableDrawer.drawer;
            return null;
        }

        private void DrawAsset(Rect rect)
        {
            var drawer = GetAssetDrawer();
            if (drawer == null) return;
            drawer.Draw(rect);

            if (!drawer.showDetail) return;

            settings.details = true;
            drawer.showDetail = false;
            sp1.splits[2].visible = settings.details;
            sp1.CalculateWeight();
            Repaint();
        }

        private void DrawSearch()
        {
            if (_search == null) _search = new SearchView();
            _search.DrawLayout();
        }

        protected override void OnGUI() { OnGUI2(); }

        protected bool CheckDrawImport()
        {
            if (FinderUtility.isEditorCompiling)
            {
                EditorGUILayout.HelpBox("Compiling scripts, please wait!", MessageType.Warning);
                Repaint();
                return false;
            }

            if (FinderUtility.isEditorUpdating)
            {
                EditorGUILayout.HelpBox("Importing assets, please wait!", MessageType.Warning);
                Repaint();
                return false;
            }

            InitIfNeeded();

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorGUILayout.HelpBox("Finder requires serialization mode set to FORCE TEXT!", MessageType.Warning);
                if (GUILayout.Button("FORCE TEXT")) EditorSettings.serializationMode = SerializationMode.ForceText;

                return false;
            }

            if (IsCacheReady) return DrawEnable();

            if (!HasCache)
            {
                EditorGUILayout.HelpBox(
                    "Finder cache not found!\nFinder will need a full refresh and according to your project's size this process may take several minutes to complete finish!",
                    MessageType.Warning);

                if (GUILayout.Button("Scan project"))
                {
                    CreateCache();
                    Repaint();
                }

                return false;
            }

            if (!DrawEnable()) return false;
            if (CacheSetting.workCount > 0)
            {
                string text = "Refreshing ... " + (int) (CacheSetting.Progress * CacheSetting.workCount) + " / " + CacheSetting.workCount;
                var rect = GUILayoutUtility.GetRect(1f, Screen.width, 18f, 18f);
                EditorGUI.ProgressBar(rect, CacheSetting.Progress, text);
                Repaint();
            }
            else
            {
                CacheSetting.workCount = 0;
                CacheSetting.ready = true;
            }

            return false;
        }

        protected bool IsFocusingUses => _tabs is {current: 0};
        protected bool IsFocusingUsedBy => _tabs is {current: 1};
        protected bool IsFocusingAddressable => _tabs is {current: 2};

        // 
        protected bool IsFocusingDuplicate => _toolTabs is {current: 0};
        protected bool IsFocusingGUIDs => _toolTabs is {current: 1};
        protected bool IsFocusingUnused => _toolTabs is {current: 2};
        protected bool IsFocusingUsedInBuild => _toolTabs is {current: 3};

        private static readonly HashSet<FindRefDrawer.Mode> AllowedModes = new() {FindRefDrawer.Mode.Type, FindRefDrawer.Mode.Extension, FindRefDrawer.Mode.Folder};

        private void OnTabChange()
        {
            if (IsFocusingUnused || IsFocusingUsedInBuild)
            {
                if (!AllowedModes.Contains(settings.groupMode)) settings.groupMode = FindRefDrawer.Mode.Type;
            }

            if (_deleteUnused != null) _deleteUnused.hasConfirm = false;
            _usedInBuild?.SetDirty();
        }

        private void InitTabs()
        {
            _bottomTabs = TabView.Create(this,
                true,
                Uniform.IconContent("Settings@2x", "Settings"),
                Uniform.IconContent("ShurikenCheckMarkMixed", "Ignore"),
                Uniform.IconContent("d_FilterByType@2x", "Filter by Type"));
            _bottomTabs.current = -1;
            _bottomTabs.flexibleWidth = false;

            _toolTabs = TabView.Create(this,
                false,
                "Duplicate",
                "GUID",
                "Not Referenced",
                "In Build");
            _tabs = TabView.Create(this, false, "Uses", "Used By");
            if (FindAddressable.AsmStatus == FindAddressable.EAsmStatus.AsmNotFound)
            {
                _tabs = TabView.Create(this, false, "Uses", "Used By");
            }
            else
            {
                _tabs = TabView.Create(this,
                    false,
                    "Uses",
                    "Used By",
                    "Addressables");
            }

            _tabs.onTabChange = OnTabChange;
            const float iconW = 24f;
            _tabs.offsetFirst = iconW;
            _tabs.offsetLast = iconW * 5;

            _tabs.callback = new DrawCallback
            {
                beforeDraw = (rect) =>
                {
                    rect.width = iconW;
                    if (GUI2.ToolbarToggle(ref selection.isLock,
                            selection.isLock ? Uniform.IconContent("LockIcon-On", "Click to unlock").image : Uniform.IconContent("LockIcon", "Click to lock").image,
                            Vector2.zero,
                            "Lock Selection",
                            rect))
                    {
                        WillRepaint = true;
                        OnSelectionChange();
                        if (selection.isLock) AddHistory();
                    }
                },
                afterDraw = (rect) =>
                {
                    rect.xMin = rect.xMax - iconW * 5;
                    rect.width = iconW;
                    if (GUI2.ToolbarToggle(ref settings.selection,
                            Uniform.IconContent("d_Selectable Icon").image,
                            Vector2.zero,
                            "Show / Hide Selection",
                            rect))
                    {
                        sp1.splits[0].visible = settings.selection;
                        sp1.CalculateWeight();
                        Repaint();
                    }

                    rect.x += iconW;
                    if (GUI2.ToolbarToggle(ref settings.scene,
                            Uniform.IconContent("SceneAsset Icon").image,
                            Vector2.zero,
                            "Show / Hide Scene References",
                            rect))
                    {
                        if (settings.asset == false && settings.scene == false)
                        {
                            settings.asset = true;
                            sp2.splits[1].visible = settings.asset;
                        }

                        RefreshPanelVisible();
                        Repaint();
                    }

                    rect.x += iconW;
                    if (GUI2.ToolbarToggle(ref settings.asset,
                            Uniform.IconContent("Folder Icon").image,
                            Vector2.zero,
                            "Show / Hide Asset References",
                            rect))
                    {
                        if (settings.asset == false && settings.scene == false)
                        {
                            settings.scene = true;
                            sp2.splits[0].visible = settings.scene;
                        }

                        RefreshPanelVisible();
                        Repaint();
                    }

                    rect.x += iconW;
                    if (GUI2.ToolbarToggle(ref settings.details,
                            Uniform.IconContent("d_UnityEditor.SceneHierarchyWindow").image,
                            Vector2.zero,
                            "Show / Hide Details",
                            rect))
                    {
                        sp1.splits[2].visible = settings.details;
                        sp1.CalculateWeight();
                        Repaint();
                    }

                    rect.x += iconW;
                    if (GUI2.ToolbarToggle(ref settings.bookmark,
                            Uniform.IconContent("d_Favorite").image,
                            Vector2.zero,
                            "Show / Hide Bookmarks",
                            rect))
                    {
                        sp1.splits[3].visible = settings.bookmark;
                        sp1.CalculateWeight();
                        Repaint();
                    }
                }
            };
        }

        private (Rect left, Rect flex) ExtractLeft(Rect r, float leftWidth = 16f, float space = 0f)
        {
            float deltaW = leftWidth + space;
            var (left, flex) = (new Rect(r.x, r.y, leftWidth, r.height), new Rect(r.x + deltaW, r.y, r.width - deltaW, r.height));
            if (leftWidth < 0f) Debug.LogWarning($"Invalid:: r {r} | leftWidth = {leftWidth} | left = {left} | flex = {flex}");
            return (left, flex);
        }

        private (Rect right, Rect flex) ExtractRight(Rect r, float rightWidth = 16f, float space = 0f)
        {
            float deltaW = rightWidth + space;
            return (new Rect(r.x + r.width - deltaW, r.y, deltaW, r.height), new Rect(r.x, r.y, r.width - deltaW, r.height));
        }

        protected bool DrawFooter()
        {
            _bottomTabs.DrawLayout();
            var bottomBar = GUILayoutUtility.GetLastRect();

            bottomBar.xMin += 100f; // offset for left buttons

            var (fullPathRect, flex1) = ExtractLeft(bottomBar, 24f);
            var (fileSizeRect, flex2) = ExtractLeft(flex1, 24f);
            var (extensionRect, flex3) = ExtractLeft(flex2, 24f);

            var (buttonRect, _) = ExtractRight(flex3, 24f);

            bottomBar = flex3;

            var viewModeRect = bottomBar;
            viewModeRect.xMax -= 24f;
            viewModeRect.xMin = viewModeRect.xMax - 200f;

            DrawViewModes(viewModeRect);
            DrawButton(buttonRect, ref settings.toolMode, Uniform.IconContent("CustomTool", "Advanced Tools"));
            
            if (DrawButton(fullPathRect, ref settings.showFullPath, Uniform.IconContent("UnityEditor.HierarchyWindow", "Show full asset path")))
            {
                RefreshShowFullPath();
            }
            if (DrawButton(fileSizeRect, ref settings.showFileSize, Uniform.IconContent("Download-Available@2x", "Show File Size")))
            {
                RefreshShowFileSize();
            }
            if (DrawButton(extensionRect, ref settings.showFileExtension, Uniform.IconContent("d_curvekeyframeweighted", "Show File Extension")))
            {
                RefreshShowFileExtension();
            }
            
            return false;
        }

        private bool DrawButton(Rect rect, ref bool show, GUIContent icon)
        {
            var changed = false;
            var oColor = GUI.color;
            if (show) GUI.color = new Color(0.7f, 1f, 0.7f, 1f);
            {
                if (GUI.Button(rect, icon, EditorStyles.toolbarButton))
                {
                    show = !show;
                    EditorUtility.SetDirty(this);
                    WillRepaint = true;
                    changed = true;
                }    
            }
            GUI.color = oColor;
            return changed;
        }
        
        private void DrawAssetViewSettings()
        {
            bool isDisable = !sp2.splits[1].visible;
            EditorGUI.BeginDisabledGroup(isDisable);
            {
                GUI2.ToolbarToggle(ref Setting.Settings.displayAssetBundleName,
                    Uniform.IconContent("d_SceneViewVisibility On").image,
                    Vector2.zero,
                    "Show / Hide Assetbundle Names");
#if UNITY_2017_1_OR_NEWER
                GUI2.ToolbarToggle(ref Setting.Settings.displayAtlasName, Uniform.IconContent("SpriteAtlas Icon").image, Vector2.zero, "Show / Hide Atlas packing tags");
#endif
                GUI2.ToolbarToggle(ref Setting.Settings.showUsedByClassed, Uniform.IconContent("d_PreMatSphere").image, Vector2.zero, "Show / Hide usage icons");
            }
            EditorGUI.EndDisabledGroup();
        }

        private EnumDrawer _groupModeEd;
        private EnumDrawer _toolModeEd;
        private EnumDrawer _sortModeEd;

        private void DrawViewModes(Rect rect)
        {
            var rect1 = rect;
            rect1.width = rect.width / 2f;

            var rect2 = rect1;
            rect2.x += rect1.width;

            _toolModeEd ??= new EnumDrawer {enumInfo = new EnumDrawer.EnumInfo(FindRefDrawer.Mode.Type, FindRefDrawer.Mode.Folder, FindRefDrawer.Mode.Extension)};
            _groupModeEd ??= new EnumDrawer {tooltip = "Group By"};
            _sortModeEd ??= new EnumDrawer {tooltip = "Sort By"};

            if (settings.toolMode)
            {
                var tMode = settings.toolGroupMode;
                if (_toolModeEd.Draw(rect1, ref tMode))
                {
                    settings.toolGroupMode = tMode;
                    AllMarkDirty();
                    RefreshSort();
                }
            }
            else
            {
                var gMode = settings.groupMode;
                if (_groupModeEd.Draw(rect1, ref gMode))
                {
                    settings.groupMode = gMode;
                    AllMarkDirty();
                    RefreshSort();
                }
            }

            var sMode = settings.sortMode;
            if (_sortModeEd.Draw(rect2, ref sMode))
            {
                settings.sortMode = sMode;
                RefreshSort();
            }
        }

        // Save status to temp variable so the result will be consistent between Layout & Repaint
        internal static int delayRepaint;
        internal static bool checkDrawImportResult;

        protected void OnGUI2()
        {
            if (Event.current.type == EventType.Layout) FinderUtility.RefreshEditorStatus();

            if (InternalDisabled)
            {
                DrawEnable();
                return;
            }

            if (!CacheHelper.inited) CacheHelper.InitHelper();
            if (_tabs == null) InitTabs();
            if (sp1 == null) InitPanes();

            bool result = CheckDrawImport();
            if (Event.current.type == EventType.Layout) checkDrawImportResult = result;

            if (!checkDrawImportResult) return;

            if (settings.toolMode)
            {
                EditorGUILayout.HelpBox("Tools are POWERFUL & DANGEROUS! Only use if you know what you are doing!!!", MessageType.Warning);
                _toolTabs.DrawLayout();
                DrawTools();
            }
            else
            {
                _tabs?.DrawLayout();
                sp1?.DrawLayout();
            }

            DrawSettings();
            DrawFooter();
            if (!WillRepaint) return;
            WillRepaint = false;
            Repaint();
        }


        private DeleteButton _deleteUnused;

        private void DrawTools()
        {
            if (IsFocusingDuplicate)
            {
                _duplicated.DrawLayout();
                GUILayout.FlexibleSpace();
                return;
            }

            if (IsFocusingUnused)
            {
                if (_refUnUse.refs is {Count: 0})
                {
                    EditorGUILayout.HelpBox("Wow! So clean!?", MessageType.Info);
                    EditorGUILayout.HelpBox("Your project does not has have any unused assets, or have you just hit DELETE ALL?", MessageType.Info);
                    EditorGUILayout.HelpBox("Your backups are placed at Library/Finder/ just in case you want your assets back!", MessageType.Info);
                }
                else
                {
                    _refUnUse.DrawLayout();

                    if (_deleteUnused == null)
                    {
                        _deleteUnused = new DeleteButton
                        {
                            warningMessage = "A backup (.unitypackage) will be created so you can reimport the deleted assets later!",
                            deleteLabel = MyGUIContent.From("DELETE ASSETS", Uniform.IconContent("d_TreeEditor.Trash").image),
                            confirmMessage = "Create backup at Library/Finder/"
                        };
                    }

                    GUILayout.BeginHorizontal();
                    {
                        _deleteUnused.Draw(() => { FinderUtility.BackupAndDeleteAssets(_refUnUse.Source); });
                    }
                    GUILayout.EndHorizontal();
                }

                return;
            }

            if (IsFocusingUsedInBuild)
            {
                _usedInBuild.DrawLayout();
                return;
            }

            if (IsFocusingGUIDs)
            {
                DrawGUIDs();
            }
        }

        private void DrawSettings()
        {
            if (_bottomTabs.current == -1) return;

            GUILayout.BeginVertical(GUILayout.Height(100f));
            {
                GUILayout.Space(2f);
                switch (_bottomTabs.current)
                {
                    case 0:
                    {
                        DrawFinderSettings();
                        break;
                    }

                    case 1:
                    {
                        if (AssetType.DrawIgnoreFolder()) AllMarkDirty();
                        break;
                    }

                    case 2:
                    {
                        if (AssetType.DrawSearchFilter()) AllMarkDirty();
                        break;
                    }
                }
            }
            GUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();
            rect.height = 1f;
            GUI2.Rect(rect, Color.black, 0.4f);
        }

        protected void AllMarkDirty()
        {
            _usedByDrawer.SetDirty();
            _usesDrawer.SetDirty();
            _duplicated.SetDirty();
            _addressableDrawer.SetDirty();
            _sceneToAssetDrawer.SetDirty();
            _refUnUse.SetDirty();

            _refInScene.SetDirty();
            _refSceneInScene.SetDirty();
            _sceneUsesDrawer.SetDirty();
            _usedInBuild.SetDirty();
            WillRepaint = true;
        }

        protected void RefreshShowFileExtension()
        {
            _refUnUse.drawExtension = settings.showFileExtension;
            _usesDrawer.drawExtension = settings.showFileExtension;
            _usedByDrawer.drawExtension = settings.showFileExtension;
            _sceneToAssetDrawer.drawExtension = settings.showFileExtension;
            _refInScene.drawExtension = settings.showFileExtension;
            _sceneUsesDrawer.drawExtension = settings.showFileExtension;
            _refSceneInScene.drawExtension = settings.showFileExtension;
        }
        
        protected void RefreshShowFullPath()
        {
            _refUnUse.drawFullPath = settings.showFullPath;
            _usesDrawer.drawFullPath = settings.showFullPath;
            _usedByDrawer.drawFullPath = settings.showFullPath;
            _sceneToAssetDrawer.drawFullPath = settings.showFullPath;
            _refInScene.drawFullPath = settings.showFullPath;
            _sceneUsesDrawer.drawFullPath = settings.showFullPath;
            _refSceneInScene.drawFullPath = settings.showFullPath;
        }
        
        protected void RefreshShowFileSize()
        {
            _refUnUse.drawFileSize = settings.showFileSize;
            _usesDrawer.drawFileSize = settings.showFileSize;
            _usedByDrawer.drawFileSize = settings.showFileSize;
            _sceneToAssetDrawer.drawFileSize = settings.showFileSize;
            _refInScene.drawFileSize = settings.showFileSize;
            _sceneUsesDrawer.drawFileSize = settings.showFileSize;
            _refSceneInScene.drawFileSize = settings.showFileSize;
        }
        
        protected void RefreshShowUsageType()
        {
            _refUnUse.drawUsageType = settings.showUsageType;
            _usesDrawer.drawUsageType = settings.showUsageType;
            _usedByDrawer.drawUsageType = settings.showUsageType;
            _sceneToAssetDrawer.drawUsageType = settings.showUsageType;
            _refInScene.drawUsageType = settings.showUsageType;
            _sceneUsesDrawer.drawUsageType = settings.showUsageType;
            _refSceneInScene.drawUsageType = settings.showUsageType;
        }


        protected void RefreshSort()
        {
            _usedByDrawer.RefreshSort();
            _usesDrawer.RefreshSort();
            _addressableDrawer.RefreshSort();

            _duplicated.RefreshSort();
            _sceneToAssetDrawer.RefreshSort();
            _refUnUse.RefreshSort();
            _usedInBuild.RefreshSort();
        }

        private Dictionary<string, UnityObject> _guidObjs;
        private string[] _ids;

        private void DrawGUIDs()
        {
            GUILayout.Label("GUID to Object", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            {
                string guid = EditorGUILayout.TextField(_tempGuid ?? string.Empty);
                string fileId = EditorGUILayout.TextField(_tempFileID ?? string.Empty);
                EditorGUILayout.ObjectField(_tempObject, typeof(UnityObject), false, GUILayout.Width(160f));

                if (GUILayout.Button("Paste", EditorStyles.miniButton, GUILayout.Width(70f)))
                {
                    string[] split = EditorGUIUtility.systemCopyBuffer.Split('/');
                    guid = split[0];
                    fileId = split.Length == 2 ? split[1] : string.Empty;
                }

                if ((guid != _tempGuid || fileId != _tempFileID) && !string.IsNullOrEmpty(guid))
                {
                    _tempGuid = guid;
                    _tempFileID = fileId;
                    string fullId = string.IsNullOrEmpty(fileId) ? _tempGuid : _tempGuid + "/" + _tempFileID;

                    _tempObject = FinderUtility.LoadAssetAtPath<UnityObject>(AssetDatabase.GUIDToAssetPath(fullId));
                }

                if (GUILayout.Button("Set FileID"))
                {
                    var newDict = new Dictionary<string, UnityObject>();
                    foreach (var kvp in _guidObjs)
                    {
                        string key = kvp.Key.Split('/')[0];
                        if (!string.IsNullOrEmpty(fileId)) key = key + "/" + fileId;

                        var value = FinderUtility.LoadAssetAtPath<UnityObject>(AssetDatabase.GUIDToAssetPath(key));
                        newDict.Add(key, value);
                    }

                    _guidObjs = newDict;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            if (_guidObjs == null) // || ids == null)
                return;


            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                {
                    foreach (var item in _guidObjs)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            var obj = item.Value;

                            EditorGUILayout.ObjectField(obj, typeof(UnityObject), false, GUILayout.Width(150));
                            string idi = item.Key;
                            GUILayout.TextField(idi, GUILayout.Width(320f));
                            if (GUILayout.Button("Copy", EditorStyles.miniButton, GUILayout.Width(50f)))
                            {
                                _tempObject = obj;

                                string[] arr = item.Key.Split('/');
                                _tempGuid = arr[0];
                                _tempFileID = arr[1];
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Merge Selection To"))
            {
                string fullId = string.IsNullOrEmpty(_tempFileID) ? _tempGuid : _tempGuid + "/" + _tempFileID;
                FinderExport.MergeDuplicate(fullId);
            }

            EditorGUILayout.ObjectField(_tempObject, typeof(UnityObject), false, GUILayout.Width(120f));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
    }
}