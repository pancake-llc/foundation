using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PancakeEditor.Finder
{
    public class FindSelection : IRefDraw
    {
        private readonly FindRefDrawer _drawer;
        private bool _dirty;
        private readonly HashSet<string> _guidSet = new();
        private readonly HashSet<string> _instSet = new(); // Do not reference directly to SceneObject (which might be destroyed anytime)
        internal bool isLock;
        private Dictionary<string, FindRef> _refs;

        public FindSelection(IWindow window, Func<FindRefDrawer.Sort> getSortMode, Func<FindRefDrawer.Mode> getGroupMode)
        {
            this.Window = window;
            _drawer = new FindRefDrawer(window, getSortMode, getGroupMode)
            {
                groupDrawer = {hideGroupIfPossible = true}, forceHideDetails = true, level0Group = string.Empty
            };

            _dirty = true;
            _drawer.SetDirty();
        }

        public int Count => _guidSet.Count + _instSet.Count;

        public bool IsSelectingAsset => _instSet.Count == 0;

        public IWindow Window { get; set; }

        public int ElementCount() { return _refs?.Count ?? 0; }

        public bool DrawLayout()
        {
            if (_dirty) RefreshView();
            return _drawer.DrawLayout();
        }

        public bool Draw(Rect rect)
        {
            if (_dirty) RefreshView();
            if (_refs == null) return false;

            rect.yMax -= 16f;
            return _drawer.Draw(rect);
        }

        public bool Contains(string guidOrInstID) { return _guidSet.Contains(guidOrInstID) || _instSet.Contains(guidOrInstID); }

        public bool Contains(UnityObject sceneObject)
        {
            var id = sceneObject.GetInstanceID().ToString();
            return _instSet.Contains(id);
        }

        public void Add(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            _instSet.Add(id); // hashset does not need to check exist before add
            _dirty = true;
        }

        public void AddRange(params UnityObject[] sceneObjects)
        {
            foreach (var go in sceneObjects)
            {
                var id = go.GetInstanceID().ToString();
                _instSet.Add(id); // hashset does not need to check exist before add	
            }

            _dirty = true;
        }

        public void Add(string guid)
        {
            if (_guidSet.Contains(guid)) return;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Invalid GUID: " + guid);
                return;
            }

            _guidSet.Add(guid);
            _dirty = true;
        }

        public void AddRange(params string[] guids)
        {
            foreach (string id in guids)
            {
                Add(id);
            }

            _dirty = true;
        }

        public void Remove(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            _instSet.Remove(id);
            _dirty = true;
        }

        public void Remove(string guidOrInstID)
        {
            _guidSet.Remove(guidOrInstID);
            _instSet.Remove(guidOrInstID);

            _dirty = true;
        }

        public void Clear()
        {
            _guidSet.Clear();
            _instSet.Clear();
            _dirty = true;
        }

        public void Add(FindRef rf)
        {
            if (rf.isSceneRef) Add(rf.component);
            else Add(rf.asset.guid);
        }

        public void Remove(FindRef rf)
        {
            if (rf.isSceneRef) Remove(rf.component);
            else Remove(rf.asset.guid);
        }

        public void SetDirty() { _drawer.SetDirty(); }

        public void RefreshView()
        {
            if (_refs == null) _refs = new Dictionary<string, FindRef>();
            _refs.Clear();

            if (_instSet.Count > 0)
            {
                foreach (string instId in _instSet)
                {
                    _refs.Add(instId, new FindSceneRef(0, EditorUtility.InstanceIDToObject(int.Parse(instId))));
                }
            }
            else
            {
                foreach (string guid in _guidSet)
                {
                    var asset = FinderWindowBase.CacheSetting.Get(guid);
                    _refs.Add(guid, new FindRef(0, 0, asset, null) {isSceneRef = false});
                }
            }

            _drawer.SetRefs(_refs);
            _dirty = false;
        }
    }
}