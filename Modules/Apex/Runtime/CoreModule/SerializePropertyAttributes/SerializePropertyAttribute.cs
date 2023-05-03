using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SerializePropertyAttribute : ApexAttribute
    {
    }
}