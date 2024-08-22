using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sisus.Init.EditorOnly
{
    internal class AdvancedDropdownItem : IComparable
    {
        private const int SEPARATOR_INDEX = -1000;

        internal static AdvancedDropdownItem SeparatorItem = new AdvancedDropdownItem(new GUIContent("SEPARATOR"), SEPARATOR_INDEX);

        private static class Styles
        {
            public static GUIStyle itemStyle = new GUIStyle("PR Label");

            static Styles()
            {
                itemStyle.alignment = TextAnchor.MiddleLeft;
                itemStyle.padding = new RectOffset(0, 0, 2, 2);
                itemStyle.margin = new RectOffset(0, 0, 0, 0);
            }
        }

        internal int index = -1;
        internal Vector2 scrollPosition;
        internal int selectedItem = 0;

        protected GUIContent label;
        protected GUIContent labelWhenSearching;
        private string id;
        private AdvancedDropdownItem parent;
        private List<AdvancedDropdownItem> children = new List<AdvancedDropdownItem>();

        public virtual GUIStyle lineStyle => Styles.itemStyle;
        public virtual GUIContent Content => label;
        public virtual GUIContent ContentWhenSearching => labelWhenSearching;
        public string Name => label.text;
        public string Id => id;
        public virtual AdvancedDropdownItem Parent => parent;
        public virtual List<AdvancedDropdownItem> Children => children;
        public bool HasChildren => Children.Any();
        public virtual bool drawArrow => HasChildren;
        public virtual bool searchable { get; set; }        

        public int SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if(value < 0)
                {
                    selectedItem = 0;
                }
                else if(value >= Children.Count)
                {
                    selectedItem = Children.Count - 1;
                }
                else
                {
                    selectedItem = value;
                }
            }
        }

        public AdvancedDropdownItem(GUIContent label, int index) : this(label, label, index) { }

        public AdvancedDropdownItem(GUIContent label, string id, int index)
        {
            this.label = label;
            labelWhenSearching = new GUIContent(id);
            this.id = id;
            this.index = index;
        }

        public AdvancedDropdownItem(GUIContent label, GUIContent labelWhenSearching, int index)
        {
            this.label = label;
            this.labelWhenSearching = labelWhenSearching;
            id = labelWhenSearching.text;
            this.index = index;
        }

        internal void AddChild(AdvancedDropdownItem item)
        {
            Children.Add(item);
        }

        internal void SetParent(AdvancedDropdownItem item)
        {
            parent = item;
        }

        internal void AddSeparator()
        {
            Children.Add(SeparatorItem);
        }

        internal virtual bool IsSeparator() => index == SEPARATOR_INDEX;

        public virtual bool OnAction()
        {
            return true;
        }

        public AdvancedDropdownItem GetSelectedChild()
        {
            if(Children.Count == 0 || selectedItem < 0)
            {
                return null;
            }

            return Children[selectedItem];
        }

        public int GetSelectedChildIndex()
        {
            var i = Children[selectedItem].index;
            if(i >= 0)
            {
                return i;
            }

            return selectedItem;
        }

        public IEnumerable<AdvancedDropdownItem> GetSearchableElements()
        {
            if(searchable)
            {
                yield return this;
            }

            foreach(var child in Children)
            {
                foreach(var searchableChildren in child.GetSearchableElements())
                {
                    yield return searchableChildren;
                }
            }
        }

        public virtual int CompareTo(object o)
        {
            return Name.CompareTo((o as AdvancedDropdownItem).Name);
        }

        public void MoveDownSelection()
        {
            var selectedIndex = SelectedItem;
            do
            {
                ++selectedIndex;
            }
            while(selectedIndex < Children.Count && Children[selectedIndex].IsSeparator());

            if(selectedIndex < Children.Count)
                SelectedItem = selectedIndex;
        }

        public void MoveUpSelection()
        {
            var selectedIndex = SelectedItem;

            do
            {
                --selectedIndex;
            }
            while(selectedIndex >= 0 && Children[selectedIndex].IsSeparator());
            
            if(selectedIndex >= 0)
            {
                SelectedItem = selectedIndex;
            }
        }
    }
}
