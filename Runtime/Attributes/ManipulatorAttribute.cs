using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false)]
    public abstract class ManipulatorAttribute : PancakeAttribute
    {
    }
}