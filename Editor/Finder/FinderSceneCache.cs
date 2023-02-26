#if UNITY_2018_3_OR_NEWER
#define SUPPORT_NESTED_PREFAB
#endif

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
#if SUPPORT_NESTED_PREFAB
using UnityEditor.Experimental.SceneManagement;

#endif


namespace PancakeEditor
{
    public class FinderSceneCache
    {
        private static FinderSceneCache api;
        public static Action onReady;
        public static bool ready = true;
        private Dictionary<Component, HashSet<HashValue>> _cache = new Dictionary<Component, HashSet<HashValue>>();
        public int current;
        public Dictionary<string, HashSet<Component>> folderCache = new Dictionary<string, HashSet<Component>>();

        private List<GameObject> _listGo;

        //public HashSet<string> prefabDependencies = new HashSet<string>();
        public Dictionary<GameObject, HashSet<string>> prefabDependencies = new Dictionary<GameObject, HashSet<string>>();

        public int total;
        private IWindow _window;

        public FinderSceneCache()
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

#if SUPPORT_NESTED_PREFAB
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += PrefabClose;
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += PrefabClose;


#endif
        }

        public static FinderSceneCache Api
        {
            get
            {
                if (api == null)
                {
                    api = new FinderSceneCache();
                }

                return api;
            }
        }

        private bool _dirty = true;
        public bool Dirty { get => _dirty; set => _dirty = value; }

        public Dictionary<Component, HashSet<HashValue>> Cache
        {
            get
            {
                if (_cache == null)
                {
                    RefreshCache(_window);
                }

                return _cache;
            }
        }

        public void RefreshCache(IWindow window)
        {
            if (window == null)
            {
                return;
            }

            // if(!ready) return;
            this._window = window;

            _cache = new Dictionary<Component, HashSet<HashValue>>();
            folderCache = new Dictionary<string, HashSet<Component>>();
            prefabDependencies = new Dictionary<GameObject, HashSet<string>>();

            ready = false;

            List<GameObject> listRootGo = null;

#if SUPPORT_NESTED_PREFAB

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                GameObject rootPrefab = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                if (rootPrefab != null)
                {
                    listRootGo = new List<GameObject> {rootPrefab};
                }
            }

#else
#endif
            if (listRootGo == null)
            {
                _listGo = FinderUnity.GetAllObjsInCurScene().ToList();
            }
            else
            {
                _listGo = new List<GameObject>();
                foreach (GameObject item in listRootGo)
                {
                    _listGo.AddRange(FinderUnity.GetAllChild(item, true));
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
            for (var i = 0; i < 5 * FinderCache.priority; i++)
            {
                if (_listGo == null || _listGo.Count <= 0)
                {
                    //done
                    // Debug.Log("done");
                    EditorApplication.update -= OnUpdate;
                    ready = true;
                    Dirty = false;
                    _listGo = null;
                    if (onReady != null)
                    {
                        onReady();
                    }

                    if (_window != null)
                    {
                        _window.OnSelectionChange();
                    }

                    return;
                }

                int index = _listGo.Count - 1;

                GameObject go = _listGo[index];
                if (go == null)
                {
                    continue;
                }

                string prefabGuid = FinderUnity.GetPrefabParent(go);
                if (!string.IsNullOrEmpty(prefabGuid))
                {
                    Transform parent = go.transform.parent;
                    while (parent != null)
                    {
                        GameObject g = parent.gameObject;
                        if (!prefabDependencies.ContainsKey(g))
                        {
                            prefabDependencies.Add(g, new HashSet<string>());
                        }

                        prefabDependencies[g].Add(prefabGuid);
                        parent = parent.parent;
                    }
                }

                Component[] components = go.GetComponents<Component>();

                foreach (Component com in components)
                {
                    if (com == null)
                    {
                        continue;
                    }

                    var serialized = new SerializedObject(com);
                    SerializedProperty it = serialized.GetIterator().Copy();
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType != SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        if (it.objectReferenceValue == null)
                        {
                            continue;
                        }

                        var isSceneObject = true;
                        string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dir = Path.GetDirectoryName(path);
                            if (!string.IsNullOrEmpty(dir))
                            {
                                isSceneObject = false;
                                if (!folderCache.ContainsKey(dir))
                                {
                                    folderCache.Add(dir, new HashSet<Component>());
                                }

                                if (!folderCache[dir].Contains(com))
                                {
                                    folderCache[dir].Add(com);
                                }
                            }
                        }

                        if (!_cache.ContainsKey(com))
                        {
                            _cache.Add(com, new HashSet<HashValue>());
                        }

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
#if SUPPORT_NESTED_PREFAB

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
#endif

#if UNITY_2017_1_OR_NEWER
        private void OnSceneChanged(Scene scene, LoadSceneMode mode) { OnSceneChanged(); }

        private void OnSceneChanged(Scene arg0, Scene arg1) { OnSceneChanged(); }
#endif
    }
}