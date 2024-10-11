using System;
using System.Diagnostics;

namespace Pancake
{
    /// <summary>
    /// Specify a texture name from your assets which you want to be assigned as an icon to the MonoScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("UNITY_EDITOR")]
    public class EditorIconAttribute : Attribute
    {
        public string Name { get; set; }

        public EditorIconAttribute(string name) { Name = name; }
    }
}