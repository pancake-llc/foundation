using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public abstract class InlineDecoratorAttribute : ApexAttribute
    {
    }
}