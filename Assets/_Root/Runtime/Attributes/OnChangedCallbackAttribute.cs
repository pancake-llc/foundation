using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class OnChangedCallbackAttribute : PancakeAttribute
    {
        public readonly string method;

        /// <param name="method">
        /// <br>Method format: <b>void OnEnable(SerializedProperty property, GUIContent label);</b></br>
        /// </param>
        public OnChangedCallbackAttribute(string method) { this.method = method; }
    }
}