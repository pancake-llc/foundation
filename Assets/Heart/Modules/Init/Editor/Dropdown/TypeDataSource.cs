using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class TypeDataSource : AdvancedDropdownDataSource
	{
		private readonly IEnumerable<Type> types;
		private readonly Func<Type, (string fullPath, Texture icon)> itemContentGetter;
		private readonly HashSet<string> selectedItemPaths;
		private readonly string title;
		public TypeDataSource(IEnumerable<Type> types, IEnumerable<Type> selectedTypes, string title, [AllowNull] Func<Type, (string fullPath, Texture icon)> itemContentGetter)
		{
			this.types = types;
			this.title = title;
			this.itemContentGetter = itemContentGetter;
			selectedItemPaths = new HashSet<string>(GetFullPaths(selectedTypes));
		}

		private IEnumerable<string> GetFullPaths(IEnumerable<Type> selectedTypes)
		{
			foreach(var type in selectedTypes)
			{
				yield return itemContentGetter(type).fullPath;
			}
		}

		protected override AdvancedDropdownItem GetData()
		{
			AdvancedDropdownItem root = new TypeDropdownItem(title);
			int index = 0;
			foreach(var type in types)
			{
				AddType(root, index, type);
				index++;
			}

			return root;
		}

		private void AddType(AdvancedDropdownItem root, int index, Type type)
		{
			if(type == typeof(Separator))
			{
				root.AddSeparator();
				return;
			}

			(string fullPath, Texture icon) = itemContentGetter(type);
			fullPath = fullPath.Replace(".", " > ");
			var pathParts = fullPath.Split('/');
			var parent = root;
			int lastIndex = pathParts.Length - 1;
			for(int i = 0; i < lastIndex; i++)
			{
				parent = GetOrAddGroup(parent, pathParts[i]);
			}

			AddLeafItem(parent, new GUIContent(pathParts[lastIndex], icon), fullPath, index);
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

			var group = TypeDropdownItem.CreateGroup(name);
			group.SetParent(parent);
			parent.AddChild(group);
			return group;
		}

		private void AddLeafItem(AdvancedDropdownItem parent, GUIContent label, string fullPath, int index)
		{
			bool selected = selectedItemPaths.Contains(fullPath);
			var element = new TypeDropdownItem(label, fullPath, selected, index);
			element.searchable = true;
			element.SetParent(parent);
			parent.AddChild(element);
		}
	}
}