using System;
using System.Diagnostics;

namespace Pancake
{
    /// <summary>
    /// Attribute that makes a searchable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public class SearchableAttribute : Attribute
    {
    }
}