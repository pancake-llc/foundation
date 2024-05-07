// using Pancake;
// using Pancake.ApexEditor;
// using UnityEditor;
// using UnityEngine;
//
// namespace PancakeEditor
// {
//     //[ViewTarget(typeof(IgnoreTypeMismatchAttribute))]
//     public class IgnoreTypeMismatchDrawer : FieldView, ITypeValidationCallback
//     {
//         public override void OnGUI(Rect position, SerializedField element, GUIContent label)
//         {
//             EditorGUI.BeginProperty(position, label, element.GetSerializedProperty());
//
//             EditorGUI.ObjectField(position,
//                 label,
//                 element.GetSerializedProperty().objectReferenceValue,
//                 typeof(Object),
//                 true);
//
//             EditorGUI.EndProperty();
//         }
//
//         public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ObjectReference; }
//     }
// }

