using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Pancake.Common
{
    /// <summary>
    /// Allows Assigning in inspector for UnityEngine.Objects that implement T (event for interfaces)
    /// It is only ensured by the inspector during edit-time that Target actually implements T. So don't abuse it :D
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class InterfaceHelper<T>
    {
        [SerializeField] private UnityEngine.Object target;

        /// <summary>
        /// Notes: if <see cref="target"/> equal null. The value of value types may not always be the default value (0),
        /// the value of reference types may not always be null so be careful to make sure the value of the <see cref="target"/> is assigned before use.
        /// <br/>
        /// We are not checking for null here against target for performance reasons.
        /// <br/>
        /// Make sure that the object assigned to the value must be <see cref="UnityEngine.Object"/> and not a pure class.
        /// </summary>
        public T Value { get => UnsafeUtility.As<UnityEngine.Object, T>(ref target); set => target = UnsafeUtility.As<T, UnityEngine.Object>(ref value); }
    }
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(InterfaceHelper<>))]
    internal class InterfaceHelperDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect rect, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var generic = fieldInfo.FieldType.GetGenericArguments()[0];
            var component = property.FindPropertyRelative("target");
            UnityEditor.EditorGUI.BeginProperty(rect, label, property);
            component.objectReferenceValue = UnityEditor.EditorGUI.ObjectField(rect,
                label,
                component.objectReferenceValue,
                generic,
                true);

            UnityEditor.EditorGUI.EndProperty();
        }
    }
#endif
}