using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Linq;
using UnityEditor;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine;

namespace Pancake.Editor.LevelEditor
{
    internal class LevelEditor : EditorWindow
    {
        private readonly string[] _optionsSpawn = {"Default", "Index", "Custom"};
        private readonly string[] _optionsMode = {"Renderer", "Ignore"};

        private Vector2 _pickObjectScrollPosition;
        private PickObject _currentPickObject;
        private List<PickObject> _pickObjects;
        private SerializedObject _pathFolderSerializedObject;
        private SerializedProperty _pathFolderProperty;
        private int _selectedSpawn;
        private int _selectedMode;
        private GameObject _rootSpawn;
        private int _rootIndexSpawn;
        private GameObject _previewPickupObject;
        private string _dataPath;
        private const float DROP_AREA_HEIGHT_FOLDOUT = 110f;
        private const float DEFAULT_HEADER_HEIGHT = 54f;
        private const float SELECTED_OBJECT_PREVIEW_HEIGHT = 100f;
        private float _height;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static InEditor.ProjectSetting<PathSetting> levelEditorSettings = new InEditor.ProjectSetting<PathSetting>("LevelEditorSettings");

        private static Vector2 EventMousePoint
        {
            get
            {
                var position = Event.current.mousePosition;
                position.y = Screen.height - position.y - 60f;
                return position;
            }
        }

        private List<PickObject> PickObjects
        {
            get
            {
                if (_pickObjects == null) _pickObjects = new List<PickObject>();
                return _pickObjects;
            }
        }

        public void Init()
        {
            SceneView.duringSceneGui += GridUpdate;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void OnEnable()
        {
            _dataPath = Application.dataPath.Replace('/', '\\');
            Uniform.LoadFoldoutSetting();
            levelEditorSettings.LoadSetting();
            RefreshPickObject();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= GridUpdate;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            SceneView.duringSceneGui -= OnSceneGUI;

            levelEditorSettings.SaveSetting();
            Uniform.SaveFoldoutSetting();
        }

        private void PlayModeStateChanged(PlayModeStateChange obj) { }

        private void OnProjectChange() { TryClose(); }

        private void OnHierarchyChange() { TryClose(); }

        private void GridUpdate(SceneView sceneView) { }

        private bool TryClose() { return false; }

        // ReSharper disable once UnusedMember.Local
        private void RefreshAll()
        {
            LevelWindow.ClearPreviews();
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
            if (!levelEditorSettings.Settings.blacklistPaths.IsNullOrEmpty())
            {
                blacklistAssets = AssetDatabase.FindAssets("t:GameObject", levelEditorSettings.Settings.blacklistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string blacklistPath in levelEditorSettings.Settings.blacklistPaths)
                {
                    if (File.Exists(blacklistPath)) blacklistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(blacklistPath));
                }
            }

            if (!levelEditorSettings.Settings.whitelistPaths.IsNullOrEmpty())
            {
                whitelistAssets = AssetDatabase.FindAssets("t:GameObject", levelEditorSettings.Settings.whitelistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string whitelistPath in levelEditorSettings.Settings.whitelistPaths)
                {
                    if (File.Exists(whitelistPath)) whitelistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(whitelistPath));
                }
            }

            var resultAssets = whitelistAssets.Filter(_ => !blacklistAssets.Contains(_));
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
            _height = 0f;
            Uniform.SpaceTwoLine();
            if (TryClose()) return;
            if (CheckEscape()) return;
            SceneView.RepaintAll();
            InternalDrawDropArea();
            Uniform.SpaceOneLine();
            InternalDrawSetting();
            Uniform.SpaceOneLine();
            InternalDrawPickupArea();
        }

        private void InternalDrawDropArea()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            Uniform.DrawGroupFoldout("LEVEL_EDITOR_DROP_AREA", "DROP AREA", DrawDropArea);

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
                                        if (File.Exists(path))
                                        {
                                            var r = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                                            if (r.GetType() != typeof(UnityEngine.GameObject)) continue;
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
                                            if (r.GetType() != typeof(UnityEngine.GameObject)) continue;
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
                                foreach (string t in levelEditorSettings.Settings.whitelistPaths.ToList())
                                {
                                    _height -= 18;
                                    DrawRow(t, width, _ => levelEditorSettings.Settings.whitelistPaths.Remove(_));
                                }
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
                                foreach (string t in levelEditorSettings.Settings.blacklistPaths.ToList())
                                {
                                    DrawRow(t, width, _ => levelEditorSettings.Settings.blacklistPaths.Remove(_));
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
                            levelEditorSettings.SaveSetting();
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
            foreach (string whitePath in levelEditorSettings.Settings.whitelistPaths)
            {
                if (InEditor.IsChildOfPath(path, whitePath)) check = true;
            }

            if (!check) levelEditorSettings.Settings.whitelistPaths.Add(path);
            levelEditorSettings.Settings.whitelistPaths = Enumerable.Distinct(levelEditorSettings.Settings.whitelistPaths).ToList(); //unique
        }

        private void AddToBlacklist(string path)
        {
            var check = false;
            foreach (string blackPath in levelEditorSettings.Settings.blacklistPaths)
            {
                if (InEditor.IsChildOfPath(path, blackPath)) check = true;
            }

            if (!check) levelEditorSettings.Settings.blacklistPaths.Add(path);
            levelEditorSettings.Settings.blacklistPaths = Enumerable.Distinct(levelEditorSettings.Settings.blacklistPaths).ToList(); //unique
        }

        private void InternalDrawSetting()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            Uniform.DrawGroupFoldout("LEVEL_EDITOR_CONFIG", "SETTING", DrawSetting);

            void DrawSetting()
            {
                _height -= DEFAULT_HEADER_HEIGHT;
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
                                Uniform.SpaceOneLine();
                                _rootIndexSpawn = EditorGUILayout.IntField(new GUIContent("Index spawn", "Index from root stage contex"), _rootIndexSpawn);
                            }
                            else
                            {
                                Uniform.HelpBox("Index spawn mode only work in PrefabMode!", MessageType.Warning);
                            }

                            break;
                        case "custom":
                            Uniform.SpaceOneLine();
                            _rootSpawn = (GameObject) EditorGUILayout.ObjectField("Spawn in GO here -->", _rootSpawn, typeof(GameObject), true);
                            break;
                    }
                }

                Uniform.SpaceOneLine();
                _selectedMode = EditorGUILayout.Popup("Mode", _selectedMode, _optionsMode);
                if (EditorGUI.EndChangeCheck())
                {
                    switch (_optionsMode[_selectedMode].ToLower())
                    {
                        case "renderer":
                            Uniform.HelpBox("Based on Renderer detection", MessageType.Info);
                            break;
                        case "ignore":
                            Uniform.HelpBox("GameObject will be spawn correcty at raycast location\nIgnore calculate bound object", MessageType.Info);
                            break;
                    }
                }
            }
        }

        private void InternalDrawPickupArea()
        {
            _height -= DEFAULT_HEADER_HEIGHT;
            _height -= DEFAULT_HEADER_HEIGHT;
            _height -= DEFAULT_HEADER_HEIGHT;
            _height -= 18f;
            Uniform.DrawGroupFoldoutWithRightClick("LEVEL_EDITOR_PICKUP_AREA", "PICKUP AREA", DrawPickupArea, ShowMenuRefresh);

            void DrawPickupArea()
            {
                var tex = LevelWindow.GetPreview(_currentPickObject?.pickedObject);
                if (tex)
                {
                    _height -= SELECTED_OBJECT_PREVIEW_HEIGHT;
                    _height -= DEFAULT_HEADER_HEIGHT;
                    string pickObjectName = _currentPickObject?.pickedObject.name;
                    Uniform.SpaceOneLine();
                    Uniform.Horizontal(() =>
                    {
                        GUILayout.Space(position.width / 2 - 50);
                        if (GUILayout.Button(tex, GUILayout.Height(80), GUILayout.Width(80))) _currentPickObject = null;
                    });

                    EditorGUILayout.LabelField($"Selected: <color=#80D2FF>{pickObjectName}</color>\nPress Icon Again Or Escape Key To Deselect",
                        Uniform.HtmlText,
                        GUILayout.Height(40));
                    Uniform.HelpBox("Shift + Click To Add", MessageType.Info);
                }
                else
                {
                    Uniform.HelpBox("Select An Object First", MessageType.Info);
                }

                _height += position.height;
                _pickObjectScrollPosition = GUILayout.BeginScrollView(_pickObjectScrollPosition, GUILayout.Height(_height));
                var resultSplitGroupObjects = PickObjects.GroupBy(_ => _.group).Select(_ => _.ToList()).ToList();
                foreach (var splitGroupObject in resultSplitGroupObjects)
                {
                    string nameGroup = splitGroupObject[0].group.ToUpper();
                    _height -= DEFAULT_HEADER_HEIGHT;
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
                        var tex = LevelWindow.GetPreview(go);
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