#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    /// <summary>
    /// Allows Assigning in inspector for UnityEngine.Objects that implement T (event for interfaces)
    /// It is only ensured by the inspector during edit-time that Target actually implements T. So don't abuse it :D
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class InterfaceHelper<T>
    {
        [SerializeField] private Object target;

        public T Value
        {
            get
            {
                if (target == null) return default(T);
                return UnsafeUtility.As<Object, T>(ref target);
            }
            set
            {
                if (!(value is Object obj)) throw new InvalidCastException("value is not an UnityEngine.Object");
                target = obj;
            }
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InterfaceHelper<>))]
    public class InterfaceHelperDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var generic = this.fieldInfo.FieldType.GetGenericArguments()[0];
            var component = property.FindPropertyRelative("target");

            EditorGUI.BeginProperty(position, label, property);
            component.objectReferenceValue = EditorGUI.ObjectField(position,
                label,
                component.objectReferenceValue,
                generic,
                true);
            EditorGUI.EndProperty();
        }
    }
#endif
}