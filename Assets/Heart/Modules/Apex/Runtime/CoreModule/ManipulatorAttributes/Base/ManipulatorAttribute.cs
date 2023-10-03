using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public abstract class ManipulatorAttribute : ApexAttribute
    {
    }
}