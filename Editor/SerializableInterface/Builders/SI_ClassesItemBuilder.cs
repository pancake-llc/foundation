using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Assertions;

namespace Pancake.Editor
{
    internal sealed class SI_ClassesItemBuilder
    {
        private readonly Dictionary<string, AdvancedDropdownItem> _splitToItem;
        private readonly Type _interfaceType;

        public SI_ClassesItemBuilder(Type interfaceType)
        {
            Assert.IsNotNull(interfaceType);
            Assert.IsTrue(interfaceType.IsInterface);

            _splitToItem = new Dictionary<string, AdvancedDropdownItem>();
            _interfaceType = interfaceType;
        }

        public AdvancedDropdownItem Build()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem("Classes");

            TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(_interfaceType);
            foreach (Type type in types)
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.IsSubclassOf(typeof(UnityEngine.Object))) continue;

                AdvancedDropdownItem parent = GetOrCreateParentItem(type, root);
                parent.AddChild(new SI_ClassDropdownItem(type));
            }

            return root;
        }

        private AdvancedDropdownItem GetOrCreateParentItem(Type type, AdvancedDropdownItem root)
        {
            Assert.IsNotNull(type);
            Assert.IsNotNull(root);
            Assert.IsNotNull(_splitToItem);

            if (string.IsNullOrEmpty(type.Namespace))
                return root;

            string[] splits = type.Namespace.Split('.');

            AdvancedDropdownItem splitItem = root;

            string currentPath = string.Empty;
            foreach (string split in splits)
            {
                currentPath += split + '.';

                if (_splitToItem.TryGetValue(currentPath, out AdvancedDropdownItem foundItem))
                {
                    splitItem = foundItem;
                    continue;
                }

                AdvancedDropdownItem newSplitItem = new AdvancedDropdownItem(split) {icon = SI_IconUtility.FolderIcon};
                _splitToItem.Add(currentPath, newSplitItem);
                splitItem.AddChild(newSplitItem);
                splitItem = newSplitItem;
            }

            return splitItem;
        }
    }
}