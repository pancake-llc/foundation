using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ColorPaletteAttribute))]
    sealed class ColorPaletteView : FieldView
    {
        private readonly static Color[] EmptyPalette = new Color[0];

        private FieldInfo fieldInfo;
        private PropertyInfo propertyInfo;
        private MethodInfo methodInfo;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            ColorPaletteAttribute attribute = viewAttribute as ColorPaletteAttribute;

            Type type = element.serializedObject.targetObject.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

            fieldInfo = type.GetAllMembers(attribute.member, flags).Where(m => m is FieldInfo).FirstOrDefault() as FieldInfo;

            propertyInfo = type.GetAllMembers(attribute.member, flags).Where(m => m is PropertyInfo).FirstOrDefault() as PropertyInfo;

            methodInfo = type.GetAllMembers(attribute.member, flags).Where(m => m is MethodInfo).FirstOrDefault() as MethodInfo;
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

            Rect previewPosition = new Rect(position.x, position.y, 20, 20);
            GUI.Box(previewPosition, GUIContent.none, Uniform.ContentBackground);

            Rect previewColorPosition = new Rect(previewPosition.x + 2, previewPosition.y + 2, 16, 16);
            EditorGUI.DrawRect(previewColorPosition, element.serializedProperty.colorValue);

            Rect palettePosition = new Rect(previewPosition.xMax + 2, position.y, position.width - 22, previewPosition.height);
            GUI.Box(palettePosition, GUIContent.none, Uniform.ContentBackground);

            Color[] palette = GetPalette(element.serializedObject.targetObject);
            palettePosition.x += 2;
            palettePosition.y += 2;
            palettePosition.width -= 4;
            palettePosition.height -= 4;
            Rect[] palettePositions = HorizontalContainer.SplitRectangle(palettePosition, palette.Length);
            Color lastColor = GUI.color;
            for (int i = 0; i < palette.Length; i++)
            {
                Rect colorPosition = palettePositions[i];
                Color colorValue = palette[i];
                GUI.color = colorValue;
                EditorGUI.DrawRect(colorPosition, colorValue);
                if (GUI.Button(colorPosition, GUIContent.none, GUI.skin.label))
                {
                    element.serializedProperty.colorValue = colorValue;
                }
            }

            GUI.color = lastColor;
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return 20; }

        public Color[] GetPalette(object target)
        {
            if (fieldInfo != null)
            {
                object colors = fieldInfo.GetValue(target);
                if (colors is Color[] palette)
                {
                    return palette;
                }
                else if (colors is IEnumerable enumerable)
                {
                    List<Color> temp = new List<Color>();
                    foreach (object item in enumerable)
                    {
                        if (item is Color color)
                        {
                            temp.Add(color);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (propertyInfo != null)
            {
                object colors = propertyInfo.GetValue(target);
                if (colors is Color[] palette)
                {
                    return palette;
                }
                else if (colors is IEnumerable enumerable)
                {
                    List<Color> temp = new List<Color>();
                    foreach (object item in enumerable)
                    {
                        if (item is Color color)
                        {
                            temp.Add(color);
                        }
                    }

                    return temp.ToArray();
                }
            }
            else if (methodInfo != null)
            {
                object colors = methodInfo.Invoke(target, null);
                if (colors is Color[] palette)
                {
                    return palette;
                }
                else if (colors is IEnumerable enumerable)
                {
                    List<Color> temp = new List<Color>();
                    foreach (object item in enumerable)
                    {
                        if (item is Color color)
                        {
                            temp.Add(color);
                        }
                    }

                    return temp.ToArray();
                }
            }

            return EmptyPalette;
        }
    }
}