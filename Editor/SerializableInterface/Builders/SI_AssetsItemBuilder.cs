using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pancake.Editor
{
    internal sealed class SI_AssetsItemBuilder
    {
        private readonly HashSet<string> _addedItems;
        private readonly Dictionary<string, AdvancedDropdownItem> _splitToItem;
        private readonly Type _interfaceType;

        public SI_AssetsItemBuilder(Type interfaceType)
        {
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);

            _addedItems = new HashSet<string>();
            _splitToItem = new Dictionary<string, AdvancedDropdownItem>();
            _interfaceType = interfaceType;
        }

        public AdvancedDropdownItem Build()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Assets");
            _splitToItem.Add("Assets/", root); // Needs the trailing slash to be recognized later on

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssetPaths)
            {
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (_interfaceType.IsAssignableFrom(assetType))
                {
                    CreateItemForPath(root, assetPath);
                }
                else if (assetType == typeof(GameObject))
                {
                    GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (gameObject.GetComponent(_interfaceType) != null)
                        CreateItemForPath(root, assetPath);
                }
            }

            return root;
        }

        private void CreateItemForPath(AdvancedDropdownItem root, string path)
        {
            if (_addedItems.Contains(path))
                return;

            AdvancedDropdownItem parent = GetOrCreateParentItem(root, path);
            parent.AddChild(new SI_AssetDropdownItem(path));
            _addedItems.Add(path);
        }

        private AdvancedDropdownItem GetOrCreateParentItem(AdvancedDropdownItem root, string path)
        {
            string currentPath = string.Empty;
            string[] splits = path.Split('/');

            AdvancedDropdownItem item = root;

            for (int i = 0; i < splits.Length - 1; i++)
            {
                string split = splits[i];
                currentPath += split + '/';

                if (_splitToItem.TryGetValue(currentPath, out AdvancedDropdownItem foundItem))
                {
                    item = foundItem;
                    continue;
                }

                AdvancedDropdownItem advancedDropdownItem = new AdvancedDropdownItem(split) {icon = SI_IconUtility.FolderIcon};
                item.AddChild(advancedDropdownItem);
                item = advancedDropdownItem;
                _splitToItem.Add(currentPath, advancedDropdownItem);
            }

            return item;
        }
    }
}