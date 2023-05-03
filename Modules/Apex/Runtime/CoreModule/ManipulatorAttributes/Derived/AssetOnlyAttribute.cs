using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class AssetOnlyAttribute : ManipulatorAttribute
    {
    }
}