using System;

namespace Pancake.BakingSheet
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class NonSerializedAttribute : Attribute
    {
    }
}