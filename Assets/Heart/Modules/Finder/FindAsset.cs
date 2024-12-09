using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityObject = UnityEngine.Object;

namespace PancakeEditor.Finder
{
    public enum EFinderAssetType
    {
        Unknown,
        Folder,
        Script,
        Scene,
        DLL,
        Referencable,
        BinaryAsset,
        Model,
        Terrain,
        LightingData,
        NonReadable,
        BuiltIn
    }

    public enum EFinderAssetState
    {
        New,
        Cache,
        Missing
    }

    [Serializable]
    public class FindAsset
    {
        // ------------------------------ CONSTANTS ---------------------------

        private static readonly HashSet<string> ScriptExtensions = new()
        {
            ".cs",
            ".js",
            ".boo",
            ".h",
            ".java",
            ".cpp",
            ".m",
            ".mm",
            ".shader",
            ".hlsl",
            ".cginclude",
            ".shadersubgraph"
        };

        private static readonly HashSet<string> ReferencableExtensions = new()
        {
            ".anim",
            ".controller",
            ".mat",
            ".unity",
            ".guiskin",
            ".prefab",
            ".overridecontroller",
            ".mask",
            ".rendertexture",
            ".cubemap",
            ".flare",
            ".playable",
            ".mat",
            ".physicsmaterial",
            ".fontsettings",
            ".asset",
            ".prefs",
            ".spriteatlas",
            ".terrainlayer",
            ".asmdef",
            ".preset",
            ".spriteLib"
        };

        private static readonly HashSet<string> ReferencableJson = new() {".shadergraph", ".shadersubgraph"};

        private static readonly HashSet<string> UiToolkit = new() {".uss", ".uxml", ".tss"};

        private static readonly HashSet<string> ReferencableMeta = new() {".texture2darray"};

        internal static readonly HashSet<string> BuiltInAssets = new()
        {
            "0000000000000000f000000000000000", "0000000000000000e000000000000000", "0000000000000000d000000000000000"
        };

        private static readonly Dictionary<long, Type> HashClasses = new();
        internal static Dictionary<string, GUIContent> cacheImage = new();

        public static float ignoreTs;

        private static int binaryLoaded;

        // ----------------------------- DRAW  ---------------------------------------

        [SerializeField] public string guid;

        // Need to read FileInfo: soft-cache (always re-read when needed)
        [SerializeField] public EFinderAssetType type;
        [SerializeField] private string mFileInfoHash;
        [SerializeField] private string mAssetbundle;
        [SerializeField] private string mAddressable;

        [SerializeField] private string mAtlas;
        [SerializeField] private long mFileSize;

        [SerializeField] private int mAssetChangeTs; // Realtime when asset changed (trigger by import asset operation)
        [SerializeField] private int mFileInfoReadTs; // Realtime when asset being read
        [SerializeField] private bool mForceIncludeInBuild;

        [SerializeField] private int mFileWriteTs; // file's lastModification (file content + meta)
        [SerializeField] private int mCachefileWriteTs; // file's lastModification at the time the content being read

        [SerializeField] internal int refreshStamp; // use to check if asset has been deleted (refreshStamp not updated)
        [SerializeField] internal List<Classes> useGUIDsList = new();

        public string DebugUseGUID() => $"{guid} : {AssetPath}\n{string.Join("\n", useGUIDsList.Select(item => item.guid).ToArray())}";


        private bool _isExcluded;
        private Dictionary<string, HashSet<long>> _useGUIDs;
        private float _excludeTs;


        // ----------------------------- DRAW  ---------------------------------------
        [NonSerialized] private GUIContent _fileSizeText;
        internal HashSet<long> hashUsedByClassesIds = new();
        [NonSerialized] private string _mAssetFolder;
        [NonSerialized] private string _mAssetName;
        [NonSerialized] private string _mAssetPath;
        [NonSerialized] private string _mExtension;
        [NonSerialized] private bool _inEditor;
        [NonSerialized] private bool _inPlugins;
        [NonSerialized] private bool _inPackage;
        [NonSerialized] private bool _inResources;
        [NonSerialized] private bool _inStreamingAsset;
        [NonSerialized] private bool _isAssetFile;

        // easy to recalculate: will not cache
        [NonSerialized] private bool _mPathLoaded;


        // Do not cache
        [NonSerialized] internal EFinderAssetState state;
        internal Dictionary<string, FindAsset> usedByMap = new();

        public FindAsset(string guid)
        {
            this.guid = guid;
            type = BuiltInAssets.Contains(guid) ? EFinderAssetType.BuiltIn : EFinderAssetType.Unknown;
        }

        public bool ForcedIncludedInBuild => mForceIncludeInBuild;

        public string AssetName
        {
            get
            {
                LoadPathInfo();
                return _mAssetName;
            }
        }

        public string AssetPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_mAssetPath)) return _mAssetPath;
                _mAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(_mAssetPath)) state = EFinderAssetState.Missing;
                return _mAssetPath;
            }
        }

        public string ParentFolderPath
        {
            get
            {
                LoadPathInfo();
                return _mAssetFolder;
            }
        }

        public string AssetFolder
        {
            get
            {
                LoadPathInfo();
                return _mAssetFolder;
            }
        }

        public string Extension
        {
            get
            {
                LoadPathInfo();
                return _mExtension;
            }
        }

        public bool InEditor
        {
            get
            {
                LoadPathInfo();
                return _inEditor;
            }
        }

        public bool InPlugins
        {
            get
            {
                LoadPathInfo();
                return _inPlugins;
            }
        }

        public bool InPackages
        {
            get
            {
                LoadPathInfo();
                return _inPackage;
            }
        }

        public bool InResources
        {
            get
            {
                LoadPathInfo();
                return _inResources;
            }
        }

        public bool InStreamingAsset
        {
            get
            {
                LoadPathInfo();
                return _inStreamingAsset;
            }
        }

        // ----------------------- TYPE INFO ------------------------

        internal bool IsFolder => type == EFinderAssetType.Folder;
        internal bool IsScript => type == EFinderAssetType.Script;
        internal bool IsMissing => state == EFinderAssetState.Missing && !IsBuiltIn;

        internal bool IsReferencable => type == EFinderAssetType.Referencable || type == EFinderAssetType.Scene;

        internal bool IsBinaryAsset =>
            type == EFinderAssetType.BinaryAsset || type == EFinderAssetType.Model || type == EFinderAssetType.Terrain || type == EFinderAssetType.LightingData;

        // ----------------------- PATH INFO ------------------------
        public bool FileInfoDirty => type == EFinderAssetType.Unknown || mFileInfoReadTs <= mAssetChangeTs;
        public bool FileContentDirty => mFileWriteTs != mCachefileWriteTs && !IsBuiltIn;
        public bool IsDirty => (FileInfoDirty || FileContentDirty) && !IsBuiltIn;
        public bool IsBuiltIn => type == EFinderAssetType.BuiltIn;

        internal string FileInfoHash
        {
            get
            {
                LoadFileInfo();
                return mFileInfoHash;
            }
        }

        internal long FileSize
        {
            get
            {
                LoadFileInfo();
                return mFileSize;
            }
        }

        public string AtlasName
        {
            get
            {
                LoadFileInfo();
                return mAtlas;
            }
        }

        public string AssetBundleName
        {
            get
            {
                LoadFileInfo();
                return mAssetbundle;
            }
        }

        public string AddressableName
        {
            get
            {
                LoadFileInfo();
                return mAddressable;
            }
        }

        public Dictionary<string, HashSet<long>> UseGUIDs
        {
            get
            {
                if (_useGUIDs != null) return _useGUIDs;

                _useGUIDs = new Dictionary<string, HashSet<long>>(useGUIDsList.Count);
                for (var i = 0; i < useGUIDsList.Count; i++)
                {
                    string id = useGUIDsList[i].guid;
                    if (_useGUIDs.ContainsKey(id))
                    {
                        for (var j = 0; j < useGUIDsList[i].ids.Count; j++)
                        {
                            long val = useGUIDsList[i].ids[j];
                            if (_useGUIDs[id].Contains(val)) continue;

                            _useGUIDs[id].Add(useGUIDsList[i].ids[j]);
                        }
                    }
                    else _useGUIDs.Add(id, new HashSet<long>(useGUIDsList[i].ids));
                }

                return _useGUIDs;
            }
        }

        // ------------------------------- GETTERS -----------------------------
        internal bool IsExcluded
        {
            get
            {
                if (_excludeTs >= ignoreTs) return _isExcluded;

                _excludeTs = ignoreTs;
                _isExcluded = false;

                var h = FinderWindowBase.IgnoreAsset;
                foreach (string item in h)
                {
                    if (!_mAssetPath.StartsWith(item, false, CultureInfo.InvariantCulture)) continue;
                    _isExcluded = true;
                    return true;
                }

                return false;
            }
        }

        // ----------------------- PATH INFO ------------------------
        public void LoadPathInfo()
        {
            if (_mPathLoaded) return;
            _mPathLoaded = true;

            _mAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(AssetPath))
            {
                state = EFinderAssetState.Missing;
                return;
            }

            FinderUtility.SplitPath(_mAssetPath, out _mAssetName, out _mExtension, out _mAssetFolder);

            if (_mAssetFolder.StartsWith("Assets/")) _mAssetFolder = _mAssetFolder.Substring(7);
            else if (!FinderUtility.StringStartsWith(_mAssetPath, "Packages/", "Project Settings/", "Library/")) _mAssetFolder = "built-in/";

            _inEditor = _mAssetPath.Contains("/Editor/") || _mAssetPath.Contains("/Editor Default Resources/");
            _inResources = _mAssetPath.Contains("/Resources/");
            _inStreamingAsset = _mAssetPath.Contains("/StreamingAssets/");
            _inPlugins = _mAssetPath.Contains("/Plugins/");
            _inPackage = _mAssetPath.StartsWith("Packages/");
            _isAssetFile = _mAssetPath.EndsWith(".asset", StringComparison.Ordinal);
        }

        private bool ExistOnDisk()
        {
            if (IsBuiltIn) return true;
            if (IsMissing) return false; // asset not exist - no need to check FileSystem!
            if (type == EFinderAssetType.Folder || type == EFinderAssetType.Unknown)
            {
                if (Directory.Exists(_mAssetPath))
                {
                    if (type == EFinderAssetType.Unknown) type = EFinderAssetType.Folder;
                    return true;
                }

                if (type == EFinderAssetType.Folder) return false;
            }

            // must be file here
            if (!File.Exists(_mAssetPath)) return false;

            if (type == EFinderAssetType.Unknown) GuessAssetType();
            return true;
        }

        internal void LoadFileInfo()
        {
            if (!FileInfoDirty) return;
            if (string.IsNullOrEmpty(_mAssetPath)) LoadPathInfo(); // always reload Path Info

            mFileInfoReadTs = FinderUtility.Epoch(DateTime.Now);
            if (IsBuiltIn) return;
            if (IsMissing) return;

            if (!ExistOnDisk())
            {
                state = EFinderAssetState.Missing;
                return;
            }

            if (type == EFinderAssetType.Folder) return; // nothing to read

            var assetType = AssetDatabase.GetMainAssetTypeAtPath(_mAssetPath);

            var info = new FileInfo(_mAssetPath);
            mFileSize = info.Length;
            mFileInfoHash = info.Length + info.Extension;
            mAddressable = FinderUtility.GetAddressable(guid);

            mAssetbundle = AssetDatabase.GetImplicitAssetBundleName(_mAssetPath);

            if (assetType == typeof(Texture2D))
            {
                var importer = AssetImporter.GetAtPath(_mAssetPath);
                if (importer is TextureImporter)
                {
                    var tImporter = importer as TextureImporter;
#pragma warning disable CS0618
                    if (tImporter.qualifiesForSpritePacking) mAtlas = tImporter.spritePackingTag;
#pragma warning restore CS0618
                }
            }

            // check if file content changed
            var metaInfo = new FileInfo(_mAssetPath + ".meta");
            int assetTime = FinderUtility.Epoch(info.LastWriteTime);
            int metaTime = FinderUtility.Epoch(metaInfo.LastWriteTime);

            // update fileChangeTimeStamp
            mFileWriteTs = Mathf.Max(metaTime, assetTime);
        }

        public void AddUsedBy(string guid, FindAsset asset)
        {
            if (usedByMap.ContainsKey(guid)) return;

            if (guid == this.guid) return;

            usedByMap.Add(guid, asset);
            if (hashUsedByClassesIds == null) hashUsedByClassesIds = new HashSet<long>();

            if (asset.UseGUIDs.TryGetValue(this.guid, out var output))
            {
                foreach (int item in output)
                {
                    hashUsedByClassesIds.Add(item);
                }
            }
        }

        public int UsageCount() { return usedByMap.Count; }

        public override string ToString() { return $"FindAsset[{_mAssetName}]"; }

        //--------------------------------- STATIC ----------------------------

        internal void MarkAsDirty(bool isMoved = true, bool force = false)
        {
            if (isMoved)
            {
                string newPath = AssetDatabase.GUIDToAssetPath(guid);
                if (newPath != _mAssetPath)
                {
                    _mPathLoaded = false;
                    _mAssetPath = newPath;
                }
            }

            state = EFinderAssetState.Cache;
            mAssetChangeTs = FinderUtility.Epoch(DateTime.Now); // re-read FileInfo
            if (force) mCachefileWriteTs = 0;
        }

        // --------------------------------- APIs ------------------------------

        internal void GuessAssetType()
        {
            if (ScriptExtensions.Contains(_mExtension)) type = EFinderAssetType.Script;
            else if (ReferencableExtensions.Contains(_mExtension))
            {
                bool isUnity = _mExtension == ".unity";
                type = isUnity ? EFinderAssetType.Scene : EFinderAssetType.Referencable;

                if (_mExtension == ".asset" || isUnity || _mExtension == ".spriteatlas")
                {
                    var buffer = new byte[5];
                    FileStream stream = null;

                    try
                    {
                        stream = File.OpenRead(_mAssetPath);
                        stream.Read(buffer, 0, 5);
                        stream.Close();
                    }
                    catch
                    {
                        stream?.Close();
                        state = EFinderAssetState.Missing;
                        return;
                    }
                    finally
                    {
                        stream?.Close();
                    }

                    var str = string.Empty;
                    foreach (byte t in buffer)
                    {
                        str += (char) t;
                    }

                    if (str != "%YAML") type = EFinderAssetType.BinaryAsset;
                }
            }
            else if (ReferencableJson.Contains(_mExtension) || UiToolkit.Contains(_mExtension))
            {
                type = EFinderAssetType.Referencable;
            }
            else if (ReferencableMeta.Contains(_mExtension))
            {
                type = EFinderAssetType.Referencable;
            }
            else if (_mExtension == ".fbx") type = EFinderAssetType.Model;
            else if (_mExtension == ".dll") type = EFinderAssetType.DLL;
            else type = EFinderAssetType.NonReadable;
        }


        internal void LoadContent()
        {
            if (!FileContentDirty) return;
            mCachefileWriteTs = mFileWriteTs;
            mForceIncludeInBuild = false;

            if (IsMissing || type == EFinderAssetType.NonReadable) return;

            if (type == EFinderAssetType.DLL) return;

            if (!ExistOnDisk())
            {
                state = EFinderAssetState.Missing;
                return;
            }

            ClearUseGUIDs();

            if (IsFolder) LoadFolder();
            else if (IsReferencable) LoadYaml2();
            else if (IsBinaryAsset) LoadBinaryAsset();
        }

        internal void AddUseGuid(string fguid, long fFileId = -1) { AddUseGuid(fguid, fFileId, true); }

        internal void AddUseGuid(string fguid, long fFileId, bool checkExist)
        {
            if (!UseGUIDs.ContainsKey(fguid))
            {
                useGUIDsList.Add(new Classes {guid = fguid, ids = new List<long>()});
                UseGUIDs.Add(fguid, new HashSet<long>());
            }

            if (fFileId == -1) return;
            if (UseGUIDs[fguid].Contains(fFileId)) return;

            UseGUIDs[fguid].Add(fFileId);
            var i = useGUIDsList.FirstOrDefault(x => x.guid == fguid);
            if (i != null) i.ids.Add(fFileId);
        }

        // ----------------------------- STATIC  ---------------------------------------

        internal static int SortByExtension(FindAsset a1, FindAsset a2)
        {
            if (a1 == null) return -1;
            if (a2 == null) return 1;

            int result = string.Compare(a1._mExtension, a2._mExtension, StringComparison.Ordinal);
            return result == 0 ? string.Compare(a1._mAssetName, a2._mAssetName, StringComparison.Ordinal) : result;
        }

        internal static List<FindAsset> FindUsage(FindAsset asset)
        {
            if (asset == null) return null;

            var refs = FinderWindowBase.CacheSetting.FindAssets(asset.UseGUIDs.Keys.ToArray(), true);

            return refs;
        }

        internal static List<FindAsset> FindUsedBy(FindAsset asset) { return asset.usedByMap.Values.ToList(); }

        internal static List<string> FindUsageGUIDs(FindAsset asset, bool includeScriptSymbols)
        {
            var result = new HashSet<string>();
            if (asset == null)
            {
                Debug.LogWarning("Asset invalid : " + asset._mAssetName);
                return result.ToList();
            }

            foreach (var item in asset.UseGUIDs)
            {
                result.Add(item.Key);
            }

            return result.ToList();
        }

        internal static List<string> FindUsedByGUIDs(FindAsset asset) { return asset.usedByMap.Keys.ToList(); }

        internal float Draw(
            Rect r,
            bool highlight,
            bool drawPath = true,
            bool showFileSize = true,
            bool showAbName = false,
            bool showAtlasName = false,
            bool showUsageIcon = true,
            IWindow window = null,
            bool drawExtension = true)
        {
            bool singleLine = r.height <= 18f;
            float rw = r.width;
            bool selected = AssetBookmark.Contains(guid);

            r.height = 16f;
            bool hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                if (_mExtension == ".prefab") menu.AddItem(MyGUIContent.FromString("Edit in Scene"), false, EditPrefab);

                menu.AddItem(MyGUIContent.FromString("Open"), false, Open);
                menu.AddItem(MyGUIContent.FromString("Ping"), false, Ping);
                menu.AddItem(MyGUIContent.FromString(guid), false, CopyGuid);

                menu.AddItem(MyGUIContent.FromString("Select in Project Panel"), false, Select);

                menu.AddSeparator(string.Empty);
                menu.AddItem(MyGUIContent.FromString("Copy path"), false, CopyAssetPath);
                menu.AddItem(MyGUIContent.FromString("Copy full path"), false, CopyAssetPathFull);

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (IsMissing)
            {
                if (!singleLine) r.y += 16f;

                if (Event.current.type != EventType.Repaint) return 0;

                GUI.Label(r, MyGUIContent.FromString(guid), EditorStyles.whiteBoldLabel);
                return 0;
            }

            var iconRect = GUI2.LeftRect(16f, ref r);
            if (Event.current.type == EventType.Repaint)
            {
                var icon = AssetDatabase.GetCachedIcon(_mAssetPath);
                if (icon != null) GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                var pingRect = FinderWindowBase.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : iconRect;
                if (pingRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.control || Event.current.command)
                    {
                        if (selected) RemoveFromSelection();
                        else AddToSelection();

                        window?.Repaint();
                    }
                    else Ping();
                }
            }

            if (Event.current.type != EventType.Repaint) return 0;
            if (usedByMap != null && usedByMap.Count > 0)
            {
                var str = MyGUIContent.FromInt(usedByMap.Count);
                var countRect = iconRect;
                countRect.x -= 16f;
                countRect.xMin = -10f;
                GUI.Label(countRect, str, GUI2.MiniLabelAlignRight);
            }

            float pathW = drawPath && !string.IsNullOrEmpty(AssetFolder) ? EditorStyles.miniLabel.CalcSize(MyGUIContent.FromString(AssetFolder)).x : 8f;
            float nameW = drawPath
                ? EditorStyles.boldLabel.CalcSize(MyGUIContent.FromString(AssetName)).x
                : EditorStyles.label.CalcSize(MyGUIContent.FromString(AssetName)).x;
            float extW = string.IsNullOrEmpty(Extension) ? 0f : EditorStyles.miniLabel.CalcSize(MyGUIContent.FromString(Extension)).x;
            var cc = FinderWindowBase.SelectedColor;

            if (singleLine)
            {
                var lbRect = GUI2.LeftRect(pathW + nameW + extW, ref r);

                if (selected)
                {
                    var c1 = GUI.color;
                    GUI.color = cc;
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c1;
                }

                if (drawPath)
                {
                    if (!string.IsNullOrEmpty(AssetFolder))
                    {
                        var c2 = GUI.color;
                        GUI.color = new Color(c2.r, c2.g, c2.b, c2.a * 0.5f);
                        GUI.Label(GUI2.LeftRect(pathW, ref lbRect), MyGUIContent.FromString(AssetFolder), EditorStyles.miniLabel);
                        GUI.color = c2;
                    }

                    GUI.Label(lbRect, MyGUIContent.FromString(AssetName), EditorStyles.boldLabel);
                }
                else
                {
                    var c2 = GUI.color;
                    GUI.color = new Color(c2.r, c2.g, c2.b, c2.a * 0.9f);
                    GUI.Label(lbRect, MyGUIContent.FromString(AssetName));
                    GUI.color = c2;
                }

                lbRect.xMin += nameW - 2f;
                lbRect.y += 1f;

                if (!string.IsNullOrEmpty(Extension) && drawExtension)
                {
                    var c3 = GUI.color;
                    GUI.color = new Color(c3.r, c3.g, c3.b, c3.a * 0.5f);
                    GUI.Label(lbRect, MyGUIContent.FromString(Extension), EditorStyles.miniLabel);
                    GUI.color = c3;
                }
            }
            else
            {
                if (drawPath) GUI.Label(new Rect(r.x, r.y + 16f, r.width, r.height), MyGUIContent.FromString(_mAssetFolder), EditorStyles.miniLabel);
                var lbRect = GUI2.LeftRect(nameW, ref r);
                if (selected) GUI2.Rect(lbRect, cc);
                GUI.Label(lbRect, MyGUIContent.FromString(AssetName), EditorStyles.boldLabel);
            }

            var rr = GUI2.RightRect(10f, ref r); //margin
            if (highlight)
            {
                rr.xMin += 2f;
                rr.width = 1f;
                GUI2.Rect(rr, GUI2.darkGreen);
            }

            var c = GUI.color;
            GUI.color = new Color(c.r, c.g, c.b, c.a * 0.5f);

            if (showFileSize)
            {
                var fsRect = GUI2.RightRect(40f, ref r); // filesize label
                if (_fileSizeText == null) _fileSizeText = MyGUIContent.FromString(FileSize.GetSizeInMemory());
                GUI.Label(fsRect, _fileSizeText, GUI2.MiniLabelAlignRight);
            }

            if (!string.IsNullOrEmpty(mAddressable))
            {
                var adRect = GUI2.RightRect(100f, ref r);
                GUI.Label(adRect, MyGUIContent.FromString(mAddressable), GUI2.MiniLabelAlignRight);
            }

            if (showUsageIcon && hashUsedByClassesIds != null)
                foreach (int item in hashUsedByClassesIds)
                {
                    if (!FinderUtility.HashClassesNormal.ContainsKey(item)) continue;

                    string name = FinderUtility.HashClassesNormal[item];
                    if (!HashClasses.TryGetValue(item, out var t))
                    {
                        t = FinderUtility.GetType(name);
                        HashClasses.Add(item, t);
                    }

                    bool isExisted = cacheImage.TryGetValue(name, out var content);
                    if (content == null) content = t == null ? GUIContent.none : MyGUIContent.FromType(t, name);

                    if (!isExisted) cacheImage.Add(name, content);
                    else cacheImage[name] = content;

                    if (content != null)
                    {
                        try
                        {
                            GUI.Label(GUI2.RightRect(15f, ref r), content, GUI2.MiniLabelAlignRight);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

            if (showAtlasName)
            {
                GUI2.RightRect(10f, ref r); //margin
                var abRect = GUI2.RightRect(120f, ref r); // filesize label
                if (!string.IsNullOrEmpty(mAtlas)) GUI.Label(abRect, MyGUIContent.FromString(mAtlas), GUI2.MiniLabelAlignRight);
            }

            if (showAbName)
            {
                GUI2.RightRect(10f, ref r); //margin
                var abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(mAssetbundle)) GUI.Label(abRect, MyGUIContent.FromString(mAssetbundle), GUI2.MiniLabelAlignRight);
            }

            if (true)
            {
                GUI2.RightRect(10f, ref r); //margin
                var abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(mAddressable)) GUI.Label(abRect, MyGUIContent.FromString(mAddressable), GUI2.MiniLabelAlignRight);
            }

            GUI.color = c;

            if (Event.current.type == EventType.Repaint) return rw < pathW + nameW ? 32f : 18f;

            return r.height;
        }


        internal GenericMenu AddArray(GenericMenu menu, List<string> list, string prefix, string title, string emptyTitle, bool showAsset, int max = 10)
        {
            menu.AddItem(MyGUIContent.FromString(emptyTitle), true, null);

            return menu;
        }

        internal void CopyGuid()
        {
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log(guid);
        }

        internal void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = _mAssetName;
            Debug.Log(_mAssetName);
        }

        internal void CopyAssetPath()
        {
            EditorGUIUtility.systemCopyBuffer = _mAssetPath;
            Debug.Log(_mAssetPath);
        }

        internal void CopyAssetPathFull()
        {
            string fullName = new FileInfo(_mAssetPath).FullName;
            EditorGUIUtility.systemCopyBuffer = fullName;
            Debug.Log(fullName);
        }

        internal void Select()
        {
            var window = EditorWindow.focusedWindow;
            if (window != null && window is FinderWindow w && w.selection != null) w.selection.isLock = true;

            if (!AssetBookmark.Contains(guid)) Selection.objects = new[] {FinderUtility.LoadAssetAtPath<UnityEngine.Object>(AssetPath)};
            else AssetBookmark.Commit();
        }

        internal void RemoveFromSelection()
        {
            if (AssetBookmark.Contains(guid)) AssetBookmark.Remove(guid);
        }

        internal void AddToSelection()
        {
            if (!AssetBookmark.Contains(guid)) AssetBookmark.Add(guid);
        }

        internal void Ping()
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject)));
            Event.current.Use();
        }

        internal void Open() { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject))); }

        internal void EditPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject));
            UnityObject.Instantiate(prefab);
        }

        // ----------------------------- LOAD ASSETS ---------------------------------------

        internal void LoadGameObject(GameObject go)
        {
            var compList = go.GetComponentsInChildren<Component>();
            for (var i = 0; i < compList.Length; i++)
            {
                LoadSerialized(compList[i]);
            }
        }

        internal void LoadSerialized(UnityObject target)
        {
            var props = FinderUtility.XGetSerializedProperties(target, true);

            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].propertyType != SerializedPropertyType.ObjectReference) continue;

                var refObj = props[i].objectReferenceValue;
                if (refObj == null) continue;

                string refGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(refObj));

                AddUseGuid(refGuid);
            }
        }

        private void AddTextureGuid(SerializedProperty prop)
        {
            if (prop == null || prop.objectReferenceValue == null) return;
            string path = AssetDatabase.GetAssetPath(prop.objectReferenceValue);
            if (string.IsNullOrEmpty(path)) return;
            AddUseGuid(AssetDatabase.AssetPathToGUID(path));
        }

        internal void LoadLightingData(LightingDataAsset asset)
        {
            foreach (var texture in FindLightmap.Read(asset))
            {
                if (texture == null) continue;
                string path = AssetDatabase.GetAssetPath(texture);
                string assetGuid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(assetGuid)) AddUseGuid(assetGuid);
            }
        }

        internal void LoadTerrainData(TerrainData terrain)
        {
#if UNITY_2018_3_OR_NEWER
            var arr0 = terrain.terrainLayers;
            for (var i = 0; i < arr0.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr0[i]);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }
#endif


            var arr = terrain.detailPrototypes;

            for (var i = 0; i < arr.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr[i].prototypeTexture);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }

            var arr2 = terrain.treePrototypes;
            for (var i = 0; i < arr2.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr2[i].prefab);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }

            var arr3 = FinderUtility.GetTerrainTextureDatas(terrain);
            for (var i = 0; i < arr3.Length; i++)
            {
                var texs = arr3[i];
                for (var k = 0; k < texs.textures.Length; k++)
                {
                    var tex = texs.textures[k];
                    if (tex == null) continue;

                    string aPath = AssetDatabase.GetAssetPath(tex);
                    if (string.IsNullOrEmpty(aPath)) continue;

                    string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                    if (string.IsNullOrEmpty(refGuid)) continue;

                    AddUseGuid(refGuid);
                }
            }
        }

        private void ClearUseGUIDs()
        {
            UseGUIDs.Clear();
            useGUIDsList.Clear();
        }

        internal void LoadBinaryAsset()
        {
            ClearUseGUIDs();

            var assetData = AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject));
            if (assetData is GameObject data)
            {
                type = EFinderAssetType.Model;
                LoadGameObject(data);
            }
            else if (assetData is TerrainData terrainData)
            {
                type = EFinderAssetType.Terrain;
                LoadTerrainData(terrainData);
            }
            else if (assetData is LightingDataAsset lightingData)
            {
                type = EFinderAssetType.LightingData;
                LoadLightingData(lightingData);
            }
            else LoadSerialized(assetData);


            assetData = null;

            if (binaryLoaded++ <= 30) return;
            binaryLoaded = 0;
            FinderUtility.UnloadUnusedAssets();
        }

        internal void LoadYaml2()
        {
            if (!_mPathLoaded) LoadPathInfo();

            if (!File.Exists(_mAssetPath))
            {
                state = EFinderAssetState.Missing;
                return;
            }

            if (_mAssetPath == "ProjectSettings/EditorBuildSettings.asset")
            {
                var listScenes = EditorBuildSettings.scenes;
                foreach (var scene in listScenes)
                {
                    if (!scene.enabled) continue;
                    string path = scene.path;
                    string id = AssetDatabase.AssetPathToGUID(path);

                    AddUseGuid(id, 0);
                }
            }

            if (string.IsNullOrEmpty(Extension)) Debug.LogWarning($"Something wrong? <{_mExtension}>");

            if (Extension == ".spriteatlas") // check for force include in build
            {
                var atlasAsset = AssetDatabase.LoadAssetAtPath<UnityObject>(_mAssetPath);
                if (atlasAsset != null)
                {
                    var so = new SerializedObject(atlasAsset);
                    var prop = so.FindProperty("m_EditorData.bindAsDefault");
                    mForceIncludeInBuild = prop.boolValue;
                }
            }

            if (UiToolkit.Contains(_mExtension))
            {
                if (_mExtension == ".tss")
                {
                    FinderParser.ReadTss(_mAssetPath, AddUseGuid);
                }
                else
                {
                    FinderParser.ReadUssUxml(_mAssetPath, AddUseGuid);
                }

                return;
            }

            if (ReferencableJson.Contains(_mExtension))
            {
                FinderParser.ReadJson(_mAssetPath, AddUseGuid);
                return;
            }

            if (ReferencableMeta.Contains(_mExtension))
            {
                FinderParser.ReadYaml($"{_mAssetPath}.meta", AddUseGuid);
                return;
            }

            FinderParser.ReadYaml(_mAssetPath, AddUseGuid);
        }

        internal void LoadFolder()
        {
            if (!Directory.Exists(_mAssetPath))
            {
                state = EFinderAssetState.Missing;
                return;
            }

            // do not analyse folders outside project
            if (!_mAssetPath.StartsWith("Assets/")) return;

            try
            {
                string[] files = Directory.GetFiles(_mAssetPath);
                string[] dirs = Directory.GetDirectories(_mAssetPath);

                foreach (string f in files)
                {
                    if (f.EndsWith(".meta", StringComparison.Ordinal)) continue;

                    string fguid = AssetDatabase.AssetPathToGUID(f);
                    if (string.IsNullOrEmpty(fguid)) continue;

                    AddUseGuid(fguid);
                }

                foreach (string d in dirs)
                {
                    string fguid = AssetDatabase.AssetPathToGUID(d);
                    if (string.IsNullOrEmpty(fguid)) continue;

                    AddUseGuid(fguid);
                }
            }

            catch
            {
                state = EFinderAssetState.Missing;
            }
        }


        // ----------------------------- REPLACE GUIDS ---------------------------------------

        internal bool ReplaceReference(string fromGuid, string toGuid, TerrainData terrain = null)
        {
            if (IsMissing) return false;

            if (IsReferencable)
            {
                if (!File.Exists(_mAssetPath))
                {
                    state = EFinderAssetState.Missing;
                    return false;
                }

                try
                {
                    string text = File.ReadAllText(_mAssetPath).Replace("\r", "\n");
                    File.WriteAllText(_mAssetPath, text.Replace(fromGuid, toGuid));

                    return true;
                }
                catch (Exception e)
                {
                    state = EFinderAssetState.Missing;

                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + _mAssetPath);
                }

                return false;
            }

            if (type == EFinderAssetType.Terrain)
            {
                var fromObj = FinderUtility.LoadAssetWithGuid<UnityObject>(fromGuid);
                var toObj = FinderUtility.LoadAssetWithGuid<UnityObject>(toGuid);
                var found = 0;


                if (fromObj is Texture2D)
                {
                    var arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D) fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D) toObj;
                        }
                    }

                    terrain.detailPrototypes = arr;
                    FinderUtility.ReplaceTerrainTextureDatas(terrain, (Texture2D) fromObj, (Texture2D) toObj);
                }

                if (fromObj is GameObject)
                {
                    var arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject) fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject) toObj;
                        }
                    }

                    terrain.treePrototypes = arr2;
                }

                return found > 0;
            }

            return false;
        }

        internal string ReplaceFileIdIfNeeded(string line, long toFileId)
        {
            const string fileID = "fileID: ";
            int index = line.IndexOf(fileID, StringComparison.Ordinal);
            if (index < 0 || toFileId <= 0) return line;
            int startIndex = index + fileID.Length;
            int endIndex = line.IndexOf(',', startIndex);
            if (endIndex > startIndex)
            {
                string fromFileId = line.Substring(startIndex, endIndex - startIndex);
                if (long.TryParse(fromFileId, out long fileType) && fileType.ToString().StartsWith(toFileId.ToString().Substring(0, 3)))
                {
                    Debug.Log($"ReplaceReference: fromFileId {fromFileId} to File Id {toFileId}");
                    return line.Replace(fromFileId, toFileId.ToString());
                }

                Debug.LogWarning($"[Skip] Difference file type: {fromFileId} -> {toFileId}");
            }
            else
            {
                Debug.LogWarning("Cannot parse fileID in the line.");
            }

            return line;
        }

        internal bool ReplaceReference(string fromGuid, string toGuid, long toFileId, TerrainData terrain = null)
        {
            Debug.Log($"{AssetPath} : ReplaceReference from " + fromGuid + "  to: " + toGuid + "  toFileId: " + toFileId);

            if (IsMissing) return false;

            if (IsReferencable)
            {
                if (!File.Exists(_mAssetPath))
                {
                    state = EFinderAssetState.Missing;
                    return false;
                }

                try
                {
                    var sb = new StringBuilder();
                    string text = File.ReadAllText(AssetPath);
                    var currentIndex = 0;

                    while (currentIndex < text.Length)
                    {
                        int lineEndIndex = text.IndexOfAny(new[] {'\r', '\n'}, currentIndex);
                        if (lineEndIndex == -1) lineEndIndex = text.Length;

                        string line = text.Substring(currentIndex, lineEndIndex - currentIndex);

                        // Check if the line contains the GUID and possibly the fileID
                        if (line.Contains(fromGuid))
                        {
                            line = ReplaceFileIdIfNeeded(line, toFileId);
                            line = line.Replace(fromGuid, toGuid);
                        }

                        sb.Append(line);

                        // Skip through any EOL characters
                        while (lineEndIndex < text.Length)
                        {
                            char c = text[lineEndIndex];
                            if (c == '\r' || c == '\n')
                            {
                                sb.Append(c);
                                lineEndIndex++;
                            }

                            break;
                        }

                        currentIndex = lineEndIndex;
                    }

                    File.WriteAllText(AssetPath, sb.ToString());

                    return true;
                }
                catch (Exception e)
                {
                    state = EFinderAssetState.Missing;

                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + _mAssetPath);
                }

                return false;
            }

            if (type == EFinderAssetType.Terrain)
            {
                var fromObj = FinderUtility.LoadAssetWithGuid<UnityObject>(fromGuid);
                var toObj = FinderUtility.LoadAssetWithGuid<UnityObject>(toGuid);
                var found = 0;


                if (fromObj is Texture2D)
                {
                    var arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D) fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D) toObj;
                        }
                    }

                    terrain.detailPrototypes = arr;
                    FinderUtility.ReplaceTerrainTextureDatas(terrain, (Texture2D) fromObj, (Texture2D) toObj);
                }

                if (fromObj is GameObject)
                {
                    var arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject) fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject) toObj;
                        }
                    }

                    terrain.treePrototypes = arr2;
                }


                return found > 0;
            }

            Debug.LogWarning("Something wrong, should never be here - Ignored <" + _mAssetPath + "> : not a readable type, can not replace ! " + type);
            return false;
        }

        [Serializable]
        internal class Classes
        {
            public string guid;
            public List<long> ids;
        }
    }
}