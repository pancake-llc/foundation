using System;
using System.Diagnostics;

namespace Pancake.Sound
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class AudioPickupAttribute : Attribute
    {
    }
}