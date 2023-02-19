using System;
using System.Collections.Generic;
using System.Reflection;
using Pancake.Editor;
using UnityEngine;

[assembly: RegisterTypeProcessor(typeof(RectOffsetTypeProcessor), 1)]

namespace Pancake.Editor
{
    public class RectOffsetTypeProcessor : TypeProcessor
    {
        private static readonly string[] DrawnProperties = new[] {"left", "right", "top", "bottom",};

        public override void ProcessType(Type type, List<PropertyDefinition> properties)
        {
            if (type != typeof(RectOffset))
            {
                return;
            }

            for (var i = 0; i < DrawnProperties.Length; i++)
            {
                var propertyName = DrawnProperties[i];
                var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                var propertyDef = PropertyDefinition.CreateForPropertyInfo(i, propertyInfo);

                properties.Add(propertyDef);
            }
        }
    }
}