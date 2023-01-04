using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.Editor
{
    internal sealed class SI_SceneDropdownItem : AdvancedDropdownItem, SI_IDropdownItem
    {
        private readonly Object _component;

        /// <inheritdoc />
        public SI_SceneDropdownItem(Component component)
            : base(component.GetType().Name)
        {
            _component = component;
            icon = SI_IconUtility.GetIconForObject(component) ?? SI_IconUtility.ScriptIcon;
        }

        /// <inheritdoc />
        InterfaceRefMode SI_IDropdownItem.Mode => InterfaceRefMode.Unity;

        /// <inheritdoc />
        object SI_IDropdownItem.GetValue() { return _component; }
    }
}