using UnityEditor.IMGUI.Controls;

namespace UnityInternalBridge
{
    internal class AdvancedDropdownProxy
    {
        public static void SetShowHeader(AdvancedDropdown dropdown, bool showHeader)
        {
            dropdown.m_WindowInstance.showHeader = showHeader;
        }
    }
}