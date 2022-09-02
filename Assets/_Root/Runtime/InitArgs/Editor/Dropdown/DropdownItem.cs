using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.Init
{
	internal sealed class DropdownItem : AdvancedDropdownItem
    {
        private string menuPath;

        private static class Styles
        {
            public static GUIStyle itemStyle = new GUIStyle("PR Label");
            public static Texture2D groupIcon;
            public static Texture2D valueIcon;
            public static Texture2D selectedIcon;

            static Styles()
            {
                itemStyle.alignment = TextAnchor.MiddleLeft;
                itemStyle.padding.left = 0;
                itemStyle.fixedHeight = 20;
                itemStyle.margin = new RectOffset(0, 0, 0, 0);

                groupIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
                valueIcon = null;
                selectedIcon = EditorGUIUtility.IconContent("Valid").image as Texture2D;
            }
        }

        public override GUIStyle lineStyle => Styles.itemStyle;

        internal DropdownItem(string menuTitle) : base(menuTitle, -1)
        {
        }

        public DropdownItem(string path, int index) : base(path, index)
        {
            content = new GUIContent(path, Styles.groupIcon);
        }

        public DropdownItem(string name, string menuPath, int index) : base(name, index)
        {
            this.menuPath = menuPath;
            content = new GUIContent(name, Styles.valueIcon);
            contentWhenSearching = new GUIContent(content);
        }

        public DropdownItem(string name, string menuPath, bool selected, int index) : base(name, index)
        {
            this.menuPath = menuPath;
            content = new GUIContent(name, selected ? Styles.selectedIcon : null);
            contentWhenSearching = new GUIContent(content);
        }

        public override bool OnAction()
        {
            var values = DropdownWindow.values;
            var value = values.ElementAtOrDefault(index);
            DropdownWindow.instance.OnValueSelected(value);
            return true;
        }

        public override string ToString()
        {
            return menuPath;
        }
    }
}
