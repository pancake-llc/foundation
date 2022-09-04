#if PANCAKE_ADDRESSABLE_SUPPORT
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Pancake.Editor
{
    public static partial class InEditor
    {
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
            var settings = AddressableAssetSettingsDefaultObject.Settings;
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
    }
}

#endif