using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class ValidatorAttribute : PancakeAttribute
    {
    }
}