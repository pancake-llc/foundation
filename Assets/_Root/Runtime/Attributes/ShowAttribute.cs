using System;
using UnityEngine;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ShowAttribute : PropertyAttribute
    {
    }
}