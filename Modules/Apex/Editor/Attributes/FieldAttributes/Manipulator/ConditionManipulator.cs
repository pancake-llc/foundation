using Pancake.Apex;
using System;
using Pancake.ExLib.Reflection;
using System.Reflection;
using Vexe.Runtime.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    public abstract class ConditionManipulator : MemberManipulator
    {
        private Func<bool> expression;

        /// <summary>
        /// Called once when initializing member manipulator.
        /// </summary>
        /// <param name="serializedMember">Serialized member with ManipulatorAttribute.</param>
        /// <param name="ManipulatorAttribute">ManipulatorAttribute of serialized member.</param>
        public override void Initialize(SerializedMember serializedMember, ManipulatorAttribute ManipulatorAttribute)
        {
            ConditionAttribute attribute = ManipulatorAttribute as ConditionAttribute;
            FindExpression(serializedMember.GetDeclaringObject(), attribute.name, attribute.arg);
        }

        /// <summary>
        /// Called once to find and save expression.
        /// </summary>
        /// <param name="serializedMember">Serialized member with ManipulatorAttribute.</param>
        /// <param name="attribute">ConditionAttribute of serialized member.</param>
        /// <param name="expression">Expression which will be evaluated.</param>
        protected virtual void FindExpression(object target, string name, object arg)
        {
            expression = () => false;

            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
            {
                if (memberInfo.Name == name)
                {
                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        if (arg == null && fieldInfo.FieldType == typeof(bool))
                        {
                            MemberGetter<object, object> getter = fieldInfo.DelegateForGet<object, object>();
                            expression = () => (bool) getter.Invoke(target);
                        }
                        else
                        {
                            MemberGetter<object, object> comparer = fieldInfo.DelegateForGet<object, object>();
                            expression = () => comparer.Invoke(target).Equals(arg);
                        }

                        break;
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                    {
                        if (arg == null && propertyInfo.PropertyType == typeof(bool))
                        {
                            MemberGetter<object, object> getter = propertyInfo.DelegateForGet<object, object>();
                            expression = () => (bool) getter.Invoke(target);
                        }
                        else
                        {
                            MemberGetter<object, object> comparer = propertyInfo.DelegateForGet<object, object>();
                            expression = () => comparer.Invoke(target).Equals(arg);
                        }

                        break;
                    }
                    else if (memberInfo is MethodInfo methodInfo)
                    {
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        if (parameters.Length == 0)
                        {
                            if (arg == null && methodInfo.ReturnType == typeof(bool))
                            {
                                MethodCaller<object, object> caller = methodInfo.DelegateForCall<object, object>();
                                expression = () => (bool) caller.Invoke(target, null);
                            }
                            else
                            {
                                MethodCaller<object, object> comparer = methodInfo.DelegateForCall<object, object>();
                                expression = () => comparer.Invoke(target, null).Equals(arg);
                            }
                        }
                        else if (parameters.Length == 1 && arg != null && methodInfo.ReturnType == typeof(bool))
                        {
                            MethodCaller<object, object> caller = methodInfo.DelegateForCall<object, object>();
                            expression = () => (bool) caller.Invoke(target, new object[1] {arg});
                        }

                        break;
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