using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UseDefaultEditor : ApexAttribute
    {
    }
}