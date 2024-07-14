using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sisus.Init.EditorOnly.Internal
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

        internal DropdownItem(string menuTitle) : base(new GUIContent(menuTitle), -1)
        {
        }

        public DropdownItem(string label, int index) : base(new GUIContent(label), index)
        {
            base.label = new GUIContent(label, Styles.groupIcon);
        }

        public DropdownItem(GUIContent label, string menuPath, int index) : base(label, index)
        {
            this.menuPath = menuPath;
            base.label = label;
			labelWhenSearching = new GUIContent(base.label);
        }

        public DropdownItem(GUIContent name, string menuPath, bool selected, int index) : base(name, index)
        {
            this.menuPath = menuPath;
            label =  selected ? new GUIContent(name.text, Styles.selectedIcon) : name;
            labelWhenSearching = new GUIContent(label);
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
