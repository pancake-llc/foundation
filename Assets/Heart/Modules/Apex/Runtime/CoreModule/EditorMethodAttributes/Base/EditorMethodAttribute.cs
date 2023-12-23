using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class EditorMethodAttribute : ApexAttribute
    {
    }
}