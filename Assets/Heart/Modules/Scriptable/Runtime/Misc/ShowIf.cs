using System;
using UnityEngine;

namespace Pancake.Scriptable
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    internal class ShowIfAttribute : PropertyAttribute
    {
        public readonly string conditionFieldName;
        public readonly object comparisonValue;

        public ShowIfAttribute(string conditionFieldName) { this.conditionFieldName = conditionFieldName; }

        public ShowIfAttribute(string conditionFieldName, object comparisonValue = null)
        {
            this.conditionFieldName = conditionFieldName;
            this.comparisonValue = comparisonValue;
        }
    }
}