using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class ViewAttribute : ApexAttribute
    {
    }
}