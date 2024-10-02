using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Pancake.Sound
{
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public sealed class AudioPickupAttribute : PropertyAttribute
    {
    }
}