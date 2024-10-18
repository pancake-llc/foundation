namespace Pancake
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class TagAttribute : Attribute
    {
    }
}