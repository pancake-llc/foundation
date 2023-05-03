using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SceneObjectOnlyAttribute : ManipulatorAttribute
    {
    }
}