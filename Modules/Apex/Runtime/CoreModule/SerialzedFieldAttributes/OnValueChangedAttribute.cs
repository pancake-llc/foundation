using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class OnValueChangedAttribute : ApexAttribute
    {
        public readonly string name;

        /// <summary>
        /// <br>Method format: <b>void OnValueChanged()</b></br>
        /// </summary>
        /// <param name="name">Method name.</param>
        public OnValueChangedAttribute(string name) { this.name = name; }
    }
}