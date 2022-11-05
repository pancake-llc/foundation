using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class DrawerAttribute : PancakeAttribute
    {
    }
}