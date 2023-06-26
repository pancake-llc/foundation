using UnityEngine;
using UnityEditor;
using Pancake.Sensor;

namespace Pancake.SensorEditor
{
    [CustomPropertyDrawer(typeof(NavMeshMaskAttribute))]
    public class NavMeshMaskDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label)
        {
            using (new GUILayout.HorizontalScope())
            {
                position = EditorGUI.PrefixLabel(position, label);

                EditorGUI.BeginChangeCheck();

                string[] areaNames = GameObjectUtility.GetNavMeshAreaNames();
                string[] completeAreaNames = new string[areaNames.Length];

                foreach (string name in areaNames)
                {
                    completeAreaNames[GameObjectUtility.GetNavMeshAreaFromName(name)] = name;
                }

                int mask = serializedProperty.intValue;

                mask = EditorGUI.MaskField(position, mask, completeAreaNames);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedProperty.intValue = mask;
                }
            }
        }
    }
}