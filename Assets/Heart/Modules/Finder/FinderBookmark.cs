using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace PancakeEditor
{
    public class FinderBookmark : IRefDraw
    {
        private static readonly HashSet<string> GuidSet = new();
        private static readonly HashSet<string> InstSet = new(); // Do not reference directly to SceneObject (which might be destroyed anytime)

        public static int Count => GuidSet.Count + InstSet.Count;

        public static bool Contains(string guidOrInstID) { return GuidSet.Contains(guidOrInstID) || InstSet.Contains(guidOrInstID); }

        public static bool Contains(UnityObject sceneObject)
        {
            var id = sceneObject.GetInstanceID().ToString();
            return InstSet.Contains(id);
        }

        public static bool Contains(FinderRef rf)
        {
            if (rf.isSceneRef) return InstSet != null && InstSet.Contains(rf.component.GetInstanceID().ToString());

            return GuidSet != null && GuidSet.Contains(rf.asset.guid);
        }

        public static void Add(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            InstSet.Add(id); // hashset does not need to check exist before add
            dirty = true;
        }

        public static void Add(string guid)
        {
            if (GuidSet.Contains(guid)) return;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Invalid GUID: " + guid);
                return;
            }

            GuidSet.Add(guid);
            dirty = true;
        }

        public static void Remove(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            InstSet.Remove(id);
            dirty = true;
        }

        public static void Remove(string guidOrInstID)
        {
            GuidSet.Remove(guidOrInstID);
            InstSet.Remove(guidOrInstID);
            dirty = true;
        }

        public static void Clear()
        {
            GuidSet.Clear();
            InstSet.Clear();
            dirty = true;
        }

        public static void Add(FinderRef rf)
        {
            if (rf.isSceneRef) Add(rf.component);
            else Add(rf.asset.guid);
        }

        public static void Remove(FinderRef rf)
        {
            if (rf.isSceneRef) Remove(rf.component);
            else Remove(rf.asset.guid);
        }

        public static void Commit()
        {
            var list = new List<UnityObject>();

            foreach (string guid in GuidSet)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityObject));
                if (obj != null) list.Add(obj);
            }

            foreach (string instID in InstSet)
            {
                int id = int.Parse(instID);
                var obj = EditorUtility.InstanceIDToObject(id);
                if (obj != null) list.Add(obj);
            }

            Selection.objects = list.ToArray();
        }

        // ------------ instance


        private static bool dirty;
        private readonly FinderRefDrawer _drawer;
        internal Dictionary<string, FinderRef> refs = new();

        public FinderBookmark(IWindow window)
        {
            Window = window;
            _drawer = new FinderRefDrawer(window)
            {
                messageNoRefs = "Do bookmark something!", groupDrawer = {hideGroupIfPossible = true}, forceHideDetails = true, level0Group = string.Empty
            };

            dirty = true;
            _drawer.SetDirty();
        }

        public IWindow Window { get; set; }

        public int ElementCount() { return refs?.Count ?? 0; }

        public bool DrawLayout()
        {
            if (dirty) RefreshView();
            return _drawer.DrawLayout();
        }

        public bool Draw(Rect rect)
        {
            if (dirty) RefreshView();
            if (refs == null)
            {
                Debug.Log("Refs is null!");
                return false;
            }

            var bottomRect = new Rect(rect.x + 1f, rect.yMax - 16f, rect.width - 2f, 16f);
            DrawButtons(bottomRect);

            rect.yMax -= 16f;
            return _drawer.Draw(rect);
        }

        public void SetDirty() { _drawer.SetDirty(); }

        private void DrawButtons(Rect rect)
        {
            if (Count == 0) return;

            GUILayout.BeginArea(rect);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Select"))
                    {
                        Commit();
                        Window.WillRepaint = true;
                    }

                    if (GUILayout.Button("Clear"))
                    {
                        Clear();
                        Window.WillRepaint = true;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        public void RefreshView()
        {
            refs ??= new Dictionary<string, FinderRef>();
            refs.Clear();

            foreach (string guid in GuidSet)
            {
                var asset = FinderWindowBase.CacheSetting.Get(guid, false);
                refs.Add(guid, new FinderRef(0, 0, asset, null));
            }

            foreach (string instId in InstSet)
            {
                refs.Add(instId, new FinderSceneRef(0, EditorUtility.InstanceIDToObject(int.Parse(instId))));
            }


            _drawer.SetRefs(refs);

            dirty = false;
        }

        internal void RefreshSort() { _drawer.RefreshSort(); }
    }
}