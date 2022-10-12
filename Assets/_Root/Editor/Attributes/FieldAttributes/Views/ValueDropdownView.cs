using Pancake.Editor.Window.Searchable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private FieldInfo source;

        private FieldInfo fieldValues;
        private PropertyInfo propertyValues;
        private MethodInfo methodValues;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            ValueDropdownAttribute attribute = viewAttribute as ValueDropdownAttribute;

            Type type = element.serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            fieldValues = type.GetAllMembers(attribute.member, flags).Where(m => m is FieldInfo).FirstOrDefault() as FieldInfo;

            propertyValues = type.GetAllMembers(attribute.member, flags).Where(m => m is PropertyInfo).FirstOrDefault() as PropertyInfo;

            methodValues = type.GetAllMembers(attribute.member, flags).Where(m => m is MethodInfo).FirstOrDefault() as MethodInfo;

            source = element.memberInfo as FieldInfo;
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            object value = source.GetValue(element.serializedObject.targetObject);
            if (GUI.Button(position, value?.ToString() ?? "null", EditorStyles.popup))
            {
                SearchableWindow searchableWindow = SearchableWindow.Create();
                SearchItem defalutItem = new SearchItem(new GUIContent("Default"), true, false);
                defalutItem.OnClickCallback += () => source.SetValue(element.serializedObject.targetObject, null);
                defalutItem.OnClickCallback += () => EditorUtility.SetDirty(element.serializedObject.targetObject);
                defalutItem.OnClickCallback += () => EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                searchableWindow.AddItem(defalutItem);

                object[] values = GetValues(element.serializedObject.targetObject, source.FieldType);
                for (int i = 0; i < values.Length; i++)
                {
                    object obj = values[i];
                    SearchItem item = new SearchItem(new GUIContent(obj.ToString()), true, false);
                    item.OnClickCallback += () => source.SetValue(element.serializedObject.targetObject, obj);
                    defalutItem.OnClickCallback += () => EditorUtility.SetDirty(element.serializedObject.targetObject);
                    defalutItem.OnClickCallback += () => EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    searchableWindow.AddItem(item);
                }

                searchableWindow.ShowAsDropDown(position);
            }
        }

        public object[] GetValues(object target, Type fieldType)
        {
            if (fieldValues != null)
            {
                object values = fieldValues.GetValue(target);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == fieldType)
                        {
                            temp.Add(item);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (propertyValues != null)
            {
                object values = propertyValues.GetValue(target);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == fieldType)
                        {
                            temp.Add(item);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (methodValues != null)
            {
                object values = methodValues.Invoke(target, null);
                if (values is IEnumerable enumerable)
                {
                    List<object> temp = new List<object>();
                    foreach (object item in enumerable)
                    {
                        if (item.GetType() == fieldType)
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