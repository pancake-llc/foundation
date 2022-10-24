using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(ColorPaletteAttribute))]
    sealed class ColorPaletteView : FieldView
    {
        private readonly static Color[] EmptyPalette = new Color[0];

        private object target;
        private MemberInfo memberInfo;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            ColorPaletteAttribute attribute = viewAttribute as ColorPaletteAttribute;

            target = serializedField.GetMemberTarget();
            Type type = target.GetType();
            foreach (MemberInfo memberInfo in type.AllMembers())
            {
                if (memberInfo.Name == attribute.member)
                {
                    this.memberInfo = memberInfo;
                    break;
                }
            }
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            Rect previewPosition = new Rect(position.x, position.y, 20, 20);
            GUI.Box(previewPosition, GUIContent.none, Uniform.ContentBackground);

            Rect previewColorPosition = new Rect(previewPosition.x + 2, previewPosition.y + 2, 16, 16);
            EditorGUI.DrawRect(previewColorPosition, serializedField.GetSerializedProperty().colorValue);

            Rect palettePosition = new Rect(previewPosition.xMax + 2, position.y, position.width - 22, previewPosition.height);
            GUI.Box(palettePosition, GUIContent.none, Uniform.ContentBackground);

            Color[] palette = GetPalette();
            palettePosition.x += 2;
            palettePosition.y += 2;
            palettePosition.width -= 4;
            palettePosition.height -= 4;
            Rect[] palettePositions = HorizontalContainer.SplitRectangle(palettePosition, palette.Length);
            for (int i = 0; i < palette.Length; i++)
            {
                Rect colorPosition = palettePositions[i];
                Color colorValue = palette[i];
                EditorGUI.DrawRect(colorPosition, colorValue);
                if (GUI.Button(colorPosition, GUIContent.none, GUI.skin.label))
                {
                    serializedField.GetSerializedProperty().colorValue = colorValue;
                }
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return 20; }

        public Color[] GetPalette()
        {
            if (memberInfo is FieldInfo fieldInfo)
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
            else if (memberInfo is PropertyInfo propertyInfo)
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
            else if (memberInfo is MethodInfo methodInfo)
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