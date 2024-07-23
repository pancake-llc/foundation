using System.Collections.Generic;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal class DataSource : AdvancedDropdownDataSource
	{
		private const string NullName = "Null";

		private readonly IEnumerable<string> names;
		private readonly HashSet<string> selected;
		private readonly string title;

		public DataSource(IEnumerable<string> names, IEnumerable<string> selected, string title)
		{
			this.names = names;
			this.selected = new HashSet<string>(selected);
			this.title = title;
		}

		protected override AdvancedDropdownItem GetData()
		{
			AdvancedDropdownItem root = new DropdownItem(title);

			var names = this.names.GetEnumerator();

			if(!names.MoveNext())
			{
				return root;
			}
			int index = 0;

			var first = names.Current;
			if(first is null)
			{
				AddLeafItem(root, new GUIContent(NullName), NullName, index);
			}
			else
			{
				AddItem(root, index, first);
			}

			if(!names.MoveNext())
			{
				return root;
			}
			index++;

			do
			{
				var type = names.Current;
				AddItem(root, index, type);
				index++;
			}
			while(names.MoveNext());

			return root;
		}

		private void AddItem(AdvancedDropdownItem root, int index, string fullPath)
		{
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

				AddLeafItem(parent, new GUIContent(name), fullPath, index);
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

			var group = new DropdownItem(name, -1);
			group.SetParent(parent);
			parent.AddChild(group);
			return group;
		}

		private void AddLeafItem(AdvancedDropdownItem parent, GUIContent label, string fullPath, int index)
		{
			bool selected = this.selected.Contains(fullPath);
			var element = new DropdownItem(label, fullPath, selected, index);
			element.searchable = true;
			element.SetParent(parent);
			parent.AddChild(element);
		}
	}
}