using UnityEditor.IMGUI.Controls;

namespace Pancake.Editor
{
    public class SI_NoneDropdownItem : AdvancedDropdownItem, SI_IDropdownItem
    {
        public SI_NoneDropdownItem()
            : base("None")
        {
        }

        InterfaceRefMode SI_IDropdownItem.Mode => InterfaceRefMode.Raw;

        public object GetValue() { return null; }
    }
}