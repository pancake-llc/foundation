using System;
using System.Linq;
using Pancake.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
using Pancake.LogicExpressionParser;
#endif

namespace Pancake
{
    /// <summary>
    /// Hide a field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class HideAttribute : PropertyAttribute
    {
        public HideAttribute(string conditionalExpression)
        {
#if UNITY_EDITOR
            _logicExpression = _parser.Parse(conditionalExpression);
#endif
        }


#if UNITY_EDITOR

        private static Parser _parser = new Parser(new ParsingContext(false), new ExpressionContext(false));
        private readonly LogicExpression _logicExpression;

        [CustomPropertyDrawer(typeof(HideAttribute))]
        private class HideDrawer : BasePropertyDrawer<HideAttribute>
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                if (!attribute._logicExpression.GetResult())
                {
                    return base.GetPropertyHeight(property, label);
                }

                return 0;
            }

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var result = GetAllFields(property.serializedObject.targetObject).ToList();
                foreach (var info in attribute._logicExpression.Context.Variables)
                {
                    var r = result.Filter(_ => _.Name.Equals(info.Key)).FirstOrDefault();
                    if (r != null)
                    {
                        
                    }
                }

                if (!attribute._logicExpression.GetResult()) base.OnGUI(position, property, label);
            }
        }

#endif
    }
}