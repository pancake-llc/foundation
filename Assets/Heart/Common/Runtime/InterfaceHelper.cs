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

        public T Value
        {
            get => target == null ? default : UnsafeUtility.As<UnityEngine.Object, T>(ref target);
            set
            {
                if (value is not UnityEngine.Object obj) throw new InvalidCastException("value is not an UnityEngine.Object");
                target = obj;
            }
        }
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