using System;
using System.Diagnostics;
using UnityEngine;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public sealed class EnumFlagsAttribute : PropertyAttribute
    {
    }
}