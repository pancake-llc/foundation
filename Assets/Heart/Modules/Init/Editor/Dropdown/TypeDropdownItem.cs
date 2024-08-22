using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
{
	internal sealed class TypeDropdownItem : AdvancedDropdownItem
	{
		private static class Styles
		{
			public static GUIStyle itemStyle = new GUIStyle("PR Label");
			public static Texture2D groupIcon;
			public static Texture2D typeIcon;
			public static Texture2D selectedIcon;

			static Styles()
			{
				itemStyle.alignment = TextAnchor.MiddleLeft;
				itemStyle.padding.left = 0;
				itemStyle.fixedHeight = 20;
				itemStyle.margin = new RectOffset(0, 0, 0, 0);

				groupIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
				typeIcon = null;
				selectedIcon = EditorGUIUtility.IconContent("Valid").image as Texture2D;
			}
		}

		private readonly string fullPath;

		public override GUIStyle lineStyle => Styles.itemStyle;

		public static TypeDropdownItem CreateGroup(string path) => new TypeDropdownItem(path)
		{
			label = new GUIContent(path, Styles.groupIcon)
		};

		public TypeDropdownItem(string menuTitle) : base(new GUIContent(menuTitle), -1) { }

		public TypeDropdownItem(GUIContent label, string fullPath, int index) : base(label, index)
		{
			this.fullPath = fullPath;
			base.label = label;
			labelWhenSearching = new GUIContent(base.label);
		}

		public TypeDropdownItem(GUIContent label, string fullPath, bool selected, int index) : base(label, index)
		{
			this.fullPath = fullPath;
			this.label = selected ? new GUIContent(label.text, Styles.selectedIcon) : label;
			labelWhenSearching = new GUIContent(label);
		}

		public override bool OnAction()
		{
			var types = TypeDropdownWindow.types;
			var type = types.ElementAtOrDefault(index);
			TypeDropdownWindow.instance.OnTypeSelected(type);
			return true;
		}

		public override string ToString()
		{
			return fullPath;
		}
	}
}