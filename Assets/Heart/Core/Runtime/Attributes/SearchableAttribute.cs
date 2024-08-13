using System;
using System.Diagnostics;

namespace Pancake
{
    /// <summary>
    /// Attribute that makes a searchable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class SearchableAttribute : Attribute
    {
    }
}