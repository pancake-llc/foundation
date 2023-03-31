using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityObject = UnityEngine.Object;

namespace PancakeEditor
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
        NonReadable
    }

    public enum EFinderAssetState
    {
        New,
        Cache,
        Missing
    }

    [Serializable]
    public class FinderAsset
    {
        // ------------------------------ CONSTANTS ---------------------------

        private static readonly HashSet<string> ScriptExtensions = new HashSet<string>
        {
            ".cs",
            ".js",
            ".boo",
            ".h",
            ".java",
            ".cpp",
            ".m",
            ".mm"
        };

        private static readonly HashSet<string> ReferencableExtensions = new HashSet<string>
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
            ".mat",
            ".prefab",
            ".physicsmaterial",
            ".fontsettings",
            ".asset",
            ".prefs",
            ".spriteatlas"
        };

        private static readonly Dictionary<int, Type> HashClasses = new Dictionary<int, Type>();
        internal static Dictionary<string, GUIContent> cacheImage = new Dictionary<string, GUIContent>();

        private bool _isExcluded;
        private Dictionary<string, HashSet<int>> _useGUIDs;
        private float _excludeTs;

        public static float ignoreTs;


        // ----------------------------- DRAW  ---------------------------------------
        [NonSerialized] private GUIContent _fileSizeText;

        // ----------------------------- DRAW  ---------------------------------------

        [SerializeField] public string guid;

        // easy to recalculate: will not cache
        [NonSerialized] private bool _mPathLoaded;
        [NonSerialized] private string _mAssetFolder;
        [NonSerialized] private string _mAssetName;
        [NonSerialized] private string _mAssetPath;
        [NonSerialized] private string _mExtension;
        [NonSerialized] private bool _mInEditor;
        [NonSerialized] private bool _mInPlugins;
        [NonSerialized] private bool _mInResources;
        [NonSerialized] private bool _mInStreamingAsset;
        [NonSerialized] private bool _mIsAssetFile;

        // Need to read FileInfo: soft-cache (always re-read when needed)
        [SerializeField] public EFinderAssetType type;
        [FormerlySerializedAs("m_fileInfoHash")] [SerializeField] private string mFileInfoHash;
        [FormerlySerializedAs("m_assetbundle")] [SerializeField] private string mAssetbundle;
        [FormerlySerializedAs("m_addressable")] [SerializeField] private string mAddressable;

        [FormerlySerializedAs("m_fileSize")] [SerializeField] private long mFileSize;

        [FormerlySerializedAs("m_assetChangeTS")] [SerializeField] private int mAssetChangeTs; // Realtime when asset changed (trigger by import asset operation)
        [FormerlySerializedAs("m_fileInfoReadTS")] [SerializeField] private int mFileInfoReadTs; // Realtime when asset being read

        [FormerlySerializedAs("m_fileWriteTS")] [SerializeField] private int mFileWriteTs; // file's lastModification (file content + meta)

        [FormerlySerializedAs("m_cachefileWriteTS")] [SerializeField]
        private int mCachefileWriteTs; // file's lastModification at the time the content being read

        [SerializeField] internal int refreshStamp; // use to check if asset has been deleted (refreshStamp not updated)


        // Do not cache
        [NonSerialized] internal EFinderAssetState state;
        internal Dictionary<string, FinderAsset> usedByMap = new Dictionary<string, FinderAsset>();
        internal HashSet<int> hashUsedByClassesIds = new HashSet<int>();
        [FormerlySerializedAs("UseGUIDsList")] [SerializeField] private List<Classes> useGUIDsList = new List<Classes>();

        public FinderAsset(string guid)
        {
            this.guid = guid;
            type = EFinderAssetType.Unknown;
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

            FinderUnity.SplitPath(_mAssetPath, out _mAssetName, out _mExtension, out _mAssetFolder);

            if (_mAssetFolder.StartsWith("Assets/"))
            {
                _mAssetFolder = _mAssetFolder.Substring(7);
            }
            else if (!FinderUnity.StringStartsWith(_mAssetPath, "Packages/", "Project Settings/", "Library/"))
            {
                _mAssetFolder = "built-in/";
            }

            _mInEditor = _mAssetPath.Contains("/Editor/") || _mAssetPath.Contains("/Editor Default Resources/");
            _mInResources = _mAssetPath.Contains("/Resources/");
            _mInStreamingAsset = _mAssetPath.Contains("/StreamingAssets/");
            _mInPlugins = _mAssetPath.Contains("/Plugins/");
            _mIsAssetFile = _mAssetPath.EndsWith(".asset", StringComparison.Ordinal);
        }

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
                if (string.IsNullOrEmpty(_mAssetPath))
                {
                    state = EFinderAssetState.Missing;
                }

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
                return _mInEditor;
            }
        }

        public bool InPlugins
        {
            get
            {
                LoadPathInfo();
                return _mInPlugins;
            }
        }

        public bool InResources
        {
            get
            {
                LoadPathInfo();
                return _mInResources;
            }
        }

        public bool InStreamingAsset
        {
            get
            {
                LoadPathInfo();
                return _mInStreamingAsset;
            }
        }

        // ----------------------- TYPE INFO ------------------------

        internal bool IsFolder => type == EFinderAssetType.Folder;
        internal bool IsScript => type == EFinderAssetType.Script;
        internal bool IsMissing => state == EFinderAssetState.Missing;
        internal bool IsReferencable => type == EFinderAssetType.Referencable || type == EFinderAssetType.Scene;
        internal bool IsBinaryAsset => type == EFinderAssetType.BinaryAsset || type == EFinderAssetType.Model || type == EFinderAssetType.Terrain;

        // ----------------------- PATH INFO ------------------------
        public bool FileInfoDirty => (type == EFinderAssetType.Unknown) || (mFileInfoReadTs <= mAssetChangeTs);
        public bool FileContentDirty => mFileWriteTs != mCachefileWriteTs;

        public bool IsDirty => FileInfoDirty || FileContentDirty;

        bool ExistOnDisk()
        {
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

            //Debug.Log("--> Read: " + assetPath + " --> " + m_fileInfoReadTS + "<" + m_assetChangeTS);
            mFileInfoReadTs = FinderUnity.Epoch(DateTime.Now);

            if (IsMissing)
            {
                Debug.LogWarning("Should never be here! - missing files can not trigger LoadFileInfo()");
                return;
            }

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
            mAddressable = FinderUnity.GetAddressable(guid);
            //if (!string.IsNullOrEmpty(m_addressable)) Debug.LogWarning(guid + " --> " + m_addressable);
            mAssetbundle = AssetDatabase.GetImplicitAssetBundleName(_mAssetPath);

            // check if file content changed
            var metaInfo = new FileInfo(_mAssetPath + ".meta");
            var assetTime = FinderUnity.Epoch(info.LastWriteTime);
            var metaTime = FinderUnity.Epoch(metaInfo.LastWriteTime);

            // update fileChangeTimeStamp
            mFileWriteTs = Mathf.Max(metaTime, assetTime);
        }

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


        public Dictionary<string, HashSet<int>> UseGUIDs
        {
            get
            {
                if (_useGUIDs != null)
                {
                    return _useGUIDs;
                }

                _useGUIDs = new Dictionary<string, HashSet<int>>(useGUIDsList.Count);
                for (var i = 0; i < useGUIDsList.Count; i++)
                {
                    string guid = useGUIDsList[i].guid;
                    if (_useGUIDs.ContainsKey(guid))
                    {
                        for (var j = 0; j < useGUIDsList[i].ids.Count; j++)
                        {
                            int val = useGUIDsList[i].ids[j];
                            if (_useGUIDs[guid].Contains(val))
                            {
                                continue;
                            }

                            _useGUIDs[guid].Add(useGUIDsList[i].ids[j]);
                        }
                    }
                    else
                    {
                        _useGUIDs.Add(guid, new HashSet<int>(useGUIDsList[i].ids));
                    }
                }

                return _useGUIDs;
            }
        }

        // ------------------------------- GETTERS -----------------------------


        internal bool IsExcluded
        {
            get
            {
                if (_excludeTs >= ignoreTs)
                {
                    return _isExcluded;
                }

                _excludeTs = ignoreTs;
                _isExcluded = false;

                foreach (string item in FinderWindowBase.IgnoreAsset)
                {
                    if (_mAssetPath.StartsWith(item, false, CultureInfo.InvariantCulture))
                    {
                        _isExcluded = true;
                        break;
                    }
                }

                return _isExcluded;
            }
        }

        public void AddUsedBy(string guid, FinderAsset asset)
        {
            if (usedByMap.ContainsKey(guid)) return;

            if (guid == this.guid) return;

            usedByMap.Add(guid, asset);
            if (hashUsedByClassesIds == null) hashUsedByClassesIds = new HashSet<int>();


            if (asset.UseGUIDs.TryGetValue(this.guid, out var output))
            {
                foreach (int item in output)
                {
                    hashUsedByClassesIds.Add(item);
                }
            }
        }

        public int UsageCount() { return usedByMap.Count; }

        public override string ToString() { return string.Format("FinderAsset[{0}]", _mAssetName); }

        //--------------------------------- STATIC ----------------------------

        internal void MarkAsDirty(bool isMoved = true, bool force = false)
        {
            if (isMoved)
            {
                var newPath = AssetDatabase.GUIDToAssetPath(guid);
                if (newPath != _mAssetPath)
                {
                    _mPathLoaded = false;
                    _mAssetPath = newPath;
                }
            }

            state = EFinderAssetState.Cache;
            mAssetChangeTs = FinderUnity.Epoch(DateTime.Now); // re-read FileInfo
            if (force) mCachefileWriteTs = 0;
        }

        // --------------------------------- APIs ------------------------------

        internal void GuessAssetType()
        {
            if (ScriptExtensions.Contains(_mExtension))
            {
                type = EFinderAssetType.Script;
            }
            else if (ReferencableExtensions.Contains(_mExtension))
            {
                bool isUnity = _mExtension == ".unity";
                type = isUnity ? EFinderAssetType.Scene : EFinderAssetType.Referencable;

                if (_mExtension == ".asset" || isUnity || _mExtension == ".spriteatlas")
                {
                    var buffer = new byte[5];

                    try
                    {
                        FileStream stream = File.OpenRead(_mAssetPath);
                        stream.Read(buffer, 0, 5);
                        stream.Close();
                    }
                    catch
                    {
                        state = EFinderAssetState.Missing;
                        return;
                    }

                    string str = string.Empty;
                    foreach (byte t in buffer)
                    {
                        str += (char) t;
                    }

                    if (str != "%YAML")
                    {
                        type = EFinderAssetType.BinaryAsset;
                    }
                }
            }
            else if (_mExtension == ".fbx")
            {
                type = EFinderAssetType.Model;
            }
            else if (_mExtension == ".dll")
            {
                type = EFinderAssetType.DLL;
            }
            else
            {
                type = EFinderAssetType.NonReadable;
            }
        }


        internal void LoadContent()
        {
            if (!FileContentDirty) return;
            mCachefileWriteTs = mFileWriteTs;

            if (IsMissing || type == EFinderAssetType.NonReadable)
            {
                return;
            }

            if (type == EFinderAssetType.DLL)
            {
                return;
            }

            if (!ExistOnDisk())
            {
                state = EFinderAssetState.Missing;
                return;
            }

            ClearUseGUIDs();

            if (IsFolder)
            {
                LoadFolder();
            }
            else if (IsReferencable)
            {
                LoadYaml2();
            }
            else if (IsBinaryAsset)
            {
                LoadBinaryAsset();
            }
        }

        internal void AddUseGuid(string fguid, int fFileId = -1, bool checkExist = true)
        {
            if (!UseGUIDs.ContainsKey(fguid))
            {
                useGUIDsList.Add(new Classes {guid = fguid, ids = new List<int>()});
                UseGUIDs.Add(fguid, new HashSet<int>());
            }

            if (fFileId != -1)
            {
                if (UseGUIDs[fguid].Contains(fFileId))
                {
                    return;
                }

                UseGUIDs[fguid].Add(fFileId);
                Classes i = useGUIDsList.FirstOrDefault(x => x.guid == fguid);
                if (i != null)
                {
                    i.ids.Add(fFileId);
                }
            }
        }

        // ----------------------------- STATIC  ---------------------------------------

        internal static int SortByExtension(FinderAsset a1, FinderAsset a2)
        {
            if (a1 == null)
            {
                return -1;
            }

            if (a2 == null)
            {
                return 1;
            }

            int result = string.Compare(a1._mExtension, a2._mExtension, StringComparison.Ordinal);
            return result == 0 ? string.Compare(a1._mAssetName, a2._mAssetName, StringComparison.Ordinal) : result;
        }

        internal static List<FinderAsset> FindUsage(FinderAsset asset)
        {
            if (asset == null)
            {
                return null;
            }

            List<FinderAsset> refs = FinderWindowBase.CacheSetting.FindAssets(asset.UseGUIDs.Keys.ToArray(), true);


            return refs;
        }

        internal static List<FinderAsset> FindUsedBy(FinderAsset asset) { return asset.usedByMap.Values.ToList(); }

        internal static List<string> FindUsageGUIDs(FinderAsset asset, bool includeScriptSymbols)
        {
            var result = new HashSet<string>();
            if (asset == null)
            {
                Debug.LogWarning("Asset invalid : " + asset._mAssetName);
                return result.ToList();
            }

            // for (var i = 0;i < asset.UseGUIDs.Count; i++)
            // {
            // 	result.Add(asset.UseGUIDs[i]);
            // }
            foreach (KeyValuePair<string, HashSet<int>> item in asset.UseGUIDs)
            {
                result.Add(item.Key);
            }

            return result.ToList();
        }

        internal static List<string> FindUsedByGUIDs(FinderAsset asset) { return asset.usedByMap.Keys.ToList(); }

        internal float Draw(
            Rect r,
            bool highlight,
            bool drawPath = true,
            bool showFileSize = true,
            bool showAbName = false,
            bool showAtlasName = false,
            bool showUsageIcon = true,
            IWindow window = null)
        {
            bool singleLine = r.height <= 18f;
            float rw = r.width;
            bool selected = FinderBookmark.Contains(guid);

            r.height = 16f;
            bool hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                if (_mExtension == ".prefab")
                {
                    menu.AddItem(new GUIContent("Edit in Scene"), false, EditPrefab);
                }

                menu.AddItem(new GUIContent("Open"), false, Open);
                menu.AddItem(new GUIContent("Ping"), false, Ping);
                menu.AddItem(new GUIContent(guid), false, CopyGuid);
                //menu.AddItem(new GUIContent("Reload"), false, Reload);

                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Bookmark"), selected, AddToSelection);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Copy path"), false, CopyAssetPath);
                menu.AddItem(new GUIContent("Copy full path"), false, CopyAssetPathFull);

                //if (IsScript)
                //{
                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptSymbols, "+ ", "Definitions", "No Definition", false);

                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptUsage, "-> ", "Depends", "No Dependency", true);
                //}

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (IsMissing)
            {
                if (!singleLine)
                {
                    r.y += 16f;
                }

                if (Event.current.type != EventType.Repaint)
                {
                    return 0;
                }

                GUI.Label(r, "(missing) " + guid, EditorStyles.whiteBoldLabel);
                return 0;
            }

            Rect iconRect = GUI2.LeftRect(16f, ref r);
            if (Event.current.type == EventType.Repaint)
            {
                Texture icon = AssetDatabase.GetCachedIcon(_mAssetPath);
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon);
                }
            }


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Rect pingRect = FinderWindowBase.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : iconRect;
                if (pingRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.control || Event.current.command)
                    {
                        if (selected)
                        {
                            RemoveFromSelection();
                        }
                        else
                        {
                            AddToSelection();
                        }

                        if (window != null)
                        {
                            window.Repaint();
                        }
                    }
                    else
                    {
                        Ping();
                    }


                    //Event.current.Use();
                }
            }

            if (Event.current.type != EventType.Repaint)
            {
                return 0;
            }

            if (usedByMap != null && usedByMap.Count > 0)
            {
                var str = new GUIContent(usedByMap.Count.ToString());
                Rect countRect = iconRect;
                countRect.x -= 16f;
                countRect.xMin = -10f;
                GUI.Label(countRect, str, GUI2.MiniLabelAlignRight);
            }

            float pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(_mAssetFolder)).x : 0;
            float nameW = EditorStyles.boldLabel.CalcSize(new GUIContent(_mAssetName)).x;
            Color cc = FinderWindowBase.SelectedColor;

            if (singleLine)
            {
                Rect lbRect = GUI2.LeftRect(pathW + nameW, ref r);

                if (selected)
                {
                    Color c1 = GUI.color;
                    GUI.color = cc;
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c1;
                }

                if (drawPath)
                {
                    Color c2 = GUI.color;
                    GUI.color = new Color(c2.r, c2.g, c2.b, c2.a * 0.5f);
                    GUI.Label(GUI2.LeftRect(pathW, ref lbRect), _mAssetFolder, EditorStyles.miniLabel);
                    GUI.color = c2;

                    lbRect.xMin -= 4f;
                    GUI.Label(lbRect, _mAssetName, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(lbRect, _mAssetName);
                }
            }
            else
            {
                if (drawPath)
                {
                    GUI.Label(new Rect(r.x, r.y + 16f, r.width, r.height), _mAssetFolder, EditorStyles.miniLabel);
                }

                Rect lbRect = GUI2.LeftRect(nameW, ref r);
                if (selected)
                {
                    GUI2.Rect(lbRect, cc);
                }

                GUI.Label(lbRect, _mAssetName, EditorStyles.boldLabel);
            }

            var rr = GUI2.RightRect(10f, ref r); //margin
            if (highlight)
            {
                rr.xMin += 2f;
                rr.width = 1f;
                GUI2.Rect(rr, GUI2.darkGreen);
            }

            Color c = GUI.color;
            GUI.color = new Color(c.r, c.g, c.b, c.a * 0.5f);

            if (showFileSize)
            {
                Rect fsRect = GUI2.RightRect(40f, ref r); // filesize label

                if (_fileSizeText == null)
                {
                    _fileSizeText = new GUIContent(FileSize.GetSizeInMemory());
                }


                GUI.Label(fsRect, _fileSizeText, GUI2.MiniLabelAlignRight);
            }

            if (!string.IsNullOrEmpty(mAddressable))
            {
                Rect adRect = GUI2.RightRect(100f, ref r);
                GUI.Label(adRect, mAddressable, GUI2.MiniLabelAlignRight);
            }


            if (showUsageIcon && hashUsedByClassesIds != null)
            {
                foreach (int item in hashUsedByClassesIds)
                {
                    if (!FinderUnity.HashClassesNormal.ContainsKey(item))
                    {
                        continue;
                    }

                    string name = FinderUnity.HashClassesNormal[item];
                    Type t = null;
                    if (!HashClasses.TryGetValue(item, out t))
                    {
                        t = FinderUnity.GetType(name);
                        HashClasses.Add(item, t);
                    }

                    GUIContent content = null;
                    var isExisted = cacheImage.TryGetValue(name, out content);
                    if (content == null) content = new GUIContent(EditorGUIUtility.ObjectContent(null, t).image, name);

                    if (!isExisted)
                    {
                        cacheImage.Add(name, content);
                    }
                    else
                    {
                        cacheImage[name] = content;
                    }

                    if (content != null)
                    {
                        try
                        {
                            GUI.Label(GUI2.RightRect(15f, ref r), content, GUI2.MiniLabelAlignRight);
                        }
                        catch
                        {
                            //
                        }
                    }
                }
            }

            if (showAbName)
            {
                GUI2.RightRect(10f, ref r); //margin
                Rect abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(mAssetbundle))
                {
                    GUI.Label(abRect, mAssetbundle, GUI2.MiniLabelAlignRight);
                }
            }

            if (true)
            {
                GUI2.RightRect(10f, ref r); //margin
                Rect abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(mAddressable))
                {
                    GUI.Label(abRect, mAddressable, GUI2.MiniLabelAlignRight);
                }
            }

            GUI.color = c;

            if (Event.current.type == EventType.Repaint)
            {
                return rw < pathW + nameW ? 32f : 18f;
            }

            return r.height;
        }


        internal GenericMenu AddArray(GenericMenu menu, List<string> list, string prefix, string title, string emptyTitle, bool showAsset, int max = 10)
        {
            menu.AddItem(new GUIContent(emptyTitle), true, null);
            
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

        internal void AddToSelection()
        {
            if (!FinderBookmark.Contains(guid))
            {
                FinderBookmark.Add(guid);
            }
        }

        internal void RemoveFromSelection()
        {
            if (FinderBookmark.Contains(guid))
            {
                FinderBookmark.Remove(guid);
            }
        }

        internal void Ping() { EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject))); }

        internal void Open() { AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject))); }

        internal void EditPrefab()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject));
            UnityObject.Instantiate(prefab);
        }
        
        // ----------------------------- SERIALIZED UTILS ---------------------------------------


        // ----------------------------- LOAD ASSETS ---------------------------------------

        internal void LoadGameObject(GameObject go)
        {
            Component[] compList = go.GetComponentsInChildren<Component>();
            for (var i = 0; i < compList.Length; i++)
            {
                LoadSerialized(compList[i]);
            }
        }

        internal void LoadSerialized(UnityObject target)
        {
            SerializedProperty[] props = FinderUnity.XGetSerializedProperties(target, true);

            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                UnityObject refObj = props[i].objectReferenceValue;
                if (refObj == null)
                {
                    continue;
                }

                string refGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(refObj));

                AddUseGuid(refGuid);
            }
        }

        internal void LoadTerrainData(TerrainData terrain)
        {
#if UNITY_2018_3_OR_NEWER
            TerrainLayer[] arr0 = terrain.terrainLayers;
            for (var i = 0; i < arr0.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr0[i]);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }
#endif


            DetailPrototype[] arr = terrain.detailPrototypes;

            for (var i = 0; i < arr.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr[i].prototypeTexture);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }

            TreePrototype[] arr2 = terrain.treePrototypes;
            for (var i = 0; i < arr2.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr2[i].prefab);
                string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGuid(refGuid);
            }

            FinderUnity.TerrainTextureData[] arr3 = FinderUnity.GetTerrainTextureDatas(terrain);
            for (var i = 0; i < arr3.Length; i++)
            {
                FinderUnity.TerrainTextureData texs = arr3[i];
                for (var k = 0; k < texs.textures.Length; k++)
                {
                    Texture2D tex = texs.textures[k];
                    if (tex == null)
                    {
                        continue;
                    }

                    string aPath = AssetDatabase.GetAssetPath(tex);
                    if (string.IsNullOrEmpty(aPath))
                    {
                        continue;
                    }

                    string refGuid = AssetDatabase.AssetPathToGUID(aPath);
                    if (string.IsNullOrEmpty(refGuid))
                    {
                        continue;
                    }

                    AddUseGuid(refGuid);
                }
            }
        }

        private void ClearUseGUIDs()
        {
            UseGUIDs.Clear();
            useGUIDsList.Clear();
        }

        static int binaryLoaded;

        internal void LoadBinaryAsset()
        {
            ClearUseGUIDs();

            UnityObject assetData = AssetDatabase.LoadAssetAtPath(_mAssetPath, typeof(UnityObject));
            if (assetData is GameObject)
            {
                type = EFinderAssetType.Model;
                LoadGameObject(assetData as GameObject);
            }
            else if (assetData is TerrainData)
            {
                type = EFinderAssetType.Terrain;
                LoadTerrainData(assetData as TerrainData);
            }
            else
            {
                LoadSerialized(assetData);
            }

            assetData = null;

            if (binaryLoaded++ <= 30) return;
            binaryLoaded = 0;
            FinderUnity.UnloadUnusedAssets();
        }

        internal void LoadYaml2()
        {
            if (!File.Exists(_mAssetPath))
            {
                state = EFinderAssetState.Missing;
                return;
            }

            if (_mAssetPath == "ProjectSettings/EditorBuildSettings.asset")
            {
                var listScenes = UnityEditor.EditorBuildSettings.scenes;
                foreach (var scene in listScenes)
                {
                    if (!scene.enabled) continue;
                    var path = scene.path;
                    var guid = AssetDatabase.AssetPathToGUID(path);

                    AddUseGuid(guid, 0);
                }
            }

            // var text = string.Empty;
            try
            {
                using (var sr = new StreamReader(_mAssetPath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        int index = line.IndexOf("guid: ");
                        if (index < 0)
                        {
                            continue;
                        }

                        string refGuid = line.Substring(index + 6, 32);
                        int indexFileId = line.IndexOf("fileID: ");
                        int fileID = -1;
                        if (indexFileId >= 0)
                        {
                            indexFileId += 8;
                            string fileIDStr = line.Substring(indexFileId, line.IndexOf(',', indexFileId) - indexFileId);
                            try
                            {
                                fileID = int.Parse(fileIDStr) / 100000;
                            }
                            catch
                            {
                            }
                        }

                        AddUseGuid(refGuid, fileID);
                    }
                }
            }
            catch
            {
                state = EFinderAssetState.Missing;
            }
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
                    if (f.EndsWith(".meta", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string fguid = AssetDatabase.AssetPathToGUID(f);
                    if (string.IsNullOrEmpty(fguid))
                    {
                        continue;
                    }

                    AddUseGuid(fguid);
                }

                foreach (string d in dirs)
                {
                    string fguid = AssetDatabase.AssetPathToGUID(d);
                    if (string.IsNullOrEmpty(fguid))
                    {
                        continue;
                    }

                    AddUseGuid(fguid);
                }
            }
            catch
            {
                state = EFinderAssetState.Missing;
            }
        }

        [Serializable]
        private class Classes
        {
            public string guid;
            public List<int> ids;
        }
    }
}