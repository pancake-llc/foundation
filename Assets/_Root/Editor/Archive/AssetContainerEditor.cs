using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    internal class AssetContainerEditor : EditorWindow
    {
        private AssetContainer _provider;
        private Vector2 _scroll;
        private List<AssetEntry> _assetEntries;
        private string _pathFolderProperty;
        private string _dataPath;
        private const float DROP_AREA_HEIGHT_FOLDOUT = 110f;
        private const float DEFAULT_HEADER_HEIGHT = 30f;
        private float _height;

        private List<AssetEntry> AssetEntries
        {
            get
            {
                if (_assetEntries == null) _assetEntries = new List<AssetEntry>();
                return _assetEntries;
            }
        }

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static InEditor.ProjectSetting<PathSetting> assetContainerSettings = new InEditor.ProjectSetting<PathSetting>("AssetContainerSettings");

        [MenuItem("Tools/Pancake/Asset Container")]
        private static void ShowWindow() { GetWindow<AssetContainerEditor>("Asset Container").Show(); }

        private void OnEnable() { _provider = Resources.Load<AssetContainer>("AssetContainer"); }

        private void OnGUI()
        {
            var obj = new SerializedObject(_provider);
            obj.Update();
            var objectsProperty = obj.FindProperty("savedAssets");
            _height = 0;
            Uniform.SpaceTwoLine();
            SceneView.RepaintAll();
            InternalDrawDropArea();
            Uniform.SpaceOneLine();
            InternalDrawAsset();
            obj.ApplyModifiedProperties();
        }

        private void InternalDrawAsset()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            _height -= DEFAULT_HEADER_HEIGHT;
            _height -= 18f;
            //Uniform.DrawUppercaseSectionWithRightClick("ASSET_CONTAINER_ASSET_AREA", "ASSET AREA", DrawPickupArea, ShowMenuRefresh);

            void DrawPickupArea()
            {
                _height += position.height;
                _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(_height));
                //var resultSplitGroupObjects = PickObjects.GroupBy(_ => _.group).Select(_ => _.ToList()).ToList();
                // foreach (var splitGroupObject in resultSplitGroupObjects)
                // {
                //     string nameGroup = splitGroupObject[0].group.ToUpper();
                //     _height -= DEFAULT_HEADER_HEIGHT;
                //     Uniform.DrawUppercaseSection($"LEVEL_EDITOR_PICKUP_AREA_CHILD_{nameGroup}", nameGroup, () => DrawInGroup(splitGroupObject));
                // }

                GUILayout.EndScrollView();
            }

            GUI.enabled = false;
            //EditorGUILayout.PropertyField(objectsProperty, true);
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void InternalDrawDropArea()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            Uniform.DrawUppercaseSection("ASSET_CONTAINER_DROP_AREA", "DROP AREA", DrawDropArea);

            void DrawDropArea()
            {
                _height -= DROP_AREA_HEIGHT_FOLDOUT - DEFAULT_HEADER_HEIGHT;
                GUILayout.Space(2);
                float width = 0;
                var @event = Event.current;
                Uniform.Horizontal(() =>
                {
                    var whiteArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
                    var blackArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
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
                                        ValidateWhitelist(path, ref assetContainerSettings.Settings.blacklistPaths);
                                        AddToWhitelist(path);
                                    }

                                    InEditor.ReduceScopeDirectory(ref assetContainerSettings.Settings.whitelistPaths);
                                    assetContainerSettings.SaveSetting();
                                    Repaint();
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
                                        ValidateBlacklist(path, ref assetContainerSettings.Settings.whitelistPaths);
                                        AddToBlacklist(path);
                                    }

                                    InEditor.ReduceScopeDirectory(ref assetContainerSettings.Settings.blacklistPaths);
                                    assetContainerSettings.SaveSetting();
                                    Repaint();
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
                                        assetContainerSettings.Settings.whitelistPaths.Clear();
                                        assetContainerSettings.SaveSetting();
                                        Repaint();
                                    });
                            }
                            else if (blackArea.Contains(@event.mousePosition))
                            {
                                menu.AddItem(new GUIContent("Clear All [BLACK LIST]"),
                                    false,
                                    () =>
                                    {
                                        assetContainerSettings.Settings.blacklistPaths.Clear();
                                        assetContainerSettings.SaveSetting();
                                        Repaint();
                                    });
                            }

                            menu.ShowAsContext();
                            break;
                    }
                });

                Uniform.Horizontal(() =>
                {
                    Uniform.VerticalScope(() =>
                        {
                            if (assetContainerSettings.Settings.whitelistPaths.Count == 0)
                            {
                                EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                            }
                            else
                            {
                                foreach (string t in assetContainerSettings.Settings.whitelistPaths.ToList())
                                {
                                    _height -= 18;
                                    DrawRow(t, width, _ => assetContainerSettings.Settings.whitelistPaths.Remove(_));
                                }
                            }
                        },
                        GUILayout.Width(width - 10));
                    Uniform.SpaceOneLine();
                    Uniform.VerticalScope(() =>
                        {
                            if (assetContainerSettings.Settings.blacklistPaths.Count == 0)
                            {
                                EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                            }
                            else
                            {
                                foreach (string t in assetContainerSettings.Settings.blacklistPaths.ToList())
                                {
                                    DrawRow(t, width, _ => assetContainerSettings.Settings.blacklistPaths.Remove(_));
                                }
                            }
                        },
                        GUILayout.Width(width - 15));
                });
            }

            void DrawRow(string content, float width, Action<string> action)
            {
                Uniform.Horizontal(() =>
                {
                    EditorGUILayout.LabelField(new GUIContent(content), GUILayout.Width(width - 80));
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
                            assetContainerSettings.SaveSetting();
                            Repaint();
                        });
                });
            }
        }

        /// <summary>
        /// display picked object in editor
        /// </summary>
        private void RefreshAssetEntries()
        {
            _provider.AssetGuids = Array.Empty<string>();
            EditorUtility.SetDirty(_provider);

            foreach (string whitepath in assetContainerSettings.Settings.whitelistPaths)
            {
                MakeGroupPrefab(whitepath);

                if (!Directory.Exists(whitepath)) continue;
                var directories = new List<string>();
                InEditor.GetAllChildDirectories(whitepath, ref directories);

                foreach (string directory in directories)
                {
                    string dir = directory.Replace('\\', '/');
                    MakeGroupPrefab(dir);
                }
            }

            void MakeGroupPrefab(string whitePath)
            {
                if (!Directory.Exists(whitePath) && !File.Exists(whitePath) || !whitePath.StartsWith("Assets"))
                {
                    Debug.LogWarning("[Asset Container]: Can not found folder '" + whitePath + "'");
                    return;
                }

                var objs = new List<Object>();
                if (File.Exists(whitePath))
                {
                    objs.Add(AssetDatabase.LoadAssetAtPath<Object>(whitePath));
                }
                else
                {
                    var removeList = new List<string>();
                    var nameFileExclude = new List<string>();

                    foreach (string blackPath in assetContainerSettings.Settings.blacklistPaths)
                    {
                        if (InEditor.IsChildOfPath(blackPath, whitePath)) removeList.Add(blackPath);
                    }

                    if (removeList.Contains(whitePath) || assetContainerSettings.Settings.blacklistPaths.Contains(whitePath)) return;

                    foreach (string str in removeList)
                    {
                        if (File.Exists(str)) nameFileExclude.Add(Path.GetFileNameWithoutExtension(str));
                    }

                    objs = InEditor.FindAllAssetsWithPath<Object>(whitePath.Replace(Application.dataPath, "").Replace("Assets/", ""))
                        .Where(lo => !(lo is null) && !nameFileExclude.Exists(_ => _.Equals(lo.name)))
                        .ToList();
                }

                string group = whitePath.Split('/').Last();
                if (File.Exists(whitePath))
                {
                    var pathInfo = new DirectoryInfo(whitePath);
                    if (pathInfo.Parent != null) group = pathInfo.Parent.Name;
                }

                var newEntries = new List<string>();
                foreach (var obj in objs)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                    var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);

                    if (!TryGetValue(guid, out _))
                    {
                        newEntries.Add(guid);
                        var po = new AssetEntry {group = group, asset = obj, guid = guid};
                        AssetEntries.Add(po);
                    }
                }

                _provider.AssetGuids = newEntries.ToArray();
                UnityEditor.EditorUtility.SetDirty(_provider);
            }

            bool TryGetValue(string s, out string entry)
            {
                foreach (var str in _provider.AssetGuids)
                {
                    if (str != s) continue;

                    entry = str;
                    return true;
                }

                entry = null;
                return false;
            }
        }

        private void ValidateWhitelist(string path, ref List<string> blackList)
        {
            foreach (string t in blackList.ToList())
            {
                if (path.Equals(t)) blackList.Remove(t);
            }
        }

        private void ValidateBlacklist(string path, ref List<string> whiteList)
        {
            foreach (string t in whiteList.ToList())
            {
                if (path.Equals(t) || InEditor.IsChildOfPath(t, path)) whiteList.Remove(t);
            }
        }

        private void AddToWhitelist(string path)
        {
            var check = false;
            foreach (string whitePath in assetContainerSettings.Settings.whitelistPaths)
            {
                if (InEditor.IsChildOfPath(path, whitePath)) check = true;
            }

            if (!check) assetContainerSettings.Settings.whitelistPaths.Add(path);
            assetContainerSettings.Settings.whitelistPaths = assetContainerSettings.Settings.whitelistPaths.Distinct().ToList(); //unique
        }

        private void AddToBlacklist(string path)
        {
            var check = false;
            foreach (string blackPath in assetContainerSettings.Settings.blacklistPaths)
            {
                if (InEditor.IsChildOfPath(path, blackPath)) check = true;
            }

            if (!check) assetContainerSettings.Settings.blacklistPaths.Add(path);
            assetContainerSettings.Settings.blacklistPaths = assetContainerSettings.Settings.blacklistPaths.Distinct().ToList(); //unique
        }

        // private void DrawPaths(SerializedProperty pathsProperty)
        // {
        //     var selectContent = new GUIContent("Select Path");
        //     var removeContent = new GUIContent("x");
        //
        //     for (int i = 0; i < pathsProperty.arraySize; i++)
        //     {
        //         EditorGUILayout.BeginHorizontal();
        //
        //         var element = pathsProperty.GetArrayElementAtIndex(i);
        //         EditorGUILayout.PropertyField(element, new GUIContent($"Path {i.ToString()}"));
        //
        //         if (GUILayout.Button(selectContent, EditorStyles.miniButtonLeft, GUILayout.Width(75f)))
        //         {
        //             string path = EditorUtility.OpenFolderPanel("Select path", "Assets", "");
        //
        //             if (path != "Assets" && !string.IsNullOrEmpty(path))
        //                 path = path.Substring(path.IndexOf("Assets"));
        //
        //             if (AssetDatabase.IsValidFolder(path))
        //                 element.stringValue = path;
        //             else
        //                 pathsProperty.DeleteArrayElementAtIndex(i);
        //         }
        //
        //         if (GUILayout.Button(removeContent, EditorStyles.miniButtonLeft, GUILayout.Width(30f)))
        //             pathsProperty.DeleteArrayElementAtIndex(i);
        //
        //         EditorGUILayout.EndHorizontal();
        //     }
        //
        //     GUILayout.Space(10f);
        //
        //     if (GUILayout.Button("Add Path", GUILayout.Height(30f)))
        //         pathsProperty.InsertArrayElementAtIndex(pathsProperty.arraySize);
        // }

        // private void DrawButtons()
        // {
        //     EditorGUILayout.BeginHorizontal();
        //
        //     if (GUILayout.Button("Load assets at paths"))
        //         _provider.LoadAssets();
        //
        //     if (GUILayout.Button("Remove assets from container"))
        //     {
        //         if (EditorUtility.DisplayDialog("Clear", "Do you really want to remove all referenced assets?", "Yes", "No"))
        //             _provider.Clear();
        //     }
        //
        //     EditorGUILayout.EndHorizontal();
        //     GUILayout.Space(15f);
        // }
    }
}