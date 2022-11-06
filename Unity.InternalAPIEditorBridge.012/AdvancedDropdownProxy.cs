using UnityEditor.IMGUI.Controls;

namespace InspectorUnityInternalBridge
{
    internal class AdvancedDropdownProxy
    {
        public static void SetShowHeader(AdvancedDropdown dropdown, bool showHeader)
        {
            dropdown.m_WindowInstance.showHeader = showHeader;
        }
    }
}