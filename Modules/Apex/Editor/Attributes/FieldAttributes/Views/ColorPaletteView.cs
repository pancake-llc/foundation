using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(ColorPaletteAttribute))]
    public sealed class ColorPaletteView : FieldView
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

            target = serializedField.GetDeclaringObject();
            var type = target.GetType();
            var limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);
            
            foreach (MemberInfo memberInfo in type.AllMembers(limitDescendant))
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

            float width = position.width;

            position.width = 20;
            position.height = 20;
            GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);

            position.x += 2;
            position.y += 2;
            position.width = 16;
            position.height = 16;
            EditorGUI.DrawRect(position, serializedField.GetSerializedProperty().colorValue);

            position.x = position.xMax + 4;
            position.y -= 2;
            position.width = width - 24;
            position.height = 20;
            GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);

            position.x += 2;
            position.y += 2;
            position.width -= 4;
            position.height -= 4;
            Color[] palette = GetPalette();

            position.width /= palette.Length;
            for (int i = 0; i < palette.Length; i++)
            {
                Color colorValue = palette[i];
                EditorGUI.DrawRect(position, colorValue);
                if (GUI.Button(position, GUIContent.none, GUI.skin.label))
                {
                    serializedField.GetSerializedProperty().colorValue = colorValue;
                }

                position.x = position.xMax;
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