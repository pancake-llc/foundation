using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public static class FindAddressable
    {
        [Serializable]
        public class EAddressInfo
        {
            public string address;
            public string bundleGroup;
            public HashSet<string> assetGUIDs;
            public HashSet<string> childGUIDs;
        }

        public enum EAsmStatus
        {
            None,
            AsmNotFound,
            TypeNotFound,
            FieldNotFound,
            AsmOk
        }

        public enum EProjectStatus
        {
            None,
            NoSettings,
            NoGroup,
            Ok
        }

        private static Assembly asm;
        private static Type addressableAssetGroupType;
        private static Type addressableAssetEntryType;

        private static PropertyInfo entriesProperty;
        private static PropertyInfo groupNameProperty;
        private static PropertyInfo addressProperty;
        private static PropertyInfo guidProperty;
        private static PropertyInfo settingsProperty;
        private static PropertyInfo groupsProperty;

        public static bool IsOk => AsmStatus == EAsmStatus.AsmOk && ProjectStatus == EProjectStatus.Ok;

        static FindAddressable() { Scan(); }

        public static void Scan()
        {
            asm = GetAssembly();
            if (asm == null)
            {
                AsmStatus = EAsmStatus.AsmNotFound;
                return;
            }

            var addressableSettingsType = GetAddressableType("UnityEditor.AddressableAssets.Settings.AddressableAssetSettings");
            var addressableSettingsDefaultObjectType = GetAddressableType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
            addressableAssetGroupType = GetAddressableType("UnityEditor.AddressableAssets.Settings.AddressableAssetGroup");
            addressableAssetEntryType = GetAddressableType("UnityEditor.AddressableAssets.Settings.AddressableAssetEntry");

            if (addressableSettingsType == null || addressableSettingsDefaultObjectType == null || addressableAssetGroupType == null || addressableAssetEntryType == null)
            {
                AsmStatus = EAsmStatus.TypeNotFound;
                return;
            }

            entriesProperty = addressableAssetGroupType.GetProperty("entries", BindingFlags.Public | BindingFlags.Instance);
            groupNameProperty = addressableAssetGroupType.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            addressProperty = addressableAssetEntryType.GetProperty("address", BindingFlags.Public | BindingFlags.Instance);
            guidProperty = addressableAssetEntryType.GetProperty("guid", BindingFlags.Public | BindingFlags.Instance);
            settingsProperty = addressableSettingsDefaultObjectType.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static);
            groupsProperty = addressableSettingsType.GetProperty("groups", BindingFlags.Public | BindingFlags.Instance);

            if (entriesProperty == null || groupNameProperty == null || addressProperty == null || guidProperty == null)
            {
                AsmStatus = EAsmStatus.FieldNotFound;
                return;
            }

            AsmStatus = EAsmStatus.AsmOk;
            ProjectStatus = EProjectStatus.None;
        }

        public static EAsmStatus AsmStatus { get; private set; }
        public static EProjectStatus ProjectStatus { get; private set; }

        private static Assembly GetAssembly()
        {
            const string dll = "Unity.Addressables.Editor";
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in allAssemblies)
            {
                if (item.GetName().Name != dll) continue;
                return item;
            }

            return null;
        }

        private static Type GetAddressableType(string typeName) { return asm == null ? null : asm.GetType(typeName); }

        /// <summary>
        /// Get a map between address -> AddressInfo (assetGUIDs + childGUIDs)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, EAddressInfo> GetAddresses()
        {
            if (AsmStatus != EAsmStatus.AsmOk) return null;

            // Get the AddressableAssetSettings instance
            object settings = settingsProperty?.GetValue(null);

            if (settings == null)
            {
                // Debug.LogWarning("Addressable Asset Settings could not be found.");
                ProjectStatus = EProjectStatus.NoSettings;
                return null;
            }

            var addresses = new Dictionary<string, EAddressInfo>();
            var groups = groupsProperty?.GetValue(settings) as IEnumerable<object>;

            if (groups == null)
            {
                ProjectStatus = EProjectStatus.NoGroup; // Debug.LogWarning("No groups found in Addressable Asset Settings.");
                return null;
            }

            ProjectStatus = EProjectStatus.Ok;

            // Loop through each group
            foreach (object group in groups)
            {
                if (group == null || addressableAssetGroupType == null || addressableAssetEntryType == null) continue;

                // Get the group's 'entries' property
                var entries = entriesProperty?.GetValue(group) as IEnumerable<object>;

                if (entries == null) continue;

                // Get the group's 'Name' property
                var groupName = groupNameProperty?.GetValue(group)?.ToString();

                // Loop through each entry in the group
                foreach (object entry in entries)
                {
                    if (entry == null) continue;

                    // Get the entry's 'address' and 'guid' properties
                    var address = addressProperty?.GetValue(entry)?.ToString();
                    var guid = guidProperty?.GetValue(entry)?.ToString();

                    if (address == null || guid == null) continue;

                    if (!addresses.TryGetValue(address, out var fr2Address))
                    {
                        // New address entry
                        fr2Address = new EAddressInfo
                        {
                            address = address, bundleGroup = groupName, assetGUIDs = new HashSet<string>(), childGUIDs = new HashSet<string>()
                        };

                        addresses.Add(address, fr2Address);
                    }

                    if (fr2Address.assetGUIDs.Add(guid)) // folder?
                    {
                        AppendChildGUIDs(fr2Address.childGUIDs, guid);
                    }
                }
            }

            return addresses;
        }

        private static void AppendChildGUIDs(HashSet<string> h, string guid)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(folderPath)) return;
            string[] allGUIDs = AssetDatabase.FindAssets("*", new[] {folderPath});
            foreach (string child in allGUIDs)
            {
                var asset = FinderWindowBase.CacheSetting.Get(child);
                if (asset.IsExcluded || asset.IsMissing || asset.IsScript || asset.type == EFinderAssetType.Unknown) continue;
                if (asset.InEditor || asset.InResources || asset.InStreamingAsset || asset.InPlugins) continue;
                if (asset.Extension == ".asmdef") continue;
                if (asset.Extension == ".wlt") continue;
                if (asset.IsFolder) continue;
                h.Add(child);
            }
        }
    }

    internal class FindAddressableDrawer : IRefDraw
    {
        private const string AUTO_DEPEND_TITLE = "(Auto dependency)";

        internal readonly FindRefDrawer drawer;
        private bool _dirty;
        internal Dictionary<string, FindRef> refs;
        internal readonly Dictionary<string, FindAddressable.EAddressInfo> map = new();
        internal List<string> groups;
        internal float maxWidth;

        public FindAddressableDrawer(IWindow addressWindow, Func<FindRefDrawer.Sort> getSortMode, Func<FindRefDrawer.Mode> getGroupMode)
        {
            Window = addressWindow;
            drawer = new FindRefDrawer(addressWindow, getSortMode, getGroupMode)
            {
                messageNoRefs = "No Addressable Asset",
                messageEmpty = "No Addressable Asset",
                forceHideDetails = true,
                customGetGroup = GetGroup,
                customDrawGroupLabel = DrawGroupLabel,
                beforeItemDraw = BeforeDrawItem,
                afterItemDraw = AfterDrawItem,
            };

            _dirty = true;
            drawer.SetDirty();
        }


        private string GetGroup(FindRef rf) => rf.group;

        private void DrawGroupLabel(Rect r, string label, int childCount)
        {
            var c = GUI.contentColor;
            if (label == AUTO_DEPEND_TITLE)
            {
                var c1 = c;
                c1.a = 0.5f;
                GUI.contentColor = c1;
            }

            GUI.Label(r, MyGUIContent.FromString(label), EditorStyles.boldLabel);
            GUI.contentColor = c;
        }

        private void BeforeDrawItem(Rect r, FindRef rf)
        {
            string guid = rf.asset.guid;
            if (map.TryGetValue(guid, out var address)) return;
            var c = GUI.contentColor;
            c.a = 0.35f;
            GUI.contentColor = c;
        }

        private void AfterDrawItem(Rect r, FindRef rf)
        {
            string guid = rf.asset.guid;
            if (!map.TryGetValue(guid, out var address))
            {
                var c2 = GUI.contentColor;
                c2.a = 1f;
                GUI.contentColor = c2;
                return;
            }

            var c = GUI.contentColor;
            var c1 = c;
            c1.a = 0.5f;

            GUI.contentColor = c1;
            {
                r.xMin = r.xMax - maxWidth;
                GUI.Label(r, MyGUIContent.FromString(address.address), EditorStyles.miniLabel);
            }
            GUI.contentColor = c;
        }

        public IWindow Window { get; set; }

        public int ElementCount() { return refs?.Count ?? 0; }

        public bool Draw(Rect rect)
        {
            if (_dirty) RefreshView();
            if (refs == null) return false;

            rect.yMax -= 24f;
            bool result = drawer.Draw(rect);

            var btnRect = rect;
            btnRect.xMin = btnRect.xMax - 24f;
            btnRect.yMin = btnRect.yMax;
            btnRect.height = 24f;

            if (GUI.Button(btnRect, Uniform.IconContent("d_Refresh@2x").image))
            {
                FindAddressable.Scan();
                RefreshView();
            }

            return result;
        }

        public bool DrawLayout()
        {
            if (_dirty) RefreshView();
            return drawer.DrawLayout();
        }

        public void SetDirty()
        {
            _dirty = true;
            drawer.SetDirty();
        }

        private readonly Dictionary<FindAddressable.EAsmStatus, string> _asmMessage = new()
        {
            {FindAddressable.EAsmStatus.None, "-"},
            {FindAddressable.EAsmStatus.AsmNotFound, "Addressable Package not imported!"},
            {FindAddressable.EAsmStatus.TypeNotFound, "Addressable Classes not found (addressable library code changed?)!"},
            {FindAddressable.EAsmStatus.FieldNotFound, "Addressable Fields not found (addressable library code changed?)!"},
            {FindAddressable.EAsmStatus.AsmOk, "-"}
        };

        private readonly Dictionary<FindAddressable.EProjectStatus, string> _projectStatusMessage = new()
        {
            {FindAddressable.EProjectStatus.None, "-"},
            {
                FindAddressable.EProjectStatus.NoSettings,
                "No Addressables Settings found!\nOpen [Window/Asset Management/Addressables/Groups] to create new Addressables Settings!\n \n"
            },
            {FindAddressable.EProjectStatus.NoGroup, "No AssetBundle Group created!"},
            {FindAddressable.EProjectStatus.Ok, "-"},
        };


        public void RefreshView()
        {
            if (refs == null) refs = new Dictionary<string, FindRef>();
            refs.Clear();

            var addresses = FindAddressable.GetAddresses();
            if (FindAddressable.AsmStatus != FindAddressable.EAsmStatus.AsmOk)
            {
                drawer.messageNoRefs = _asmMessage[FindAddressable.AsmStatus];
            }
            else if (FindAddressable.ProjectStatus != FindAddressable.EProjectStatus.Ok)
            {
                drawer.messageNoRefs = _projectStatusMessage[FindAddressable.ProjectStatus];
            }

            drawer.messageEmpty = drawer.messageNoRefs;

            if (addresses == null) addresses = new Dictionary<string, FindAddressable.EAddressInfo>();
            groups = addresses.Keys.ToList();
            map.Clear();

            if (addresses.Count > 0)
            {
                var maxLengthGroup = string.Empty;
                foreach (var kvp in addresses)
                {
                    foreach (string guid in kvp.Value.assetGUIDs)
                    {
                        if (refs.ContainsKey(guid)) continue;
                        var asset = FinderWindowBase.CacheSetting.Get(guid);
                        refs.Add(guid,
                        new FindRef(0,
                            1,
                            asset,
                            null,
                            null) {isSceneRef = false, group = kvp.Value.bundleGroup});

                        map.Add(guid, kvp.Value);
                        if (maxLengthGroup.Length < kvp.Value.address.Length) maxLengthGroup = kvp.Value.address;
                    }

                    foreach (string guid in kvp.Value.childGUIDs)
                    {
                        if (refs.ContainsKey(guid)) continue;

                        var asset = FinderWindowBase.CacheSetting.Get(guid);
                        refs.Add(guid,
                        new FindRef(0,
                            1,
                            asset,
                            null,
                            null) {isSceneRef = false, group = kvp.Value.bundleGroup});

                        map.Add(guid, kvp.Value);
                        if (maxLengthGroup.Length < kvp.Value.address.Length) maxLengthGroup = kvp.Value.address;
                    }
                }

                maxWidth = EditorStyles.miniLabel.CalcSize(MyGUIContent.FromString(maxLengthGroup)).x + 16f;

                // Find usage
                var usages = FindRef.FindUsage(map.Keys.ToArray());
                foreach (var kvp in usages)
                {
                    if (refs.ContainsKey(kvp.Key)) continue;
                    var v = kvp.Value;

                    // do not take script
                    if (v.asset.IsScript) continue;
                    if (v.asset.IsExcluded) continue;

                    refs.Add(kvp.Key, kvp.Value);
                    kvp.Value.depth = 1;
                    kvp.Value.group = AUTO_DEPEND_TITLE;
                }
            }

            _dirty = false;
            drawer.SetRefs(refs);
        }

        internal void RefreshSort() { drawer.RefreshSort(); }
    }
}