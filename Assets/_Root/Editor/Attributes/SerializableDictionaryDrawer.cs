using Pancake.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DrawerTarget(typeof(SerializableDictionaryBase), Subclasses = true)]
    [DrawerTarget(typeof(SerializableHashSetBase), Subclasses = true)]
    sealed class SerializableDictionaryDrawer : FieldDrawer
    {
        private const string KeysFieldName = "keys";
        private const string ValuesFieldName = "values";

        private readonly static Dictionary<PropertyIdentity, ConflictState> ConflictStateHash;
        private readonly static Dictionary<SerializedPropertyType, PropertyInfo> ValueAccessorsHash;

        private static readonly GUIContent IconPlus = CreateIconContent("Toolbar Plus", "Add entry");
        private static readonly GUIContent IconMinus = CreateIconContent("Toolbar Minus", "Remove entry");
        private static readonly GUIContent WarningIconConflict = CreateIconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
        private static readonly GUIContent WarningIconOther = CreateIconContent("console.infoicon.sml", "Conflicting key");
        private static readonly GUIContent WarningIconNull = CreateIconContent("console.warnicon.sml", "Null key, this entry will be lost");
        private static readonly GUIContent TempContent = new GUIContent();
        private static readonly GUIStyle ButtonStyle = GUIStyle.none;

        static SerializableDictionaryDrawer()
        {
            ConflictStateHash = new Dictionary<PropertyIdentity, ConflictState>();
            ValueAccessorsHash = new Dictionary<SerializedPropertyType, PropertyInfo>();

            Dictionary<SerializedPropertyType, string> accessorsNameDictionary = new Dictionary<SerializedPropertyType, string>()
            {
                {SerializedPropertyType.Integer, "intValue"},
                {SerializedPropertyType.Boolean, "boolValue"},
                {SerializedPropertyType.Float, "floatValue"},
                {SerializedPropertyType.String, "stringValue"},
                {SerializedPropertyType.Color, "colorValue"},
                {SerializedPropertyType.ObjectReference, "objectReferenceValue"},
                {SerializedPropertyType.LayerMask, "intValue"},
                {SerializedPropertyType.Enum, "intValue"},
                {SerializedPropertyType.Vector2, "vector2Value"},
                {SerializedPropertyType.Vector3, "vector3Value"},
                {SerializedPropertyType.Vector4, "vector4Value"},
                {SerializedPropertyType.Rect, "rectValue"},
                {SerializedPropertyType.ArraySize, "intValue"},
                {SerializedPropertyType.Character, "intValue"},
                {SerializedPropertyType.AnimationCurve, "animationCurveValue"},
                {SerializedPropertyType.Bounds, "boundsValue"},
                {SerializedPropertyType.Quaternion, "quaternionValue"},
            };

            Type serializedPropertyType = typeof(SerializedProperty);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            foreach (KeyValuePair<SerializedPropertyType, string> accessor in accessorsNameDictionary)
            {
                PropertyInfo propertyInfo = serializedPropertyType.GetProperty(accessor.Value, flags);
                ValueAccessorsHash.Add(accessor.Key, propertyInfo);
            }

            IconPlus = CreateIconContent("Toolbar Plus", "Add entry");
            IconMinus = CreateIconContent("Toolbar Minus", "Remove entry");
            WarningIconConflict = CreateIconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");
            WarningIconOther = CreateIconContent("console.infoicon.sml", "Conflicting key");
            WarningIconNull = CreateIconContent("console.warnicon.sml", "Null key, this entry will be lost");
            TempContent = new GUIContent();
            ButtonStyle = GUIStyle.none;
        }

        private enum ButtonAction
        {
            None,
            Add,
            Remove
        }

        private class ConflictState
        {
            public object conflictKey = null;
            public object conflictValue = null;
            public int conflictIndex = -1;
            public int conflictOtherIndex = -1;
            public bool conflictKeyPropertyExpanded = false;
            public bool conflictValuePropertyExpanded = false;
            public float conflictLineHeight = 0f;
        }

        private struct PropertyIdentity
        {
            public string propertyPath;
            public UnityEngine.Object instance;

            public PropertyIdentity(SerializedProperty property)
            {
                propertyPath = property.propertyPath;
                instance = property.serializedObject.targetObject;
            }
        }

        private struct EnumerationEntry
        {
            public SerializedField key;
            public SerializedField value;
            public int index;

            public EnumerationEntry(SerializedField key, SerializedField value, int index)
            {
                this.key = key;
                this.value = value;
                this.index = index;
            }
        }

        private FoldoutContainer foldoutContainer;
        private ButtonAction buttonAction;
        private int buttonActionIndex;

        /// <summary>
        /// Called once when initializing element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, GUIContent label)
        {
            foldoutContainer = new FoldoutContainer(label.text, "Group", null)
            {
                onChildrenGUI = (position) => OnDictionaryGUI(position, element, label),
                getChildrenHeight = () => GetDictionaryHeight(element),
                onMenuButtonClick = (position) =>
                {
                    buttonAction = ButtonAction.Add;
                    buttonActionIndex = element.serializedProperty.FindPropertyRelative(KeysFieldName).arraySize;
                },
                menuIconContent = EditorGUIUtility.IconContent("Toolbar Plus")
            };
        }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the element drawer GUI.</param>
        /// <param name="element">Reference of serialized element with drawer attribute.</param>
        /// <param name="label">Display label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            foldoutContainer.OnGUI(position);
            buttonAction = ButtonAction.None;
            buttonActionIndex = 0;
        }

        private void OnDictionaryGUI(Rect position, SerializedField element, GUIContent label)
        {
            SerializedProperty keyArray = element.serializedProperty.FindPropertyRelative(KeysFieldName);
            SerializedProperty valueArray = element.serializedProperty.FindPropertyRelative(ValuesFieldName);

            ConflictState conflictState = GetConflictState(element.serializedProperty);

            if (conflictState.conflictIndex != -1)
            {
                keyArray.InsertArrayElementAtIndex(conflictState.conflictIndex);
                SerializedProperty keyProperty = keyArray.GetArrayElementAtIndex(conflictState.conflictIndex);
                SetPropertyValue(keyProperty, conflictState.conflictKey);
                keyProperty.isExpanded = conflictState.conflictKeyPropertyExpanded;

                if (valueArray != null)
                {
                    valueArray.InsertArrayElementAtIndex(conflictState.conflictIndex);
                    var valueProperty = valueArray.GetArrayElementAtIndex(conflictState.conflictIndex);
                    SetPropertyValue(valueProperty, conflictState.conflictValue);
                    valueProperty.isExpanded = conflictState.conflictValuePropertyExpanded;
                }
            }

            const int buttonWidth = 26;

            foreach (EnumerationEntry entry in EnumerateEntries(keyArray, valueArray))
            {
                SerializedField keyProperty = entry.key;
                SerializedField valueProperty = entry.value;
                int index = entry.index;

                Rect pairPosition = new Rect(position.x + 4, position.y, position.width - buttonWidth, position.height);
                float height = DrawPair(pairPosition, keyProperty, valueProperty, index);

                Rect buttonPosition = new Rect(pairPosition.xMax + 4, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(buttonPosition, IconMinus, "IconButton"))
                {
                    buttonAction = ButtonAction.Remove;
                    buttonActionIndex = index;
                }

                Rect iconPosition = new Rect(pairPosition.x, pairPosition.y + 1, 16, 16);
                if (index == conflictState.conflictIndex && conflictState.conflictOtherIndex == -1)
                {
                    GUI.Label(iconPosition, WarningIconNull);
                }
                else if (index == conflictState.conflictIndex)
                {
                    GUI.Label(iconPosition, WarningIconConflict);
                }
                else if (index == conflictState.conflictOtherIndex)
                {
                    GUI.Label(iconPosition, WarningIconOther);
                }

                position.y += height + EditorGUIUtility.standardVerticalSpacing;
            }

            if (buttonAction == ButtonAction.Add)
            {
                keyArray.InsertArrayElementAtIndex(buttonActionIndex);
                if (valueArray != null)
                    valueArray.InsertArrayElementAtIndex(buttonActionIndex);
            }
            else if (buttonAction == ButtonAction.Remove)
            {
                DeleteArrayElementAtIndex(keyArray, buttonActionIndex);
                if (valueArray != null)
                    DeleteArrayElementAtIndex(valueArray, buttonActionIndex);
            }

            conflictState.conflictKey = null;
            conflictState.conflictValue = null;
            conflictState.conflictIndex = -1;
            conflictState.conflictOtherIndex = -1;
            conflictState.conflictLineHeight = 0f;
            conflictState.conflictKeyPropertyExpanded = false;
            conflictState.conflictValuePropertyExpanded = false;

            foreach (EnumerationEntry entry1 in EnumerateEntries(keyArray, valueArray))
            {
                SerializedField keyProperty1 = entry1.key;
                int index = entry1.index;
                object keyValue = GetPropertyValue(keyProperty1.serializedProperty);

                if (keyValue == null)
                {
                    SerializedField valueProperty1 = entry1.value;
                    SaveProperty(keyProperty1.serializedProperty,
                        valueProperty1.serializedProperty,
                        index,
                        -1,
                        conflictState);
                    DeleteArrayElementAtIndex(keyArray, index);
                    if (valueArray != null)
                    {
                        DeleteArrayElementAtIndex(valueArray, index);
                    }

                    break;
                }


                foreach (EnumerationEntry entry2 in EnumerateEntries(keyArray, valueArray, index + 1))
                {
                    SerializedField keyProperty2 = entry2.key;
                    int j = entry2.index;
                    object keyProperty2Value = GetPropertyValue(keyProperty2.serializedProperty);

                    if (ComparePropertyValues(keyValue, keyProperty2Value))
                    {
                        var valueProperty2 = entry2.value;
                        SaveProperty(keyProperty2.serializedProperty,
                            valueProperty2.serializedProperty,
                            j,
                            index,
                            conflictState);
                        DeleteArrayElementAtIndex(keyArray, j);
                        if (valueArray != null)
                        {
                            DeleteArrayElementAtIndex(valueArray, j);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get height which needed to element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return foldoutContainer.GetHeight(); }

        private float GetDictionaryHeight(SerializedField element)
        {
            float propertyHeight = 0;

            SerializedProperty keysProperty = element.serializedProperty.FindPropertyRelative(KeysFieldName);
            SerializedProperty valuesProperty = element.serializedProperty.FindPropertyRelative(ValuesFieldName);

            foreach (EnumerationEntry entry in EnumerateEntries(keysProperty, valuesProperty))
            {
                SerializedField keyProperty = entry.key;
                SerializedField valueProperty = entry.value;
                float keyPropertyHeight = keyProperty.GetHeight();
                float valuePropertyHeight = valueProperty != null ? valueProperty.GetHeight() : 0f;
                float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                propertyHeight += lineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            ConflictState conflictState = GetConflictState(element.serializedProperty);

            if (conflictState.conflictIndex != -1)
            {
                propertyHeight += conflictState.conflictLineHeight;
            }

            return propertyHeight;
        }


        private static float DrawPair(Rect position, SerializedField key, SerializedField value, int index)
        {
            bool keyCanBeExpanded = IsExpandebleProperty(key.serializedProperty);

            if (value != null)
            {
                bool valueCanBeExpanded = IsExpandebleProperty(value.serializedProperty);

                if (!keyCanBeExpanded && valueCanBeExpanded)
                {
                    return DrawExpandedPair(position, key, value);
                }
                else
                {
                    string keyLabel = keyCanBeExpanded ? ("Key " + index.ToString()) : string.Empty;
                    string valueLabel = valueCanBeExpanded ? ("Value " + index.ToString()) : string.Empty;
                    return DrawPairSample(position,
                        key,
                        keyLabel,
                        value,
                        valueLabel);
                }
            }
            else
            {
                if (!keyCanBeExpanded)
                {
                    return DrawKey(position, key, null);
                }
                else
                {
                    var keyLabel = string.Format("{0} {1}", ObjectNames.NicifyVariableName(key.serializedProperty.type), index);
                    return DrawKey(position, key, keyLabel);
                }
            }
        }

        private static float DrawExpandedPair(Rect position, SerializedField key, SerializedField value)
        {
            Rect keyPosition = new Rect(position.x, position.y, position.width, key.GetHeight());
            key.OnGUI(keyPosition);

            float valueHeight = value.GetHeight();
            Rect valueFoldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            value.IsExpanded(EditorGUI.Foldout(valueFoldoutPosition, value.IsExpanded(), GUIContent.none));
            if (value.IsExpanded())
            {
                Rect childrenPosition = new Rect(position.x, valueFoldoutPosition.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, valueHeight);
                EditorGUI.indentLevel++;
                value.DrawChildren(childrenPosition);
                EditorGUI.indentLevel--;
            }

            return Mathf.Max(keyPosition.height, valueHeight);
        }

        private static float DrawKey(Rect position, SerializedField key, string label)
        {
            Rect keyPosition = new Rect(position.x, position.y, position.width, key.GetHeight());
            key.SetLabel(label != null ? CreateTempContent(label) : GUIContent.none);
            key.OnGUI(keyPosition);
            return keyPosition.height;
        }

        private static float DrawPairSample(Rect position, SerializedField key, string keyLabel, SerializedField value, string valueLabel)
        {
            float space = 15;
            if (valueLabel == string.Empty)
            {
                space = 4;
            }

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            float width = position.width / 2;
            Rect keyPosition = new Rect(position.x, position.y, width - space, key.GetHeight());
            key.GetLabel().text = keyLabel;
            key.OnGUI(keyPosition);

            Rect valuePosition = new Rect(keyPosition.xMax + space, position.y, width, value.GetHeight());
            value.GetLabel().text = valueLabel;
            value.OnGUI(valuePosition);
            EditorGUIUtility.labelWidth = labelWidth;
            return Mathf.Max(keyPosition.height, valuePosition.height);
        }

        private static bool IsExpandebleProperty(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Quaternion:
                    return true;
                default:
                    return false;
            }
        }

        private static void SaveProperty(SerializedProperty keyProperty, SerializedProperty valueProperty, int index, int otherIndex, ConflictState conflictState)
        {
            conflictState.conflictKey = GetPropertyValue(keyProperty);
            conflictState.conflictValue = valueProperty != null ? GetPropertyValue(valueProperty) : null;
            float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
            float valuePropertyHeight = valueProperty != null ? EditorGUI.GetPropertyHeight(valueProperty) : 0f;
            float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
            conflictState.conflictLineHeight = lineHeight;
            conflictState.conflictIndex = index;
            conflictState.conflictOtherIndex = otherIndex;
            conflictState.conflictKeyPropertyExpanded = keyProperty.isExpanded;
            conflictState.conflictValuePropertyExpanded = valueProperty != null ? valueProperty.isExpanded : false;
        }

        private static ConflictState GetConflictState(SerializedProperty property)
        {
            ConflictState conflictState;
            PropertyIdentity propId = new PropertyIdentity(property);
            if (!ConflictStateHash.TryGetValue(propId, out conflictState))
            {
                conflictState = new ConflictState();
                ConflictStateHash.Add(propId, conflictState);
            }

            return conflictState;
        }

        private static GUIContent CreateIconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        private static GUIContent CreateTempContent(string text)
        {
            TempContent.text = text;
            return TempContent;
        }

        private static void DeleteArrayElementAtIndex(SerializedProperty array, int index)
        {
            var property = array.GetArrayElementAtIndex(index);
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = null;
            }

            array.DeleteArrayElementAtIndex(index);
        }

        public static object GetPropertyValue(SerializedProperty property)
        {
            PropertyInfo propertyInfo;
            if (ValueAccessorsHash.TryGetValue(property.propertyType, out propertyInfo))
            {
                return propertyInfo.GetValue(property, null);
            }
            else
            {
                if (property.isArray)
                    return GetPropertyValueArray(property);
                else
                    return GetPropertyValueGeneric(property);
            }
        }

        private static void SetPropertyValue(SerializedProperty property, object value)
        {
            if (ValueAccessorsHash.TryGetValue(property.propertyType, out PropertyInfo propertyInfo))
            {
                propertyInfo.SetValue(property, value, null);
            }
            else
            {
                if (property.isArray)
                {
                    SetPropertyValueArray(property, value);
                }
                else
                {
                    SetPropertyValueGeneric(property, value);
                }
            }
        }

        private static object GetPropertyValueArray(SerializedProperty property)
        {
            object[] array = new object[property.arraySize];
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                array[i] = GetPropertyValue(item);
            }

            return array;
        }

        private static object GetPropertyValueGeneric(SerializedProperty property)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    object value = GetPropertyValue(iterator);
                    dict.Add(name, value);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }

            return dict;
        }

        private static void SetPropertyValueArray(SerializedProperty property, object value)
        {
            object[] array = (object[]) value;
            property.arraySize = array.Length;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty item = property.GetArrayElementAtIndex(i);
                SetPropertyValue(item, array[i]);
            }
        }

        private static void SetPropertyValueGeneric(SerializedProperty property, object v)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>) v;
            var iterator = property.Copy();
            if (iterator.Next(true))
            {
                var end = property.GetEndProperty();
                do
                {
                    string name = iterator.name;
                    SetPropertyValue(iterator, dict[name]);
                } while (iterator.Next(false) && iterator.propertyPath != end.propertyPath);
            }
        }

        private static bool ComparePropertyValues(object value1, object value2)
        {
            if (value1 is Dictionary<string, object> && value2 is Dictionary<string, object>)
            {
                var dict1 = (Dictionary<string, object>) value1;
                var dict2 = (Dictionary<string, object>) value2;
                return CompareDictionaries(dict1, dict2);
            }
            else
            {
                return object.Equals(value1, value2);
            }
        }

        private static bool CompareDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            if (dict1.Count != dict2.Count)
                return false;

            foreach (var kvp1 in dict1)
            {
                var key1 = kvp1.Key;
                object value1 = kvp1.Value;

                object value2;
                if (!dict2.TryGetValue(key1, out value2))
                    return false;

                if (!ComparePropertyValues(value1, value2))
                    return false;
            }

            return true;
        }

        private static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
        {
            if (keyArrayProperty.arraySize > startIndex)
            {
                int index = startIndex;
                SerializedProperty keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
                SerializedProperty valueProperty = valueArrayProperty?.GetArrayElementAtIndex(startIndex);
                SerializedProperty endProperty = keyArrayProperty.GetEndProperty();

                do
                {
                    yield return new EnumerationEntry(keyProperty, valueProperty, index);
                    index++;
                } while (keyProperty.Next(false) && (valueProperty != null ? valueProperty.Next(false) : true) &&
                         !SerializedProperty.EqualContents(keyProperty, endProperty));
            }
        }
    }

    [DrawerTarget(typeof(SerializationStorageBase), Subclasses = true)]
    public class SerializableDictionaryStoragePropertyDrawer : FieldDrawer
    {
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            element.serializedProperty.Next(true);
            EditorGUI.PropertyField(position, element.serializedProperty, label, true);
        }

        public override float GetHeight(SerializedField element, GUIContent label)
        {
            element.serializedProperty.Next(true);
            return EditorGUI.GetPropertyHeight(element.serializedProperty);
        }
    }
}