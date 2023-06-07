using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.LevelSystemEditor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static class UtilitiesLevelSystemDrawer
    {
        private static PreviewGenerator previewGenerator;
        private static Dictionary<GameObject, Texture2D> previewDict;

        private static PreviewGenerator PreviewGenerator
        {
            get
            {
                if (previewGenerator != null) return previewGenerator;
                return previewGenerator = new PreviewGenerator {width = 512, height = 512, transparentBackground = true, sizingType = PreviewGenerator.ImageSizeType.Fit};
            }
        }

        private static Vector2 EventMousePoint
        {
            get
            {
                var position = Event.current.mousePosition;
                position.y = Screen.height - position.y - 60f;
                return position;
            }
        }

        #region Level Editor

        public static void ClearPreviews()
        {
            if (previewDict == null) return;

            foreach (var kvp in previewDict.ToList())
            {
                previewDict[kvp.Key] = null;
            }

            previewDict.Clear();
        }

        public static void ClearPreview(GameObject go)
        {
            if (previewDict?.TryGetValue(go, out var tex) ?? false)
            {
                Object.DestroyImmediate(tex);
                previewDict.Remove(go);
            }
        }

        public static Texture2D GetPreview(GameObject go, bool canCreate = true)
        {
            if (!go) return null;
            previewDict ??= new Dictionary<GameObject, Texture2D>();
            previewDict.TryGetValue(go, out var tex);
            if (!canCreate) return tex != null ? tex : default;

            if (tex) return tex;
            tex = PreviewGenerator.CreatePreview(go.gameObject);
            previewDict[go] = tex;

            return tex;
        }

        #endregion

        #region Level System

        #endregion


        public static void OnInspectorGUI(Rect position) { RefreshPickObject(); }

        private static void RefreshPickObject()
        {
            var blacklistAssets = new List<GameObject>();
            var whitelistAssets = new List<GameObject>();
            if (!ScriptableLevelSystemSetting.Instance.blacklistPaths.IsNullOrEmpty())
            {
                blacklistAssets = AssetDatabase.FindAssets("t:GameObject", ScriptableLevelSystemSetting.Instance.blacklistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string blacklistPath in ScriptableLevelSystemSetting.Instance.blacklistPaths)
                {
                    if (File.Exists(blacklistPath)) blacklistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(blacklistPath));
                }
            }

            if (!ScriptableLevelSystemSetting.Instance.whitelistPaths.IsNullOrEmpty())
            {
                whitelistAssets = AssetDatabase.FindAssets("t:GameObject", ScriptableLevelSystemSetting.Instance.whitelistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string whitelistPath in ScriptableLevelSystemSetting.Instance.whitelistPaths)
                {
                    if (File.Exists(whitelistPath)) whitelistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(whitelistPath));
                }
            }

            var resultAssets = whitelistAssets.Where(_ => !blacklistAssets.Contains(_));
            foreach (var o in resultAssets)
            {
                string group = Path.GetDirectoryName(AssetDatabase.GetAssetPath(o))?.Replace('\\', '/').Split('/').Last();
                var po = new PickObject {pickedObject = o.gameObject, group = group};
                ScriptableLevelSystemSetting.PickObjects.Add(po);
            }
        }

        private static void DrawDropAreaInternal(Rect position) { Uniform.DrawGroupFoldout("levelsystem_droparea", "Drop Area", () => DrawDropArea(position)); }

        private static void DrawDropArea(Rect position)
        {
            float width = 0;
            var @event = Event.current;

            EditorGUILayout.BeginHorizontal();
            var whiteArea = GUILayoutUtility.GetRect(0.0f, 50f, GUILayout.ExpandWidth(true));
            var blackArea = GUILayoutUtility.GetRect(0.0f, 50f, GUILayout.ExpandWidth(true));
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (whiteArea.width == 1f) width = position.width / 2;
            else width = whiteArea.width;
            GUI.backgroundColor = new Color(0f, 0.83f, 1f);
            GUI.Box(whiteArea, "[WHITE LIST]", new GUIStyle(EditorStyles.helpBox) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic});
            GUI.backgroundColor = Color.white;
            GUI.backgroundColor = new Color(1f, 0.13f, 0f);
            GUI.Box(blackArea, "[BLACK LIST]", new GUIStyle(EditorStyles.helpBox) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic});
            GUI.backgroundColor = Color.white;
            switch (@event.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (whiteArea.Contains(@event.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (@event.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (string path in DragAndDrop.paths)
                            {
                                if (File.Exists(path))
                                {
                                    var r = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                    if (r.GetType() != typeof(GameObject)) continue;
                                }

                                ValidateWhitelist(path, ref ScriptableLevelSystemSetting.Instance.blacklistPaths);
                                AddToWhitelist(path);
                            }

                            ReduceScopeDirectory(ref ScriptableLevelSystemSetting.Instance.whitelistPaths);
                            levelEditorSettings.SaveSetting();
                            RefreshAll();
                        }
                    }
                    else if (blackArea.Contains(@event.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (@event.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (string path in DragAndDrop.paths)
                            {
                                if (File.Exists(path))
                                {
                                    var r = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                    if (r.GetType() != typeof(GameObject)) continue;
                                }

                                ValidateBlacklist(path, ref levelEditorSettings.Settings.whitelistPaths);
                                AddToBlacklist(path);
                            }

                            ReduceScopeDirectory(ref levelEditorSettings.Settings.blacklistPaths);
                            levelEditorSettings.SaveSetting();
                            RefreshAll();
                        }
                    }

                    break;
                case EventType.MouseDown when @event.button == 1:
                    var menu = new GenericMenu();
                    if (whiteArea.Contains(@event.mousePosition))
                    {
                        menu.AddItem(new GUIContent("Clear All [WHITE LIST]"),
                            false,
                            () =>
                            {
                                levelEditorSettings.Settings.whitelistPaths.Clear();
                                levelEditorSettings.SaveSetting();
                                RefreshAll();
                            });
                    }
                    else if (blackArea.Contains(@event.mousePosition))
                    {
                        menu.AddItem(new GUIContent("Clear All [BLACK LIST]"),
                            false,
                            () =>
                            {
                                levelEditorSettings.Settings.blacklistPaths.Clear();
                                levelEditorSettings.SaveSetting();
                                RefreshAll();
                            });
                    }

                    menu.ShowAsContext();
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (var _ = new EditorGUILayout.HorizontalScope(GUILayout.Width(width - 10)))
            {
                if (ScriptableLevelSystemSetting.WhitelistPaths.Count == 0)
                {
                    EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                }
                else
                {
                    ScriptableLevelSystemSetting.WhiteScrollPosition =
                        GUILayout.BeginScrollView(ScriptableLevelSystemSetting.WhiteScrollPosition, false, false, GUILayout.Height(250));
                    foreach (string t in ScriptableLevelSystemSetting.WhitelistPaths.ToList())
                    {
                        DrawRow(t, width, _ => ScriptableLevelSystemSetting.WhitelistPaths.Remove(_));
                    }

                    GUILayout.EndScrollView();
                }
            }

            ;

            EditorGUILayout.EndHorizontal();

            Uniform.Horizontal(() =>
            {
                Uniform.VerticalScope(() =>
                    {
                        if (levelEditorSettings.Settings.whitelistPaths.Count == 0)
                        {
                            EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                        }
                        else
                        {
                            Uniform.SpaceHalfLine();
                            _whiteScrollPosition = GUILayout.BeginScrollView(_whiteScrollPosition, false, false, GUILayout.Height(250));
                            foreach (string t in levelEditorSettings.Settings.whitelistPaths.ToList())
                            {
                                DrawRow(t, width, _ => levelEditorSettings.Settings.whitelistPaths.Remove(_));
                            }

                            GUILayout.EndScrollView();
                        }
                    },
                    GUILayout.Width(width - 10));
                Uniform.SpaceOneLine();
                Uniform.VerticalScope(() =>
                    {
                        if (levelEditorSettings.Settings.blacklistPaths.Count == 0)
                        {
                            EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                        }
                        else
                        {
                            Uniform.SpaceHalfLine();
                            _blackScrollPosition = GUILayout.BeginScrollView(_blackScrollPosition, false, false, GUILayout.Height(250));
                            foreach (string t in levelEditorSettings.Settings.blacklistPaths.ToList())
                            {
                                DrawRow(t, width, _ => levelEditorSettings.Settings.blacklistPaths.Remove(_));
                            }

                            GUILayout.EndScrollView();
                        }
                    },
                    GUILayout.Width(width - 15));
            });


            void DrawRow(string content, float width, Action<string> action)
            {
                Uniform.Horizontal(() =>
                {
                    EditorGUILayout.LabelField(new GUIContent(content), GUILayout.Width(width - 100));
                    GUILayout.FlexibleSpace();
                    Uniform.Button(Uniform.IconContent("d_scenevis_visible_hover", "Ping Selection"),
                        () =>
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(content);
                            Selection.activeObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        });
                    Uniform.Button(Uniform.IconContent("Toolbar Minus", "Remove"),
                        () =>
                        {
                            action?.Invoke(content);
                            levelEditorSettings.SaveSetting();
                            RefreshAll();
                        });
                });
            }

            void ValidateBlacklist(string path, ref List<string> whiteList)
            {
                foreach (string t in whiteList.ToList())
                {
                    if (path.Equals(t) || IsChildOfPath(t, path)) whiteList.Remove(t);
                }
            }

            void AddToWhitelist(string path)
            {
                var check = false;
                foreach (string whitePath in ScriptableLevelSystemSetting.WhitelistPaths)
                {
                    if (IsChildOfPath(path, whitePath)) check = true;
                }

                if (!check) ScriptableLevelSystemSetting.WhitelistPaths.Add(path);
                ScriptableLevelSystemSetting.WhitelistPaths = ScriptableLevelSystemSetting.WhitelistPaths.Distinct().ToList(); //unique
            }

            void AddToBlacklist(string path)
            {
                var check = false;
                foreach (string blackPath in ScriptableLevelSystemSetting.BlacklistPaths)
                {
                    if (IsChildOfPath(path, blackPath)) check = true;
                }

                if (!check) ScriptableLevelSystemSetting.BlacklistPaths.Add(path);
                ScriptableLevelSystemSetting.BlacklistPaths = ScriptableLevelSystemSetting.BlacklistPaths.Distinct().ToList(); //unique
            }
        }

        private static void ReduceScopeDirectory(ref List<string> source)
        {
            var arr = new string[source.Count];
            source.CopyTo(arr);
            var valueRemove = new List<string>();
            var unique = arr.Distinct().ToList();
            foreach (string u in unique)
            {
                var check = false;
                foreach (string k in unique)
                {
                    if (IsChildOfPath(u, k)) check = true;
                }

                if (check) valueRemove.Add(u);
            }

            foreach (string i in valueRemove)
            {
                unique.Remove(i);
            }

            source = unique;
        }

        private static void ValidateWhitelist(string path, ref List<string> blackList)
        {
            foreach (string t in blackList.ToList())
            {
                if (path.Equals(t)) blackList.Remove(t);
            }
        }

        private static bool EqualPath(FileSystemInfo info, string str)
        {
            string relativePath = info.FullName;
            if (relativePath.StartsWith(Application.dataPath.Replace('/', '\\'))) relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            relativePath = relativePath.Replace('\\', '/');
            return str.Equals(relativePath);
        }

        private static bool IsChildOfPath(string child, string parent)
        {
            if (child.Equals(parent)) return false;
            var allParent = new List<DirectoryInfo>();
            GetAllParentDirectories(new DirectoryInfo(child), ref allParent);

            foreach (var p in allParent)
            {
                bool check = EqualPath(p, parent);
                if (check) return true;
            }

            return false;
        }

        private static void GetAllParentDirectories(DirectoryInfo directoryToScan, ref List<DirectoryInfo> directories)
        {
            while (true)
            {
                if (directoryToScan == null || directoryToScan.Name == directoryToScan.Root.Name || !directoryToScan.FullName.Contains("Assets")) return;

                directories.Add(directoryToScan);
                directoryToScan = directoryToScan.Parent;
            }
        }
    }
}