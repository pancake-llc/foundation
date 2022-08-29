using System;

namespace Pancake.SaveData
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class ArchiveInclude : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class ArchiveIgnore : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Properties : Attribute
    {
        public readonly string[] members;

        public Properties(params string[] members) { this.members = members; }
    }
}