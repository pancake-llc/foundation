using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public abstract class InlineDecoratorAttribute : PancakeAttribute
    {
    }
}