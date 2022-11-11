using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class DeclareBoxGroupAttribute : DeclareGroupBaseAttribute
    {
        public DeclareBoxGroupAttribute(string path)
            : base(path)
        {
        }

        public string Title { get; set; }
        public bool Foldout { get; set; }
    }
}