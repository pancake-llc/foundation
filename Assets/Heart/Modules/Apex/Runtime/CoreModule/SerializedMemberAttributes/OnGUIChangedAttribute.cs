using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class OnGUIChangedAttribute : ApexAttribute
    {
        public readonly string name;

        /// <param name="method">
        /// <br>Method format: <b>void OnChanged()</b></br>
        /// </param>
        public OnGUIChangedAttribute(string method) { this.name = method; }
    }
}