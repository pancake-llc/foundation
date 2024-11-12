using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    [InitializeOnLoad]
    public class CacheHelper : AssetPostprocessor
    {
        [NonSerialized] private static HashSet<string> scenes;
        [NonSerialized] private static HashSet<string> guidsIgnore;
        [NonSerialized] internal static bool inited = false;

        static CacheHelper()
        {
            try
            {
                EditorApplication.update -= InitHelper;
                EditorApplication.update += InitHelper;
            }
            catch (Exception)
            {
                //ignored
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            FinderWindowBase.DelayCheck4Changes();

            if (!FinderWindowBase.IsCacheReady) return;

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
                var asset = FinderWindowBase.CacheSetting.Get(guid);
                asset?.MarkAsDirty();
            }

            FinderWindowBase.CacheSetting.Check4Work();
        }

        internal static void InitHelper()
        {
            if (FinderUtility.isEditorCompiling || FinderUtility.isEditorUpdating) return;

            if (!FinderWindowBase.IsCacheReady) return;
            EditorApplication.update -= InitHelper;

            inited = true;
            InitListScene();
            InitIgnore();

#if UNITY_2018_1_OR_NEWER
            EditorBuildSettings.sceneListChanged -= InitListScene;
            EditorBuildSettings.sceneListChanged += InitListScene;
#endif

#if UNITY_2022_1_OR_NEWER
            EditorApplication.projectWindowItemInstanceOnGUI -= OnGUIProjectInstance;
            EditorApplication.projectWindowItemInstanceOnGUI += OnGUIProjectInstance;
#else
            EditorApplication.projectWindowItemOnGUI -= OnGUIProjectItem;
            EditorApplication.projectWindowItemOnGUI += OnGUIProjectItem;
#endif

            InitIgnore();
        }

        public static void InitIgnore()
        {
            guidsIgnore = new HashSet<string>();
            foreach (string item in FinderWindowBase.IgnoreAsset)
            {
                string guid = AssetDatabase.AssetPathToGUID(item);
                guidsIgnore.Add(guid);
            }
        }

        private static void InitListScene()
        {
            scenes = new HashSet<string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                string sce = AssetDatabase.AssetPathToGUID(scene.path);
                scenes.Add(sce);
            }
        }

        private static string lastGuid;

        private static void OnGUIProjectInstance(int instanceID, Rect selectionRect)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceID, out string guid, out long localId)) return;

            bool isMainAsset = guid != lastGuid;
            lastGuid = guid;

            if (isMainAsset)
            {
                DrawProjectItem(guid, selectionRect);
                return;
            }

            if (!FinderWindowBase.ShowSubAssetFileId) return;
            var rect2 = selectionRect;
            var label = new GUIContent(localId.ToString());
            rect2.xMin = rect2.xMax - EditorStyles.miniLabel.CalcSize(label).x;
            var c = GUI.color;
            GUI.color = new Color(.5f, .5f, .5f, 0.5f);
            GUI.Label(rect2, label, EditorStyles.miniLabel);
            GUI.color = c;
        }

        private static void OnGUIProjectItem(string guid, Rect rect)
        {
            bool isMainAsset = guid != lastGuid;
            lastGuid = guid;
            if (isMainAsset) DrawProjectItem(guid, rect);
        }

        private static void DrawProjectItem(string guid, Rect rect)
        {
            var r = new Rect(rect.x, rect.y, 1f, 16f);
            if (scenes.Contains(guid)) EditorGUI.DrawRect(r, GUI2.Theme(new Color32(72, 150, 191, 255), Color.blue));
            else if (guidsIgnore.Contains(guid))
            {
                var ignoreRect = new Rect(rect.x + 3f, rect.y + 6f, 2f, 2f);
                EditorGUI.DrawRect(ignoreRect, GUI2.darkRed);
            }

            if (!FinderWindowBase.IsCacheReady) return; // not ready

            if (!FinderWindowBase.ShowReferenceCount) return;

            if (FinderWindowBase.CacheSetting.assetMap == null) FinderWindowBase.CacheSetting.Check4Changes(false);

            // ReSharper disable once PossibleNullReferenceException
            if (!FinderWindowBase.CacheSetting.assetMap.TryGetValue(guid, out var item)) return;

            if (item == null || item.usedByMap == null) return;

            if (item.usedByMap.Count > 0)
            {
                var content = MyGUIContent.FromString(item.usedByMap.Count.ToString());
                r.width = 0f;
                r.xMin -= 100f;
                GUI.Label(r, content, GUI2.MiniLabelAlignRight);
            }
            else if (item.ForcedIncludedInBuild)
            {
                var c = GUI.color;
                GUI.color = c.ChangeAlpha(0.2f);
                var content = MyGUIContent.FromString("+");
                r.width = 0f;
                r.xMin -= 100f;
                GUI.Label(r, content, GUI2.MiniLabelAlignRight);
                GUI.color = c;
            }
        }
    }

    [Serializable]
    internal class FinderSetting
    {
        private static FinderSetting d;
        [NonSerialized] internal static HashSet<string> hashIgnore;
        public static Action onIgnoreChange;
        public bool alternateColor = true;
        public int excludeTypes;
        public List<string> listIgnore = new();
        public bool pingRow = true;
        public bool referenceCount = true;
        public bool showPackageAsset = true;
        public bool showSubAssetFileId;
        public bool showFileSize;
        public bool displayFileSize = true;
        public bool displayAtlasName;
        public bool displayAssetBundleName;
        public bool showUsedByClassed = true;
        public int treeIndent = 10;
        public bool disableInPlayMode = true;
        public bool disabled;

        public Color32 rowColor = new(0, 0, 0, 12);
        public Color32 selectedColor = new(0, 0, 255, 63);
    }

    [Serializable]
    public class AssetCache
    {
        internal static int cacheStamp;
        internal static Action onCacheReady;
        internal static bool triedToLoadCache;
        internal static int priority = 5;

        public List<FindAsset> assetList;
        internal bool ready;

        internal int frameSkipped;
        [NonSerialized] internal Dictionary<string, FindAsset> assetMap;
        [NonSerialized] internal List<FindAsset> queueLoadContent;
        [NonSerialized] internal int workCount;

        private static readonly HashSet<string> SpecialUseAssets = new()
        {
            "Assets/link.xml", // this file used to control build/link process do not remove
            "Assets/csc.rsp",
            "Assets/mcs.rsp",
            "Assets/GoogleService-Info.plist",
            "Assets/google-services.json"
        };

        private static readonly HashSet<string> SpecialExtensions = new()
        {
            ".asmdef",
            ".cginc",
            ".cs",
            ".dll",
            ".mdb",
            ".pdb",
            ".rsp",
            ".md",
            ".winmd",
            ".xml",
            ".XML",
            ".tsv",
            ".csv",
            ".json",
            ".pdf",
            ".txt",
            ".giparams",
            ".wlt",
            ".preset",
            ".exr",
            ".aar",
            ".srcaar",
            ".pom",
            ".bin",
            ".html",
            ".chm",
            ".data",
            ".jsp",
            ".unitypackage"
        };

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
            if (FinderWindowBase.InternalDisabled) Debug.LogWarning("Something wrong??? Finder is disabled!");

            if (assetList == null) assetList = new List<FindAsset>();

            FinderUtility.Clear(ref queueLoadContent);
            FinderUtility.Clear(ref assetMap);

            for (var i = 0; i < assetList.Count; i++)
            {
                var item = assetList[i];
                item.state = EFinderAssetState.Cache;

                var path = AssetDatabase.GUIDToAssetPath(item.guid);
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
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || FinderWindowBase.Disabled)
            {
                FinderWindowBase.DelayCheck4Changes();
                return;
            }

            ready = false;
            ReadFromProject(force);
            Check4Work();
        }

        internal void RefreshSelection()
        {
            string[] list = FinderUtility.SelectionAssetGUIDs;
            for (var i = 0; i < list.Length; i++)
            {
                RefreshAsset(list[i], true);
            }

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

        internal void RefreshAsset(FindAsset asset, bool force)
        {
            asset.MarkAsDirty(true, force);
            FinderWindowBase.DelayCheck4Changes();
        }

        internal void ReadFromProject(bool force)
        {
            if (FinderWindowBase.InternalDisabled) Debug.LogWarning("Something wrong??? Finder is disabled!");
            if (assetMap == null || assetMap.Count == 0) ReadFromCache();
            foreach (string b in FindAsset.BuiltInAssets)
            {
                if (assetMap.ContainsKey(b)) continue;
                var asset = new FindAsset(b);
                assetMap.Add(b, asset);
                assetList.Add(asset);
            }

            string[] paths = AssetDatabase.GetAllAssetPaths();
            cacheStamp++;
            workCount = 0;
            queueLoadContent?.Clear();

            // Check for new assets
            foreach (string p in paths)
            {
                var isValid = FinderUtility.StringStartsWith(p,
                    "Assets/",
                    "Packages/",
                    "Library/",
                    "ProjectSettings/");

                if (!isValid) continue;

                string guid = AssetDatabase.AssetPathToGUID(p);

                if (!assetMap.TryGetValue(guid, out var asset)) AddAsset(guid);
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

            var asset = new FindAsset(guid);
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

        internal void RemoveAsset(FindAsset asset)
        {
            assetList.Remove(asset);
            asset.state = EFinderAssetState.Missing;
        }

        internal void Check4Usage()
        {
            foreach (var item in assetList)
            {
                if (item.IsMissing) continue;
                FinderUtility.Clear(ref item.usedByMap);
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
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || FinderWindowBase.InternalDisabled) return;

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
                var last = arr[c];
                arr.RemoveAt(c);
                action(c, last);

                float dt = Time.realtimeSinceStartup - t - frameDuration;
                if (dt >= 0) return false;

                counter++;
            }

            return true;
        }

        internal void AsyncLoadContent(int idx, FindAsset asset)
        {
            if (asset.FileInfoDirty) asset.LoadFileInfo();
            if (asset.FileContentDirty) asset.LoadContent();
        }

        internal void AsyncUsedBy(FindAsset asset)
        {
            if (assetMap == null) Check4Changes(false);
            if (asset.IsFolder) return;
            foreach (var item in asset.UseGUIDs)
            {
                if (assetMap.TryGetValue(item.Key, out var tAsset))
                {
                    if (tAsset == null || tAsset.usedByMap == null) continue;

                    if (!tAsset.usedByMap.ContainsKey(asset.guid)) tAsset.AddUsedBy(asset.guid, asset);
                }
            }
        }

        internal FindAsset Get(string guid, bool isForce = false) { return assetMap.ContainsKey(guid) ? assetMap[guid] : null; }

        internal List<FindAsset> FindAssets(string[] guids, bool scanFolder)
        {
            if (assetMap == null) Check4Changes(false);
            var result = new List<FindAsset>();
            if (!ready) return result;
            var folderList = new List<FindAsset>();
            if (guids.Length == 0) return result;

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (!assetMap.TryGetValue(guid, out var asset)) continue;

                if (asset.IsMissing) continue;

                if (asset.IsFolder)
                {
                    if (!folderList.Contains(asset)) folderList.Add(asset);
                }
                else result.Add(asset);
            }

            if (!scanFolder || folderList.Count == 0) return result;

            int count = folderList.Count;
            for (var i = 0; i < count; i++)
            {
                var item = folderList[i];

                foreach (var useM in item.UseGUIDs)
                {
                    if (!assetMap.TryGetValue(useM.Key, out var a)) continue;
                    if (a.IsMissing) continue;
                    if (a.IsFolder)
                    {
                        if (!folderList.Contains(a))
                        {
                            folderList.Add(a);
                            count++;
                        }
                    }
                    else result.Add(a);
                }
            }

            return result;
        }

        internal List<List<string>> ScanSimilar(Action ignoreWhenScan, Action ignoreFolderWhenScan)
        {
            if (assetMap == null) Check4Changes(true);

            var dict = new Dictionary<string, List<FindAsset>>();
            foreach (var item in assetMap)
            {
                if (item.Value == null) continue;
                if (item.Value.IsMissing || item.Value.IsFolder) continue;
                if (item.Value.InPlugins) continue;
                if (item.Value.InEditor) continue;
                if (item.Value.IsExcluded) continue;
                if (!item.Value.AssetPath.StartsWith("Assets/")) continue;
                if (FinderWindowBase.IsTypeExcluded(AssetType.GetIndex(item.Value.Extension)))
                {
                    ignoreWhenScan?.Invoke();
                    continue;
                }

                string hash = item.Value.FileInfoHash;
                if (string.IsNullOrEmpty(hash)) continue;

                if (!dict.TryGetValue(hash, out var list))
                {
                    list = new List<FindAsset>();
                    dict.Add(hash, list);
                }

                list.Add(item.Value);
            }

            return dict.Values.Where(item => item.Count > 1)
                .OrderByDescending(item => item[0].FileSize)
                .Select(item => item.Select(asset => asset.AssetPath).ToList())
                .ToList();
        }

        internal List<FindAsset> ScanUnused()
        {
            if (assetMap == null) Check4Changes(false);

            // Get Addressable assets
            var addressable = FindAddressable.IsOk
                ? FindAddressable.GetAddresses().SelectMany(item => item.Value.assetGUIDs.Union(item.Value.childGUIDs)).ToHashSet()
                : new HashSet<string>();

            var result = new List<FindAsset>();
            foreach (var item in assetMap)
            {
                var v = item.Value;
                if (v.IsMissing || v.InEditor || v.IsScript || v.InResources || v.InPlugins || v.InStreamingAsset || v.IsFolder)
                    continue;

                if (!v.AssetPath.StartsWith("Assets/")) continue; // ignore built-in / packages assets
                if (v.ForcedIncludedInBuild) continue; // ignore assets that are forced to be included in build
                if (v.AssetName == "LICENSE") continue; // ignore license files

                if (SpecialUseAssets.Contains(v.AssetPath)) continue; // ignore assets with special use (can not remove)
                if (SpecialExtensions.Contains(v.Extension)) continue;

                if (v.type == EFinderAssetType.DLL) continue;
                if (v.type == EFinderAssetType.Script) continue;
                if (v.type == EFinderAssetType.Unknown) continue;
                if (addressable.Contains(v.guid)) continue;

                // special handler for .spriteatlas
                if (v.Extension == ".spriteatlas")
                {
                    var isInUsed = false;
                    List<string> allSprites = v.UseGUIDs.Keys.ToList();
                    foreach (string spriteGuid in allSprites)
                    {
                        var asset = FinderWindowBase.CacheSetting.Get(spriteGuid);
                        if (asset.usedByMap.Count <= 1) continue; // only use by this atlas

                        isInUsed = true;
                        break; // this one is used by other assets
                    }

                    if (isInUsed) continue;
                }

                if (v.IsExcluded) continue;

                if (!string.IsNullOrEmpty(v.AtlasName)) continue;
                if (!string.IsNullOrEmpty(v.AssetBundleName)) continue;
                if (!string.IsNullOrEmpty(v.AddressableName)) continue;

                if (v.usedByMap.Count == 0) result.Add(v);
            }

            result.Sort((item1, item2) => item1.Extension == item2.Extension
                ? string.Compare(item1.AssetPath, item2.AssetPath, StringComparison.Ordinal)
                : string.Compare(item1.Extension, item2.Extension, StringComparison.Ordinal));

            return result;
        }
    }
}