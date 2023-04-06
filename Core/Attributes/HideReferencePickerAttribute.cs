using System;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideReferencePickerAttribute : System.Attribute
    {
    }
}