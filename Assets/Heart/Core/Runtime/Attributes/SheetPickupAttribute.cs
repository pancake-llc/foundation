using System;
using System.Diagnostics;

namespace Pancake
{
    /// <summary>
    /// provides a way to select the name of the class from a collected list of names of all classes that inherit from T
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class SheetPickupAttribute : Attribute
    {
    }
}