using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SortingLayerAttribute : Attribute
    {
    }
}