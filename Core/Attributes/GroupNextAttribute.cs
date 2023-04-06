using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Pancake.Attribute
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class GroupNextAttribute : System.Attribute
    {
        public GroupNextAttribute(string path) { Path = path; }

        [CanBeNull] public string Path { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    [Conditional("UNITY_EDITOR")]
    public class UnGroupNextAttribute : GroupNextAttribute
    {
        public UnGroupNextAttribute()
            : base(null)
        {
        }
    }
}