using System;

namespace Pancake
{
    /// <summary>
    /// Attribute that makes an searchable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class SearchableAttribute : System.Attribute
    {
    }
}