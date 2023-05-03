using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PancakeEditor
{
    [InitializeOnLoad]
    public class FinderCacheHelper : AssetPostprocessor
    {
        private static HashSet<string> scenes;
        private static HashSet<string> guidsIgnore;

        static FinderCacheHelper()
        {
            EditorApplication.update -= InitHelper;
            EditorApplication.update += InitHelper;
        }

        private static void InitHelper()
        {
            if (EditorApplication.isCompiling) return;
            if (!FinderWindowBase.IsCacheReady) return;
            if (!FinderWindowBase.CacheDisabled)
            {
                InitListScene();
                InitIgnore();

#if UNITY_2018_1_OR_NEWER
                UnityEditor.EditorBuildSettings.sceneListChanged -= InitListScene;
                UnityEditor.EditorBuildSettings.sceneListChanged += InitListScene;
#endif

                EditorApplication.projectWindowItemOnGUI -= OnGUIProjectItem;
                EditorApplication.projectWindowItemOnGUI += OnGUIProjectItem;

                FinderCache.onCacheReady -= OnCacheReady;
                FinderCache.onCacheReady += OnCacheReady;
            }

            EditorApplication.update -= InitHelper;
        }

        private static void OnCacheReady()
        {
            InitIgnore();
            // force repaint all project panels
            EditorApplication.RepaintProjectWindow();
        }

        public static void InitIgnore()
        {
            guidsIgnore = new HashSet<string>();
            foreach (string item in FinderWindowBase.IgnoreAsset)
            {
                string guid = AssetDatabase.AssetPathToGUID(item);
                if (guidsIgnore.Contains(guid))
                {
                    continue;
                }

                guidsIgnore.Add(guid);
            }
        }

        private static void InitListScene()
        {
            scenes = new HashSet<string>();

            foreach (EditorBuildSettingsScene scene in UnityEditor.EditorBuildSettings.scenes)
            {
                string sce = AssetDatabase.AssetPathToGUID(scene.path);

                if (scenes.Contains(sce))
                {
                    continue;
                }

                scenes.Add(sce);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            FinderWindowBase.FinderDelayCheck4Changes();

            if (!FinderWindowBase.IsCacheReady)
            {
                return;
            }

            if (FinderWindowBase.CacheSetting.assetMap == null) return;

            for (var i = 0; i < importedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(importedAssets[i]);

                if (FinderWindowBase.CacheSetting.assetMap.ContainsKey(guid))
                {
                    FinderWindowBase.CacheSetting.RefreshAsset(guid, true);

                    continue;
                }

                FinderWindowBase.CacheSetting.AddAsset(guid);
            }

            for (var i = 0; i < deletedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(deletedAssets[i]);
                FinderWindowBase.CacheSetting.RemoveAsset(guid);
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                FinderAsset asset = FinderWindowBase.CacheSetting.Get(guid);
                if (asset != null)
                {
                    asset.MarkAsDirty(true, false);
                }
            }

            FinderWindowBase.CacheSetting.Check4Work();
        }

        private static void OnGUIProjectItem(string guid, Rect rect)
        {
            var r = new Rect(rect.x, rect.y, 1f, 16f);
            if (scenes.Contains(guid))
            {
                EditorGUI.DrawRect(r, GUI2.Theme(new Color32(72, 150, 191, 255), Color.blue));
            }
            else if (guidsIgnore.Contains(guid))
            {
                var ignoreRect = new Rect(rect.x + 3f, rect.y + 6f, 2f, 2f);
                EditorGUI.DrawRect(ignoreRect, GUI2.darkRed);
            }

            if (!FinderWindowBase.IsCacheReady)
            {
                return; // not ready
            }

            if (!FinderWindowBase.ShowReferenceCount)
            {
                return;
            }

            if (FinderWindowBase.CacheSetting.assetMap == null) FinderWindowBase.CacheSetting.Check4Changes(false);

            if (!FinderWindowBase.CacheSetting.assetMap.TryGetValue(guid, out var item)) return;

            if (item == null || item.usedByMap == null) return;

            if (item.usedByMap.Count > 0)
            {
                var content = new GUIContent(item.usedByMap.Count.ToString());
                r.width = 0f;
                r.xMin -= 100f;
                GUI.Label(r, content, GUI2.MiniLabelAlignRight);
            }
        }
    }

    [Serializable]
    internal class FinderSetting
    {
        public bool alternateColor = true;
        public int excludeTypes; //32-bit type Mask
        public FinderRefDrawer.Mode groupMode;
        public List<string> listIgnore = new List<string>();
        public bool pingRow = true;
        public bool showReferenceCount = true;

        public bool showFileSize;
        public bool displayFileSize = true;
        public bool displayAtlasName = false;
        public bool displayAssetBundleName = false;

        public bool showUsedByClassed = true;
        public FinderRefDrawer.Sort sortMode;

        public int treeIndent = 10;

        public Color32 rowColor = new Color32(0, 0, 0, 12);
        public Color32 scanColor = new Color32(0, 204, 102, 255);
        [Newtonsoft.Json.JsonIgnore]
        public Color selectedColor = new Color(0, 0f, 1f, 0.25f);

        [NonSerialized] internal static HashSet<string> hashIgnore;

        public static Action onIgnoreChange;
    }

    [Serializable]
    public class FinderCache
    {
        internal static int cacheStamp;
        internal static Action onCacheReady;
        internal static bool triedToLoadCache;
        internal static int priority = 5;

        public bool disabled;
        public List<FinderAsset> assetList;
        internal bool ready;

        internal int frameSkipped;
        [NonSerialized] internal Dictionary<string, FinderAsset> assetMap;
        [NonSerialized] internal List<FinderAsset> queueLoadContent;
        [NonSerialized] internal int workCount;


        internal float Progress
        {
            get
            {
                int n = workCount - queueLoadContent.Count;
                return workCount == 0 ? 1 : n / (float) workCount;
            }
        }

        internal void ReadFromCache()
        {
            if (assetList == null) assetList = new List<FinderAsset>();

            FinderUnity.Clear(ref queueLoadContent);
            FinderUnity.Clear(ref assetMap);

            for (var i = 0; i < assetList.Count; i++)
            {
                var item = assetList[i];
                item.state = EFinderAssetState.Cache;

                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(item.guid);
                if (string.IsNullOrEmpty(path))
                {
                    item.type = EFinderAssetType.Unknown; // to make sure if GUIDs being reused for a different kind of asset
                    item.state = EFinderAssetState.Missing;
                    assetMap.Add(item.guid, item);
                    continue;
                }

                if (assetMap.ContainsKey(item.guid)) continue;

                assetMap.Add(item.guid, item);
            }
        }

        internal void Check4Changes(bool force)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                FinderWindowBase.FinderDelayCheck4Changes();
                return;
            }

            ready = false;
            ReadFromProject(force);
            Check4Work();
        }

        internal void RefreshAsset(string guid, bool force)
        {
            if (!assetMap.TryGetValue(guid, out var asset))
            {
                return;
            }

            RefreshAsset(asset, force);
        }

        internal void RefreshAsset(FinderAsset asset, bool force)
        {
            asset.MarkAsDirty(true, force);
            FinderWindowBase.FinderDelayCheck4Changes();
        }

        internal void ReadFromProject(bool force)
        {
            if (assetMap == null || assetMap.Count == 0) ReadFromCache();

            var paths = AssetDatabase.GetAllAssetPaths();
            cacheStamp++;
            workCount = 0;
            if (queueLoadContent != null) queueLoadContent.Clear();

            // Check for new assets
            foreach (var p in paths)
            {
                var isValid = FinderUnity.StringStartsWith(p,
                    "Assets/",
                    "Packages/",
                    "Library/",
                    "ProjectSettings/");

                if (!isValid) continue;

                var guid = AssetDatabase.AssetPathToGUID(p);

                FinderAsset asset;
                if (!assetMap.TryGetValue(guid, out asset))
                {
                    AddAsset(guid);
                }
                else
                {
                    asset.refreshStamp = cacheStamp; // mark this asset so it won't be deleted
                    if (!asset.IsDirty && !force) continue;

                    if (force) asset.MarkAsDirty(true, true);


                    workCount++;
                    queueLoadContent.Add(asset);
                }
            }

            // Check for deleted assets
            for (var i = assetList.Count - 1; i >= 0; i--)
            {
                if (assetList[i].refreshStamp != cacheStamp)
                {
                    RemoveAsset(assetList[i]);
                }
            }
        }

        internal void AddAsset(string guid)
        {
            if (assetMap.ContainsKey(guid))
            {
                Debug.LogWarning("guid already exist <" + guid + ">");
                return;
            }

            var asset = new FinderAsset(guid);
            asset.LoadPathInfo();
            asset.refreshStamp = cacheStamp;

            assetList.Add(asset);
            assetMap.Add(guid, asset);

            workCount++;
            queueLoadContent.Add(asset);
        }

        internal void RemoveAsset(string guid)
        {
            if (!assetMap.ContainsKey(guid)) return;
            RemoveAsset(assetMap[guid]);
        }

        internal void RemoveAsset(FinderAsset asset)
        {
            assetList.Remove(asset);
            asset.state = EFinderAssetState.Missing;
        }

        internal void Check4Usage()
        {
            foreach (var item in assetList)
            {
                if (item.IsMissing) continue;
                FinderUnity.Clear(ref item.usedByMap);
            }

            foreach (var item in assetList)
            {
                if (item.IsMissing) continue;
                AsyncUsedBy(item);
            }

            workCount = 0;
            ready = true;
        }

        internal void Check4Work()
        {
            if (disabled) return;

            if (workCount == 0)
            {
                Check4Usage();
                return;
            }

            ready = false;
            UnregisterAsyncProcess();
            EditorApplication.update += AsyncProcess;
        }

        internal void AsyncProcess()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            if (frameSkipped++ < 10 - 2 * priority) return;

            frameSkipped = 0;
            float t = Time.realtimeSinceStartup;

            if (!AsyncWork(queueLoadContent, AsyncLoadContent, t)) return;

            UnregisterAsyncProcess();
            Check4Usage();
        }

        internal void UnregisterAsyncProcess() { EditorApplication.update -= AsyncProcess; }

        internal bool AsyncWork<T>(List<T> arr, Action<int, T> action, float t)
        {
            float frameDuration = 1 / 1000f * (priority * 5 + 1); //prevent zero

            int c = arr.Count;
            var counter = 0;

            while (c-- > 0)
            {
                T last = arr[c];
                arr.RemoveAt(c);
                action(c, last);
                //workCount--;

                float dt = Time.realtimeSinceStartup - t - frameDuration;
                if (dt >= 0)
                {
                    return false;
                }

                counter++;
            }

            return true;
        }

        internal void AsyncLoadContent(int idx, FinderAsset asset)
        {
            if (asset.FileInfoDirty) asset.LoadFileInfo();
            if (asset.FileContentDirty) asset.LoadContent();
        }

        internal void AsyncUsedBy(FinderAsset asset)
        {
            if (assetMap == null) Check4Changes(false);
            if (asset.IsFolder) return;
            foreach (KeyValuePair<string, HashSet<int>> item in asset.UseGUIDs)
            {
                FinderAsset tAsset;
                if (assetMap.TryGetValue(item.Key, out tAsset))
                {
                    if (tAsset == null || tAsset.usedByMap == null)
                    {
                        continue;
                    }

                    if (!tAsset.usedByMap.ContainsKey(asset.guid))
                    {
                        tAsset.AddUsedBy(asset.guid, asset);
                    }
                }
            }
        }

        internal FinderAsset Get(string guid, bool isForce = false) { return assetMap.ContainsKey(guid) ? assetMap[guid] : null; }

        internal List<FinderAsset> FindAssets(string[] guids, bool scanFolder)
        {
            if (assetMap == null)
            {
                Check4Changes(false);
            }

            var result = new List<FinderAsset>();

            if (!ready)
            {
                return result;
            }

            var folderList = new List<FinderAsset>();

            if (guids.Length == 0)
            {
                return result;
            }

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                FinderAsset asset;
                if (!assetMap.TryGetValue(guid, out asset))
                {
                    continue;
                }

                if (asset.IsMissing)
                {
                    continue;
                }

                if (asset.IsFolder)
                {
                    if (!folderList.Contains(asset))
                    {
                        folderList.Add(asset);
                    }
                }
                else
                {
                    result.Add(asset);
                }
            }

            if (!scanFolder || folderList.Count == 0)
            {
                return result;
            }

            int count = folderList.Count;
            for (var i = 0; i < count; i++)
            {
                FinderAsset item = folderList[i];

                foreach (KeyValuePair<string, HashSet<int>> useM in item.UseGUIDs)
                {
                    FinderAsset a;
                    if (!assetMap.TryGetValue(useM.Key, out a))
                    {
                        continue;
                    }

                    if (a.IsMissing)
                    {
                        continue;
                    }

                    if (a.IsFolder)
                    {
                        if (!folderList.Contains(a))
                        {
                            folderList.Add(a);
                            count++;
                        }
                    }
                    else
                    {
                        result.Add(a);
                    }
                }
            }

            return result;
        }
    }
}