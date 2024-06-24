using UnityEngine;
using UnityEditor;
using System;
using Pancake.Sound;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(FrequencyAttribute))]
    public class FrequencyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                var suffixRect = EditorGUI.PrefixLabel(position, label);

                float freq = EditorAudioEx.DrawLogarithmicSlider_Horizontal(suffixRect, property.floatValue, AudioConstant.MIN_FREQUENCY, AudioConstant.MAX_FREQUENCY);
                property.floatValue = (float) Math.Floor(freq);
            }
        }
    }
}