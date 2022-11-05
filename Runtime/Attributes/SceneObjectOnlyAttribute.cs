using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SceneObjectOnlyAttribute : ManipulatorAttribute
    {
    }
}