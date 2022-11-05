using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UseDefaultEditor : PancakeAttribute
    {
    }
}