using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    /// <summary>
    /// add filter
    /// </summary>
    internal class AssetContainerWindow : EditorWindow
    {
        private AssetContainer _provider;
        private Vector2 _scroll;
        private string _pathFolderProperty;
        private string _dataPath;
        private const float DROP_AREA_HEIGHT_FOLDOUT = 110f;
        private const float DEFAULT_HEADER_HEIGHT = 30f;
        private float _height;
        private Dictionary<SerializedProperty, SerializedProperty> _cache = new Dictionary<SerializedProperty, SerializedProperty>();
        private SerializedObject _serializedObject;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static InEditor.ProjectSetting<AssetContainerSetting> assetContainerSettings =
            new InEditor.ProjectSetting<AssetContainerSetting>("AssetContainerSettings");

        [MenuItem("Tools/Pancake/Asset Container &_4")]
        internal static void ShowWindow()
        {
            string resourcePath = InEditor.DefaultResourcesPath();
            if (!$"{resourcePath}/AssetContainer.asset".FileExists())
            {
                var instance = UnityEngine.ScriptableObject.CreateInstance<AssetContainer>();
                AssetDatabase.CreateAsset(instance, $"{resourcePath}/AssetContainer.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GetWindow<AssetContainerWindow>("Asset Container", true, InEditor.InspectorWindow).Show();
        }

        private void OnEnable()
        {
            _provider = Resources.Load<AssetContainer>("AssetContainer");
            _serializedObject = new SerializedObject(_provider);
            RefreshAssetEntries();
        }

        private void OnGUI()
        {
            _serializedObject.Update();
            AssetContainerEditor.callFromEditorWindow = true;
            _height = 0;
            Uniform.SpaceTwoLine();
            SceneView.RepaintAll();
            InternalDrawDropArea();
            Uniform.SpaceOneLine();
            DrawAssets();
            Uniform.SpaceTwoLine();
            _serializedObject.ApplyModifiedProperties();
            AssetContainerEditor.callFromEditorWindow = false;
        }

        private void DrawAssets()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            GUI.enabled = false;

            foreach (var property in _cache.ToArray())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property.Key, new GUIContent(""), GUILayout.Width(280));
                EditorGUILayout.PropertyField(property.Value, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
            }

            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }


        private void InternalDrawDropArea()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            Uniform.DrawUppercaseSectionWithRightClick("ASSET_CONTAINER_DROP_AREA", "DROP AREA", DrawDropArea, ShowMenuRefresh);

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
                                        ValidateBlacklist(path, ref assetContainerSettings.Settings.whitelistPaths);
                                        AddToBlacklist(path);
                                    }

                                    InEditor.ReduceScopeDirectory(ref assetContainerSettings.Settings.blacklistPaths);
                                    assetContainerSettings.SaveSetting();
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
                                        assetContainerSettings.Settings.whitelistPaths.Clear();
                                        assetContainerSettings.SaveSetting();
                                        RefreshAll();
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
                                        RefreshAll();
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

            void ShowMenuRefresh()
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Refresh"), false, RefreshAll);
                menu.ShowAsContext();
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
                            RefreshAll();
                        });
                });
            }
        }

        /// <summary>
        /// display picked object in editor
        /// </summary>
        private void RefreshAssetEntries()
        {
            _cache = new Dictionary<SerializedProperty, SerializedProperty>();
            _provider.assetEntries = Array.Empty<AssetEntry>();
            var blacklistAssets = new List<Object>();
            var whitelistAssets = new List<Object>();

            if (!assetContainerSettings.Settings.blacklistPaths.IsNullOrEmpty())
            {
                blacklistAssets = AssetDatabase.FindAssets("t:Object", assetContainerSettings.Settings.blacklistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<Object>)
                    .Where(_ =>
                    {
                        var fileNamespace = _.GetType().Namespace;
                        return _ != null && (fileNamespace == null || !fileNamespace.Contains("UnityEditor"));
                    })
                    .ToList();

                foreach (string blacklistPath in assetContainerSettings.Settings.blacklistPaths)
                {
                    if (File.Exists(blacklistPath)) blacklistAssets.Add(AssetDatabase.LoadAssetAtPath<Object>(blacklistPath));
                }
            }

            if (!assetContainerSettings.Settings.whitelistPaths.IsNullOrEmpty())
            {
                whitelistAssets = AssetDatabase.FindAssets("t:Object", assetContainerSettings.Settings.whitelistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<Object>)
                    .Where(_ =>
                    {
                        var pathObj = AssetDatabase.GetAssetPath(_);
                        pathObj = pathObj.Replace('\\', '/');
                        string[] fs = pathObj.Split('/');
                        var fileNamespace = _.GetType().Namespace;
                        return _ != null && (fileNamespace == null || !fileNamespace.Contains("UnityEditor")) && !fs.Contains("Editor");
                    })
                    .ToList();

                foreach (string whitelistPath in assetContainerSettings.Settings.whitelistPaths)
                {
                    if (File.Exists(whitelistPath) && !whitelistAssets.Contains(AssetDatabase.LoadAssetAtPath<Object>(whitelistPath)))
                        whitelistAssets.Add(AssetDatabase.LoadAssetAtPath<Object>(whitelistPath));
                }
            }

            var resultAssets = whitelistAssets.Where(_ => !blacklistAssets.Contains(_)).ToList();
            var newEntries = new List<AssetEntry>();
            foreach (var o in resultAssets)
            {
                var path = AssetDatabase.GetAssetPath(o);
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (!_provider.TryGetValue(o, out _)) newEntries.Add(new AssetEntry(guid, o));

                var childAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(o));

                foreach (var child in childAssets)
                {
                    if (_provider.TryGetValue(child, out _)) continue;

                    if (!assetContainerSettings.Settings.entities.Exists(_ => _.name == $"{guid}_{child.name}"))
                    {
                        var childGuid = Ulid.NewUlid().ToString().ToLower();
                        assetContainerSettings.Settings.entities.Add(new AssetContainerSetting.SubEntityId {name = $"{guid}_{child.name}", guid = childGuid});
                        newEntries.Add(new AssetEntry(childGuid, child));
                        assetContainerSettings.SaveSetting();
                    }
                    else
                    {
                        newEntries.Add(
                            new AssetEntry(assetContainerSettings.Settings.entities.Filter(_ => _.name == $"{guid}_{child.name}").FirstOrDefault().guid, child));
                    }
                }
            }

            ArrayUtility.AddRange(ref _provider.assetEntries, newEntries.ToArray());
            EditorUtility.SetDirty(_provider);
            AssetDatabase.SaveAssetIfDirty(_provider);

            EditorCoroutine.Start(DelayUpdateRender());
        }

        private IEnumerator DelayUpdateRender()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            var objectsProperty = _serializedObject.FindProperty("assetEntries");
            for (int i = 0; i < objectsProperty.arraySize; i++)
            {
                var prop = objectsProperty.GetArrayElementAtIndex(i);
                _cache.Add(prop.FindPropertyRelative("guid"), prop.FindPropertyRelative("asset"));
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void RefreshAll()
        {
            RefreshAssetEntries();
            Repaint();
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
            assetContainerSettings.Settings.whitelistPaths = Enumerable.Distinct(assetContainerSettings.Settings.whitelistPaths).ToList(); //unique
        }

        private void AddToBlacklist(string path)
        {
            var check = false;
            foreach (string blackPath in assetContainerSettings.Settings.blacklistPaths)
            {
                if (InEditor.IsChildOfPath(path, blackPath)) check = true;
            }

            if (!check) assetContainerSettings.Settings.blacklistPaths.Add(path);
            assetContainerSettings.Settings.blacklistPaths = Enumerable.Distinct(assetContainerSettings.Settings.blacklistPaths).ToList(); //unique
        }
    }
}