using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pancake.Common;
using PancakeEditor.Common;
using Pancake.LevelSystemEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PancakeEditor
{
    internal static class LevelSystemWindow
    {
        public static void OnEnabled() { SceneView.duringSceneGui += OnSceneGUI; }

        public static void OnDisabled() { SceneView.duringSceneGui -= OnSceneGUI; }

        private static void OnSceneGUI(SceneView sceneView)
        {
            try
            {
                if (LevelSystemEditorSetting.Instance != null) TryFakeRender(sceneView);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void OnInspectorGUI(ref Wizard.LevelEditorTabType tabType, Rect position)
        {
            var scriptableSetting = Resources.Load<LevelSystemEditorSetting>(nameof(LevelSystemEditorSetting));
            if (scriptableSetting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink_500;
                if (GUILayout.Button("Setup Level Editor", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<LevelSystemEditorSetting>();
                    if (!Directory.Exists(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(LevelSystemEditorSetting)}.asset");
                    AssetDatabase.SaveAssets();
                    RegistryManager.AddPackage("com.unity.addressables", "1.21.20");
                    RegistryManager.Resolve();
                    AssetDatabase.Refresh();
                    Debug.Log(
                        $"{nameof(LevelSystemEditorSetting).SetColor("f75369")} was created ad {Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(LevelSystemEditorSetting)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (LevelSystemEditorSetting.Instance.PickObjects.Count == 0) RefreshAll();
                DrawTab(ref tabType);
                if (tabType == Wizard.LevelEditorTabType.Setting)
                {
                    DrawTabSetting();
                    DrawDropArea(position);
                }
                else DrawTabPickup(position);
            }
        }

        #region pickup

        private static Dictionary<GameObject, Texture2D> previewDict;
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

        private static void DrawTabPickup(Rect position)
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            var tex = GetPreview(LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject);
            if (tex)
            {
                string pickObjectName = LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject.name;

                #region horizontal

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(lastRect.width / 2 - 50);
                if (LevelSystemEditorSetting.Instance.EditorInpsectorPreview == null || LevelSystemEditorSetting.Instance.PreviousObjectInpectorPreview !=
                    LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject)
                {
                    LevelSystemEditorSetting.Instance.EditorInpsectorPreview =
                        UnityEditor.Editor.CreateEditor(LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject);
                }

                var rect = GUILayoutUtility.GetLastRect();
                LevelSystemEditorSetting.Instance.EditorInpsectorPreview.DrawPreview(new Rect(new Vector2(lastRect.width / 2 - 50, rect.position.y),
                    new Vector2(100, 100)));
                LevelSystemEditorSetting.Instance.PreviousObjectInpectorPreview = LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject;
                GUI.color = new Color(1, 1, 1, 0f);
                if (GUILayout.Button(tex, GUILayout.Height(80), GUILayout.Width(80)))
                {
                }

                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();

                #endregion


                EditorGUILayout.LabelField($"Selected: <color=#80D2FF>{pickObjectName}</color>\nPress Icon Again Or Escape Key To Deselect",
                    new GUIStyle(EditorStyles.label) {richText = true},
                    GUILayout.Height(40));
                EditorGUILayout.HelpBox("Shift + Click To Add", MessageType.Info);

                LevelSystemEditorSetting.Instance.GizmosRadius = EditorGUILayout.FloatField("Radius", LevelSystemEditorSetting.Instance.GizmosRadius);
                LevelSystemEditorSetting.Instance.ElementSize = EditorGUILayout.FloatField("Element Size", LevelSystemEditorSetting.Instance.ElementSize);
            }
            else
            {
                EditorGUILayout.HelpBox("Select An Object First", MessageType.Info);
            }

            LevelSystemEditorSetting.Instance.PickObjectScrollPosition = GUILayout.BeginScrollView(LevelSystemEditorSetting.Instance.PickObjectScrollPosition);
            var resultSplitGroupObjects = LevelSystemEditorSetting.Instance.PickObjects.GroupBy(o => o.group).Select(o => o.ToList()).ToList();
            foreach (var splitGroupObject in resultSplitGroupObjects)
            {
                string nameGroup = splitGroupObject[0].group.ToUpper();
                Uniform.DrawGroupFoldout($"level_editor_pickup_area_child_{nameGroup}", nameGroup, () => DrawInGroup(splitGroupObject, position));
            }

            GUILayout.EndScrollView();
        }

        private static void TryFakeRender(SceneView sceneView)
        {
            var e = Event.current;
            if (!e.shift)
            {
                if (LevelSystemEditorSetting.Instance.PreviewPickupObject != null)
                    UnityEngine.Object.DestroyImmediate(LevelSystemEditorSetting.Instance.PreviewPickupObject);
                return;
            }

            if (LevelSystemEditorSetting.Instance.CurrentPickObject == null || !LevelSystemEditorSetting.Instance.CurrentPickObject.pickedObject) return;
            Vector3 mousePosition;
            Vector3 normal;
            if (sceneView.in2DMode)
            {
                bool state = Common.Editor.Get2DMouseScenePosition(out var mousePosition2d);
                mousePosition = mousePosition2d;
                if (!state) return;
                Common.Editor.FakeRenderSprite(LevelSystemEditorSetting.Instance.CurrentPickObject.pickedObject, mousePosition, Vector3.one, Quaternion.identity);
                SceneView.RepaintAll();

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    AddPickObject(LevelSystemEditorSetting.Instance.CurrentPickObject, mousePosition);
                    Common.Editor.SkipEvent();
                }
            }
            else
            {
                var pos = Common.Editor.GetInnerGuiPosition(sceneView);
                if (pos.Contains(e.mousePosition))
                {
                    var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers))
                    {
                        mousePosition = hit.point;
                        normal = hit.normal;
                        float radius = LevelSystemEditorSetting.Instance.GizmosRadius;
                        Color discColor = Event.current.control ? Event.current.shift ? Color.yellow : Color.red : Color.cyan;
                        Handles.color = discColor;
                        Handles.DrawWireDisc(hit.point, hit.normal, radius);
                        discColor.a = 0.1f;
                        Handles.color = discColor;
                        Handles.DrawSolidDisc(hit.point, hit.normal, radius);


                        if (!LevelSystemEditorSetting.Instance.PreviewPickupObject)
                        {
                            LevelSystemEditorSetting.Instance.PreviewPickupObject =
                                (GameObject) PrefabUtility.InstantiatePrefab(LevelSystemEditorSetting.Instance.CurrentPickObject?.pickedObject);
                            StageUtility.PlaceGameObjectInCurrentStage(LevelSystemEditorSetting.Instance.PreviewPickupObject);
                            LevelSystemEditorSetting.Instance.PreviewPickupObject.hideFlags = HideFlags.HideAndDontSave;
                            LevelSystemEditorSetting.Instance.PreviewPickupObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        }

                        SetPosition();

                        if (e.type == EventType.MouseDown && e.button == 0 && LevelSystemEditorSetting.Instance.PreviewPickupObject)
                        {
                            AddPickObject(LevelSystemEditorSetting.Instance.CurrentPickObject, LevelSystemEditorSetting.Instance.PreviewPickupObject.transform.position);
                            Common.Editor.SkipEvent();
                        }
                    }

                    void SetPosition()
                    {
                        LevelSystemEditorSetting.Instance.PreviewPickupObject.transform.position = mousePosition;

                        switch (LevelSystemEditorSetting.Instance.optionsMode[LevelSystemEditorSetting.Instance.SelectedMode].ToLower())
                        {
                            case "renderer":
                                if (LevelSystemEditorSetting.Instance.PreviewPickupObject.CalculateBounds(out var bounds,
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

                                    LevelSystemEditorSetting.Instance.PreviewPickupObject.transform.position += difference * normal;
                                }

                                break;
                            case "ignore":
                                break;
                        }
                    }
                }
            }

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Spawn pickup object
        /// </summary>
        /// <param name="pickObject"></param>
        /// <param name="worldPos"></param>
        private static void AddPickObject(PickObject pickObject, Vector3 worldPos)
        {
            if (pickObject?.pickedObject)
            {
                var inst = (GameObject) PrefabUtility.InstantiatePrefab(pickObject.pickedObject, GetParent());
                inst.transform.position = worldPos;
                Undo.RegisterCreatedObjectUndo(inst.gameObject, "Create pick obj");
                Selection.activeObject = inst;
            }
        }

        private static Transform GetParent()
        {
            Transform parent = null;
            var currentPrefabState = PrefabStageUtility.GetCurrentPrefabStage();

            if (currentPrefabState != null)
            {
                var prefabRoot = currentPrefabState.prefabContentsRoot.transform;
                switch (LevelSystemEditorSetting.Instance.optionsSpawn[LevelSystemEditorSetting.Instance.SelectedSpawn].ToLower())
                {
                    case "default":
                        parent = prefabRoot;
                        break;
                    case "index":
                        if (LevelSystemEditorSetting.Instance.RootIndexSpawn < 0) parent = prefabRoot;
                        else if (prefabRoot.childCount - 1 > LevelSystemEditorSetting.Instance.RootIndexSpawn)
                            parent = prefabRoot.GetChild(LevelSystemEditorSetting.Instance.RootIndexSpawn);
                        else parent = prefabRoot;
                        break;
                    case "custom":
                        parent = LevelSystemEditorSetting.Instance.RootSpawn ? LevelSystemEditorSetting.Instance.RootSpawn.transform : prefabRoot;
                        break;
                }
            }
            else
            {
                switch (LevelSystemEditorSetting.Instance.optionsSpawn[LevelSystemEditorSetting.Instance.SelectedSpawn].ToLower())
                {
                    case "default":
                    case "index":
                        //parent = null;
                        break;
                    case "custom":
                        parent = LevelSystemEditorSetting.Instance.RootSpawn ? LevelSystemEditorSetting.Instance.RootSpawn.transform : null;
                        break;
                }
            }

            return parent;
        }

        private static void DrawInGroup(IReadOnlyList<PickObject> pickObjectsInGroup, Rect position)
        {
            const int spacing = 25;
            var counter = 0;
            CalculateIdealCount(position.width - 65 * 4, spacing, out int count, out float size);
            count = Mathf.Max(1, count);
            while (counter >= 0 && counter < pickObjectsInGroup.Count)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(8);
                for (var x = 0; x < count; x++)
                {
                    var pickObj = pickObjectsInGroup[counter];
                    var go = pickObj.pickedObject;
                    var tex = GetPreview(go);

                    if (GUILayout.Button(new GUIContent(""), GUIStyle.none, GUILayout.Width(size), GUILayout.Height(size)))
                    {
                        if (Event.current.button == 1)
                        {
                            ShowMenuRightClickItem(pickObj);
                        }
                        else
                        {
                            LevelSystemEditorSetting.Instance.CurrentPickObject = LevelSystemEditorSetting.Instance.CurrentPickObject == pickObj ? null : pickObj;
                        }
                    }

                    var rect = GUILayoutUtility.GetLastRect();

                    var forcegroundRect = new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10);
                    if (pickObj != LevelSystemEditorSetting.Instance.CurrentPickObject) EditorGUI.DrawRect(forcegroundRect, new Color(1, 1, 1, 0.2f));

                    if (tex) GUI.DrawTexture(Grown(rect, Vector2.one * -10), tex, ScaleMode.ScaleToFit);
                    if (go)
                    {
                        EditorGUI.LabelField(Grown(rect, new Vector2(0, 15)), go.name, new GUIStyle(EditorStyles.miniLabel) {alignment = TextAnchor.LowerCenter,});
                    }

                    if (pickObj == LevelSystemEditorSetting.Instance.CurrentPickObject) EditorGUI.DrawRect(forcegroundRect, new Color(0, 1, 0, 0.25f));

                    counter++;
                    if (counter >= pickObjectsInGroup.Count) break;
                    GUILayout.Space(4);
                    continue;

                    Rect Grown(Rect r, Vector2 half) { return new Rect(r.position - half, r.size + half * 2); }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(spacing);
            }
        }

        private static void ShowMenuRefresh()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Refresh Pickup  Area"),
                false,
                () =>
                {
                    LevelSystemEditorSetting.Instance.CurrentPickObject = null;
                    RefreshAll();
                });
            menu.ShowAsContext();
        }

        private static void ShowMenuRightClickItem(PickObject pickObj)
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

        private static void IgnorePath(PickObject pickObj)
        {
            var path = AssetDatabase.GetAssetPath(pickObj.pickedObject);
            ValidateBlacklist(path, ref LevelSystemEditorSetting.Instance.whitelistPaths);
            AddToBlacklist(path);
            ReduceScopeDirectory(ref LevelSystemEditorSetting.Instance.blacklistPaths);
            RefreshAll();
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

        /// <summary>
        /// Calculate count item pickup can display
        /// </summary>
        /// <param name="availableSpace"></param>
        /// <param name="spacing"></param>
        /// <param name="count"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static void CalculateIdealCount(float availableSpace, float spacing, out int count, out float size)
        {
            float halfSpacing = spacing / 2f;
            count = Mathf.FloorToInt(availableSpace / (LevelSystemEditorSetting.Instance.ElementSize + halfSpacing));
            size = (availableSpace - halfSpacing * (count - 1) - (count - 1) * (count / 10f)) / count;
        }

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

        #endregion

        #region setting

        private static void DrawTabSetting()
        {
            GUILayout.Space(4);
            LevelSystemEditorSetting.Instance.SelectedSpawn = EditorGUILayout.Popup("Where Spawn",
                LevelSystemEditorSetting.Instance.SelectedSpawn,
                LevelSystemEditorSetting.Instance.optionsSpawn);

            if (EditorGUI.EndChangeCheck())
            {
                switch (LevelSystemEditorSetting.Instance.optionsSpawn[LevelSystemEditorSetting.Instance.SelectedSpawn].ToLower())
                {
                    case "default":
                        break;
                    case "index":
                        var currentPrefabState = PrefabStageUtility.GetCurrentPrefabStage();
                        if (currentPrefabState != null)
                        {
                            LevelSystemEditorSetting.Instance.RootIndexSpawn = EditorGUILayout.IntField(new GUIContent("Index spawn", "Index from root stage contex"),
                                LevelSystemEditorSetting.Instance.RootIndexSpawn);
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Index spawn only work in PrefabMode!", MessageType.Warning);
                        }

                        break;
                    case "custom":
                        LevelSystemEditorSetting.Instance.RootSpawn = (GameObject) EditorGUILayout.ObjectField("Spawn in GO here -->",
                            LevelSystemEditorSetting.Instance.RootSpawn,
                            typeof(GameObject),
                            true);
                        break;
                }
            }

            LevelSystemEditorSetting.Instance.SelectedMode =
                EditorGUILayout.Popup("Mode", LevelSystemEditorSetting.Instance.SelectedMode, LevelSystemEditorSetting.Instance.optionsMode);

            if (EditorGUI.EndChangeCheck())
            {
                switch (LevelSystemEditorSetting.Instance.optionsMode[LevelSystemEditorSetting.Instance.SelectedMode].ToLower())
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

        private static void DrawDropArea(Rect position)
        {
            GUILayout.Space(2);

            var @event = Event.current;

            #region horizontal

            EditorGUILayout.BeginHorizontal();

            var tempRectWhite = GUILayoutUtility.GetRect(0f, 150f, GUILayout.ExpandWidth(true));
            var tempRectBlack = GUILayoutUtility.GetRect(0f, 150f, GUILayout.ExpandWidth(true));
            var whiteArea = new Rect(tempRectWhite.x, tempRectWhite.y, tempRectWhite.width - 1, tempRectWhite.height);
            var blackArea = new Rect(tempRectBlack.x, tempRectBlack.y, tempRectBlack.width - 1, tempRectBlack.height);

            GUI.color = new Color(0f, 0.83f, 1f);
            GUI.Box(whiteArea, "[WHITE LIST]", new GUIStyle(EditorStyles.helpBox) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic});
            float width = GUILayoutUtility.GetLastRect().width;
            if (Mathf.Approximately(width, 1f)) width = position.width / 2 - 65 * 2;

            GUI.color = new Color(1f, 0.13f, 0f);
            GUI.Box(blackArea, "[BLACK LIST]", new GUIStyle(EditorStyles.helpBox) {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic});
            GUI.color = Color.white;
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

                                ValidateWhitelist(path, ref LevelSystemEditorSetting.Instance.blacklistPaths);
                                AddToWhitelist(path);
                            }

                            ReduceScopeDirectory(ref LevelSystemEditorSetting.Instance.whitelistPaths);
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

                                ValidateBlacklist(path, ref LevelSystemEditorSetting.Instance.whitelistPaths);
                                AddToBlacklist(path);
                            }

                            ReduceScopeDirectory(ref LevelSystemEditorSetting.Instance.blacklistPaths);
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
                                LevelSystemEditorSetting.Instance.whitelistPaths.Clear();
                                SaveLevelSystemSetting();
                                RefreshAll();
                            });
                    }
                    else if (blackArea.Contains(@event.mousePosition))
                    {
                        menu.AddItem(new GUIContent("Clear All [BLACK LIST]"),
                            false,
                            () =>
                            {
                                LevelSystemEditorSetting.Instance.blacklistPaths.Clear();
                                SaveLevelSystemSetting();
                                RefreshAll();
                            });
                    }

                    menu.ShowAsContext();
                    break;
            }

            EditorGUILayout.EndHorizontal();

            #endregion


            #region horizontal

            EditorGUILayout.BeginHorizontal();

            #region vertical scope

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(width - 10)))
            {
                if (LevelSystemEditorSetting.Instance.whitelistPaths.Count == 0)
                {
                    EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                }
                else
                {
                    GUILayout.Space(2);
                    LevelSystemEditorSetting.Instance.WhitelistScrollPosition = EditorGUILayout.BeginScrollView(LevelSystemEditorSetting.Instance.WhitelistScrollPosition,
                        false,
                        false);
                    foreach (string t in LevelSystemEditorSetting.Instance.whitelistPaths.ToList())
                    {
                        DrawRow(t,
                            width,
                            s =>
                            {
                                LevelSystemEditorSetting.Instance.whitelistPaths.Remove(s);
                                SaveLevelSystemSetting();
                            });
                    }

                    GUILayout.EndScrollView();
                }
            }

            #endregion


            GUILayout.Space(4);

            #region vertical scope

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(width - 20)))
            {
                if (LevelSystemEditorSetting.Instance.blacklistPaths.Count == 0)
                {
                    EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(width - 50), GUILayout.Height(0));
                }
                else
                {
                    GUILayout.Space(2);
                    LevelSystemEditorSetting.Instance.BlacklistScrollPosition = EditorGUILayout.BeginScrollView(LevelSystemEditorSetting.Instance.BlacklistScrollPosition,
                        false,
                        false);
                    foreach (string t in LevelSystemEditorSetting.Instance.blacklistPaths.ToList())
                    {
                        DrawRow(t,
                            width,
                            s =>
                            {
                                LevelSystemEditorSetting.Instance.blacklistPaths.Remove(s);
                                SaveLevelSystemSetting();
                            });
                    }

                    GUILayout.EndScrollView();
                }
            }

            #endregion


            EditorGUILayout.EndHorizontal();

            #endregion
        }

        private static void DrawRow(string content, float width, Action<string> action)
        {
            #region horizontal

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(content), GUILayout.Width(width - 100));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Uniform.IconContent("d_scenevis_visible_hover", "Ping Selection")))
            {
                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(content);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            if (GUILayout.Button(Uniform.IconContent("Toolbar Minus", "Remove")))
            {
                action?.Invoke(content);
                RefreshAll();
            }

            EditorGUILayout.EndHorizontal();

            #endregion
        }

        private static void ValidateWhitelist(string path, ref List<string> blackList)
        {
            foreach (string t in blackList.ToList())
            {
                if (path.Equals(t)) blackList.Remove(t);
            }
        }

        private static void AddToWhitelist(string path)
        {
            var check = false;
            foreach (string whitePath in LevelSystemEditorSetting.Instance.whitelistPaths)
            {
                if (IsChildOfPath(path, whitePath)) check = true;
            }

            if (!check) LevelSystemEditorSetting.Instance.whitelistPaths.Add(path);
            LevelSystemEditorSetting.Instance.whitelistPaths = LevelSystemEditorSetting.Instance.whitelistPaths.Distinct().ToList(); //unique
            SaveLevelSystemSetting();
        }

        // return true if child is childrent of parent
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

        private static bool EqualPath(FileSystemInfo info, string str)
        {
            string relativePath = info.FullName;
            if (relativePath.StartsWith(Application.dataPath.Replace('/', '\\'))) relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            relativePath = relativePath.Replace('\\', '/');
            return str.Equals(relativePath);
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

        private static void ValidateBlacklist(string path, ref List<string> whiteList)
        {
            foreach (string t in whiteList.ToList())
            {
                if (path.Equals(t) || IsChildOfPath(t, path)) whiteList.Remove(t);
            }
        }

        private static void AddToBlacklist(string path)
        {
            var check = false;
            foreach (string blackPath in LevelSystemEditorSetting.Instance.blacklistPaths)
            {
                if (IsChildOfPath(path, blackPath)) check = true;
            }

            if (!check) LevelSystemEditorSetting.Instance.blacklistPaths.Add(path);
            LevelSystemEditorSetting.Instance.blacklistPaths = LevelSystemEditorSetting.Instance.blacklistPaths.Distinct().ToList(); //unique
            SaveLevelSystemSetting();
        }

        private static void SaveLevelSystemSetting()
        {
            EditorUtility.SetDirty(LevelSystemEditorSetting.Instance);
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// display picked object in editor
        /// </summary>
        private static void RefreshPickObject()
        {
            LevelSystemEditorSetting.Instance.PickObjects = new List<PickObject>();
            var blacklistAssets = new List<GameObject>();
            var whitelistAssets = new List<GameObject>();
            if (!LevelSystemEditorSetting.Instance.blacklistPaths.IsNullOrEmpty())
            {
                blacklistAssets = AssetDatabase.FindAssets("t:GameObject", LevelSystemEditorSetting.Instance.blacklistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string blacklistPath in LevelSystemEditorSetting.Instance.blacklistPaths)
                {
                    if (File.Exists(blacklistPath)) blacklistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(blacklistPath));
                }
            }

            if (!LevelSystemEditorSetting.Instance.whitelistPaths.IsNullOrEmpty())
            {
                whitelistAssets = AssetDatabase.FindAssets("t:GameObject", LevelSystemEditorSetting.Instance.whitelistPaths.ToArray())
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                    .ToList();

                foreach (string whitelistPath in LevelSystemEditorSetting.Instance.whitelistPaths)
                {
                    if (File.Exists(whitelistPath)) whitelistAssets.Add(AssetDatabase.LoadAssetAtPath<GameObject>(whitelistPath));
                }
            }

            var resultAssets = whitelistAssets.Where(o => !blacklistAssets.Contains(o));
            foreach (var o in resultAssets)
            {
                string group = Path.GetDirectoryName(AssetDatabase.GetAssetPath(o))?.Replace('\\', '/').Split('/').Last();
                var po = new PickObject {pickedObject = o.gameObject, group = group};
                LevelSystemEditorSetting.Instance.PickObjects.Add(po);
            }
        }

        private static void RefreshAll()
        {
            ClearPreviews();
            RefreshPickObject();
        }

        #endregion

        private static void DrawTab(ref Wizard.LevelEditorTabType tabType)
        {
            EditorGUILayout.BeginHorizontal();
            DrawButtonSetting(ref tabType);
            DrawButtonPickupArea(ref tabType);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawButtonPickupArea(ref Wizard.LevelEditorTabType tabType)
        {
            bool clicked = GUILayout.Toggle(tabType == Wizard.LevelEditorTabType.PickupArea,
                "Pickup Area",
                GUI.skin.button,
                GUILayout.ExpandHeight(true),
                GUILayout.Height(22));
            if (clicked && tabType != Wizard.LevelEditorTabType.PickupArea)
            {
                tabType = Wizard.LevelEditorTabType.PickupArea;
                RefreshAll();
            }
        }

        private static void DrawButtonSetting(ref Wizard.LevelEditorTabType tabType)
        {
            bool clicked = GUILayout.Toggle(tabType == Wizard.LevelEditorTabType.Setting,
                "Settings",
                GUI.skin.button,
                GUILayout.ExpandHeight(true),
                GUILayout.Height(22));
            if (clicked && tabType != Wizard.LevelEditorTabType.Setting) tabType = Wizard.LevelEditorTabType.Setting;
        }
    }
}