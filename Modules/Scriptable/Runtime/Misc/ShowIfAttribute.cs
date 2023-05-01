using System;
using UnityEngine;

namespace Obvious.Soap.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string conditionFieldName;
        public readonly object comparisonValue;
        
        public ShowIfAttribute(string conditionFieldName)
        {
            this.conditionFieldName = conditionFieldName;
        }
        
        public ShowIfAttribute(string conditionFieldName, object comparisonValue = null)
        {
            this.conditionFieldName = conditionFieldName;
            this.comparisonValue = comparisonValue;
        }
    }
}