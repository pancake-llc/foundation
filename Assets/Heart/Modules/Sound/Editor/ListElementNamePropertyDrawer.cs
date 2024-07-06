using System;
using Pancake.Sound;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(ListElementName))]
    public class ListElementNamePropertyDrawer : PropertyDrawer
    {
        public const string UnityDefaultElementName = "Element ";
        public const float PropertyPadding = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.hasVisibleChildren)
            {
                Rect foldoutRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GetElementName(property), EditorStyles.foldout);
                if (property.isExpanded)
                {
                    float currentDrawY = 0;
                    IterateChildProperties(property,
                        childProp =>
                        {
                            if (currentDrawY == 0)
                            {
                                currentDrawY = position.y + EditorGUIUtility.singleLineHeight + PropertyPadding;
                            }

                            Rect fieldRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight, y = currentDrawY};
                            EditorGUI.PropertyField(fieldRect, childProp, childProp.propertyType == SerializedPropertyType.Generic);
                            currentDrawY += EditorGUI.GetPropertyHeight(childProp) + PropertyPadding;
                        });
                }

                EditorGUI.EndFoldoutHeaderGroup();
            }
            else
            {
                EditorGUI.LabelField(position, GetElementName(property));
                EditorGUI.PropertyField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.hasVisibleChildren && property.isExpanded)
            {
                IterateChildProperties(property, childProp => { height += EditorGUI.GetPropertyHeight(childProp) + PropertyPadding; });
                return height;
            }
            else
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        private void IterateChildProperties(SerializedProperty property, Action<SerializedProperty> onGetProperty)
        {
            var drawingProp = property.Copy();
            int drawingDepth = drawingProp.depth + 1;
            while (drawingProp.NextVisible(true))
            {
                if (drawingProp.depth == drawingDepth)
                {
                    onGetProperty?.Invoke(drawingProp);
                }
                else if (drawingProp.depth < drawingDepth)
                {
                    break;
                }
            }
        }

        private string GetElementName(SerializedProperty property)
        {
            ListElementName elementNameAtrr = attribute as ListElementName;
            if (elementNameAtrr != null)
            {
                if (elementNameAtrr.IsUsingFirstPropertyValueAsName)
                {
                    var copiedProp = property.Copy();
                    copiedProp.Next(true);
                    switch (copiedProp.propertyType)
                    {
                        case SerializedPropertyType.Enum:
                            return copiedProp.enumNames[copiedProp.enumValueIndex];
                        case SerializedPropertyType.String:
                            return copiedProp.stringValue;
                    }
                }

                string numString = property.displayName.Substring(UnityDefaultElementName.Length);
                int index = int.Parse(numString);

                if (!elementNameAtrr.IsStartFromZero)
                {
                    numString = (index + 1).ToString();
                }

                if (elementNameAtrr.IsStringFormat)
                {
                    return string.Format(elementNameAtrr.InspectorName, numString);
                }

                return elementNameAtrr.InspectorName + numString;
            }

            return property.displayName;
        }
    }
}