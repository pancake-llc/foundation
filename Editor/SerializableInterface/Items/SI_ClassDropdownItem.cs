using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace Pancake.Editor
{
    internal sealed class SI_ClassDropdownItem : AdvancedDropdownItem, SI_IDropdownItem
    {
        private readonly Type _type;

        /// <inheritdoc />
        public SI_ClassDropdownItem(Type type)
            : base(type.Name)
        {
            _type = type;
            enabled = type.GetConstructors().Any(x => x.GetParameters().Length == 0);
            icon = SI_IconUtility.ScriptIcon;
        }

        /// <inheritdoc />
        InterfaceRefMode SI_IDropdownItem.Mode => InterfaceRefMode.Raw;

        /// <inheritdoc />
        object SI_IDropdownItem.GetValue() { return Activator.CreateInstance(_type); }
    }
}