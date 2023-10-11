using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public class FinderWindow : FinderWindowBase, IHasCustomMenu
    {
        internal static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<PancakeEditor.FinderWindow>("Finder", true, Editor.InspectorWindow);
            if (window != null)
            {
                window.InitIfNeeded();
                window.Show();
            }
        }

        [NonSerialized] internal FinderBookmark bookmark;
        [NonSerialized] internal FinderSelection selection;

        [NonSerialized] internal FinderRefDrawer usesDrawer; // [Selected Assets] are [USING] (depends on / contains reference to) ---> those assets
        [NonSerialized] internal FinderRefDrawer usedByDrawer; // [Selected Assets] are [USED BY] <---- those assets 
        [NonSerialized] internal FinderRefDrawer sceneToAssetDrawer; // [Selected GameObjects in current Scene] are [USING] ---> those assets

        [NonSerialized] internal FinderRefDrawer refInScene; // [Selected Assets] are [USED BY] <---- those components in current Scene 
        [NonSerialized] internal FinderRefDrawer sceneUsesDrawer; // [Selected GameObjects] are [USING] ---> those components / GameObjects in current scene
        [NonSerialized] internal FinderRefDrawer refSceneInScene; // [Selected GameObjects] are [USED BY] <---- those components / GameObjects in current scene


        internal int level;
        private Vector2 _scrollPos;
        private string _tempGuid;
        private Object _tempObject;

        protected bool LockSelection => selection != null && selection.isLock;


        private void OnEnable()
        {
            Repaint();

            CacheSetting?.Check4Changes(false);
        }

        private void OnDisable()
        {
            Cache.SaveSetting();
            Setting.SaveSetting();
        }

        protected void InitIfNeeded()
        {
            if (usesDrawer != null) return;

            usesDrawer = new FinderRefDrawer(this) {messageEmpty = "[Selected Assets] are not [USING] (depends on / contains reference to) any other assets!"};

            usedByDrawer = new FinderRefDrawer(this) {messageEmpty = "[Selected Assets] are not [USED BY] any other assets!"};

            sceneToAssetDrawer = new FinderRefDrawer(this) {messageEmpty = "[Selected GameObjects] (in current open scenes) are not [USING] any assets!"};

            bookmark = new FinderBookmark(this);
            selection = new FinderSelection(this);

            sceneUsesDrawer = new FinderRefDrawer(this) {messageEmpty = "[Selected GameObjects] are not [USING] any other GameObjects in scenes"};

            refInScene = new FinderRefDrawer(this) {messageEmpty = "[Selected Assets] are not [USED BY] any GameObjects in opening scenes!"};

            refSceneInScene = new FinderRefDrawer(this) {messageEmpty = "[Selected GameObjects] are not [USED BY] by any GameObjects in opening scenes!"};

#if UNITY_2018_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#elif UNITY_2017_OR_NEWER
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged -= OnSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChanged += OnSceneChanged;
#endif

            FinderCache.onCacheReady -= OnReady;
            FinderCache.onCacheReady += OnReady;

            FinderSetting.onIgnoreChange -= OnIgnoreChanged;
            FinderSetting.onIgnoreChange += OnIgnoreChanged;

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
        protected void OnIgnoreChanged() { OnSelectionChange(); }

        protected void OnReady() { OnSelectionChange(); }

        public override void OnSelectionChange()
        {
            Repaint();

            if (!IsCacheReady)
            {
                return;
            }

            if (focusedWindow == null)
            {
                return;
            }

            if (sceneUsesDrawer == null)
            {
                InitIfNeeded();
            }

            if (usesDrawer == null)
            {
                InitIfNeeded();
            }

            if (!LockSelection)
            {
                _ids = FinderUnity.SelectionAssetGUIDs;
                selection.Clear();

                //ignore selection on asset when selected any object in scene
                if (Selection.gameObjects.Length > 0 && !FinderUnity.IsInAsset(Selection.gameObjects[0]))
                {
                    _ids = new string[0];
                    selection.AddRange(Selection.gameObjects);
                }
                else
                {
                    selection.AddRange(_ids);
                }

                level = 0;

                if (selection.IsSelectingAsset)
                {
                    usesDrawer.Reset(_ids, true);
                    usedByDrawer.Reset(_ids, false);
                    refInScene.Reset(_ids, this as IWindow);
                }
                else
                {
                    refSceneInScene.ResetSceneInScene(Selection.gameObjects);
                    sceneToAssetDrawer.Reset(Selection.gameObjects, true, true);
                    sceneUsesDrawer.ResetSceneUseSceneObjects(Selection.gameObjects);
                }

                // auto disable enable scene / asset
                if (IsFocusingUses)
                {
                    sp2.splits[0].visible = !selection.IsSelectingAsset;
                    sp2.splits[1].visible = true;
                    sp2.CalculateWeight();
                }

                if (IsFocusingUsedBy)
                {
                    sp2.splits[0].visible = true;
                    sp2.splits[1].visible = selection.IsSelectingAsset;
                    sp2.CalculateWeight();
                }
            }

            if (FinderSceneCache.Api.Dirty && !Application.isPlaying)
            {
                FinderSceneCache.Api.RefreshCache(this);
            }

            EditorApplication.delayCall -= Repaint;
            EditorApplication.delayCall += Repaint;
        }


        public FinderSplitView sp1; // container : Selection / sp2 / Bookmark 
        public FinderSplitView sp2; // Scene / Assets

        void InitPanes()
        {
            sp2 = new FinderSplitView(this)
            {
                isHorz = false,
                splits = new List<FinderSplitView.Info>()
                {
                    new FinderSplitView.Info() {title = new GUIContent("Scene", Uniform.IconContent("SceneAsset Icon").image), draw = DrawScene},
                    new FinderSplitView.Info() {title = new GUIContent("Assets", Uniform.IconContent("Folder Icon").image), draw = DrawAsset},
                }
            };

            sp2.CalculateWeight();

            sp1 = new FinderSplitView(this)
            {
                isHorz = true,
                splits = new List<FinderSplitView.Info>()
                {
                    new FinderSplitView.Info()
                    {
                        title = new GUIContent("Selection", Uniform.IconContent("d_RectTransformBlueprint").image),
                        weight = 0.4f,
                        visible = true,
                        draw = (rect) => selection.Draw(rect)
                    },
                    new FinderSplitView.Info()
                    {
                        draw = (r) =>
                        {
                            if (IsFocusingUses || IsFocusingUsedBy)
                            {
                                sp2.Draw(r);
                            }
                            else
                            {
                                DrawTools(r);
                            }
                        }
                    },
                    new FinderSplitView.Info()
                    {
                        title = new GUIContent("Bookmark", Uniform.IconContent("Favorite On Icon").image),
                        weight = 0.4f,
                        visible = false,
                        draw = (rect) => bookmark.Draw(rect)
                    },
                }
            };

            sp1.CalculateWeight();
        }

        private FinderTabView _tabs;
        private FinderTabView _bottomTabs;
        private FinderSearchView _search;

        void DrawScene(Rect rect)
        {
            FinderRefDrawer drawer = IsFocusingUses ? (selection.IsSelectingAsset ? null : sceneUsesDrawer) : (selection.IsSelectingAsset ? refInScene : refSceneInScene);
            if (drawer == null) return;

            if (!FinderSceneCache.ready)
            {
                var rr = rect;
                rr.height = 16f;

                int cur = FinderSceneCache.Api.current, total = FinderSceneCache.Api.total;
                EditorGUI.ProgressBar(rr, cur * 1f / total, string.Format("{0} / {1}", cur, total));
                WillRepaint = true;
                return;
            }

            drawer.Draw(rect);

            var refreshRect = new Rect(rect.xMax - 16f, rect.yMin - 14f, 18f, 18f);
            if (GUI2.ColorIconButton(refreshRect, Uniform.IconContent("d_Refresh@2x").image, FinderSceneCache.Api.Dirty ? (Color?) GUI2.lightRed : null))
            {
                FinderSceneCache.Api.RefreshCache(drawer.Window);
            }
        }


        FinderRefDrawer GetAssetDrawer()
        {
            if (IsFocusingUses)
            {
                return selection.IsSelectingAsset ? usesDrawer : sceneToAssetDrawer;
            }

            if (IsFocusingUsedBy)
            {
                return selection.IsSelectingAsset ? usedByDrawer : null;
            }

            return null;
        }

        void DrawAsset(Rect rect)
        {
            var drawer = GetAssetDrawer();
            if (drawer != null) drawer.Draw(rect);
        }

        void DrawSearch()
        {
            if (_search == null) _search = new FinderSearchView();
            _search.DrawLayout();
        }

        protected override void OnGUI() { OnGUI2(); }

        protected bool CheckDrawImport()
        {
            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Compiling scripts, please wait!", MessageType.Warning);
                Repaint();
                return false;
            }

            if (EditorApplication.isUpdating)
            {
                EditorGUILayout.HelpBox("Importing assets, please wait!", MessageType.Warning);
                Repaint();
                return false;
            }

            InitIfNeeded();

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorGUILayout.HelpBox("Finder requires serialization mode set to FORCE TEXT!", MessageType.Warning);
                if (GUILayout.Button("FORCE TEXT"))
                {
                    EditorSettings.serializationMode = SerializationMode.ForceText;
                }

                return false;
            }

            if (!IsCacheReady)
            {
                if (!HasCache)
                {
                    EditorGUILayout.HelpBox(
                        "Finder Cache not found!\nFirst scan may takes quite some time to finish but you would be able to work normally while the scan works in background...",
                        MessageType.Warning);

                    DrawPriorityGUI();

                    if (GUILayout.Button("Scan project"))
                    {
                        CreateCache();
                        Repaint();
                    }

                    return false;
                }

                DrawPriorityGUI();

                if (!DrawEnable())
                {
                    return false;
                }


                string text = "Refreshing ... " + (int) (CacheSetting.Progress * CacheSetting.workCount) + " / " + CacheSetting.workCount;
                Rect rect = GUILayoutUtility.GetRect(1f, Screen.width, 18f, 18f);
                EditorGUI.ProgressBar(rect, CacheSetting.Progress, text);
                Repaint();
                return false;
            }

            if (!DrawEnable())
            {
                return false;
            }

            return true;
        }

        protected bool IsFocusingUses => _tabs != null && _tabs.current == 0;
        protected bool IsFocusingUsedBy => _tabs != null && _tabs.current == 1;

        void OnTabChange() { }

        void InitTabs()
        {
            _tabs = FinderTabView.Create(this, false, "Uses", "Used By");
            _tabs.onTabChange = OnTabChange;
            _tabs.callback = new DrawCallback()
            {
                beforeDraw = () =>
                {
                    if (GUI2.ToolbarToggle(ref selection.isLock,
                            selection.isLock ? Uniform.IconContent("LockIcon-On").image : Uniform.IconContent("LockIcon").image,
                            new Vector2(-1, 2),
                            "Lock Selection"))
                    {
                        WillRepaint = true;
                    }
                },
                afterDraw = () =>
                {
                    //GUILayout.Space(16f);

                    if (GUI2.ToolbarToggle(ref sp1.isHorz, Uniform.IconContent("LockIcon").image, Vector2.zero, "Layout"))
                    {
                        sp1.CalculateWeight();
                        Repaint();
                    }

                    if (GUI2.ToolbarToggle(ref sp1.splits[0].visible, Uniform.IconContent("d_RectTransformBlueprint").image, Vector2.zero, "Show / Hide Selection"))
                    {
                        sp1.CalculateWeight();
                        Repaint();
                    }

                    if (GUI2.ToolbarToggle(ref sp2.splits[0].visible, Uniform.IconContent("SceneAsset Icon").image, Vector2.zero, "Show / Hide Scene References"))
                    {
                        sp2.CalculateWeight();
                        Repaint();
                    }

                    if (GUI2.ToolbarToggle(ref sp2.splits[1].visible, Uniform.IconContent("Folder Icon").image, Vector2.zero, "Show / Hide Asset References"))
                    {
                        sp2.CalculateWeight();
                        Repaint();
                    }


                    if (GUI2.ToolbarToggle(ref sp1.splits[2].visible, Uniform.IconContent("Favorite On Icon").image, Vector2.zero, "Show / Hide Bookmarks"))
                    {
                        sp1.CalculateWeight();
                        Repaint();
                    }
                }
            };
        }

        protected bool DrawHeader()
        {
            if (_tabs == null) InitTabs();
            if (_bottomTabs == null)
            {
                _bottomTabs = FinderTabView.Create(this,
                    true,
                    Uniform.IconContent("Settings@2x", "Settings"),
                    Uniform.IconContent("d_SceneViewVisibility@2x", "Ignore"),
                    Uniform.IconContent("d_FilterByType@2x", "Filter by Type"));
                _bottomTabs.current = -1;
            }

            _tabs.DrawLayout();

            return true;
        }


        protected bool DrawFooter()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                _bottomTabs.DrawLayout();
                GUILayout.FlexibleSpace();
                DrawAssetViewSettings();
                GUILayout.FlexibleSpace();
                DrawViewModes();
            }
            GUILayout.EndHorizontal();
            return false;
        }

        void DrawAssetViewSettings()
        {
            var isDisable = !sp2.splits[1].visible;
            EditorGUI.BeginDisabledGroup(isDisable);
            {
                GUI2.ToolbarToggle(ref Setting.Settings.displayAssetBundleName,
                    Uniform.IconContent("d_FilterByLabel@2x").image,
                    Vector2.zero,
                    "Show / Hide Assetbundle Names");
#if UNITY_2017_1_OR_NEWER
                GUI2.ToolbarToggle(ref Setting.Settings.displayAtlasName,
                    Uniform.IconContent("SpriteAtlasAsset Icon").image,
                    Vector2.zero,
                    "Show / Hide Atlas packing tags");
#endif
                GUI2.ToolbarToggle(ref Setting.Settings.showUsedByClassed, Uniform.IconContent("d_RenderTexture Icon").image, Vector2.zero, "Show / Hide usage icons");
                GUI2.ToolbarToggle(ref Setting.Settings.displayFileSize, Uniform.IconContent("d_Package Manager").image, Vector2.zero, "Show / Hide file size");
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawViewModes()
        {
            var gMode = GroupMode;
            if (GUI2.EnumPopup(ref gMode, Uniform.IconContent("EditCollider", "Group by"), EditorStyles.toolbarPopup, GUILayout.Width(100f)))
            {
                GroupMode = gMode;
                MarkDirty();
            }

            GUILayout.Space(16f);

            var sMode = SortMode;
            if (GUI2.EnumPopup(ref sMode, Uniform.IconContent("AlphabeticalSorting", "Sort by"), EditorStyles.toolbarPopup, GUILayout.Width(50f)))
            {
                SortMode = sMode;
                RefreshSort();
            }
        }

        protected void OnGUI2()
        {
            if (!CheckDrawImport())
            {
                return;
            }

            if (sp1 == null) InitPanes();

            DrawHeader();
            sp1.DrawLayout();
            DrawSettings();
            DrawFooter();

            if (WillRepaint)
            {
                Repaint();
            }
        }


        void DrawTools(Rect rect) { }

        void DrawSettings()
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
                        if (FinderAssetType.DrawIgnoreFolder())
                        {
                            MarkDirty();
                        }

                        break;
                    }

                    case 2:
                    {
                        if (FinderAssetType.DrawSearchFilter())
                        {
                            MarkDirty();
                        }

                        break;
                    }
                }
            }
            GUILayout.EndVertical();

            var rect = GUILayoutUtility.GetLastRect();
            rect.height = 1f;
            GUI2.Rect(rect, Color.black, 0.4f);
        }

        protected void MarkDirty()
        {
            usedByDrawer.SetDirty();
            usesDrawer.SetDirty();
            sceneToAssetDrawer.SetDirty();

            refInScene.SetDirty();
            refSceneInScene.SetDirty();
            sceneUsesDrawer.SetDirty();
            WillRepaint = true;
        }

        protected void RefreshSort()
        {
            usedByDrawer.RefreshSort();
            usesDrawer.RefreshSort();
            sceneToAssetDrawer.RefreshSort();
        }

        private Dictionary<string, Object> _objs;
        private string[] _ids;
    }
}