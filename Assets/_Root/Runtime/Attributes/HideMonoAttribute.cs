using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HideMonoAttribute : PancakeAttribute
    {
    }
}