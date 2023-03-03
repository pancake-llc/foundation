using System;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class HideReferencePickerAttribute : System.Attribute
    {
    }
}