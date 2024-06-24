using Pancake.Sound;
using UnityEngine;
using UnityEditor;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(VolumeAttribute))]
    public class VolumeAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float && attribute is VolumeAttribute volAttr)
            {
                if (volAttr.canBoost) property.floatValue = EditorAudioEx.DrawVolumeSlider(position, label, property.floatValue);
                else
                    property.floatValue = EditorGUI.Slider(position,
                        label,
                        property.floatValue,
                        0f,
                        AudioConstant.FULL_VOLUME);
            }
        }
    }
}