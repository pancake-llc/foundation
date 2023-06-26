using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Reflection;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

#endif

namespace Pancake.Sensor
{
    public interface IObservable
    {
        event Action OnChanged;
    }

    // Base class implemented by all Observable types.
    public abstract class Observable : IObservable
    {
        public abstract event Action OnChanged;

        // Can be called in a MonoBehaviours OnValidate section so events can fire after an UNDO operation
        public abstract void OnValidate();

        protected abstract string ValuePropName { get; }

        protected abstract void OnBeginGui();

#if UNITY_EDITOR
        /* A Property Drawer for all subclasses of ObservableBase. It simply draws the underlying data type, such
         * that the fact its an Observable is hidden in the inspector.
         */
        [CustomPropertyDrawer(typeof(Observable), true)]
        public class ObservableDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var obs = fieldInfo.GetValue(GetParent(property)) as Observable;

                // Instruct the observable to prepare for its value to be changed in the inspector
                obs.OnBeginGui();

                var val = property.FindPropertyRelative(obs.ValuePropName);

                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(position, val, label, true);

                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();
                    // Will cause the observable to fire any events caused by this change
                    obs.OnValidate();
                }
            }

            /* The complexity here is to accurately calculate the height of the drawer when its nested inside classes or
             * arrays in the serialized object.
             */
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var obs = fieldInfo.GetValue(GetParent(property)) as Observable;
                SerializedProperty val = property.FindPropertyRelative(obs.ValuePropName);
                return EditorGUI.GetPropertyHeight(val, label, true);
            }

            public static object GetParent(SerializedProperty prop)
            {
                var path = prop.propertyPath.Replace(".Array.data[", "[");
                object obj = prop.serializedObject.targetObject;
                var elements = path.Split('.');
                foreach (var element in elements.Take(elements.Length - 1))
                {
                    if (element.Contains("["))
                    {
                        var elementName = element.Substring(0, element.IndexOf("["));
                        var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                        obj = GetValue(obj, elementName, index);
                    }
                    else
                    {
                        obj = GetValue(obj, element);
                    }
                }

                return obj;
            }

            static object GetValue(object source, string name)
            {
                if (source == null)
                    return null;
                var type = source.GetType();
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f == null)
                {
                    var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (p == null)
                        return null;
                    return p.GetValue(source, null);
                }

                return f.GetValue(source);
            }

            static object GetValue(object source, string name, int index)
            {
                var enumerable = GetValue(source, name) as IEnumerable;
                var enm = enumerable.GetEnumerator();
                while (index-- >= 0)
                    enm.MoveNext();
                return enm.Current;
            }
        }

#if ODIN_INSPECTOR
        /* An alternative implementation for Odin Inspector. The Unity drawer above has some
         * issues when using Odin.
         */
        public class ObservableOdinDrawer<T> : OdinValueDrawer<T> where T : Observable {

            protected override void DrawPropertyLayout(GUIContent label) {
                var obs = ValueEntry.SmartValue;

                obs.OnBeginGui();

                var val = ValueEntry.Property.FindChild(
                    delegate (InspectorProperty obj) { return obj.Name == obs.ValuePropName; },
                    false);

                EditorGUI.BeginChangeCheck();

                if (val != null) {
                    val.Draw(label);
                } else {
                    CallNextDrawer(label);
                }

                if (EditorGUI.EndChangeCheck()) {
                    ValueEntry.Property.Tree.ApplyChanges();
                    obs.OnValidate();
                }
            }
        }
#endif
#endif
    }
}