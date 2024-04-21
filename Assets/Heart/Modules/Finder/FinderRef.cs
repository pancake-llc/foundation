using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public class FinderSceneRef : FinderRef
    {
        internal static readonly Dictionary<string, Type> CacheType = new();

        // ------------------------- Ref in scene
        private static Action<Dictionary<string, FinderRef>> onFindRefInSceneComplete;
        private static Dictionary<string, FinderRef> refs = new();
        private static string[] cacheAssetGuids;
        public readonly string sceneFullPath = "";
        public readonly string scenePath = "";
        public readonly string targetType;
        public readonly HashSet<string> usingType = new();

        public FinderSceneRef(int index, int depth, FinderAsset asset, FinderAsset by)
            : base(index, depth, asset, by)
        {
            isSceneRef = false;
        }

        public FinderSceneRef(int depth, Object target)
            : base(0, depth, null, null)
        {
            component = target;
            this.depth = depth;
            isSceneRef = true;
            var obj = target as GameObject;
            if (obj == null)
            {
                var com = target as Component;
                if (com != null) obj = com.gameObject;
            }

            scenePath = FinderUnity.GetGameObjectPath(obj, false);
            if (component == null) return;

            sceneFullPath = scenePath + component.name;
            targetType = component.GetType().Name;
        }

        public static IWindow Window { get; set; }

        public override bool IsSelected() { return component != null && FinderBookmark.Contains(component); }

        public void Draw(Rect r, IWindow window, bool showDetails)
        {
            bool selected = IsSelected();
            DrawToogleSelect(r);

            var margin = 2;
            var left = new Rect(r) {width = r.width / 3f};

            var right = new Rect(r);
            right.xMin += left.width + margin;


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var pingRect = FinderWindowBase.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : left;

                if (pingRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.control || Event.current.command)
                    {
                        if (selected) FinderBookmark.Remove(this);
                        else FinderBookmark.Add(this);

                        window?.Repaint();
                    }
                    else EditorGUIUtility.PingObject(component);

                    Event.current.Use();
                }
            }

            EditorGUI.ObjectField(showDetails ? left : r,
                GUIContent.none,
                component,
                typeof(GameObject),
                true);
            if (!showDetails) return;

            bool drawPath = FinderWindowBase.GroupMode != FinderRefDrawer.Mode.Folder;
            float pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(scenePath)).x : 0;
            string assetName = component.name;

            var cc = FinderWindowBase.SelectedColor;

            var lableRect = new Rect(right.x, right.y, pathW + EditorStyles.boldLabel.CalcSize(new GUIContent(assetName)).x, right.height);
            if (selected)
            {
                var c = GUI.color;
                GUI.color = cc;
                GUI.DrawTexture(lableRect, EditorGUIUtility.whiteTexture);
                GUI.color = c;
            }

            if (drawPath)
            {
                GUI.Label(LeftRect(pathW, ref right), scenePath, EditorStyles.miniLabel);
                right.xMin -= 4f;
                GUI.Label(right, assetName, EditorStyles.boldLabel);
            }
            else GUI.Label(right, assetName);

            if (!FinderWindowBase.ShowUsedByClassed || usingType == null) return;

            const float sub = 10;
            var re = new Rect(r.x + r.width - sub, r.y, 20, r.height);
            foreach (string item in usingType)
            {
                string name = item;
                if (!CacheType.TryGetValue(item, out var t))
                {
                    t = FinderUnity.GetType(name);
                    CacheType.Add(item, t);
                }

                float width;
                if (!FinderAsset.cacheImage.TryGetValue(name, out var content))
                {
                    if (t == null) content = new GUIContent(name);
                    else
                    {
                        var text = EditorGUIUtility.ObjectContent(null, t).image;
                        content = text == null ? new GUIContent(name) : new GUIContent(text, name);
                    }

                    FinderAsset.cacheImage.Add(name, content);
                }

                if (content.image == null) width = EditorStyles.label.CalcSize(content).x;
                else width = 20;

                re.x -= width;
                re.width = width;

                GUI.Label(re, content);
                re.x -= margin; // margin;
            }
        }

        private Rect LeftRect(float w, ref Rect rect)
        {
            rect.x += w;
            rect.width -= w;
            return new Rect(rect.x - w, rect.y, w, rect.height);
        }

        // ------------------------- Scene use scene objects
        public static Dictionary<string, FinderRef> FindSceneUseSceneObjects(GameObject[] targets)
        {
            var results = new Dictionary<string, FinderRef>();
            var objs = Selection.gameObjects;
            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUnity.IsInAsset(objs[i])) continue;

                var key = objs[i].GetInstanceID().ToString();
                if (!results.ContainsKey(key)) results.Add(key, new FinderSceneRef(0, objs[i]));

                var coms = objs[i].GetComponents<Component>();
                var sceneCache = FinderSceneCache.Api.Cache;
                for (var j = 0; j < coms.Length; j++)
                {
                    if (coms[j] == null) continue; // missing component

                    if (!sceneCache.TryGetValue(coms[j], out var hash)) continue;
                    foreach (var item in hash)
                    {
                        if (!item.isSceneObject) continue;
                        var obj = item.target;
                        var key1 = obj.GetInstanceID().ToString();
                        if (!results.ContainsKey(key1)) results.Add(key1, new FinderSceneRef(1, obj));
                    }
                }
            }

            return results;
        }

        // ------------------------- Scene in scene
        public static Dictionary<string, FinderRef> FindSceneInScene(GameObject[] targets)
        {
            var results = new Dictionary<string, FinderRef>();
            var objs = Selection.gameObjects;
            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUnity.IsInAsset(objs[i])) continue;

                var key = objs[i].GetInstanceID().ToString();
                if (!results.ContainsKey(key)) results.Add(key, new FinderSceneRef(0, objs[i]));

                foreach (var item in FinderSceneCache.Api.Cache)
                foreach (var item1 in item.Value)
                {
                    GameObject ob;
                    if (item1.target is GameObject target) ob = target;
                    else
                    {
                        var com = item1.target as Component;
                        if (com == null) continue;
                        ob = com.gameObject;
                    }

                    if (ob == null) continue;

                    if (ob != objs[i]) continue;

                    key = item.Key.GetInstanceID().ToString();
                    if (!results.ContainsKey(key)) results.Add(key, new FinderSceneRef(1, item.Key));

                    (results[key] as FinderSceneRef)?.usingType.Add(item1.target.GetType().FullName);
                }
            }

            return results;
        }

        public static Dictionary<string, FinderRef> FindRefInScene(string[] assetGUIDs, bool depth, Action<Dictionary<string, FinderRef>> onComplete, IWindow win)
        {
            Window = win;
            cacheAssetGuids = assetGUIDs;
            onFindRefInSceneComplete = onComplete;
            if (FinderSceneCache.ready) FindRefInScene();
            else
            {
                FinderSceneCache.onReady -= FindRefInScene;
                FinderSceneCache.onReady += FindRefInScene;
            }

            return refs;
        }

        private static void FindRefInScene()
        {
            refs = new Dictionary<string, FinderRef>();
            for (var i = 0; i < cacheAssetGuids.Length; i++)
            {
                var asset = FinderWindowBase.CacheSetting.Get(cacheAssetGuids[i]);
                if (asset == null) continue;

                Add(refs, asset, 0);
                ApplyFilter(refs, asset);
            }

            onFindRefInSceneComplete?.Invoke(refs);

            FinderSceneCache.onReady -= FindRefInScene;
        }

        private static void ApplyFilter(Dictionary<string, FinderRef> refs, FinderAsset asset)
        {
            string targetPath = AssetDatabase.GUIDToAssetPath(asset.guid);
            if (string.IsNullOrEmpty(targetPath)) return; // asset not found - might be deleted!

            //asset being moved!
            if (targetPath != asset.AssetPath) asset.MarkAsDirty();

            var target = AssetDatabase.LoadAssetAtPath(targetPath, typeof(Object));
            if (target == null) return;

            bool targetIsGameobject = target is GameObject;

            if (targetIsGameobject)
            {
                foreach (var item in FinderUnity.GetAllObjsInCurScene())
                {
                    if (FinderUnity.CheckIsPrefab(item))
                    {
                        string itemGuid = FinderUnity.GetPrefabParent(item);
                        if (itemGuid == asset.guid) Add(refs, item, 1);
                    }
                }
            }

            string dir = Path.GetDirectoryName(targetPath);
            if (FinderSceneCache.Api.folderCache.ContainsKey(dir))
            {
                foreach (var item in FinderSceneCache.Api.folderCache[dir])
                {
                    if (!FinderSceneCache.Api.Cache.ContainsKey(item)) continue;
                    foreach (var item1 in FinderSceneCache.Api.Cache[item])
                    {
                        if (targetPath == AssetDatabase.GetAssetPath(item1.target)) Add(refs, item, 1);
                    }
                }
            }
        }

        private static void Add(Dictionary<string, FinderRef> refs, FinderAsset asset, int depth)
        {
            string targetId = asset.guid;
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FinderRef(0, depth, asset, null));
        }

        private static void Add(Dictionary<string, FinderRef> refs, Object target, int depth)
        {
            var targetId = target.GetInstanceID().ToString();
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FinderSceneRef(depth, target));
        }
    }

    public class FinderRef
    {
        private static int CsvSorter(FinderRef item1, FinderRef item2)
        {
            int r = item1.depth.CompareTo(item2.depth);
            if (r != 0) return r;

            int t = item1.type.CompareTo(item2.type);
            if (t != 0) return t;

            return item1.index.CompareTo(item2.index);
        }


        public static FinderRef[] FromDict(Dictionary<string, FinderRef> dict)
        {
            if (dict == null || dict.Count == 0) return null;

            var result = new List<FinderRef>();

            foreach (var kvp in dict)
            {
                if (kvp.Value == null) continue;
                if (kvp.Value.asset == null) continue;

                result.Add(kvp.Value);
            }

            result.Sort(CsvSorter);

            return result.ToArray();
        }

        public static FinderRef[] FromList(List<FinderRef> list)
        {
            if (list == null || list.Count == 0) return null;

            list.Sort(CsvSorter);
            var result = new List<FinderRef>();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].asset == null) continue;
                result.Add(list[i]);
            }

            return result.ToArray();
        }

        public FinderAsset addBy;
        public FinderAsset asset;
        public Object component;
        public int depth;
        public string group;
        public int index;

        public bool isSceneRef;
        public int matchingScore;
        public int type;

        public override string ToString()
        {
            if (isSceneRef)
            {
                var sr = (FinderSceneRef) this;
                return sr.scenePath;
            }

            return asset.AssetPath;
        }

        public FinderRef(int index, int depth, FinderAsset asset, FinderAsset by)
        {
            this.index = index;
            this.depth = depth;

            this.asset = asset;
            if (asset != null)
            {
                type = FinderAssetType.GetIndex(asset.Extension);
            }

            addBy = by;
        }

        public FinderRef(int index, int depth, FinderAsset asset, FinderAsset by, string group)
            : this(index, depth, asset, by)
        {
            this.group = group;
        }

        public string GetSceneObjId() { return component == null ? string.Empty : component.GetInstanceID().ToString(); }

        public virtual bool IsSelected() { return FinderBookmark.Contains(asset.guid); }

        public virtual void DrawToogleSelect(Rect r)
        {
            bool s = IsSelected();
            r.width = 16f;

            if (!GUI2.Toggle(r, ref s)) return;

            if (s) FinderBookmark.Add(this);
            else FinderBookmark.Remove(this);
        }


        internal List<FinderRef> Append(Dictionary<string, FinderRef> dict, params string[] guidList)
        {
            var result = new List<FinderRef>();

            if (FinderWindowBase.CacheSetting.disabled) return result;

            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Cache not yet ready! Please wait!");
                return result;
            }

            //filter to remove items that already in dictionary
            for (var i = 0; i < guidList.Length; i++)
            {
                string guid = guidList[i];
                if (dict.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;

                var r = new FinderRef(dict.Count, depth + 1, child, asset);
                if (!asset.IsFolder) dict.Add(guid, r);

                result.Add(r);
            }

            return result;
        }

        internal void AppendUsedBy(Dictionary<string, FinderRef> result, bool deep)
        {
            var h = asset.usedByMap;
            var list = deep ? new List<FinderRef>() : null;

            if (asset.usedByMap == null) return;

            foreach (var kvp in h)
            {
                string guid = kvp.Key;
                if (result.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;

                if (child.IsMissing) continue;

                var r = new FinderRef(result.Count, depth + 1, child, asset);
                if (!asset.IsFolder) result.Add(guid, r);

                if (deep) list.Add(r);
            }

            if (!deep) return;

            foreach (var item in list)
            {
                item.AppendUsedBy(result, true);
            }
        }

        internal void AppendUsage(Dictionary<string, FinderRef> result, bool deep)
        {
            var h = asset.UseGUIDs;
            var list = deep ? new List<FinderRef>() : null;

            foreach (var kvp in h)
            {
                string guid = kvp.Key;
                if (result.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;

                if (child.IsMissing) continue;

                var r = new FinderRef(result.Count, depth + 1, child, asset);
                if (!asset.IsFolder) result.Add(guid, r);

                if (deep) list.Add(r);
            }

            if (!deep) return;

            foreach (var item in list)
            {
                item.AppendUsage(result, true);
            }
        }

        // --------------------- STATIC UTILS -----------------------

        internal static Dictionary<string, FinderRef> FindRefs(string[] guids, bool usageOrUsedBy, bool addFolder)
        {
            var dict = new Dictionary<string, FinderRef>();
            var list = new List<FinderRef>();

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (dict.ContainsKey(guid)) continue;

                var asset = FinderWindowBase.CacheSetting.Get(guid);
                if (asset == null) continue;

                var r = new FinderRef(i, 0, asset, null);
                if (!asset.IsFolder || addFolder) dict.Add(guid, r);

                list.Add(r);
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (usageOrUsedBy) list[i].AppendUsage(dict, true);
                else list[i].AppendUsedBy(dict, true);
            }

            return dict;
        }


        public static Dictionary<string, FinderRef> FindUsage(string[] guids) { return FindRefs(guids, true, true); }

        public static Dictionary<string, FinderRef> FindUsedBy(string[] guids) { return FindRefs(guids, false, true); }

        public static Dictionary<string, FinderRef> FindUsageScene(GameObject[] objs, bool depth)
        {
            var dict = new Dictionary<string, FinderRef>();


            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUnity.IsInAsset(objs[i])) continue; //only get in scene 

                //add selection
                if (!dict.ContainsKey(objs[i].GetInstanceID().ToString()))
                {
                    dict.Add(objs[i].GetInstanceID().ToString(), new FinderSceneRef(0, objs[i]));
                }

                foreach (var item in FinderUnity.GetAllRefObjects(objs[i]))
                {
                    AppendUsageScene(dict, item);
                }

                if (depth)
                {
                    foreach (var child in FinderUnity.GetAllChild(objs[i]))
                    foreach (var item2 in FinderUnity.GetAllRefObjects(child))
                    {
                        AppendUsageScene(dict, item2);
                    }
                }
            }

            return dict;
        }

        private static void AppendUsageScene(Dictionary<string, FinderRef> dict, Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return;

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            if (dict.ContainsKey(guid)) return;

            var asset = FinderWindowBase.CacheSetting.Get(guid);
            if (asset == null) return;

            var r = new FinderRef(0, 1, asset, null);
            dict.Add(guid, r);
        }
    }


    public class FinderRefDrawer : IRefDraw
    {
        public enum Mode
        {
            Dependency,
            Depth,
            Type,
            Extension,
            Folder,
            AssetBundle,

            None
        }

        public enum Sort
        {
            Type,
            Path,
            Size
        }

        internal readonly FinderTreeUI.GroupDrawer groupDrawer;
        private readonly bool _showSearch = true;
        public bool caseSensitive = false;

        // STATUS
        private bool _dirty;
        private int _excludeCount;

        public string level0Group;
        public bool forceHideDetails;

        public string lable;
        internal List<FinderRef> list;
        internal Dictionary<string, FinderRef> refs;

        // FILTERING
        private readonly string _searchTerm = string.Empty;
        private bool _selectFilter;
        private bool _showIgnore;


        // ORIGINAL
        internal FinderRef[] Source => FinderRef.FromList(list);


        public FinderRefDrawer(IWindow window)
        {
            Window = window;
            groupDrawer = new FinderTreeUI.GroupDrawer(DrawGroup, DrawAsset);
        }

        public string messageNoRefs = "Do select something!";
        public string messageEmpty = "It's empty!";

        public IWindow Window { get; set; }

        private void DrawEmpty(Rect rect, string text)
        {
            rect = GUI2.Padding(rect, 2f, 2f);
            rect.height = 40f;

            EditorGUI.HelpBox(rect, text, MessageType.Info);
        }

        public bool Draw(Rect rect)
        {
            if (refs == null || refs.Count == 0)
            {
                DrawEmpty(rect, messageNoRefs);
                return false;
            }

            if (_dirty || list == null) ApplyFilter();

            if (!groupDrawer.HasChildren) DrawEmpty(rect, messageEmpty);
            else groupDrawer.Draw(rect);

            return false;
        }

        public bool DrawLayout()
        {
            if (refs == null || refs.Count == 0) return false;
            if (_dirty || list == null) ApplyFilter();
            groupDrawer.DrawLayout();
            return false;
        }

        public int ElementCount() { return refs == null ? 0 : refs.Count; }

        public void SetRefs(Dictionary<string, FinderRef> dictRefs)
        {
            refs = dictRefs;
            _dirty = true;
        }

        private void SetBookmarkGroup(string groupLabel, bool willbookmark)
        {
            string[] ids = groupDrawer.GetChildren(groupLabel);
            var info = GetBmInfo(groupLabel);

            for (var i = 0; i < ids.Length; i++)
            {
                if (!refs.TryGetValue(ids[i], out var rf)) continue;

                if (willbookmark) FinderBookmark.Add(rf);
                else FinderBookmark.Remove(rf);
            }

            info.count = willbookmark ? info.total : 0;
        }

        internal class BookmarkInfo
        {
            public int count;
            public int total;
        }

        private readonly Dictionary<string, BookmarkInfo> _gBookmarkCache = new();

        private BookmarkInfo GetBmInfo(string groupLabel)
        {
            if (!_gBookmarkCache.TryGetValue(groupLabel, out var info))
            {
                string[] ids = groupDrawer.GetChildren(groupLabel);

                info = new BookmarkInfo();
                for (var i = 0; i < ids.Length; i++)
                {
                    if (!refs.TryGetValue(ids[i], out var rf)) continue;

                    info.total++;

                    bool isBm = FinderBookmark.Contains(rf);
                    if (isBm) info.count++;
                }

                _gBookmarkCache.Add(groupLabel, info);
            }

            return info;
        }

        private void DrawToggleGroup(Rect r, string groupLabel)
        {
            var info = GetBmInfo(groupLabel);
            bool selectAll = info.count == info.total;
            r.width = 16f;
            if (GUI2.Toggle(r, ref selectAll)) SetBookmarkGroup(groupLabel, selectAll);
        }

        private void DrawGroup(Rect r, string label, int childCount)
        {
            if (FinderWindowBase.GroupMode == Mode.Folder)
            {
                var tex = AssetDatabase.GetCachedIcon("Assets");
                GUI.DrawTexture(new Rect(r.x - 2f, r.y - 2f, 16f, 16f), tex);
                r.xMin += 16f;
            }

            DrawToggleGroup(r, label);
            r.xMin += 18f;
            GUI.Label(r, label + " (" + childCount + ")", EditorStyles.boldLabel);

            bool hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);
            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Bookmark"), false, () => { SetBookmarkGroup(label, true); });
                menu.AddItem(new GUIContent("Remove Bookmark"), false, () => { SetBookmarkGroup(label, false); });

                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void DrawAsset(Rect r, string guid)
        {
            if (!refs.TryGetValue(guid, out var rf)) return;

            if (rf.isSceneRef)
            {
                if (rf.component == null) return;

                if (rf is FinderSceneRef re)
                {
                    r.x -= 16f;
                    rf.DrawToogleSelect(r);
                    r.xMin += 32f;
                    re.Draw(r, Window, !forceHideDetails);
                }
            }
            else
            {
                r.xMin -= 16f;
                rf.DrawToogleSelect(r);
                r.xMin += 32f;
                rf.asset.Draw(r,
                    rf.depth == 1,
                    !forceHideDetails && (FinderWindowBase.GroupMode != Mode.Folder),
                    !forceHideDetails && FinderWindowBase.DisplayFileSize,
                    !forceHideDetails && FinderWindowBase.DisplayAssetBundleName,
                    !forceHideDetails && FinderWindowBase.DisplayAtlasName,
                    !forceHideDetails && FinderWindowBase.ShowReferenceCount,
                    Window);
            }
        }

        private string GetGroup(FinderRef rf)
        {
            if (rf.depth == 0) return level0Group;

            if (FinderWindowBase.GroupMode == Mode.None) return "(no group)";

            FinderSceneRef sr = null;
            if (rf.isSceneRef)
            {
                sr = rf as FinderSceneRef;
                if (sr == null) return null;
            }

            if (!rf.isSceneRef)
            {
                if (rf.asset.IsExcluded) return null;
            }

            switch (FinderWindowBase.GroupMode)
            {
                case Mode.Extension: return rf.isSceneRef ? sr.targetType : rf.asset.Extension;
                case Mode.Type: return rf.isSceneRef ? sr.targetType : FinderAssetType.Filters[rf.type].name;
                case Mode.Folder: return rf.isSceneRef ? sr.scenePath : rf.asset.AssetFolder;
                case Mode.Dependency: return rf.depth == 1 ? "Direct Usage" : "Indirect Usage";
                case Mode.Depth: return "Level " + rf.depth.ToString();
                case Mode.AssetBundle:
                    return rf.isSceneRef ? "(not in assetbundle)" : string.IsNullOrEmpty(rf.asset.AssetBundleName) ? "(not in assetbundle)" : rf.asset.AssetBundleName;
            }

            return "(others)";
        }

        private void SortGroup(List<string> groups)
        {
            groups.Sort((item1, item2) =>
            {
                if (item1.Contains("(")) return 1;
                if (item2.Contains("(")) return -1;

                return item1.CompareTo(item2);
            });
        }

        public FinderRefDrawer Reset(string[] assetGUIDs, bool isUsage)
        {
            _gBookmarkCache.Clear();
            refs = isUsage ? FinderRef.FindUsage(assetGUIDs) : FinderRef.FindUsedBy(assetGUIDs);
            _dirty = true;
            list?.Clear();
            return this;
        }

        public FinderRefDrawer Reset(GameObject[] objs, bool findDept, bool findPrefabInAsset)
        {
            refs = FinderRef.FindUsageScene(objs, findDept);

            var guidss = new List<string>();
            var dependent = FinderSceneCache.Api.prefabDependencies;
            foreach (var gameObject in objs)
            {
                if (!dependent.TryGetValue(gameObject, out var hash)) continue;

                foreach (string guid in hash)
                {
                    guidss.Add(guid);
                }
            }

            var usageRefs1 = FinderRef.FindUsage(guidss.ToArray());
            foreach (var kvp in usageRefs1)
            {
                if (refs.ContainsKey(kvp.Key)) continue;

                if (guidss.Contains(kvp.Key)) kvp.Value.depth = 1;

                refs.Add(kvp.Key, kvp.Value);
            }


            if (findPrefabInAsset)
            {
                var guids = new List<string>();
                for (var i = 0; i < objs.Length; i++)
                {
                    string guid = FinderUnity.GetPrefabParent(objs[i]);
                    if (string.IsNullOrEmpty(guid)) continue;

                    guids.Add(guid);
                }

                var usageRefs = FinderRef.FindUsage(guids.ToArray());
                foreach (var kvp in usageRefs)
                {
                    if (refs.ContainsKey(kvp.Key)) continue;

                    if (guids.Contains(kvp.Key)) kvp.Value.depth = 1;

                    refs.Add(kvp.Key, kvp.Value);
                }
            }

            _dirty = true;
            list?.Clear();
            return this;
        }

        //ref in scene
        public FinderRefDrawer Reset(string[] assetGUIDs, IWindow window)
        {
            refs = FinderSceneRef.FindRefInScene(assetGUIDs, true, SetRefInScene, window);
            _dirty = true;
            list?.Clear();
            return this;
        }

        private void SetRefInScene(Dictionary<string, FinderRef> data)
        {
            refs = data;
            _dirty = true;
            list?.Clear();
        }

        //scene in scene
        public FinderRefDrawer ResetSceneInScene(GameObject[] objs)
        {
            refs = FinderSceneRef.FindSceneInScene(objs);
            _dirty = true;
            list?.Clear();
            return this;
        }

        public FinderRefDrawer ResetSceneUseSceneObjects(GameObject[] objs)
        {
            refs = FinderSceneRef.FindSceneUseSceneObjects(objs);
            _dirty = true;
            list?.Clear();
            return this;
        }

        public void RefreshSort()
        {
            if (list == null) return;

            if (list.Count > 0 && list[0].isSceneRef == false && FinderWindowBase.SortMode == Sort.Size)
            {
                list = list.OrderByDescending(x => x.asset != null ? x.asset.FileSize : 0).ToList();
            }
            else
            {
                list.Sort((r1, r2) =>
                {
                    bool isMixed = r1.isSceneRef ^ r2.isSceneRef;
                    if (isMixed)
                    {
                        int v1 = r1.isSceneRef ? 1 : 0;
                        int v2 = r2.isSceneRef ? 1 : 0;
                        return v2.CompareTo(v1);
                    }

                    if (r1.isSceneRef)
                    {
                        var rs1 = (FinderSceneRef) r1;
                        var rs2 = (FinderSceneRef) r2;

                        return SortAsset(rs1.sceneFullPath,
                            rs2.sceneFullPath,
                            rs1.targetType,
                            rs2.targetType,
                            FinderWindowBase.SortMode == Sort.Path);
                    }

                    return SortAsset(r1.asset.AssetPath,
                        r2.asset.AssetPath,
                        r1.asset.Extension,
                        r2.asset.Extension,
                        false);
                });
            }

            // clean up list
            var invalidCount = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];

                if (item.isSceneRef)
                {
                    if (string.IsNullOrEmpty(item.GetSceneObjId()))
                    {
                        invalidCount++;
                        list.RemoveAt(i);
                    }

                    continue;
                }

                if (item.asset == null)
                {
                    invalidCount++;
                    list.RemoveAt(i);
                    continue;
                }
            }

            groupDrawer.Reset(list,
                rf =>
                {
                    if (rf == null) return null;
                    if (rf.isSceneRef) return rf.GetSceneObjId();
                    return rf.asset == null ? null : rf.asset.guid;
                },
                GetGroup,
                SortGroup);
        }


        private void ApplyFilter()
        {
            _dirty = false;

            if (refs == null) return;

            if (list == null) list = new List<FinderRef>();
            else list.Clear();

            int minScore = _searchTerm.Length;

            string term1 = _searchTerm;
            if (!caseSensitive) term1 = term1.ToLower();

            string term2 = term1.Replace(" ", string.Empty);

            _excludeCount = 0;

            foreach (var item in refs)
            {
                var r = item.Value;

                if (FinderWindowBase.IsTypeExcluded(r.type))
                {
                    _excludeCount++;
                    continue; //skip this one
                }

                if (!_showSearch || string.IsNullOrEmpty(_searchTerm))
                {
                    r.matchingScore = 0;
                    list.Add(r);
                    continue;
                }

                //calculate matching score
                string name1 = r.isSceneRef ? (r as FinderSceneRef)?.sceneFullPath : r.asset.AssetName;
                if (!caseSensitive)
                {
                    name1 = name1.ToLower();
                }

                string name2 = name1.Replace(" ", string.Empty);

                int score1 = FinderUnity.StringMatch(term1, name1);
                int score2 = FinderUnity.StringMatch(term2, name2);

                r.matchingScore = Mathf.Max(score1, score2);
                if (r.matchingScore > minScore) list.Add(r);
            }

            RefreshSort();
        }

        public void SetDirty() { _dirty = true; }

        private int SortAsset(string term11, string term12, string term21, string term22, bool swap)
        {
            int v1 = string.Compare(term11, term12, StringComparison.Ordinal);
            int v2 = string.Compare(term21, term22, StringComparison.Ordinal);
            return swap ? (v1 == 0) ? v2 : v1 : (v2 == 0) ? v1 : v2;
        }

        public Dictionary<string, FinderRef> GetRefs() { return refs; }
    }
}