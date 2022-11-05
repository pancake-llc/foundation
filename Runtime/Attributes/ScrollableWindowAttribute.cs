using System;
using UnityEditor;
using UnityEngine;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ScrollableWindowAttribute : PancakeAttribute
    {
    }
}