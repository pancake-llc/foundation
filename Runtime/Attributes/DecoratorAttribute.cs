using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class DecoratorAttribute : PancakeAttribute
    {
    }
}