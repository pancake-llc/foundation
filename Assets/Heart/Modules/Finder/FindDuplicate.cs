using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    internal class AssetDuplicateTree2 : IRefDraw
    {
        private const float TIME_DELAY_DELETE = .5f;

        private static readonly FindFileCompare FileCompare = new();
        private readonly FindTreeUI2.GroupDrawer _groupDrawer;
        private List<List<string>> _cacheAssetList;
        public bool caseSensitive = false;
        private Dictionary<string, List<FindRef>> _dicIndex; //index, list

        private bool _dirty;
        private int _excludeCount;
        private string _guidPressDelete;

        private readonly Func<FindRefDrawer.Sort> _getSortMode;
        private readonly Func<FindRefDrawer.Mode> _getGroupMode;

        internal List<FindRef> list;
        internal Dictionary<string, FindRef> refs;
        public int scanExcludeByIgnoreCount;
        public int scanExcludeByTypeCount;
        private readonly string _searchTerm = "";
        private float _timePressDelete;

        public AssetDuplicateTree2(IWindow window, Func<FindRefDrawer.Sort> getSortMode, Func<FindRefDrawer.Mode> getGroupMode)
        {
            Window = window;
            _getSortMode = getSortMode;
            _getGroupMode = getGroupMode;
            _groupDrawer = new FindTreeUI2.GroupDrawer(DrawGroup, DrawAsset);
        }

        public IWindow Window { get; set; }

        public bool Draw(Rect rect) { return false; }

        public bool DrawLayout()
        {
            if (_dirty) RefreshView(_cacheAssetList);

            if (FileCompare.nChunks2 > 0 && FileCompare.nScaned < FileCompare.nChunks2)
            {
                var rect = GUILayoutUtility.GetRect(1, Screen.width, 18f, 18f);
                float p = FileCompare.nScaned / (float) FileCompare.nChunks2;

                EditorGUI.ProgressBar(rect, p, $"Scanning {FileCompare.nScaned} / {FileCompare.nChunks2}");
                GUILayout.FlexibleSpace();
            }
            else
            {
                if (_groupDrawer.HasValidTree) _groupDrawer.tree.itemPaddingRight = 60f;
                _groupDrawer.DrawLayout();
            }

            DrawHeader();
            return false;
        }

        public int ElementCount() { return list?.Count ?? 0; }

        private void DrawAsset(Rect r, string guid)
        {
            if (!refs.TryGetValue(guid, out var rf)) return;

            rf.asset.Draw(r,
                false,
                _getGroupMode() != FindRefDrawer.Mode.Folder,
                FinderWindowBase.DisplayFileSize,
                FinderWindowBase.DisplayAssetBundleName,
                FinderWindowBase.DisplayAtlasName,
                FinderWindowBase.ShowUsedByClassed,
                Window);

            var tex = AssetDatabase.GetCachedIcon(rf.asset.AssetPath);
            if (tex == null) return;

            var drawR = r;
            drawR.x = drawR.x + drawR.width; // (groupDrawer.TreeNoScroll() ? 60f : 70f) ;
            drawR.width = 40f;
            drawR.y += 1;
            drawR.height -= 2;

            if (GUI.Button(drawR, "Use", EditorStyles.miniButton))
            {
                if (FinderExport.IsMergeProcessing)
                    Debug.LogWarning("Previous merge is processing");
                else
                {
                    int index = rf.index;
                    Selection.objects = list.Where(x => x.index == index).Select(x => FinderUtility.LoadAssetAtPath<UnityEngine.Object>(x.asset.AssetPath)).ToArray();
                    FinderExport.MergeDuplicate(rf.asset.guid);
                }
            }

            if (rf.asset.UsageCount() > 0) return;

            drawR.x -= 25;
            drawR.width = 20;
            if (WasPreDelete(guid))
            {
                var col = GUI.color;
                GUI.color = Color.red;
                if (GUI.Button(drawR, "X", EditorStyles.miniButton))
                {
                    _guidPressDelete = null;
                    AssetDatabase.DeleteAsset(rf.asset.AssetPath);
                }

                GUI.color = col;
                Window.WillRepaint = true;
            }
            else
            {
                if (GUI.Button(drawR, "X", EditorStyles.miniButton))
                {
                    _guidPressDelete = guid;
                    _timePressDelete = Time.realtimeSinceStartup;
                    Window.WillRepaint = true;
                }
            }
        }

        private bool WasPreDelete(string guid)
        {
            if (_guidPressDelete == null || guid != _guidPressDelete) return false;

            if (Time.realtimeSinceStartup - _timePressDelete < TIME_DELAY_DELETE) return true;

            _guidPressDelete = null;
            return false;
        }

        private void DrawGroup(Rect r, string label, int childCount)
        {
            var asset = _dicIndex[label][0].asset;

            var tex = AssetDatabase.GetCachedIcon(asset.AssetPath);
            var rect = r;

            if (tex != null)
            {
                rect.width = 16f;
                GUI.DrawTexture(rect, tex);
            }

            rect = r;
            rect.xMin += 16f;
            GUI.Label(rect, asset.AssetName, EditorStyles.boldLabel);

            rect = r;
            rect.xMin += rect.width - 50f;
            GUI.Label(rect, asset.FileSize.GetSizeInMemory(), EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
            GUI.Label(rect, childCount.ToString(), EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
        }


        public void Reset(List<List<string>> assetList) { FileCompare.Reset(assetList, OnUpdateView, RefreshView); }

        private void OnUpdateView(List<List<string>> assetList) { }

        public bool IsExclueAnyItem() { return _excludeCount > 0 || scanExcludeByTypeCount > 0; }

        public bool IsExclueAnyItemByIgnoreFolder() { return scanExcludeByIgnoreCount > 0; }

        // void OnActive
        private void RefreshView(List<List<string>> assetList)
        {
            _cacheAssetList = assetList;
            _dirty = false;
            list = new List<FindRef>();
            refs = new Dictionary<string, FindRef>();
            _dicIndex = new Dictionary<string, List<FindRef>>();
            if (assetList == null) return;

            int minScore = _searchTerm.Length;
            string term1 = _searchTerm;
            if (!caseSensitive) term1 = term1.ToLower();

            string term2 = term1.Replace(" ", string.Empty);
            _excludeCount = 0;

            for (var i = 0; i < assetList.Count; i++)
            {
                var lst = new List<FindRef>();
                for (var j = 0; j < assetList[i].Count; j++)
                {
                    string path = assetList[i][j];
                    if (!path.StartsWith("Assets/"))
                    {
                        Debug.LogWarning("Ignore asset: " + path);
                        continue;
                    }

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid)) continue;

                    if (refs.ContainsKey(guid)) continue;

                    var asset = FinderWindowBase.CacheSetting.Get(guid);
                    if (asset == null) continue;
                    if (!asset.AssetPath.StartsWith("Assets/")) continue; // ignore builtin, packages, ...

                    var r = new FindRef(i, 0, asset, null);

                    if (FinderWindowBase.IsTypeExcluded(r.type))
                    {
                        _excludeCount++;
                        continue; //skip this one
                    }

                    if (string.IsNullOrEmpty(_searchTerm))
                    {
                        r.matchingScore = 0;
                        list.Add(r);
                        lst.Add(r);
                        refs.Add(guid, r);
                        continue;
                    }

                    //calculate matching score
                    string name1 = r.asset.AssetName;
                    if (!caseSensitive) name1 = name1.ToLower();

                    string name2 = name1.Replace(" ", string.Empty);

                    int score1 = FinderUtility.StringMatch(term1, name1);
                    int score2 = FinderUtility.StringMatch(term2, name2);

                    r.matchingScore = Mathf.Max(score1, score2);
                    if (r.matchingScore > minScore)
                    {
                        list.Add(r);
                        lst.Add(r);
                        refs.Add(guid, r);
                    }
                }

                _dicIndex.Add(i.ToString(), lst);
            }

            ResetGroup();
        }

        private void ResetGroup()
        {
            _groupDrawer.Reset(list, rf => rf.asset.guid, GetGroup);
            Window?.Repaint();
        }

        private string GetGroup(FindRef rf) { return rf.index.ToString(); }

        public void SetDirty() { _dirty = true; }

        public void RefreshSort() { }

        private void DrawHeader()
        {
            string text = _groupDrawer.HasValidTree ? "Rescan" : "Scan";

            if (GUILayout.Button(text)) OnCacheReady();
        }

        private void OnCacheReady()
        {
            scanExcludeByTypeCount = 0;
            Reset(FinderWindowBase.CacheSetting.ScanSimilar(IgnoreTypeWhenScan, IgnoreFolderWhenScan));
            AssetCache.onCacheReady -= OnCacheReady;
        }

        private void IgnoreTypeWhenScan() { scanExcludeByTypeCount++; }

        private void IgnoreFolderWhenScan() { scanExcludeByIgnoreCount++; }
    }

    internal class FindFileCompare
    {
        public static HashSet<FindChunk> hashChunksNotComplete;
        internal static int streamClosedCount;
        private List<List<string>> _cacheList;
        public readonly List<FindHead> deads = new();
        public readonly List<FindHead> heads = new();

        public int nChunks;
        public int nChunks2;
        public int nScaned;
        public Action<List<List<string>>> onCompareComplete;
        public Action<List<List<string>>> onCompareUpdate;

        public void Reset(List<List<string>> list, Action<List<List<string>>> onUpdate, Action<List<List<string>>> onComplete)
        {
            nChunks = 0;
            nScaned = 0;
            nChunks2 = 0;

            hashChunksNotComplete = new HashSet<FindChunk>();

            if (heads.Count > 0)
            {
                for (var i = 0; i < heads.Count; i++)
                {
                    heads[i].CloseChunk();
                }
            }

            deads.Clear();
            heads.Clear();

            onCompareUpdate = onUpdate;
            onCompareComplete = onComplete;
            if (list.Count <= 0)
            {
                onCompareComplete(new List<List<string>>());
                return;
            }

            _cacheList = list;

            for (var i = 0; i < list.Count; i++)
            {
                var file = new FileInfo(list[i][0]);
                int nChunk = Mathf.CeilToInt(file.Length / (float) FindHead.CHUNK_SIZE);
                nChunks2 += nChunk;
            }


            AddHead(_cacheList[_cacheList.Count - 1]);
            _cacheList.RemoveAt(_cacheList.Count - 1);

            EditorApplication.update -= ReadChunkAsync;
            EditorApplication.update += ReadChunkAsync;
        }

        public FindFileCompare AddHead(List<string> files)
        {
            if (files.Count < 2) Debug.LogWarning("Something wrong ! head should not contains < 2 elements");

            var chunkList = new List<FindChunk>();
            for (var i = 0; i < files.Count; i++)
            {
                chunkList.Add(new FindChunk {file = files[i], buffer = new byte[FindHead.CHUNK_SIZE]});
            }

            var file = new FileInfo(files[0]);
            int nChunk = Mathf.CeilToInt(file.Length / (float) FindHead.CHUNK_SIZE);

            heads.Add(new FindHead {fileSize = file.Length, currentChunk = 0, nChunk = nChunk, chunkList = chunkList});

            nChunks += nChunk;

            return this;
        }


        private void ReadChunkAsync()
        {
            bool alive = ReadChunk();
            if (alive) return;

            var update = false;
            for (int i = heads.Count - 1; i >= 0; i--)
            {
                var h = heads[i];
                if (!h.IsDead) continue;

                h.CloseChunk();
                heads.RemoveAt(i);
                if (h.chunkList.Count > 1)
                {
                    update = true;
                    deads.Add(h);
                }
            }

            if (update) Trigger(onCompareUpdate);

            if (_cacheList.Count == 0)
            {
                foreach (var item in hashChunksNotComplete)
                {
                    if (item.stream == null || !item.stream.CanRead) continue;
                    item.stream.Close();
                    item.stream = null;
                }

                hashChunksNotComplete.Clear();
                nScaned = nChunks;
                EditorApplication.update -= ReadChunkAsync;
                Trigger(onCompareComplete);
            }
            else
            {
                AddHead(_cacheList[_cacheList.Count - 1]);
                _cacheList.RemoveAt(_cacheList.Count - 1);
            }
        }

        private void Trigger(Action<List<List<string>>> cb)
        {
            if (cb == null) return;

            var list = deads.Select(item => item.GetFiles()).ToList();

            cb(list);
        }

        private bool ReadChunk()
        {
            var alive = false;

            for (var i = 0; i < heads.Count; i++)
            {
                var h = heads[i];
                if (h.IsDead) continue;

                nScaned++;
                alive = true;
                h.ReadChunk();
                h.CompareChunk(heads);
                break;
            }

            return alive;
        }
    }

    internal class FindHead
    {
        public const int CHUNK_SIZE = 10240;

        public List<FindChunk> chunkList;
        public int currentChunk;

        public long fileSize;

        public int nChunk;
        public int size; //last stream read size

        public bool IsDead => currentChunk == nChunk || chunkList.Count == 1;

        public List<string> GetFiles() { return chunkList.Select(item => item.file).ToList(); }

        public void AddToDict(byte b, FindChunk chunk, Dictionary<byte, List<FindChunk>> dict)
        {
            if (!dict.TryGetValue(b, out var list))
            {
                list = new List<FindChunk>();
                dict.Add(b, list);
            }

            list.Add(chunk);
        }

        public void CloseChunk()
        {
            for (var i = 0; i < chunkList.Count; i++)
            {
                FindFileCompare.streamClosedCount++;

                if (chunkList[i].stream != null)
                {
                    chunkList[i].stream.Close();
                    chunkList[i].stream = null;
                }
            }
        }

        public void ReadChunk()
        {
            if (currentChunk == nChunk)
            {
                Debug.LogWarning("Something wrong, should dead <" + IsDead + ">");
                return;
            }

            int from = currentChunk * CHUNK_SIZE;
            size = (int) Mathf.Min(fileSize - from, CHUNK_SIZE);

            for (var i = 0; i < chunkList.Count; i++)
            {
                var chunk = chunkList[i];
                if (chunk.streamError) continue;
                chunk.size = size;

                if (chunk.streamInited == false)
                {
                    chunk.streamInited = true;

                    try
                    {
                        chunk.stream = new FileStream(chunk.file, FileMode.Open, FileAccess.Read);
                    }
                    catch
                    {
                        chunk.streamError = true;
                        if (chunk.stream != null) // just to make sure we close the stream
                        {
                            chunk.stream.Close();
                            chunk.stream = null;
                        }
                    }

                    if (chunk.stream == null)
                    {
                        chunk.streamError = true;
                        continue;
                    }
                }

                try
                {
                    chunk.stream.Read(chunk.buffer, 0, size);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e + "\n" + chunk.file);

                    chunk.streamError = true;
                    chunk.stream.Close();
                }
            }

            // clean up dead chunks
            for (int i = chunkList.Count - 1; i >= 0; i--)
            {
                if (chunkList[i].streamError) chunkList.RemoveAt(i);
            }

            if (chunkList.Count == 1) Debug.LogWarning("No more chunk in list");

            currentChunk++;
        }

        public void CompareChunk(List<FindHead> heads)
        {
            int idx = chunkList.Count;
            byte[] buffer = chunkList[idx - 1].buffer;

            while (--idx >= 0)
            {
                var chunk = chunkList[idx];
                int diff = FirstDifferentIndex(buffer, chunk.buffer, size);
                if (diff == -1) continue;

                byte v = buffer[diff];

                var d = new Dictionary<byte, List<FindChunk>>(); //new heads
                chunkList.RemoveAt(idx);
                FindFileCompare.hashChunksNotComplete.Add(chunk);

                AddToDict(chunk.buffer[diff], chunk, d);

                for (int j = idx - 1; j >= 0; j--)
                {
                    var tChunk = chunkList[j];
                    byte tValue = tChunk.buffer[diff];
                    if (tValue == v) continue;

                    idx--;
                    FindFileCompare.hashChunksNotComplete.Add(tChunk);
                    chunkList.RemoveAt(j);
                    AddToDict(tChunk.buffer[diff], tChunk, d);
                }

                foreach (var item in d)
                {
                    var list = item.Value;
                    if (list.Count == 1)
                    {
                        list[0].stream?.Close();
                    }
                    else if (list.Count > 1) // 1 : dead head
                    {
                        heads.Add(new FindHead {nChunk = nChunk, fileSize = fileSize, currentChunk = currentChunk - 1, chunkList = list});
                    }
                }
            }
        }

        internal static int FirstDifferentIndex(byte[] arr1, byte[] arr2, int maxIndex)
        {
            for (var i = 0; i < maxIndex; i++)
            {
                if (arr1[i] != arr2[i]) return i;
            }

            return -1;
        }
    }

    internal class FindChunk
    {
        public byte[] buffer;
        public string file;
        public long size;
        public FileStream stream;
        public bool streamError;

        public bool streamInited;
    }
}