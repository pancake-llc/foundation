using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public abstract class EditorMethodAttribute : PancakeAttribute
    {
    }
}