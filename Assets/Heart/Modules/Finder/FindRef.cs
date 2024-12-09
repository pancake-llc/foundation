using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor.Finder
{
    public class FindSceneRef : FindRef
    {
        private static readonly Dictionary<string, Type> CacheType = new();

        private static Action<Dictionary<string, FindRef>> onFindRefInSceneComplete;
        private static Dictionary<string, FindRef> refs = new();
        private static string[] cacheAssetGuids;
        public readonly string sceneFullPath = "";
        public readonly string scenePath = "";
        public readonly string targetType;
        public Func<bool> drawFullPath;
        private readonly HashSet<string> _usingType = new();
        private readonly GUIContent _assetNameGC;
        
        public static IWindow Window { get; set; }

        public FindSceneRef(int index, int depth, FindAsset asset, FindAsset by)
            : base(index, depth, asset, by)
        {
            isSceneRef = false;
        }

        public FindSceneRef(int depth, Object target)
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

            scenePath = FinderUtility.GetGameObjectPath(obj, false);
            if (component == null) return;

            string cName = component.name ?? "(empty)"; // some components are hidden
            sceneFullPath = scenePath + cName;
            targetType = component.GetType().Name;
            _assetNameGC = MyGUIContent.FromString(cName);
        }
        
        public override bool IsSelected() { return component != null && AssetBookmark.Contains(component); }
        
        public void Draw(Rect r, IWindow window, FindRefDrawer.Mode groupMode, bool showDetails)
        {
            bool selected = IsSelected();
            DrawToogleSelect(r);

            var margin = 2;
            var left = new Rect(r);
            left.width = r.width / 3f;

            var right = new Rect(r);
            right.xMin += left.width + margin;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var pingRect = FinderWindowBase.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : left;

                if (pingRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.control || Event.current.command)
                    {
                        if (selected) AssetBookmark.Remove(this);
                        else AssetBookmark.Add(this);
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

            bool drawPath = groupMode != FindRefDrawer.Mode.Folder;
            float pathW = drawPath && !string.IsNullOrEmpty(scenePath) ? EditorStyles.miniLabel.CalcSize(MyGUIContent.FromString(scenePath)).x : 0;

            var cc = FinderWindowBase.SelectedColor;

            var lableRect = new Rect(right.x, right.y, pathW + EditorStyles.boldLabel.CalcSize(_assetNameGC).x, right.height);

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
                GUI.Label(right, _assetNameGC, EditorStyles.boldLabel);
            }
            else GUI.Label(right, _assetNameGC);


            if (!FinderWindowBase.ShowUsedByClassed || _usingType == null) return;

            var re = new Rect(r.x + r.width - 10, r.y, 20, r.height);
            foreach (string item in _usingType)
            {
                string name = item;
                if (!CacheType.TryGetValue(item, out var t))
                {
                    t = FinderUtility.GetType(name);
                    CacheType.Add(item, t);
                }

                float width;
                if (!FindAsset.cacheImage.TryGetValue(name, out var content))
                {
                    if (t == null)
                        content = MyGUIContent.FromString(name);
                    else
                    {
                        var text = EditorGUIUtility.ObjectContent(null, t).image;
                        if (text == null)
                            content = MyGUIContent.FromString(name);
                        else
                            content = MyGUIContent.FromTexture(text, name);
                    }


                    FindAsset.cacheImage.Add(name, content);
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
        public static Dictionary<string, FindRef> FindSceneUseSceneObjects(GameObject[] targets)
        {
            var results = new Dictionary<string, FindRef>();
            var objs = Selection.gameObjects;
            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUtility.IsInAsset(objs[i])) continue;

                var key = objs[i].GetInstanceID().ToString();
                if (!results.ContainsKey(key)) results.Add(key, new FindSceneRef(0, objs[i]));

                var coms = objs[i].GetComponents<Component>();
                var sceneCache = FindSceneCache.Api.Cache;
                for (var j = 0; j < coms.Length; j++)
                {
                    if (coms[j] == null) continue; // missing component

                    if (sceneCache.TryGetValue(coms[j], out var hash))
                        foreach (var item in hash)
                        {
                            if (item.isSceneObject)
                            {
                                var obj = item.target;
                                var key1 = obj.GetInstanceID().ToString();
                                if (!results.ContainsKey(key1)) results.Add(key1, new FindSceneRef(1, obj));
                            }
                        }
                }
            }

            return results;
        }

        // ------------------------- Scene in scene
        public static Dictionary<string, FindRef> FindSceneInScene(GameObject[] targets)
        {
            var results = new Dictionary<string, FindRef>();
            var objs = Selection.gameObjects;
            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUtility.IsInAsset(objs[i])) continue;

                var key = objs[i].GetInstanceID().ToString();
                if (!results.ContainsKey(key)) results.Add(key, new FindSceneRef(0, objs[i]));


                foreach (var item in FindSceneCache.Api.Cache)
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
                    if (!results.ContainsKey(key)) results.Add(key, new FindSceneRef(1, item.Key));

                    (results[key] as FindSceneRef)?._usingType.Add(item1.target.GetType().FullName);
                }
            }

            return results;
        }

        public static Dictionary<string, FindRef> FindRefInScene(string[] assetGUIDs, bool depth, Action<Dictionary<string, FindRef>> onComplete, IWindow win)
        {
            Window = win;
            cacheAssetGuids = assetGUIDs;
            onFindRefInSceneComplete = onComplete;
            if (FindSceneCache.ready) FindRefInScene();
            else
            {
                FindSceneCache.onReady -= FindRefInScene;
                FindSceneCache.onReady += FindRefInScene;
            }

            return refs;
        }

        private static void FindRefInScene()
        {
            refs = new Dictionary<string, FindRef>();
            for (var i = 0; i < cacheAssetGuids.Length; i++)
            {
                var asset = FinderWindowBase.CacheSetting.Get(cacheAssetGuids[i]);
                if (asset == null) continue;

                Add(refs, asset, 0);
                ApplyFilter(refs, asset);
            }

            onFindRefInSceneComplete?.Invoke(refs);
            FindSceneCache.onReady -= FindRefInScene;
        }

        private static void ApplyFilter(Dictionary<string, FindRef> refs, FindAsset asset)
        {
            string targetPath = AssetDatabase.GUIDToAssetPath(asset.guid);
            if (string.IsNullOrEmpty(targetPath)) return; // asset not found - might be deleted!

            //asset being moved!
            if (targetPath != asset.AssetPath) asset.MarkAsDirty();

            var target = AssetDatabase.LoadAssetAtPath(targetPath, typeof(Object));
            if (target == null) return;

            bool targetIsGameobject = target is GameObject;

            if (targetIsGameobject)
                foreach (var item in FinderUtility.GetAllObjsInCurScene())
                {
                    if (FinderUtility.CheckIsPrefab(item))
                    {
                        string itemGuid = FinderUtility.GetPrefabParent(item);
                        if (itemGuid == asset.guid) Add(refs, item, 1);
                    }
                }

            string dir = Path.GetDirectoryName(targetPath);
            if (FindSceneCache.Api.folderCache.ContainsKey(dir))
            {
                foreach (var item in FindSceneCache.Api.folderCache[dir])
                {
                    if (!FindSceneCache.Api.Cache.ContainsKey(item)) continue;
                    foreach (var item1 in FindSceneCache.Api.Cache[item])
                    {
                        if (targetPath == AssetDatabase.GetAssetPath(item1.target)) Add(refs, item, 1);
                    }
                }
            }
        }

        private static void Add(Dictionary<string, FindRef> refs, FindAsset asset, int depth)
        {
            string targetId = asset.guid;
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FindRef(0, depth, asset, null));
        }

        private static void Add(Dictionary<string, FindRef> refs, Object target, int depth)
        {
            var targetId = target.GetInstanceID().ToString();
            if (!refs.ContainsKey(targetId)) refs.Add(targetId, new FindSceneRef(depth, target));
        }
    }

    public class FindRef
    {
        public readonly FindAsset addBy;
        public readonly FindAsset asset;
        public Object component;
        public int depth;
        public string group;
        public readonly int index;

        public bool isSceneRef;
        public int matchingScore;
        public readonly int type;

        public FindRef() { }

        public FindRef(int index, int depth, FindAsset asset, FindAsset by)
        {
            this.index = index;
            this.depth = depth;

            this.asset = asset;
            if (asset != null) type = AssetType.GetIndex(asset.Extension);

            addBy = by;
        }

        public FindRef(int index, int depth, FindAsset asset, FindAsset by, string group)
            : this(index, depth, asset, by)
        {
            this.group = group;
        }

        private static int Sorter(FindRef item1, FindRef item2)
        {
            int r = item1.depth.CompareTo(item2.depth);
            if (r != 0) return r;

            int t = item1.type.CompareTo(item2.type);
            if (t != 0) return t;

            return item1.index.CompareTo(item2.index);
        }


        public static FindRef[] FromDict(Dictionary<string, FindRef> dict)
        {
            if (dict == null || dict.Count == 0) return null;

            var result = new List<FindRef>();

            foreach (var kvp in dict)
            {
                if (kvp.Value?.asset == null) continue;

                result.Add(kvp.Value);
            }

            result.Sort(Sorter);

            return result.ToArray();
        }

        public static FindRef[] FromList(List<FindRef> list)
        {
            if (list == null || list.Count == 0) return null;

            list.Sort(Sorter);
            var result = new List<FindRef>();
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].asset == null) continue;
                result.Add(list[i]);
            }

            return result.ToArray();
        }

        public override string ToString()
        {
            if (isSceneRef)
            {
                var sr = (FindSceneRef) this;
                return sr.scenePath;
            }

            return asset.AssetPath;
        }

        public string GetSceneObjId()
        {
            if (component == null) return string.Empty;
            return component.GetInstanceID().ToString();
        }

        public virtual bool IsSelected() { return AssetBookmark.Contains(asset.guid); }

        public virtual void DrawToogleSelect(Rect r)
        {
            bool s = IsSelected();
            r.width = 16f;
            if (!GUI2.Toggle(r, ref s)) return;

            if (s) AssetBookmark.Add(this);
            else AssetBookmark.Remove(this);
        }

        internal List<FindRef> Append(Dictionary<string, FindRef> dict, params string[] guidList)
        {
            var result = new List<FindRef>();

            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Cache not yet ready! Please wait!");
                return result;
            }

            bool excludePackage = !FinderWindowBase.ShowPackageAsset;

            //filter to remove items that already in dictionary
            for (var i = 0; i < guidList.Length; i++)
            {
                string guid = guidList[i];
                if (dict.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;
                if (excludePackage && child.InPackages) continue;

                var r = new FindRef(dict.Count, depth + 1, child, asset);
                if (!asset.IsFolder) dict.Add(guid, r);

                result.Add(r);
            }

            return result;
        }

        internal void AppendUsedBy(Dictionary<string, FindRef> result, bool deep)
        {
            var h = asset.usedByMap;
            var list = deep ? new List<FindRef>() : null;

            if (asset.usedByMap == null) return;
            bool excludePackage = !FinderWindowBase.ShowPackageAsset;

            foreach (var kvp in h)
            {
                string guid = kvp.Key;
                if (result.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;

                if (child.IsMissing) continue;
                if (excludePackage && child.InPackages) continue;

                var r = new FindRef(result.Count, depth + 1, child, asset);
                if (!asset.IsFolder) result.Add(guid, r);

                if (deep) list.Add(r);
            }

            if (!deep) return;

            foreach (var item in list)
            {
                item.AppendUsedBy(result, true);
            }
        }

        internal void AppendUsage(Dictionary<string, FindRef> result, bool deep)
        {
            var h = asset.UseGUIDs;
            bool excludePackage = !FinderWindowBase.ShowPackageAsset;
            var list = deep ? new List<FindRef>() : null;

            foreach (var kvp in h)
            {
                string guid = kvp.Key;
                if (result.ContainsKey(guid)) continue;

                var child = FinderWindowBase.CacheSetting.Get(guid);
                if (child == null) continue;
                if (child.IsMissing) continue;
                if (excludePackage && child.InPackages) continue;

                var r = new FindRef(result.Count, depth + 1, child, asset);
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

        internal static Dictionary<string, FindRef> FindRefs(string[] guids, bool usageOrUsedBy, bool addFolder)
        {
            var dict = new Dictionary<string, FindRef>();
            var list = new List<FindRef>();
            bool excludePackage = !FinderWindowBase.ShowPackageAsset;

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (dict.ContainsKey(guid)) continue;

                var asset = FinderWindowBase.CacheSetting.Get(guid);
                if (asset == null) continue;
                if (excludePackage && asset.InPackages) continue;

                var r = new FindRef(i, 0, asset, null);
                if (!asset.IsFolder || addFolder) dict.Add(guid, r);

                list.Add(r);
            }

            for (var i = 0; i < list.Count; i++)
            {
                if (usageOrUsedBy)
                    list[i].AppendUsage(dict, true);
                else
                    list[i].AppendUsedBy(dict, true);
            }

            return dict;
        }


        public static Dictionary<string, FindRef> FindUsage(string[] guids) { return FindRefs(guids, true, true); }

        public static Dictionary<string, FindRef> FindUsedBy(string[] guids) { return FindRefs(guids, false, true); }

        public static Dictionary<string, FindRef> FindUsageScene(GameObject[] objs, bool depth)
        {
            var dict = new Dictionary<string, FindRef>();

            for (var i = 0; i < objs.Length; i++)
            {
                if (FinderUtility.IsInAsset(objs[i])) continue; //only get in scene 

                //add selection
                if (!dict.ContainsKey(objs[i].GetInstanceID().ToString())) dict.Add(objs[i].GetInstanceID().ToString(), new FindSceneRef(0, objs[i]));

                foreach (var item in FinderUtility.GetAllRefObjects(objs[i]))
                {
                    AppendUsageScene(dict, item);
                }

                if (!depth) continue;

                foreach (var child in FinderUtility.GetAllChild(objs[i]))
                {
                    foreach (var item2 in FinderUtility.GetAllRefObjects(child))
                    {
                        AppendUsageScene(dict, item2);
                    }
                }
            }

            return dict;
        }

        private static void AppendUsageScene(Dictionary<string, FindRef> dict, Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) return;

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;

            if (dict.ContainsKey(guid)) return;

            var asset = FinderWindowBase.CacheSetting.Get(guid);
            if (asset == null) return;
            if (!FinderWindowBase.ShowPackageAsset && asset.InPackages) return;

            var r = new FindRef(0, 1, asset, null);
            dict.Add(guid, r);
        }
    }


    public class FindRefDrawer : IRefDraw
    {
        public enum Mode
        {
            Dependency,
            Depth,
            Type,
            Extension,
            Folder,
            Atlas,
            AssetBundle,
            None
        }

        public enum Sort
        {
            Type,
            Path,
            Size
        }

        private readonly Dictionary<string, BookmarkInfo> _gBookmarkCache = new();

        internal readonly FindTreeUI2.GroupDrawer groupDrawer;

        public readonly bool caseSensitive = false;
        public bool forceHideDetails;
        public readonly List<FindAsset> highlight = new();
        public string level0Group;
        public bool showDetail;
        public string messageEmpty = "It's empty!";
        public string messageNoRefs = "Do select something!";
        public Func<FindRef, string> customGetGroup;
        public Action<Rect, string, int> customDrawGroupLabel;
        public Action<Rect, FindRef> beforeItemDraw;
        public Action<Rect, FindRef> afterItemDraw;
        public bool drawFullPath;
        public bool drawExtension;
        public bool drawFileSize;
        public bool drawUsageType;

        internal List<FindRef> list;
        internal Dictionary<string, FindRef> refs;
        private bool _selectFilter;
        private bool _showIgnore;
        private readonly string _searchTerm = string.Empty;
        private readonly bool _showSearch = true;
        private readonly Func<Sort> _getSortMode;
        private readonly Func<Mode> _getGroupMode;
        private bool _dirty;
        private int _excludeCount;

        public FindRefDrawer(IWindow window, Func<Sort> getSortMode, Func<Mode> getGroupMode)
        {
            this.Window = window;
            _getSortMode = getSortMode;
            _getGroupMode = getGroupMode;
            groupDrawer = new FindTreeUI2.GroupDrawer(DrawGroup, DrawAsset);
        }

        internal FindRef[] Source => FindRef.FromList(list);

        public IWindow Window { get; set; }

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

        public int ElementCount() { return refs?.Count ?? 0; }

        private void DrawEmpty(Rect rect, string text)
        {
            rect = GUI2.Padding(rect, 2f, 2f);
            rect.height = 45f;

            EditorGUI.HelpBox(rect, text, MessageType.Info);
        }

        public void SetRefs(Dictionary<string, FindRef> dictRefs)
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

                if (willbookmark) AssetBookmark.Add(rf);
                else AssetBookmark.Remove(rf);
            }

            info.count = willbookmark ? info.total : 0;
        }

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

                    bool isBm = AssetBookmark.Contains(rf);
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
            if (string.IsNullOrEmpty(label)) label = "(none)";
            DrawToggleGroup(r, label);
            r.xMin += 18f;

            var groupMode = _getGroupMode();

            if (groupMode == Mode.Folder)
            {
                var tex = AssetDatabase.GetCachedIcon("Assets");
                GUI.DrawTexture(new Rect(r.x, r.y, 16f, 16f), tex);
                r.xMin += 16f;
            }

            if (customDrawGroupLabel != null)
            {
                customDrawGroupLabel.Invoke(r, label, childCount);
            }
            else
            {
                var lbContent = MyGUIContent.FromString(label);
                GUI.Label(r, lbContent, EditorStyles.boldLabel);

                var cRect = r;
                cRect.x += EditorStyles.boldLabel.CalcSize(lbContent).x;
                cRect.y += 1f;
                GUI.Label(cRect, MyGUIContent.FromString($"({childCount})"), EditorStyles.miniLabel);
            }

            bool hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);
            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                menu.AddItem(MyGUIContent.FromString("Add Bookmark"), false, () => { SetBookmarkGroup(label, true); });
                menu.AddItem(MyGUIContent.FromString("Remove Bookmark"), false, () => { SetBookmarkGroup(label, false); });

                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        public void DrawDetails(Rect rect)
        {
            var r = rect;
            r.xMin += 18f;
            r.height = 18f;

            for (var i = 0; i < highlight.Count; i++)
            {
                highlight[i]
                .Draw(r,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    Window, false);
                r.y += 18f;
                r.xMin += 18f;
            }
        }

        private void DrawAsset(Rect r, string guid)
        {
            if (!refs.TryGetValue(guid, out var rf)) return;

            if (rf.isSceneRef)
            {
                if (rf.component == null) return;
                if (!(rf is FindSceneRef re)) return;
                beforeItemDraw?.Invoke(r, rf);
                rf.DrawToogleSelect(r);
                r.xMin += 32f;
                re.Draw(r, Window, _getGroupMode(), !forceHideDetails);
            }
            else
            {
                beforeItemDraw?.Invoke(r, rf);
                rf.DrawToogleSelect(r);
                r.xMin += 32f;

                float w2 = (r.x + r.width) / 2f;
                var rRect = new Rect(w2, r.y, w2, r.height);
                bool isClick = Event.current.type == EventType.MouseDown && Event.current.button == 0;

                if (isClick && rRect.Contains(Event.current.mousePosition))
                {
                    showDetail = true;
                    highlight.Clear();
                    highlight.Add(rf.asset);

                    var p = rf.addBy;
                    var cnt = 0;

                    while (p != null && refs.ContainsKey(p.guid))
                    {
                        highlight.Add(p);

                        var findRef = refs[p.guid];
                        if (findRef != null) p = findRef.addBy;

                        if (++cnt > 100)
                        {
                            Debug.LogWarning("Break on depth 1000????");
                            break;
                        }
                    }

                    highlight.Sort((item1, item2) =>
                    {
                        int d1 = refs[item1.guid].depth;
                        int d2 = refs[item2.guid].depth;
                        return d1.CompareTo(d2);
                    });

                    Event.current.Use();
                }

                bool isHighlight = highlight.Contains(rf.asset);

                rf.asset.Draw(r,
                    isHighlight,
                    drawFullPath,
                    !forceHideDetails && drawFileSize,
                    !forceHideDetails && FinderWindowBase.DisplayAssetBundleName,
                    !forceHideDetails && FinderWindowBase.DisplayAtlasName,
                    !forceHideDetails && drawUsageType,
                    Window,
                    !forceHideDetails && drawExtension);
            }

            afterItemDraw?.Invoke(r, rf);
        }

        private string GetGroup(FindRef rf)
        {
            if (customGetGroup != null) return customGetGroup(rf);
            if (rf.depth == 0) return level0Group;
            if (_getGroupMode() == Mode.None) return "(no group)";

            FindSceneRef sr = null;
            if (rf.isSceneRef)
            {
                sr = rf as FindSceneRef;
                if (sr == null) return null;
            }

            if (!rf.isSceneRef)
            {
                if (rf.asset.IsExcluded) return null;
            }

            switch (_getGroupMode())
            {
                case Mode.Extension: return rf.isSceneRef ? sr.targetType : string.IsNullOrEmpty(rf.asset.Extension) ? "(no extension)" : rf.asset.Extension;
                case Mode.Type: return rf.isSceneRef ? sr.targetType : AssetType.Filters[rf.type].name;
                case Mode.Folder: return rf.isSceneRef ? sr.scenePath : rf.asset.AssetFolder;
                case Mode.Dependency: return rf.depth == 1 ? "Direct Usage" : "Indirect Usage";
                case Mode.Depth: return "Level " + rf.depth;
                case Mode.Atlas: return rf.isSceneRef ? "(not in atlas)" : string.IsNullOrEmpty(rf.asset.AtlasName) ? "(not in atlas)" : rf.asset.AtlasName;
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

                return String.Compare(item1, item2, StringComparison.Ordinal);
            });
        }

        public FindRefDrawer Reset(string[] assetGUIDs, bool isUsage)
        {
            _gBookmarkCache.Clear();

            refs = isUsage ? FindRef.FindUsage(assetGUIDs) : FindRef.FindUsedBy(assetGUIDs);
            _dirty = true;
            list?.Clear();

            return this;
        }

        public void Reset(Dictionary<string, FindRef> newRefs)
        {
            if (refs == null) refs = new Dictionary<string, FindRef>();
            refs.Clear();
            foreach (var kvp in newRefs)
            {
                refs.Add(kvp.Key, kvp.Value);
            }

            _dirty = true;
            if (list != null) list.Clear();
        }

        public FindRefDrawer Reset(GameObject[] objs, bool findDept, bool findPrefabInAsset)
        {
            refs = FindRef.FindUsageScene(objs, findDept);

            var guidss = new List<string>();
            var dependent = FindSceneCache.Api.prefabDependencies;
            foreach (var gameObject in objs)
            {
                if (!dependent.TryGetValue(gameObject, out var hash)) continue;

                foreach (string guid in hash)
                {
                    guidss.Add(guid);
                }
            }

            var usageRefs1 = FindRef.FindUsage(guidss.ToArray());
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
                    string guid = FinderUtility.GetPrefabParent(objs[i]);
                    if (string.IsNullOrEmpty(guid)) continue;

                    guids.Add(guid);
                }

                var usageRefs = FindRef.FindUsage(guids.ToArray());
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
        public FindRefDrawer Reset(string[] assetGUIDs, IWindow window)
        {
            refs = FindSceneRef.FindRefInScene(assetGUIDs, true, SetRefInScene, window);
            _dirty = true;
            list?.Clear();

            return this;
        }

        private void SetRefInScene(Dictionary<string, FindRef> data)
        {
            refs = data;
            _dirty = true;
            list?.Clear();
        }

        //scene in scene
        public FindRefDrawer ResetSceneInScene(GameObject[] objs)
        {
            refs = FindSceneRef.FindSceneInScene(objs);
            _dirty = true;
            list?.Clear();

            return this;
        }

        public FindRefDrawer ResetSceneUseSceneObjects(GameObject[] objs)
        {
            refs = FindSceneRef.FindSceneUseSceneObjects(objs);
            _dirty = true;
            list?.Clear();

            return this;
        }

        public FindRefDrawer ResetUnusedAsset()
        {
            var lst = FinderWindowBase.CacheSetting.ScanUnused();

            refs = lst.ToDictionary(x => x.guid, x => new FindRef(0, 1, x, null));
            _dirty = true;
            list?.Clear();

            return this;
        }

        public void RefreshSort()
        {
            if (list == null) return;

            if (list.Count > 0 && list[0].isSceneRef == false && _getSortMode() == Sort.Size) list = list.OrderByDescending(x => x.asset?.FileSize ?? 0).ToList();
            else
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
                        var rs1 = (FindSceneRef) r1;
                        var rs2 = (FindSceneRef) r2;

                        return SortAsset(rs1.sceneFullPath,
                            rs2.sceneFullPath,
                            rs1.targetType,
                            rs2.targetType,
                            _getSortMode() == Sort.Path);
                    }

                    if (r1.asset == null) return -1;
                    if (r2.asset == null) return 1;

                    return SortAsset(r1.asset.AssetPath,
                        r2.asset.AssetPath,
                        r1.asset.Extension,
                        r2.asset.Extension,
                        false);
                });

            // clean up list
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];

                if (item.isSceneRef)
                {
                    if (string.IsNullOrEmpty(item.GetSceneObjId())) list.RemoveAt(i);

                    continue;
                }

                if (item.asset == null) list.RemoveAt(i);
            }
            
            groupDrawer.Reset(list,
                rf =>
                {
                    if (rf == null) return null;
                    return rf.isSceneRef ? rf.GetSceneObjId() : rf.asset?.guid;
                },
                GetGroup,
                SortGroup);
        }

        public bool IsExclueAnyItem() { return _excludeCount > 0; }

        private void ApplyFilter()
        {
            _dirty = false;

            if (refs == null) return;

            if (list == null) list = new List<FindRef>();
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
                string name1 = r.isSceneRef ? (r as FindSceneRef)?.sceneFullPath : r.asset.AssetName;
                if (!caseSensitive) name1 = name1?.ToLower();

                string name2 = name1?.Replace(" ", string.Empty);

                int score1 = FinderUtility.StringMatch(term1, name1);
                int score2 = FinderUtility.StringMatch(term2, name2);

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
            return swap ? v1 == 0 ? v2 : v1 : v2 == 0 ? v1 : v2;
        }

        public Dictionary<string, FindRef> GetRefs() { return refs; }

        internal class BookmarkInfo
        {
            public int count;
            public int total;
        }
    }
}