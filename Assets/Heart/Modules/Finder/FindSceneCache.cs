using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace PancakeEditor.Finder
{
    public class FindSceneCache
    {
        public Dictionary<GameObject, HashSet<string>> prefabDependencies = new();
        public Dictionary<string, HashSet<Component>> folderCache = new();
        public int total;
        public int current;
        public static Action onReady;
        public static bool ready = true;

        private static FindSceneCache api;
        private Dictionary<Component, HashSet<HashValue>> _cache = new();
        private List<GameObject> _listGo;
        private IWindow _window;

        public FindSceneCache()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= OnSceneChanged;
            EditorApplication.hierarchyChanged += OnSceneChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
			EditorApplication.hierarchyWindowChanged += OnSceneChanged;
#endif

#if UNITY_2018_2_OR_NEWER
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#endif

#if UNITY_2017_1_OR_NEWER
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;

            SceneManager.sceneLoaded -= OnSceneChanged;
            SceneManager.sceneLoaded += OnSceneChanged;

            Undo.postprocessModifications -= OnModify;
            Undo.postprocessModifications += OnModify;
#endif

            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += PrefabClose;
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += PrefabClose;
        }

        public static FindSceneCache Api => api ??= new FindSceneCache();

        public bool Dirty { get; set; } = true;

        public Dictionary<Component, HashSet<HashValue>> Cache
        {
            get
            {
                if (_cache == null) RefreshCache(_window);
                return _cache;
            }
        }

        public void RefreshCache(IWindow window)
        {
            if (window == null) return;
            _window = window;
            _cache = new Dictionary<Component, HashSet<HashValue>>();
            folderCache = new Dictionary<string, HashSet<Component>>();
            prefabDependencies = new Dictionary<GameObject, HashSet<string>>();
            ready = false;
            List<GameObject> listRootGo = null;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                var rootPrefab = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                if (rootPrefab != null) listRootGo = new List<GameObject> {rootPrefab};
            }

            if (listRootGo == null) _listGo = FinderUtility.GetAllObjsInCurScene().ToList();
            else
            {
                _listGo = new List<GameObject>();
                foreach (var item in listRootGo)
                {
                    _listGo.AddRange(FinderUtility.GetAllChild(item, true));
                }
            }

            total = _listGo.Count;
            current = 0;

            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            Dirty = false;
        }

        private void OnUpdate()
        {
            for (var i = 0; i < 5 * AssetCache.priority; i++)
            {
                if (_listGo == null || _listGo.Count <= 0)
                {
                    EditorApplication.update -= OnUpdate;
                    ready = true;
                    Dirty = false;
                    _listGo = null;
                    onReady?.Invoke();
                    _window?.OnSelectionChange();
                    return;
                }

                int index = _listGo.Count - 1;

                var go = _listGo[index];
                if (go == null) continue;

                string prefabGuid = FinderUtility.GetPrefabParent(go);
                if (!string.IsNullOrEmpty(prefabGuid))
                {
                    var parent = go.transform.parent;
                    while (parent != null)
                    {
                        var g = parent.gameObject;
                        if (!prefabDependencies.ContainsKey(g)) prefabDependencies.Add(g, new HashSet<string>());

                        prefabDependencies[g].Add(prefabGuid);
                        parent = parent.parent;
                    }
                }

                var components = go.GetComponents<Component>();

                foreach (var com in components)
                {
                    if (com == null) continue;

                    var serialized = new SerializedObject(com);
                    var it = serialized.GetIterator().Copy();
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType != SerializedPropertyType.ObjectReference) continue;

                        if (it.objectReferenceValue == null) continue;

                        var isSceneObject = true;
                        string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dir = Path.GetDirectoryName(path);
                            if (!string.IsNullOrEmpty(dir))
                            {
                                isSceneObject = false;
                                if (!folderCache.ContainsKey(dir)) folderCache.Add(dir, new HashSet<Component>());

                                if (!folderCache[dir].Contains(com)) folderCache[dir].Add(com);
                            }
                        }

                        if (!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<HashValue>());

                        _cache[com].Add(new HashValue {target = it.objectReferenceValue, isSceneObject = isSceneObject});

                    }
                }

                _listGo.RemoveAt(index);
                current++;
            }
        }

        private void OnSceneChanged()
        {
            if (!Application.isPlaying)
            {
                Api.RefreshCache(_window);
                return;
            }

            SetDirty();
        }

#if UNITY_2017_1_OR_NEWER
        private UndoPropertyModification[] OnModify(UndoPropertyModification[] modifications)
        {
            for (var i = 0; i < modifications.Length; i++)
            {
                if (modifications[i].currentValue.objectReference != null)
                {
                    SetDirty();
                    break;
                }
            }

            return modifications;
        }
#endif


        public void SetDirty() { Dirty = true; }


        public class HashValue
        {
            public bool isSceneObject;
            public Object target;
        }

        private void PrefabClose(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                Api.RefreshCache(_window);
                return;
            }

            SetDirty();
        }

        private void prefabOnpen(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                Api.RefreshCache(_window);
                return;
            }

            SetDirty();
        }

#if UNITY_2017_1_OR_NEWER
        private void OnSceneChanged(Scene scene, LoadSceneMode mode) { OnSceneChanged(); }

        private void OnSceneChanged(Scene arg0, Scene arg1) { OnSceneChanged(); }
#endif
    }
}