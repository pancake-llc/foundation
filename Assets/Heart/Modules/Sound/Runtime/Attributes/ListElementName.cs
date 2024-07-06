using UnityEngine;

namespace Pancake.Sound
{
    public class ListElementName : PropertyAttribute
    {
        public readonly string InspectorName;
        public readonly bool IsStringFormat;
        public readonly bool IsStartFromZero;
        public readonly bool IsUsingFirstPropertyValueAsName;

        public ListElementName() { IsUsingFirstPropertyValueAsName = true; }

        public ListElementName(string inspectorName, bool isStringFormat = false, bool indexStartFromZero = true)
        {
            InspectorName = inspectorName;
            IsStringFormat = isStringFormat;
            IsStartFromZero = indexStartFromZero;
            IsUsingFirstPropertyValueAsName = false;
        }
    }
}