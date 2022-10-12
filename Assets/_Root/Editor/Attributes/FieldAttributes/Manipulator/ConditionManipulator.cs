using System;
using System.Linq;
using System.Reflection;

namespace Pancake.Editor
{
    public abstract class ConditionManipulator : MemberManipulator
    {
        private readonly static Func<bool> DefaultExpression = () => false;

        private object target;
        private Func<bool> expression;

        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember member, ManipulatorAttribute ManipulatorAttribute)
        {
            ConditionAttribute attribute = ManipulatorAttribute as ConditionAttribute;
            FindExpression(member, attribute, out expression);
        }

        /// <summary>
        /// Called once to find and save expression.
        /// </summary>
        /// <param name="member">Serialized member with ManipulatorAttribute.</param>
        /// <param name="attribute">ConditionAttribute of serialized member.</param>
        /// <param name="expression">Expression which will be evaluated.</param>
        protected virtual void FindExpression(SerializedMember member, ConditionAttribute attribute, out Func<bool> expression)
        {
            target = member.serializedObject.targetObject;
            MemberInfo memberInfo = target.GetType()
                .GetMember(attribute.member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault();

            expression = DefaultExpression;

            if (memberInfo != null)
            {
                if (memberInfo is FieldInfo fieldInfo)
                {
                    if (attribute.comparer == null && fieldInfo.FieldType == typeof(bool))
                    {
                        expression = () => (bool) fieldInfo.GetValue(target);
                    }
                    else
                    {
                        expression = () => fieldInfo.GetValue(target).Equals(attribute.comparer);
                    }
                }
                else if (memberInfo is MethodInfo methodInfo && methodInfo.GetParameters().Length == 0)
                {
                    if (attribute.comparer == null && methodInfo.ReturnType == typeof(bool))
                    {
                        expression = () => (bool) methodInfo.Invoke(target, null);
                    }
                    else
                    {
                        expression = () => methodInfo.Invoke(target, null).Equals(attribute.comparer);
                    }
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    if (attribute.comparer == null && propertyInfo.PropertyType == typeof(bool))
                    {
                        expression = () => (bool) propertyInfo.GetValue(target);
                    }
                    else
                    {
                        expression = () => propertyInfo.GetValue(target).Equals(attribute.comparer);
                    }
                }
            }
        }

        /// <summary>
        /// Evaluate condition expression.
        /// </summary>
        public bool EvaluateExpression() { return expression.Invoke(); }
    }
}