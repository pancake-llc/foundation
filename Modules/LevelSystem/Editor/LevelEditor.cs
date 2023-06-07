using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.ExLibEditor;
using PancakeEditor;
using UnityEditor;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;

namespace Pancake.LevelSystemEditor
{
    internal class LevelEditor : EditorWindow
    {
        private class PickObject
        {
            public string group;
            public GameObject pickedObject;
        }

        #region preview generator

        private static PreviewGenerator previewGenerator;

        private static PreviewGenerator PreviewGenerator
        {
            get
            {
                var generator = previewGenerator;
                if (generator != null) return generator;

                return previewGenerator = new PreviewGenerator {width = 512, height = 512, transparentBackground = true, sizingType = PreviewGenerator.ImageSizeType.Fit};
            }
        }

        private static Dictionary<GameObject, Texture2D> previewDict;

        public static void ClearPreviews()
        {
            if (previewDict != null)
            {
                foreach (var kvp in previewDict.ToList())
                {
                    previewDict[kvp.Key] = null;
                }

                previewDict.Clear();
            }
        }

        public static void ClearPreview(GameObject go)
        {
            if (previewDict?.TryGetValue(go, out var tex) ?? false)
            {
                UnityEngine.Object.DestroyImmediate(tex);
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


        private readonly string[] _optionsSpawn = {"Default", "Index", "Custom"};
        private readonly string[] _optionsMode = {"Renderer", "Ignore"};

        private Vector2 _pickObjectScrollPosition;
        private Vector2 _whiteScrollPosition;
        private Vector2 _blackScrollPosition;
        private PickObject _currentPickObject;
        private List<PickObject> _pickObjects;
        private SerializedObject _pathFolderSerializedObject;
        private SerializedProperty _pathFolderProperty;
        private int _selectedSpawn;
        private int _selectedMode;
        private GameObject _rootSpawn;
        private int _rootIndexSpawn;
        private GameObject _previewPickupObject;
        private UnityEngine.Object _previousObjectInpectorPreview;
        private UnityEditor.Editor _editorInpsectorPreview;

        private static Vector2 EventMousePoint
        {
            get
            {
                var position = Event.current.mousePosition;
                position.y = Screen.height - position.y - 60f;
                return position;
            }
        }

        private List<PickObject> PickObjects => _pickObjects ??= new List<PickObject>();


        private void OnEnable()
        {
            RefreshPickObject();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable() { SceneView.duringSceneGui -= OnSceneGUI; }


        private void OnProjectChange() { TryClose(); }

        private void OnHierarchyChange() { TryClose(); }

        private bool TryClose() { return false; }

        // ReSharper disable once UnusedMember.Local
        private void RefreshAll()
        {
            LevelEditor.ClearPreviews();
            RefreshPickObject();
            ClearEditor();
        }

        /// <summary>
        /// display picked object in editor
        /// </summary>
        private void RefreshPickObject()
        {
            _pickObjects = new List<PickObject>();
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
                _pickObjects.Add(po);
            }
        }

        private bool CheckEscape()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                _currentPickObject = null;
                Repaint();
                SceneView.RepaintAll();
                return true;
            }

            return false;
        }

        private void OnGUI()
        {
            if (TryClose()) return;
            if (CheckEscape()) return;
            SceneView.RepaintAll();
            InternalDrawDropArea();
            InternalDrawSetting();
            InternalDrawPickupArea();
        }

        private void InternalDrawDropArea()
        {
            Uniform.DrawGroupFoldout("level_editor_drop_area", "Drop Are", DrawDropArea);

            void DrawDropArea()
            {
                float width = 0;
                var @event = Event.current;
                Uniform.Horizontal(() =>
                {
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

                                        ValidateWhitelist(path, ref levelEditorSettings.Settings.blacklistPaths);
                                        AddToWhitelist(path);
                                    }

                                    InEditor.ReduceScopeDirectory(ref levelEditorSettings.Settings.whitelistPaths);
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

                                    InEditor.ReduceScopeDirectory(ref levelEditorSettings.Settings.blacklistPaths);
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
                });

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
            }

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
                            RefreshAll();
                        });
                });
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
            foreach (string whitePath in ScriptableLevelSystemSetting.Instance.whitelistPaths)
            {
                if (InEditor.IsChildOfPath(path, whitePath)) check = true;
            }

            if (!check) ScriptableLevelSystemSetting.Instance.whitelistPaths.Add(path);
            ScriptableLevelSystemSetting.Instance.whitelistPaths = ScriptableLevelSystemSetting.Instance.whitelistPaths.Distinct().ToList(); //unique
        }

        private void AddToBlacklist(string path)
        {
            var check = false;
            foreach (string blackPath in ScriptableLevelSystemSetting.Instance.blacklistPaths)
            {
                if (InEditor.IsChildOfPath(path, blackPath)) check = true;
            }

            if (!check) ScriptableLevelSystemSetting.Instance.blacklistPaths.Add(path);
            ScriptableLevelSystemSetting.Instance.blacklistPaths = ScriptableLevelSystemSetting.Instance.blacklistPaths.Distinct().ToList(); //unique
        }

        private void InternalDrawSetting()
        {
            Uniform.DrawGroupFoldout("level_editor_config", "Setting", DrawSetting);

            void DrawSetting()
            {
                _selectedSpawn = EditorGUILayout.Popup("Where Spawn", _selectedSpawn, _optionsSpawn);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (_optionsSpawn[_selectedSpawn].ToLower())
                    {
                        case "default":
                            break;
                        case "index":
                            var currentPrefabState = GetCurrentPrefabStage();
                            if (currentPrefabState != null)
                            {
                                _rootIndexSpawn = EditorGUILayout.IntField(new GUIContent("Index spawn", "Index from root stage contex"), _rootIndexSpawn);
                            }
                            else
                            {
                                EditorGUILayout.HelpBox("Index spawn only work in PrefabMode!", MessageType.Warning);
                            }

                            break;
                        case "custom":
                            _rootSpawn = (GameObject) EditorGUILayout.ObjectField("Spawn in GO here -->", _rootSpawn, typeof(GameObject), true);
                            break;
                    }
                }

                _selectedMode = EditorGUILayout.Popup("Mode", _selectedMode, _optionsMode);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (_optionsMode[_selectedMode].ToLower())
                    {
                        case "renderer":
                            EditorGUILayout.HelpBox("Based on Renderer detection", MessageType.Info);
                            break;
                        case "ignore":
                            EditorGUILayout.HelpBox("GameObject will be spawn correcty at raycast location\nIgnore calculate bound object", MessageType.Info);
                            break;
                    }
                }
            }
        }

        private void InternalDrawPickupArea()
        {
            float height = 0f;
            Uniform.DrawGroupFoldoutWithRightClick("level_editor_pickup_area", "Pickup Area", DrawPickupArea, ShowMenuRefresh);

            void DrawPickupArea()
            {
                var tex = LevelEditor.GetPreview(_currentPickObject?.pickedObject);
                if (tex)
                {
                    string pickObjectName = _currentPickObject?.pickedObject.name;
                    Uniform.Horizontal(() =>
                    {
                        GUILayout.Space(position.width / 2 - 50);
                        if (_editorInpsectorPreview == null || _previousObjectInpectorPreview != _currentPickObject?.pickedObject)
                        {
                            _editorInpsectorPreview = UnityEditor.Editor.CreateEditor(_currentPickObject?.pickedObject);
                        }

                        var rect = GUILayoutUtility.GetLastRect();
                        _editorInpsectorPreview.DrawPreview(new Rect(new Vector2(position.width / 2 - 50, rect.position.y), new Vector2(100, 100)));
                        _previousObjectInpectorPreview = _currentPickObject?.pickedObject;
                        GUI.color = new Color(1, 1, 1, 0f);
                        if (GUILayout.Button(tex, GUILayout.Height(80), GUILayout.Width(80)))
                        {
                        }

                        GUI.color = Color.white;
                    });
                    EditorGUILayout.LabelField($"Selected: <color=#80D2FF>{pickObjectName}</color>\nPress Icon Again Or Escape Key To Deselect",
                        Uniform.HtmlText,
                        GUILayout.Height(40));
                    height -= 128;
                    EditorGUILayout.HelpBox("Shift + Click To Add", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Select An Object First", MessageType.Info);
                }

                height -= 100;
                if (Uniform.GetFoldoutState("level_editor_drop_area"))
                {
                    if (levelEditorSettings.Settings.blacklistPaths.Count == 0 && levelEditorSettings.Settings.whitelistPaths.Count == 0)
                    {
                        height -= 94;
                    }
                    else
                    {
                        height -= 342;
                    }
                }
                else
                {
                    height -= 33;
                }

                if (Uniform.GetFoldoutState("LEVEL_EDITOR_CONFIG"))
                {
                    switch (_optionsSpawn[_selectedSpawn].ToLower())
                    {
                        case "default":
                            height -= 122;
                            break;
                        case "index":
                            var currentPrefabState = GetCurrentPrefabStage();
                            if (currentPrefabState != null)
                            {
                                height -= 146;
                            }
                            else
                            {
                                height -= 162;
                            }

                            break;
                        case "custom":
                            height -= 146;
                            break;
                    }
                }
                else
                {
                    height -= 33;
                }

                var h = position.height + height;

                _pickObjectScrollPosition = GUILayout.BeginScrollView(_pickObjectScrollPosition, GUILayout.Height(h));
                var resultSplitGroupObjects = PickObjects.GroupBy(_ => _.group).Select(_ => _.ToList()).ToList();
                foreach (var splitGroupObject in resultSplitGroupObjects)
                {
                    string nameGroup = splitGroupObject[0].group.ToUpper();
                    Uniform.DrawGroupFoldout($"LEVEL_EDITOR_PICKUP_AREA_CHILD_{nameGroup}", nameGroup, () => DrawInGroup(splitGroupObject));
                }

                GUILayout.EndScrollView();
            }

            void DrawInGroup(IReadOnlyList<PickObject> pickObjectsInGroup)
            {
                const int spacing = 25;
                var counter = 0;
                CalculateIdealCount(position.width - 50,
                    60,
                    135,
                    spacing,
                    5,
                    out int count,
                    out float size);
                count = Mathf.Max(1, count);
                while (counter >= 0 && counter < pickObjectsInGroup.Count)
                {
                    EditorGUILayout.BeginHorizontal();
                    Uniform.SpaceTwoLine();
                    for (var x = 0; x < count; x++)
                    {
                        var pickObj = pickObjectsInGroup[counter];
                        var go = pickObj.pickedObject;
                        var tex = LevelEditor.GetPreview(go);
                        if (pickObj == _currentPickObject)
                        {
                            GUI.color = new Color32(79, 213, 255, 255);
                        }
                        else
                        {
                            GUI.color = Color.white;
                        }

                        Uniform.Button("",
                            () =>
                            {
                                if (Event.current.button == 1)
                                {
                                    ShowMenuRightClickItem(pickObj);
                                    return;
                                }

                                _currentPickObject = _currentPickObject == pickObj ? null : pickObj;
                            },
                            null,
                            GUILayout.Width(size),
                            GUILayout.Height(size));

                        GUI.color = Color.white;
                        var rect = GUILayoutUtility.GetLastRect();
                        if (tex) GUI.DrawTexture(rect.Grown(-10), tex, ScaleMode.ScaleToFit);
                        if (go)
                        {
                            EditorGUI.LabelField(rect.Grown(new Vector2(0, 15)), go.name, new GUIStyle(EditorStyles.miniLabel) {alignment = TextAnchor.LowerCenter,});
                        }

                        counter++;
                        if (counter >= pickObjectsInGroup.Count) break;
                        Uniform.SpaceTwoLine();
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(spacing);
                }
            }

            void ShowMenuRefresh()
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Refresh Pickup  Area"),
                    false,
                    () =>
                    {
                        _currentPickObject = null;
                        RefreshAll();
                    });
                menu.ShowAsContext();
            }

            void ShowMenuRightClickItem(PickObject pickObj)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Ignore"), false, () => IgnorePath(pickObj));
                menu.AddItem(new GUIContent("Ping"),
                    false,
                    () =>
                    {
                        Selection.activeObject = pickObj.pickedObject;
                        EditorGUIUtility.PingObject(pickObj.pickedObject);
                    });
                menu.ShowAsContext();
            }

            void IgnorePath(PickObject pickObj)
            {
                var path = AssetDatabase.GetAssetPath(pickObj.pickedObject);
                ValidateBlacklist(path, ref levelEditorSettings.Settings.whitelistPaths);
                AddToBlacklist(path);

                InEditor.ReduceScopeDirectory(ref levelEditorSettings.Settings.blacklistPaths);
                levelEditorSettings.SaveSetting();
                RefreshAll();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (TryClose()) return;
            if (CheckEscape()) return;
            TryFakeRender(sceneView);
        }

        private void TryFakeRender(SceneView sceneView)
        {
            var e = Event.current;
            if (!e.shift)
            {
                if (_previewPickupObject != null) DestroyImmediate(_previewPickupObject);
                return;
            }

            if (_currentPickObject == null || !_currentPickObject.pickedObject) return;
            Vector3 mousePosition;
            Vector3 normal;
            if (sceneView.in2DMode)
            {
                bool state = InEditor.Get2DMouseScenePosition(out var mousePosition2d);
                mousePosition = mousePosition2d;
                if (!state) return;
                InEditor.FakeRenderSprite(_currentPickObject.pickedObject, mousePosition, Vector3.one, Quaternion.identity);
                SceneView.RepaintAll();

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    AddPickObject(_currentPickObject, mousePosition);
                    InEditor.SkipEvent();
                }
            }
            else
            {
                var pos = sceneView.GetInnerGuiPosition();
                RaycastHit? raycastHit;
                if (pos.Contains(e.mousePosition))
                {
                    var currentPrefabState = GetCurrentPrefabStage();
                    if (currentPrefabState != null)
                    {
                        var (mouseCast, hitInfo) = RaycastPoint(GetParent(), EventMousePoint);
                        mousePosition = mouseCast;
                        normal = hitInfo.HasValue ? hitInfo.Value.normal : Vector3.up;
                        raycastHit = hitInfo;
                    }
                    else
                    {
                        Probe.Pick(ProbeFilter.Default,
                            sceneView,
                            e.mousePosition,
                            out mousePosition,
                            out normal);
                        raycastHit = null;
                    }

                    float discSize = HandleUtility.GetHandleSize(mousePosition) * 0.4f;
                    Handles.color = new Color(1, 0, 0, 0.5f);
                    Handles.DrawSolidDisc(mousePosition, normal, discSize * 0.5f);

                    if (!_previewPickupObject)
                    {
                        _previewPickupObject = Instantiate(_currentPickObject?.pickedObject);
                        StageUtility.PlaceGameObjectInCurrentStage(_previewPickupObject);
                        _previewPickupObject.hideFlags = HideFlags.HideAndDontSave;
                        _previewPickupObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }

#pragma warning disable CS8321
                    void SetPosition2()
                    {
                        var rendererAttach = _currentPickObject?.pickedObject.GetComponentInChildren<Renderer>();
                        if (raycastHit == null || rendererAttach == null) return;
                        var rendererOther = raycastHit.Value.collider.transform.GetComponentInChildren<Renderer>();
                        if (rendererOther == null) return;
                        _previewPickupObject.transform.position = GetSpawnPosition(rendererAttach, rendererOther, raycastHit.Value);
                    }
#pragma warning restore CS8321

                    void SetPosition()
                    {
                        _previewPickupObject.transform.position = mousePosition;

                        switch (_optionsMode[_selectedMode].ToLower())
                        {
                            case "renderer":
                                if (_previewPickupObject.CalculateBounds(out var bounds,
                                        Space.World,
                                        true,
                                        false,
                                        false,
                                        false))
                                {
                                    float difference = 0;

                                    if (normal == Vector3.up || normal == Vector3.down)
                                    {
                                        difference = mousePosition.y - bounds.min.y;
                                    }
                                    else if (normal == Vector3.right || normal == Vector3.left)
                                    {
                                        difference = mousePosition.x - bounds.min.x;
                                    }
                                    else if (normal == Vector3.forward || normal == Vector3.back)
                                    {
                                        difference = mousePosition.z - bounds.min.z;
                                    }

                                    _previewPickupObject.transform.position += difference * normal;
                                }

                                break;
                            case "ignore":
                                break;
                        }
                    }

                    SetPosition();

                    if (e.type == EventType.MouseDown && e.button == 0 && _previewPickupObject)
                    {
                        AddPickObject(_currentPickObject, _previewPickupObject.transform.position);
                        InEditor.SkipEvent();
                    }
                }
            }
        }

        /// <summary>
        /// only use when determined root
        /// </summary>
        /// <param name="root"></param>
        /// <param name="ray"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private (bool, RaycastHit?) RayCast(Component root, Ray ray, out Vector3 point)
        {
            point = Vector3.zero;
            if (root.gameObject.scene.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
            {
                point = hit.point;
                return (true, hit);
            }

            return (false, null);
        }

        /// <summary>
        /// only use when determined root
        /// </summary>
        /// <param name="root"></param>
        /// <param name="screenPoint"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private (Vector3, RaycastHit?) RaycastPoint(Component root, Vector2 screenPoint, float distance = 20)
        {
            var ray = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(screenPoint);
            var result = RayCast(root, ray, out var point);
            if (!result.Item1)
            {
                point = ray.origin + ray.direction.normalized * distance;
            }

            return (point, result.Item2);
        }

        /// <summary>
        /// for mesh with irregular shape the returned result is incorrect
        /// missing some direction
        /// </summary>
        /// <param name="rendererAttach"></param>
        /// <param name="rendererOther"></param>
        /// <param name="hitInfo"></param>
        /// <returns></returns>
        private Vector3 GetSpawnPosition(Renderer rendererAttach, Renderer rendererOther, RaycastHit hitInfo)
        {
            var boundsAttach = rendererAttach.bounds;
            var boundsOther = rendererOther.bounds;

            var otherPos = hitInfo.collider.gameObject.transform.position;
            var pointPos = hitInfo.point;

            int isSpawnRighSide;
            if (Mathf.Abs(otherPos.x - pointPos.x) >= boundsOther.size.x / 2)
            {
                isSpawnRighSide = otherPos.x > pointPos.x ? -1 : 1;
            }
            else
            {
                isSpawnRighSide = 0;
            }

            int isSpawnUpSide;
            if (Mathf.Abs(otherPos.y - pointPos.y) >= boundsOther.size.y / 2)
            {
                isSpawnUpSide = otherPos.y > pointPos.y ? -1 : 1;
            }
            else
            {
                isSpawnUpSide = 0;
            }

            int isSpawnForwardSide;
            if (Mathf.Abs(otherPos.z - pointPos.z) >= boundsOther.size.z / 2)
            {
                isSpawnForwardSide = otherPos.z > pointPos.z ? -1 : 1;
            }
            else
            {
                isSpawnForwardSide = 0;
            }

            return new Vector3(hitInfo.point.x + (boundsAttach.size.x / 2 * isSpawnRighSide),
                hitInfo.point.y + (boundsAttach.size.y / 2 * isSpawnUpSide),
                hitInfo.point.z + (boundsAttach.size.z / 2 * isSpawnForwardSide));
        }

        /// <summary>
        /// Spawn pickup object
        /// </summary>
        /// <param name="pickObject"></param>
        /// <param name="worldPos"></param>
        private void AddPickObject(PickObject pickObject, Vector3 worldPos)
        {
            if (pickObject?.pickedObject)
            {
                var inst = pickObject.pickedObject.Instantiate(GetParent());
                inst.transform.position = worldPos;
                Undo.RegisterCreatedObjectUndo(inst.gameObject, "Create pick obj");
                Selection.activeObject = inst;
            }
        }

        private Transform GetParent()
        {
            Transform parent = null;
            var currentPrefabState = GetCurrentPrefabStage();

            if (currentPrefabState != null)
            {
                var prefabRoot = currentPrefabState.prefabContentsRoot.transform;
                switch (_optionsSpawn[_selectedSpawn].ToLower())
                {
                    case "default":
                        parent = prefabRoot;
                        break;
                    case "index":
                        if (_rootIndexSpawn < 0) parent = prefabRoot;
                        else if (prefabRoot.childCount - 1 > _rootIndexSpawn) parent = prefabRoot.GetChild(_rootIndexSpawn);
                        else parent = prefabRoot;
                        break;
                    case "custom":
                        parent = _rootSpawn ? _rootSpawn.transform : prefabRoot;
                        break;
                }
            }
            else
            {
                switch (_optionsSpawn[_selectedSpawn].ToLower())
                {
                    case "default":
                    case "index":
                        parent = null;
                        break;
                    case "custom":
                        parent = _rootSpawn ? _rootSpawn.transform : null;
                        break;
                }
            }

            return parent;
        }

        private PrefabStage GetCurrentPrefabStage() { return PrefabStageUtility.GetCurrentPrefabStage(); }

        /// <summary>
        /// Calculate count item pickup can display
        /// </summary>
        /// <param name="availableSpace"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <param name="spacing"></param>
        /// <param name="defaultCount"></param>
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static bool CalculateIdealCount(float availableSpace, float minSize, float maxSize, float spacing, int defaultCount, out int count, out float size)
        {
            float halfSpacing = spacing / 2f;
            int minCount = Mathf.FloorToInt(availableSpace / (maxSize + halfSpacing));
            int maxCount = Mathf.FloorToInt(availableSpace / (minSize + halfSpacing));
            bool goodness = defaultCount >= minCount && defaultCount <= maxCount;
            count = Mathf.Clamp(defaultCount, minCount, maxCount);
            size = (availableSpace - halfSpacing * (count - 1) - (count - 1) * (count / 10f)) / count;
            return goodness;
        }

        private void ClearEditor() { Repaint(); }
    }
}