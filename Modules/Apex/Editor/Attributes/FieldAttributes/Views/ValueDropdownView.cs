using Pancake.Apex;
using Pancake.ExLib.Reflection;
using Pancake.ExLibEditor.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ValueDropdownAttribute))]
    public sealed class ValueDropdownView : FieldView
    {
        private readonly static object[] EmptyValues = new object[0];

        private Type thisType;

        // Stored callback properties.
        private object target;
        private MemberGetter<object, object> thisGetter;
        private MemberSetter<object, object> thisSetter;
        private MemberGetter<object, object[]> valuesGetter;
        private MethodCaller<object, object> valuesCaller;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            ValueDropdownAttribute attribute = viewAttribute as ValueDropdownAttribute;
            target = serializedField.GetDeclaringObject();

            FieldInfo thisField = serializedField.GetMemberInfo() as FieldInfo;
            thisGetter = thisField.DelegateForGet();
            thisSetter = thisField.DelegateForSet();
            thisType = serializedField.GetMemberType();

            FindCallback(attribute.name);
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            object value = thisGetter.Invoke(target);
            if (GUI.Button(position, value?.ToString() ?? "null", EditorStyles.popup))
            {
                ExSearchWindow searchWindow = ExSearchWindow.Create("Values");

                object[] values = GetValues();
                for (int i = 0; i < values.Length; i++)
                {
                    object obj = values[i];
                    searchWindow.AddEntry(obj.ToString(),
                        () =>
                        {
                            thisSetter.Invoke(ref target, obj);
                            EditorUtility.SetDirty(serializedField.GetSerializedObject().targetObject);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        });
                }

                searchWindow.AddEntry("Reset",
                    () =>
                    {
                        if (thisType.IsValueType)
                        {
                            FieldInfo fieldInfo = serializedField.GetMemberInfo() as FieldInfo;
                            fieldInfo.SetValue(target, null);
                        }
                        else
                        {
                            thisSetter.Invoke(ref target, null);
                        }

                        EditorUtility.SetDirty(serializedField.GetSerializedObject().targetObject);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    });

                searchWindow.Open(position);
            }
        }

        public object[] GetValues()
        {
            object[] values = EmptyValues;
            if (valuesGetter != null)
                values = valuesGetter.Invoke(target);
            else if (valuesCaller != null)
                values = (object[]) valuesCaller.Invoke(target, null);

            if (values is IEnumerable enumerable)
            {
                List<object> temp = new List<object>();
                foreach (object item in enumerable)
                {
                    if (item.GetType() == thisType)
                    {
                        temp.Add(item);
                    }
                }

                return temp.ToArray();
            }

            return values;
        }

        private void FindCallback(string name)
        {
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
            {
                if (memberInfo.Name == name)
                {
                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        valuesGetter = fieldInfo.DelegateForGet<object, object[]>();
                        break;
                    }
                    else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                    {
                        valuesGetter = propertyInfo.DelegateForGet<object, object[]>();
                        break;
                    }
                    else if (memberInfo is MethodInfo methodInfo && methodInfo.GetParameters().Length == 0)
                    {
                        valuesCaller = methodInfo.DelegateForCall();
                        break;
                    }
                }
            }
        }
    }
}