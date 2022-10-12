using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ScrollableWindowAttribute : PancakeAttribute
    {
    }
}