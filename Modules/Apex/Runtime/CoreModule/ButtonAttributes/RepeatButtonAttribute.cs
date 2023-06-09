using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RepeatButtonAttribute : MethodButtonAttribute
    {
    }
}