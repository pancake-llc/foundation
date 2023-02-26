using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class FinderAssetType
    {
        // ------------------------------- STATIC -----------------------------

        internal static readonly FinderAssetType[] Filters =
        {
            new FinderAssetType("Scene", ".unity"), new FinderAssetType("Prefab", ".prefab"), new FinderAssetType("Model",
                ".3df",
                ".3dm",
                ".3dmf",
                ".3dv",
                ".3dx",
                ".c5d",
                ".lwo",
                ".lws",
                ".ma",
                ".mb",
                ".mesh",
                ".vrl",
                ".wrl",
                ".wrz",
                ".fbx",
                ".dae",
                ".3ds",
                ".dxf",
                ".obj",
                ".skp",
                ".max",
                ".blend"),
            new FinderAssetType("Material", ".mat", ".cubemap", ".physicsmaterial"), new FinderAssetType("Texture",
                ".ai",
                ".apng",
                ".png",
                ".bmp",
                ".cdr",
                ".dib",
                ".eps",
                ".exif",
                ".ico",
                ".icon",
                ".j",
                ".j2c",
                ".j2k",
                ".jas",
                ".jiff",
                ".jng",
                ".jp2",
                ".jpc",
                ".jpe",
                ".jpeg",
                ".jpf",
                ".jpg",
                "jpw",
                "jpx",
                "jtf",
                ".mac",
                ".omf",
                ".qif",
                ".qti",
                "qtif",
                ".tex",
                ".tfw",
                ".tga",
                ".tif",
                ".tiff",
                ".wmf",
                ".psd",
                ".exr",
                ".rendertexture"),
            new FinderAssetType("Video",
                ".asf",
                ".asx",
                ".avi",
                ".dat",
                ".divx",
                ".dvx",
                ".mlv",
                ".m2l",
                ".m2t",
                ".m2ts",
                ".m2v",
                ".m4e",
                ".m4v",
                "mjp",
                ".mov",
                ".movie",
                ".mp21",
                ".mp4",
                ".mpe",
                ".mpeg",
                ".mpg",
                ".mpv2",
                ".ogm",
                ".qt",
                ".rm",
                ".rmvb",
                ".wmv",
                ".xvid",
                ".flv"),
            new FinderAssetType("Audio",
                ".mp3",
                ".wav",
                ".ogg",
                ".aif",
                ".aiff",
                ".mod",
                ".it",
                ".s3m",
                ".xm"),
            new FinderAssetType("Script",
                ".cs",
                ".js",
                ".boo",
                ".h"),
            new FinderAssetType("Text",
                ".txt",
                ".json",
                ".xml",
                ".bytes",
                ".sql"),
            new FinderAssetType("Shader", ".shader", ".cginc"), new FinderAssetType("Animation",
                ".anim",
                ".controller",
                ".overridecontroller",
                ".mask"),
            new FinderAssetType("Unity Asset",
                ".asset",
                ".guiskin",
                ".flare",
                ".fontsettings",
                ".prefs"),
            new FinderAssetType("Others") //
        };

        private static FinderIgnore ignore;
        public HashSet<string> extension;
        public string name;

        public FinderAssetType(string name, params string[] exts)
        {
            this.name = name;
            extension = new HashSet<string>();
            for (var i = 0; i < exts.Length; i++)
            {
                extension.Add(exts[i]);
            }
        }

        private static FinderIgnore Ignore
        {
            get
            {
                if (ignore == null)
                {
                    ignore = new FinderIgnore();
                }

                return ignore;
            }
        }

        public static int GetIndex(string ext)
        {
            for (var i = 0; i < Filters.Length - 1; i++)
            {
                if (Filters[i].extension.Contains(ext))
                {
                    return i;
                }
            }

            return Filters.Length - 1; //Others
        }

        public static bool DrawSearchFilter()
        {
            int n = Filters.Length;
            var nCols = 4;
            int nRows = Mathf.CeilToInt(n / (float) nCols);
            var result = false;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("All", EditorStyles.toolbarButton) && !FinderWindowBase.IsIncludeAllType())
                {
                    FinderWindowBase.IncludeAllType();
                    result = true;
                }

                if (GUILayout.Button("None", EditorStyles.toolbarButton) && FinderWindowBase.GetExcludeType() != -1)
                {
                    FinderWindowBase.ExcludeAllType();
                    result = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (var i = 0; i < nCols; i++)
            {
                GUILayout.BeginVertical();
                for (var j = 0; j < nRows; j++)
                {
                    int idx = i * nCols + j;
                    if (idx >= n)
                    {
                        break;
                    }

                    bool s = !FinderWindowBase.IsTypeExcluded(idx);
                    bool s1 = GUILayout.Toggle(s, Filters[idx].name);
                    if (s1 != s)
                    {
                        result = true;
                        FinderWindowBase.ToggleTypeExclude(idx);
                    }
                }

                GUILayout.EndVertical();
                if ((i + 1) * nCols >= n)
                {
                    break;
                }
            }

            GUILayout.EndHorizontal();

            return result;
        }

        public static void SetDirtyIgnore() { Ignore.SetDirty(); }

        public static bool DrawIgnoreFolder()
        {
            var change = false;
            Ignore.Draw();
            
            return change;
        }

        private class FinderIgnore
        {
            public readonly FinderTreeUI.GroupDrawer groupIgnore;
            private bool _dirty;
            private Dictionary<string, FinderRef> _refs;

            public FinderIgnore()
            {
                groupIgnore = new FinderTreeUI.GroupDrawer(DrawGroup, DrawItem);
                groupIgnore.hideGroupIfPossible = false;

                ApplyFiter();
            }

            private void DrawItem(Rect r, string guid)
            {
                FinderRef rf;
                if (!_refs.TryGetValue(guid, out rf))
                {
                    return;
                }

                if (rf.depth == 1) //mode != Mode.Dependency && 
                {
                    Color c = GUI.color;
                    GUI.color = Color.blue;
                    GUI.DrawTexture(new Rect(r.x - 4f, r.y + 2f, 2f, 2f), EditorGUIUtility.whiteTexture);
                    GUI.color = c;
                }

                rf.asset.Draw(r,
                    false,
                    true,
                    false,
                    false,
                    false,
                    false,
                    null);

                Rect drawR = r;
                drawR.x = drawR.x + drawR.width - 50f; // (groupDrawer.TreeNoScroll() ? 60f : 70f) ;
                drawR.width = 30;
                drawR.y += 1;
                drawR.height -= 2;

                if (GUI.Button(drawR, "X", EditorStyles.miniButton))
                {
                    FinderWindowBase.RemoveIgnore(rf.asset.AssetPath);
                }
            }

            private void DrawGroup(Rect r, string id, int childCound)
            {
                GUI.Label(r, id, EditorStyles.boldLabel);
                if (childCound <= 1)
                {
                    return;
                }

                Rect drawR = r;
                drawR.x = drawR.x + drawR.width - 50f; // (groupDrawer.TreeNoScroll() ? 60f : 70f) ;
                drawR.width = 30;
                drawR.y += 1;
                drawR.height -= 2;
            }

            public void SetDirty() { _dirty = true; }

            public void Draw()
            {
                if (_dirty)
                {
                    ApplyFiter();
                }

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(4f);
                    var drops = GUI2.DropZone("Drag & Drop folders here to exclude", 100, 95);
                    if (drops != null && drops.Length > 0)
                    {
                        for (var i = 0; i < drops.Length; i++)
                        {
                            string path = AssetDatabase.GetAssetPath(drops[i]);

                            FinderWindowBase.AddIgnore(path);
                        }
                    }

                    groupIgnore.DrawLayout();
                }
                GUILayout.EndHorizontal();
            }


            private void ApplyFiter()
            {
                _dirty = false;
                _refs = new Dictionary<string, FinderRef>();
                
                foreach (string item2 in FinderWindowBase.ListIgnore)
                {
                    string guid = AssetDatabase.AssetPathToGUID(item2);
                    if (string.IsNullOrEmpty(guid))
                    {
                        continue;
                    }

                    FinderAsset asset = FinderWindowBase.CacheSetting.Get(guid, true);
                    var r = new FinderRef(0,
                        0,
                        asset,
                        null,
                        "Ignore");
                    _refs.Add(guid, r);
                }

                groupIgnore.Reset(_refs.Values.ToList(), rf => rf.asset != null ? rf.asset.guid : "", GetGroup, SortGroup);
            }

            private string GetGroup(FinderRef rf) { return "Ignore"; }

            private void SortGroup(List<string> groups) { }
        }
    }
}