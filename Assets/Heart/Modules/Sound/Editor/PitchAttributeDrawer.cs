using Pancake.Sound;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(Pitch))]
    public class PitchAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
#if UNITY_WEBGL
                property.floatValue = EditorGUI.Slider(position, label, property.floatValue, 0f, AudioConstant.MAX_AUDIO_SOURCE_PITCH);
#else
                property.floatValue = EditorGUI.Slider(position,
                    label,
                    property.floatValue,
                    AudioConstant.MIN_AUDIO_SOURCE_PITCH,
                    AudioConstant.MAX_AUDIO_SOURCE_PITCH);
#endif
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}