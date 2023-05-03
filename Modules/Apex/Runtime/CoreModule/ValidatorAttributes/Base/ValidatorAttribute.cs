using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class ValidatorAttribute : ApexAttribute
    {
    }
}