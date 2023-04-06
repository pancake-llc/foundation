using System;
using System.Diagnostics;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class DeclareHorizontalGroupAttribute : DeclareGroupBaseAttribute
    {
        public float[] Sizes { get; set; }

        public DeclareHorizontalGroupAttribute(string path)
            : base(path)
        {
        }
    }
}