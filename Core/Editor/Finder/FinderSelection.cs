using System.Collections.Generic;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PancakeEditor
{
    public class FinderSelection : IRefDraw
    {
        internal HashSet<string> guidSet = new HashSet<string>();
        internal HashSet<string> instSet = new HashSet<string>(); // Do not reference directly to SceneObject (which might be destroyed anytime)

        public int Count => guidSet.Count + instSet.Count;

        public bool Contains(string guidOrInstID) { return guidSet.Contains(guidOrInstID) || instSet.Contains(guidOrInstID); }

        public bool Contains(UnityObject sceneObject)
        {
            var id = sceneObject.GetInstanceID().ToString();
            return instSet.Contains(id);
        }

        public void Add(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Add(id); // hashset does not need to check exist before add
            _dirty = true;
        }

        public void AddRange(params UnityObject[] sceneObjects)
        {
            foreach (var go in sceneObjects)
            {
                var id = go.GetInstanceID().ToString();
                instSet.Add(id); // hashset does not need to check exist before add	
            }

            _dirty = true;
        }

        public void Add(string guid)
        {
            if (guidSet.Contains(guid)) return;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Invalid GUID: " + guid);
                return;
            }

            guidSet.Add(guid);
            _dirty = true;
        }

        public void AddRange(params string[] guids)
        {
            foreach (var id in guids)
            {
                Add(id);
            }

            _dirty = true;
        }

        public void Remove(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Remove(id);
            _dirty = true;
        }

        public void Remove(string guidOrInstID)
        {
            guidSet.Remove(guidOrInstID);
            instSet.Remove(guidOrInstID);

            _dirty = true;
        }

        public void Clear()
        {
            guidSet.Clear();
            instSet.Clear();
            _dirty = true;
        }

        public bool IsSelectingAsset => instSet.Count == 0;

        public void Add(FinderRef rf)
        {
            if (rf.isSceneRef)
            {
                Add(rf.component);
            }
            else
            {
                Add(rf.asset.guid);
            }
        }

        public void Remove(FinderRef rf)
        {
            if (rf.isSceneRef)
            {
                Remove(rf.component);
            }
            else
            {
                Remove(rf.asset.guid);
            }
        }

        // ------------ instance

        private bool _dirty;
        private readonly FinderRefDrawer _drawer;
        internal Dictionary<string, FinderRef> refs;
        internal bool isLock;

        public FinderSelection(IWindow window)
        {
            this.Window = window;
            _drawer = new FinderRefDrawer(window);
            _drawer.groupDrawer.hideGroupIfPossible = true;
            _drawer.forceHideDetails = true;
            _drawer.level0Group = string.Empty;

            _dirty = true;
            _drawer.SetDirty();
        }

        public IWindow Window { get; set; }

        public int ElementCount() { return refs == null ? 0 : refs.Count; }

        public bool DrawLayout()
        {
            if (_dirty) RefreshView();
            return _drawer.DrawLayout();
        }

        public bool Draw(Rect rect)
        {
            if (_dirty) RefreshView();
            if (refs == null) return false;

            DrawLock(new Rect(rect.xMax - 12f, rect.yMin - 12f, 16f, 16f));

            return _drawer.Draw(rect);
        }

        public void SetDirty() { _drawer.SetDirty(); }

        private static readonly Color Pro = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color Indie = new Color(0.1f, 0.1f, 0.1f, 1f);

        public void DrawLock(Rect rect)
        {
            GUI2.ContentColor(() =>
                {
                    var icon = isLock ? Uniform.IconContent("LockIcon-On") : Uniform.IconContent("LockIcon");
                    if (GUI2.Toggle(rect, ref isLock, icon))
                    {
                        Window.WillRepaint = true;
                        Window.OnSelectionChange();
                    }
                },
                GUI2.Theme(Pro, Indie));
        }

        public void RefreshView()
        {
            if (refs == null) refs = new Dictionary<string, FinderRef>();
            refs.Clear();

            if (instSet.Count > 0)
            {
                foreach (var instId in instSet)
                {
                    refs.Add(instId, new FinderSceneRef(0, EditorUtility.InstanceIDToObject(int.Parse(instId))));
                }
            }
            else
            {
                foreach (var guid in guidSet)
                {
                    var asset = FinderWindowBase.CacheSetting.Get(guid, false);
                    refs.Add(guid, new FinderRef(0, 0, asset, null) {isSceneRef = false});
                }
            }

            _drawer.SetRefs(refs);
            _dirty = false;
        }
    }
}