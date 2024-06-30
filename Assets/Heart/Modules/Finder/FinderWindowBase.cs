using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public interface IWindow
    {
        bool WillRepaint { get; set; }
        void Repaint();
        void OnSelectionChange();
    }

    internal interface IRefDraw
    {
        IWindow Window { get; }
        int ElementCount();
        bool DrawLayout();
        bool Draw(Rect rect);
    }

    public abstract class FinderWindowBase : EditorWindow, IWindow
    {
        public bool WillRepaint { get; set; }
        private static UserSetting<AssetCache> cache;
        private static UserSetting<FinderSetting> setting;

        internal static UserSetting<FinderSetting> Setting
        {
            get
            {
                if (setting != null) return setting;
                setting = new UserSetting<FinderSetting>();
                setting.LoadSetting();
                return setting;
            }
        }

        internal static UserSetting<AssetCache> Cache
        {
            get
            {
                if (cache != null) return cache;
                if (!AssetCache.triedToLoadCache) TryLoadCache();
                return cache;
            }
        }

        internal static AssetCache CacheSetting => Cache.Settings;

        public static int TreeIndent { get => Setting.Settings.treeIndent; set => Setting.Settings.treeIndent = value; }

        public static bool ShowReferenceCount { get => Setting.Settings.referenceCount; set => Setting.Settings.referenceCount = value; }
        public static bool ShowUsedByClassed { get => Setting.Settings.showUsedByClassed; set => Setting.Settings.showUsedByClassed = value; }

        public static bool AlternateRowColor { get => Setting.Settings.alternateColor; set => Setting.Settings.alternateColor = value; }

        public static Color32 RowColor { get => Setting.Settings.rowColor; set => Setting.Settings.rowColor = value; }
        public static Color SelectedColor { get => Setting.Settings.selectedColor; set => Setting.Settings.selectedColor = value; }

        public static bool PingRow { get => Setting.Settings.pingRow; set => Setting.Settings.pingRow = value; }

        public static List<string> ListIgnore => Setting.Settings.listIgnore;
        public static bool DisplayFileSize { get => Setting.Settings.displayFileSize; set => Setting.Settings.displayFileSize = value; }
        public static bool DisplayAtlasName { get => Setting.Settings.displayAtlasName; set => Setting.Settings.displayAtlasName = value; }
        public static bool DisplayAssetBundleName { get => Setting.Settings.displayAssetBundleName; set => Setting.Settings.displayAssetBundleName = value; }

        public static HashSet<string> IgnoreAsset
        {
            get
            {
                if (FinderSetting.hashIgnore == null)
                {
                    FinderSetting.hashIgnore = new HashSet<string>();
                    if (ListIgnore == null) return FinderSetting.hashIgnore;

                    for (int i = 0; i < ListIgnore.Count; i++)
                    {
                        if (FinderSetting.hashIgnore.Contains(ListIgnore[i])) continue;

                        FinderSetting.hashIgnore.Add(ListIgnore[i]);
                    }
                }

                return FinderSetting.hashIgnore;
            }
        }

        private static void TryLoadCache()
        {
            AssetCache.triedToLoadCache = true;
            RestoreCacheFromPath();
        }

        private static void RestoreCacheFromPath()
        {
            cache = new UserSetting<AssetCache>();
            cache.LoadSetting();
            FoundCache();
        }

        internal static void FoundCache()
        {
            CacheSetting.ReadFromCache();
            CacheSetting.Check4Changes(false);
        }

        public static void AddIgnore(string path)
        {
            if (string.IsNullOrEmpty(path) || IgnoreAsset.Contains(path) || path == "Assets") return;

            ListIgnore.Add(path);
            FinderSetting.hashIgnore.Add(path);
            AssetType.SetDirtyIgnore();
            CacheHelper.InitIgnore();

            FindAsset.ignoreTs = Time.realtimeSinceStartup;
            FinderSetting.onIgnoreChange?.Invoke();
        }

        public static void RemoveIgnore(string path)
        {
            if (!IgnoreAsset.Contains(path)) return;

            FinderSetting.hashIgnore.Remove(path);
            ListIgnore.Remove(path);
            AssetType.SetDirtyIgnore();
            CacheHelper.InitIgnore();
            FindAsset.ignoreTs = Time.realtimeSinceStartup;
            FinderSetting.onIgnoreChange?.Invoke();
        }

        public static bool IsTypeExcluded(int type) => (Setting.Settings.excludeTypes >> type & 1) != 0;

        public static void ToggleTypeExclude(int type)
        {
            if (IsTypeExcluded(type))
            {
                Setting.Settings.excludeTypes &= ~(1 << type);
            }
            else
            {
                Setting.Settings.excludeTypes |= 1 << type;
            }
        }

        public static int GetExcludeType() => Setting.Settings.excludeTypes;

        public static bool IsIncludeAllType() =>
            Setting.Settings.excludeTypes == 0 || Math.Approximately(Setting.Settings.excludeTypes.Abs(), Math.Pow(2, AssetType.Filters.Length));

        public static void ExcludeAllType() => Setting.Settings.excludeTypes = -1;
        public static void IncludeAllType() => Setting.Settings.excludeTypes = 0;

        public static void DrawFinderSettings()
        {
            if (FinderUtility.DrawToggle(ref Setting.Settings.pingRow, "Full row click to ping"))
            {
                // to_do
            }

            GUILayout.BeginHorizontal();
            {
                if (FinderUtility.DrawToggle(ref Setting.Settings.alternateColor, "Alternate Odd & Even Row Color"))
                {
                    // to_do
                    FinderUtility.RepaintFinderWindows();
                }

                EditorGUI.BeginDisabledGroup(!AlternateRowColor);
                {
                    Color c = EditorGUILayout.ColorField(RowColor);
                    if (!c.Equals(RowColor))
                    {
                        RowColor = c;
                        // to_do
                        FinderUtility.RepaintFinderWindows();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            if (FinderUtility.DrawToggle(ref Setting.Settings.referenceCount, "Show Usage Count in Project panel"))
            {
                // to_do
                FinderUtility.RepaintProjectWindows();
            }

            if (FinderUtility.DrawToggle(ref Setting.Settings.showUsedByClassed, "Show Asset Type in use"))
            {
                // to_do
                FinderUtility.RepaintFinderWindows();
            }
        }

        internal static void FinderDelayCheck4Changes()
        {
            EditorApplication.update -= FinderCheck;
            EditorApplication.update += FinderCheck;
        }

        static void FinderCheck()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            EditorApplication.update -= FinderCheck;
            CacheSetting.Check4Changes(false);
        }

        internal static bool CacheDisabled
        {
            get => CacheSetting.disabled;
            set
            {
                if (CacheSetting.disabled == value) return;

                CacheSetting.disabled = value;
                if (CacheSetting.disabled)
                {
                    CacheSetting.ready = false;
                    CacheSetting.UnregisterAsyncProcess();
                }
                else
                {
                    CacheSetting.Check4Changes(false);
                }
            }
        }

        internal static bool IsCacheReady => Cache != null && CacheSetting.ready;

        internal static bool HasCache => Cache != null;

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(MyGUIContent.FromString("Enable"), !CacheSetting.disabled, () => { CacheSetting.disabled = !CacheSetting.disabled; });
            menu.AddItem(MyGUIContent.FromString("Refresh"),
                false,
                () =>
                {
                    Resources.UnloadUnusedAssets();
                    EditorUtility.UnloadUnusedAssetsImmediate();
                    CacheSetting.Check4Changes(true);
                    FindSceneCache.Api.SetDirty();
                });
        }

        public abstract void OnSelectionChange();
        protected abstract void OnGUI();

        protected bool DrawEnable()
        {
            bool v = CacheSetting.disabled;
            if (v)
            {
                EditorGUILayout.HelpBox("Finder is disabled!", MessageType.Warning);

                if (GUILayout.Button("Enable"))
                {
                    CacheSetting.disabled = !CacheSetting.disabled;
                    Repaint();
                }

                return !CacheSetting.disabled;
            }

            return !CacheSetting.disabled;
        }

        internal static void DrawPriorityGUI()
        {
            float w = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120f;
            AssetCache.priority = EditorGUILayout.IntSlider("  Scan Priority", AssetCache.priority, 0, 5);
            EditorGUIUtility.labelWidth = w;
        }

        internal static void CreateCache()
        {
            cache = new UserSetting<AssetCache>();
            FoundCache();
        }

        internal static void DeleteCache() { Cache.DeleteSetting(); }

        internal static List<string> FindUsage(string[] listGUIDs)
        {
            if (!IsCacheReady) return null;

            var refs = CacheSetting.FindAssets(listGUIDs, true);

            for (var i = 0; i < refs.Count; i++)
            {
                var tmp = FindAsset.FindUsage(refs[i]);

                for (var j = 0; j < tmp.Count; j++)
                {
                    var itm = tmp[j];
                    if (refs.Contains(itm)) continue;

                    refs.Add(itm);
                }
            }

            return refs.Select(item => item.guid).ToList();
        }
    }
}