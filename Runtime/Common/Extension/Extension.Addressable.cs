#if PANCAKE_ADDRESSABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif


namespace Pancake
{
    public static partial class C
    {
        public static void LocationMap()
        {
            var locToKeys = new Dictionary<string, List<object>>();
            foreach (IResourceLocator locator in Addressables.ResourceLocators)
            {
                ResourceLocationMap map = locator as ResourceLocationMap;
                if (map == null) continue;
                foreach (KeyValuePair<object, IList<IResourceLocation>> keyToLocs in map.Locations)
                {
                    foreach (IResourceLocation loc in keyToLocs.Value)
                    {
                        Debug.Log(loc.PrimaryKey);
                        if (!locToKeys.ContainsKey(loc.InternalId)) locToKeys.Add(loc.InternalId, new List<object>() {keyToLocs.Key});
                        else locToKeys[loc.InternalId].Add(keyToLocs.Key);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public static bool IsAddressableWithLabel(this GameObject gameObject, string label)
        {
            bool flag = false;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return false;
            if (settings.GetLabels().Contains(label)) settings.AddLabel(label);
            AddressableAssetGroup group = settings.FindGroup("Default Local Group");
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            var guid = AssetDatabase.AssetPathToGUID(path);

            AddressableAssetEntry entry = null;
            foreach (var addressableAssetEntry in group.entries)
            {
                if (addressableAssetEntry.guid == guid)
                {
                    flag = true;
                    entry = addressableAssetEntry;
                    break;
                }
            }

            if (flag) flag = entry.labels.Contains(label);

            return flag;
        }

        public static void MarkAddressableWithLabel(this GameObject gameObject, string label)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (!settings.GetLabels().Contains(label)) settings.AddLabel(label);
            AddressableAssetGroup group = settings.FindGroup("Default Local Group");
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid)) return;
            var entry = settings.CreateOrMoveEntry(guid, group);

            if (!entry.labels.Contains(label)) entry.labels.Add(label);
            entry.address = gameObject.name;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}
#endif