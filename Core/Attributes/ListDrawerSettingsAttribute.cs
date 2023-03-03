using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Conditional("UNITY_EDITOR")]
    public class ListDrawerSettingsAttribute : System.Attribute
    {
        public bool Draggable { get; set; } = true;
        public bool HideAddButton { get; set; }
        public bool HideRemoveButton { get; set; }
        public bool AlwaysExpanded { get; set; }
        public bool MultiSelect { get; set; }
    }
}