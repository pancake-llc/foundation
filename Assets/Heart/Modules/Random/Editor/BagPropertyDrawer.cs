using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [CustomPropertyDrawer(typeof(Bag<>), true)]
    public class BagPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _data;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return EditorGUI.GetPropertyHeight(property, label, true); }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, true);

            _data = property.FindPropertyRelative("data");

            if (property.isExpanded && _data.isExpanded)
            {
                var rect = new Rect(position.x + 150, position.y + 44.6f, position.width, EditorGUIUtility.singleLineHeight);
                float minProbability = GetMinProbability();
                float maxProbability = GetMaxProbability();

                for (int i = 0; i < _data.arraySize; i++)
                {
                    var element = _data.GetArrayElementAtIndex(i);

                    EditorGUI.LabelField(rect, new GUIContent("Probability : "));

                    float probability = GetProbabilityFor(i);
                    var style = new GUIStyle {normal = {textColor = Color.red}};

                    if (probability > 0)
                    {
                        style.normal.textColor = Color.cyan;
                        if (Mathf.Approximately(maxProbability, probability))
                        {
                            style.normal.textColor = Color.green;
                        }
                        else if (Mathf.Approximately(minProbability, probability))
                        {
                            style.normal.textColor = new Color(1f, 0.6f, 0.04f, 1f);
                        }
                    }
                    else if (Mathf.Approximately(probability, 0f))
                    {
                        style.normal.textColor = Color.yellow;
                    }

                    EditorGUI.LabelField(new Rect(rect.x + 70, rect.y + 2, rect.width, rect.height), new GUIContent(probability + "%"), style);

                    rect.y += GetPropertyHeight(element, label) + 2f;
                }
            }

            EditorGUI.EndProperty();
            return;

            float GetMinProbability()
            {
                var minValue = float.MaxValue;
                for (var i = 0; i < _data.arraySize; i++)
                {
                    float probability = GetProbabilityFor(i);
                    if (minValue > probability)
                    {
                        minValue = probability;
                    }
                }

                return minValue;
            }

            float GetMaxProbability()
            {
                var maxValue = 0f;
                for (var i = 0; i < _data.arraySize; i++)
                {
                    float probability = GetProbabilityFor(i);
                    if (maxValue < probability)
                    {
                        maxValue = probability;
                    }
                }

                return maxValue;
            }

            float GetProbabilityFor(int index)
            {
                var value = 1f;
                var total = 0f;

                SerializedProperty element;
                for (var i = 0; i < _data.arraySize; i++)
                {
                    element = _data.GetArrayElementAtIndex(i);
                    element = element.FindPropertyRelative("weight");
                    total += element.floatValue;
                }

                element = _data.GetArrayElementAtIndex(index);
                element = element.FindPropertyRelative("weight");
                value = element.floatValue;

                return value / total * 100f;
            }
        }
    }
}