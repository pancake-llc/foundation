using JetBrains.Annotations;
using Pancake.Init.Internal;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Pancake.Editor.Init
{
	internal class TypeDataSource : AdvancedDropdownDataSource
    {
        private const string NullName = "Null";
        private const string ObjectName = "Object";

        private readonly IEnumerable<Type> types;
        private readonly HashSet<string> selectedTypes;
        private readonly char namespaceDelimiter;
        private readonly string title;

        public TypeDataSource(IEnumerable<Type> types, IEnumerable<Type> selectedTypes, bool useGroups, string title)
        {
            this.types = types;
            this.selectedTypes = new HashSet<string>(GetNames(selectedTypes)); 
            namespaceDelimiter = useGroups ? '/' : '\0';
            this.title = title;
        }

		private IEnumerable<string> GetNames(IEnumerable<Type> selectedTypes)
		{
            foreach(var type in selectedTypes)
			{
				yield return GetFullPath(type);
			}
		}

		protected override AdvancedDropdownItem GetData()
        {
            AdvancedDropdownItem root = new TypeDropdownItem(title);

            var enumerator = types.GetEnumerator();

            if(!enumerator.MoveNext())
            {
                return root;
            }
            int index = 0;

            var first = enumerator.Current;
            if(first is null)
            {
                AddLeafItem(root, NullName, NullName, index);
            }
            else
            {
                AddType(root, index, first);
            }

            if(!enumerator.MoveNext())
            {
                return root;
            }
            index++;

            bool addSeparator;
            var second = enumerator.Current;
            if(second == typeof(Object))
            {
                root.AddSeparator();

                AddLeafItem(root, ObjectName, ObjectName, index);
                addSeparator = true;
            }
            else
            {
                AddType(root, index, second);
                addSeparator = false;
            }

            if(!enumerator.MoveNext())
            {
                return root;
            }
            index++;

            if(addSeparator)
            {
                root.AddSeparator();
            }

            do
            {
                var type = enumerator.Current;
                AddType(root, index, type);
                index++;
            }
            while(enumerator.MoveNext());

            return root;
        }

        private void AddType(AdvancedDropdownItem root, int index, Type type)
        {
            var fullPath = GetFullPath(type);
            var pathParts = fullPath.Split('/');

            var parent = root;
            for(int i = 0, lastIndex = pathParts.Length - 1; i <= lastIndex; i++)
            {
                var name = pathParts[i];

                bool isGroup = i < lastIndex;

                if(isGroup)
                {
                    parent = GetOrAddGroup(parent, name);
                    continue;
                }

                AddLeafItem(parent, name, fullPath, index);
            }
        }

        private static AdvancedDropdownItem GetOrAddGroup(AdvancedDropdownItem parent, string name)
        {
            foreach(var existing in parent.Children)
            {
                if(string.Equals(existing.Name, name))
                {
                    return existing;
                }
            }

            var group = new TypeDropdownItem(name, -1);
            group.SetParent(parent);
            parent.AddChild(group);
            return group;
        }

        private void AddLeafItem(AdvancedDropdownItem parent, string name, string fullPath, int index)
        {
            bool selected = selectedTypes.Contains(fullPath);
            var element = new TypeDropdownItem(name, fullPath, selected, index);
            element.searchable = true;
            element.SetParent(parent);
            parent.AddChild(element);
        }

        private string GetFullPath([CanBeNull] Type type) => TypeUtility.ToString(type, namespaceDelimiter);
    }
}