using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class OnChangedCallbackAttribute : PancakeAttribute
    {
        public readonly string method;

        /// <param name="method">
        /// <br>Method format: <b>void OnChanged();</b></br>
        /// <br>Method format: <b>void OnChanged(SerializedProperty property);</b></br>
        /// </param>
        public OnChangedCallbackAttribute(string method) { this.method = method; }
    }
}