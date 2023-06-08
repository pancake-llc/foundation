using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static partial class Editor
    {
        /// <summary>
        /// thanks @JoshuaMcKenzie and @Edvard-D
        /// remove all empty object reference elements
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int RemoveEmptyArrayElements(this SerializedProperty list)
        {
            var elementsRemoved = 0;
            if (list == null) return elementsRemoved;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var element = list.GetArrayElementAtIndex(i);
                if (element.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    list.RemoveElement(i);
                    elementsRemoved++;
                }
            }

            return elementsRemoved;
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        public static void RemoveElement(this SerializedProperty list, int index)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (!list.isArray) throw new ArgumentException("Property is not an array");

            if (index < 0 || index >= list.arraySize) throw new ArgumentOutOfRangeException(nameof(list));

            list.GetArrayElementAtIndex(index).SetPropertyValue(null);
            list.DeleteArrayElementAtIndex(index);

            list.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = value as AnimationCurve;
                    break;

                case SerializedPropertyType.ArraySize:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = System.Convert.ToBoolean(value);
                    break;

                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds?) value ?? new Bounds();
                    break;

                case SerializedPropertyType.Character:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Color:
                    property.colorValue = (Color?) value ?? new Color();
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = System.Convert.ToSingle(value);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.LayerMask:
                    property.intValue = (value as LayerMask?)?.value ?? System.Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as UnityEngine.Object;
                    break;

                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion?) value ?? Quaternion.identity;
                    break;

                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect?) value ?? new Rect();
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = value as string;
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2?) value ?? Vector2.zero;
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3?) value ?? Vector3.zero;
                    break;

                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4?) value ?? Vector4.zero;
                    break;
            }
        }

        /// <summary>
        /// get inspector type to display window
        /// </summary>
        public static Type InspectorWindow => typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");

        public static string GetSizeInMemory(this long byteSize)
        {
            string[] sizes = {"B", "KB", "MB", "GB", "TB"};
            double len = Convert.ToDouble(byteSize);
            int order = 0;
            while (len >= 1024D && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0.##} {1}", len, sizes[order]);
        }
    }
}