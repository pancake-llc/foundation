using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExperimentalAttribute : ApexAttribute
    {
    }
}