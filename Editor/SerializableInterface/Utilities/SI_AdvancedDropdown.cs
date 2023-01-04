using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Pancake.Editor
{
    internal sealed class SI_AdvancedDropdown : AdvancedDropdown
    {
        private readonly Type _interfaceType;
        private readonly MethodInfo _sortChildrenMethod;
        private readonly bool _canSort;
        private readonly Scene? _relevantScene;
        private readonly SerializedProperty _property;

        public delegate void ItemSelectedDelegate(SerializedProperty property, InterfaceRefMode mode, object reference);

        public event ItemSelectedDelegate ItemSelectedEvent; // Suffixed with Event because of the override

        /// <inheritdoc />
        public SI_AdvancedDropdown(AdvancedDropdownState state, Type interfaceType, Scene? relevantScene, SerializedProperty property)
            : base(state)
        {
            Assert.IsNotNull(interfaceType);

            _sortChildrenMethod = typeof(AdvancedDropdownItem).GetMethod("SortChildren", BindingFlags.Instance | BindingFlags.NonPublic);
            _canSort = _sortChildrenMethod != null;

            minimumSize = new Vector2(0, 300);
            _interfaceType = interfaceType;
            _relevantScene = relevantScene;
            _property = property;
        }

        /// <inheritdoc />
        protected override AdvancedDropdownItem BuildRoot()
        {
            SI_AdvancedDropdownItemWrapper item = new SI_AdvancedDropdownItemWrapper(_interfaceType.Name).AddChild(new SI_AssetsItemBuilder(_interfaceType).Build())
                .AddChild(new SI_ClassesItemBuilder(_interfaceType).Build())
                .AddChild(new SI_SceneItemBuilder(_interfaceType, _relevantScene).Build());

            foreach (AdvancedDropdownItem dropdownItem in item.children)
            {
                dropdownItem.AddChild(new SI_NoneDropdownItem());
            }

            if (_canSort)
            {
                _sortChildrenMethod.Invoke(item, new object[] {(Comparison<AdvancedDropdownItem>) Sort, true});
            }

            return item;
        }

        private int Sort(AdvancedDropdownItem a, AdvancedDropdownItem b)
        {
            // For aesthetic reasons. Always puts the None first
            if (a is SI_NoneDropdownItem)
                return -1;
            if (b is SI_NoneDropdownItem)
                return 1;

            int childrenA = a.children.Count();
            int childrenB = b.children.Count();

            if (childrenA > 0 && childrenB > 0)
                return a.CompareTo(b);
            if (childrenA == 0 && childrenB == 0)
                return a.CompareTo(b);
            if (childrenA > 0 && childrenB == 0)
                return -1;
            return 1;
        }

        /// <inheritdoc />
        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is SI_IDropdownItem dropdownItem)
            {
                ItemSelectedEvent?.Invoke(_property, dropdownItem.Mode, dropdownItem.GetValue());
            }
        }
    }
}