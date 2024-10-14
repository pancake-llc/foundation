using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public class AssetUsedInBuild : IRefDraw
    {
        private readonly FindRefDrawer _drawer;
        private readonly FindTreeUI2.GroupDrawer _groupDrawer;

        private bool _dirty;
        internal Dictionary<string, FindRef> refs;

        public AssetUsedInBuild(IWindow window, Func<FindRefDrawer.Sort> getSortMode, Func<FindRefDrawer.Mode> getGroupMode)
        {
            this.Window = window;
            _drawer = new FindRefDrawer(window, getSortMode, getGroupMode) {messageNoRefs = "No scene enabled in Build Settings!"};

            _dirty = true;
            _drawer.SetDirty();
        }

        public IWindow Window { get; set; }


        public int ElementCount() { return refs?.Count ?? 0; }

        public bool Draw(Rect rect)
        {
            if (_dirty) RefreshView();
            return _drawer.Draw(rect);
        }

        public bool DrawLayout()
        {
            if (_dirty) RefreshView();
            return _drawer.DrawLayout();
        }

        public void SetDirty()
        {
            _dirty = true;
            _drawer.SetDirty();
        }

        public void RefreshView()
        {
            var scenes = new HashSet<string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene == null) continue;
                if (scene.enabled == false) continue;
                string sce = AssetDatabase.AssetPathToGUID(scene.path);
                if (scenes.Contains(sce)) continue;
                scenes.Add(sce);
            }

            refs = new Dictionary<string, FindRef>();
            var directRefs = FindRef.FindUsage(scenes.ToArray());
            foreach (string scene in scenes)
            {
                if (!directRefs.TryGetValue(scene, out var asset)) continue;
                asset.depth = 1;
            }

            var list = FinderWindowBase.CacheSetting.assetList;
            int count = list.Count;

            // Collect assets in Resources / Streaming Assets
            for (var i = 0; i < count; i++)
            {
                var item = list[i];
                if (item.InEditor) continue;
                if (item.IsExcluded) continue;
                if (item.IsFolder) continue;
                if (!item.AssetPath.StartsWith("Assets/", StringComparison.Ordinal)) continue;

                if (item.InResources || item.InStreamingAsset || item.InPlugins || item.ForcedIncludedInBuild || !string.IsNullOrEmpty(item.AssetBundleName) ||
                    !string.IsNullOrEmpty(item.AtlasName))
                {
                    if (refs.ContainsKey(item.guid)) continue;
                    refs.Add(item.guid, new FindRef(0, 1, item, null));
                }
            }

            // Collect direct references
            foreach (var kvp in directRefs)
            {
                var item = kvp.Value.asset;
                if (item.InEditor) continue;
                if (item.IsExcluded) continue;
                if (!item.AssetPath.StartsWith("Assets/", StringComparison.Ordinal)) continue;
                if (refs.ContainsKey(item.guid)) continue;
                refs.Add(item.guid, new FindRef(0, 1, item, null));
            }

            _drawer.SetRefs(refs);
            _dirty = false;
        }

        internal void RefreshSort() { _drawer.RefreshSort(); }
    }
}