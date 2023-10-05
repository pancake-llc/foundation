using Pancake.Apex.Serialization.Collections.Generic;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [DrawerTarget(typeof(SerializableDictionaryBase), Subclasses = true)]
    sealed class SerializableDictionaryDrawer : FieldDrawer
    {
        private const float HEADER_HEIGHT = 22;

        private static GUIContent KeyContent;
        private static GUIContent ValueContent;
        private static GUIContent AddButtonContent;
        private static GUIContent RemoveButtonContent;

        static SerializableDictionaryDrawer()
        {
            KeyContent = new GUIContent("Key");
            ValueContent = new GUIContent("Value");
            AddButtonContent = EditorGUIUtility.IconContent("Toolbar Plus");
            RemoveButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");
        }

        private SerializedField keys;
        private SerializedField values;
        private HashSet<int> conflicts;

        /// <summary>
        /// Called once when initializing serialized field drawer.
        /// </summary>
        /// <param name="serializedField">Serialized field with DrawerAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, GUIContent label)
        {
            SerializedObject serializedObject = serializedField.GetSerializedObject();
            SerializedProperty property = serializedField.GetSerializedProperty();
            SerializedProperty propertyKeys = property.FindPropertyRelative("keys");
            SerializedProperty propertyValues = property.FindPropertyRelative("values");

            keys = new SerializedField(serializedObject, propertyKeys.propertyPath);
            values = new SerializedField(serializedObject, propertyValues.propertyPath);
            keys.IsExpanded(true);
            values.IsExpanded(true);

            conflicts = new HashSet<int>();
        }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the serialized field drawer GUI.</param>
        /// <param name="serializedField">Reference of serialized field with drawer attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            SearchConflicts();

            position.width += 1;

            float contentHeight = Mathf.Max(0, position.height - HEADER_HEIGHT);
            float totalWidth = position.width;

            position.height = HEADER_HEIGHT;
            position.width -= HEADER_HEIGHT;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                serializedField.IsExpanded(!serializedField.IsExpanded());
            }

            position.width -= 5;
            string suffixLabel = string.Empty;
            if (conflicts.Count > 0)
            {
                suffixLabel = $"{conflicts.Count} conflicts | ";
            }

            GUI.Label(position, $"{suffixLabel}{keys.GetArrayLength()} items", ApexStyles.SuffixMessage);
            position.width += 5;

            Rect buttonPosition = new Rect(position.xMax - 1, position.y, HEADER_HEIGHT, HEADER_HEIGHT);
            position.width += HEADER_HEIGHT;
            if (GUI.Button(buttonPosition, AddButtonContent, ApexStyles.BoxCenteredButton))
            {
                keys.IncreaseArraySize();
                values.IncreaseArraySize();
            }

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.Repaint)
            {
                position.x += 4;
                ApexStyles.BoldFoldout.Draw(position,
                    label,
                    isHover,
                    false,
                    serializedField.IsExpanded(),
                    false);
                position.x -= 4;
            }

            if (serializedField.IsExpanded() && contentHeight > 0)
            {
                position.y = position.yMax - 1;
                position.width -= 1;
                position.height = contentHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);
                position.y += 3;
                using (new BoxScope(ref position, false))
                {
                    EditorGUI.indentLevel++;
                    OnChildrenGUI(position);
                    EditorGUI.indentLevel--;
                }
            }
        }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label)
        {
            float height = HEADER_HEIGHT;
            if (serializedField.IsExpanded())
            {
                height += GetChildrenHeight() + (EditorGUIUtility.standardVerticalSpacing * 3);
            }

            return height;
        }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the serialized field drawer GUI.</param>
        private void OnChildrenGUI(Rect position)
        {
            position.width -= HEADER_HEIGHT;

            int count = keys.GetArrayLength();
            for (int i = 0; i < count; i++)
            {
                SerializedField key = keys.GetArrayElement(i);
                SerializedField value = values.GetArrayElement(i);

                if (key == null || value == null)
                {
                    continue;
                }

                float yMin = position.yMin - 3;
                float keyHeight = key.GetHeight();
                float valueHeight = value.GetHeight();
                float lineHeight = keyHeight + valueHeight + 8;

                position.height = lineHeight;
                if (Event.current.type == EventType.Repaint)
                {
                    bool hasConflicts = conflicts.Contains(i);
                    if (hasConflicts)
                    {
                        GUI.color = Color.red;
                    }

                    position.x -= 5;
                    position.y -= 3;
                    position.width += HEADER_HEIGHT + 4;
                    if ((i + 1) % 2 == 0)
                    {
                        ApexStyles.BoxEntryEven.Draw(position,
                            false,
                            false,
                            false,
                            false);
                    }
                    else
                    {
                        ApexStyles.BoxEntryOdd.Draw(position,
                            false,
                            false,
                            false,
                            false);
                    }

                    position.x += 5;
                    position.y += 3;
                    position.width -= HEADER_HEIGHT + 4;

                    if (hasConflicts)
                    {
                        GUI.color = Color.white;
                    }
                }

                position.height = keyHeight;
                key.SetLabel(KeyContent);
                key.OnGUI(position);
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                position.height = valueHeight;
                value.SetLabel(ValueContent);
                value.OnGUI(position);
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

                Rect removeButtonPosition = new Rect(position.xMax + 5, yMin, HEADER_HEIGHT, lineHeight);
                if (GUI.Button(removeButtonPosition, RemoveButtonContent, ApexStyles.BoxCenteredButton))
                {
                    keys.RemoveArrayElement(i);
                    values.RemoveArrayElement(i);
                    break;
                }

                position.y += EditorGUIUtility.standardVerticalSpacing;
            }
        }

        /// <summary>
        /// Get height which needed to serialized field drawer.
        /// </summary>
        private float GetChildrenHeight()
        {
            float height = 1;
            int count = keys.GetArrayLength();
            int lastIndex = count - 1;
            for (int i = 0; i < count; i++)
            {
                SerializedField key = keys.GetArrayElement(i);
                SerializedField value = values.GetArrayElement(i);

                if (key == null || value == null)
                {
                    continue;
                }

                height += key.GetHeight() + value.GetHeight();
                if (i != lastIndex)
                {
                    height += 6;
                }
            }

            return height;
        }

        private void SearchConflicts()
        {
            conflicts.Clear();
            SerializedProperty array = keys.GetSerializedProperty();
            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty source = array.GetArrayElementAtIndex(i);
                for (int j = 0; j < array.arraySize; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }

                    SerializedProperty property = array.GetArrayElementAtIndex(j);
                    if (SerializedProperty.DataEquals(source, property))
                    {
                        conflicts.Add(j);
                    }
                }
            }
        }
    }
}