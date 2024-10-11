using Pancake.Common;
using UnityEngine;

namespace PancakeEditor.Common
{
    [UnityEditor.CustomPropertyDrawer(typeof(InterfaceHelper<>))]
    internal class InterfaceHelperDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect rect, UnityEditor.SerializedProperty property, GUIContent label)
        {
            bool isInCollection = fieldInfo.FieldType.IsCollectionType();
            var generic = !isInCollection ? fieldInfo.FieldType.GetGenericArguments()[0] : fieldInfo.FieldType.GetCorrectElementType().GetGenericArguments()[0];
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
}