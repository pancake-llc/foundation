using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ValueDropdownAttribute))]
    sealed class ValueDropdownView : FieldView
    {
        private readonly static object[] EmptyValues = new object[0];

        private object target;
        private FieldInfo sourceField;
        private MemberInfo memberInfo;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            ValueDropdownAttribute attribute = viewAttribute as ValueDropdownAttribute;

            sourceField = serializedField.GetMemberInfo() as FieldInfo;
            target = serializedField.GetMemberTarget();

            Type type = target.GetType();
            foreach (MemberInfo memberInfo in type.AllMembers())
            {
                if (memberInfo.Name == attribute.member)
                {
                    this.memberInfo = memberInfo;
                    break;
                }
            }
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

            object value = sourceField.GetValue(target);
            if (GUI.Button(position, value?.ToString() ?? "null", EditorStyles.popup))
            {
                ExSearchWindow searchWindow = ExSearchWindow.Create();

                object[] values = GetValues();
                for (int i = 0; i < values.Length; i++)
                {
                    object obj = values[i];

                    searchWindow.AddEntry(obj.ToString(),
                        () =>
                        {
                            sourceField.SetValue(serializedField.GetMemberTarget(), obj);
                            EditorUtility.SetDirty(serializedField.GetSerializedObject().targetObject);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        });
                }

                searchWindow.AddEntry("Reset",
                    () =>
                    {
                        sourceField.SetValue(serializedField.GetMemberTarget(), null);
                        EditorUtility.SetDirty(serializedField.GetSerializedObject().targetObject);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    });

                searchWindow.Open(position);
            }
        }

        public object[] GetValues()
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                object values = fieldInfo.GetValue(target);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == sourceField.FieldType)
                        {
                            temp.Add(item);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                object values = propertyInfo.GetValue(target);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == sourceField.FieldType)
                        {
                            temp.Add(item);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (memberInfo is MethodInfo methodInfo)
            {
                object values = methodInfo.Invoke(target, null);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == sourceField.FieldType)
                        {
                            temp.Add(item);
                        }
                    }

                    return temp.ToArray();
                }
            }

            return EmptyValues;
        }
    }
}