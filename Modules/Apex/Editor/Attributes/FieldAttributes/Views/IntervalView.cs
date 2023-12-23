using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(IntervalAttribute))]
    public sealed class IntervalView : FieldView, ITypeValidationCallback
    {
        private const float BUTTON_WIDTH = 15;
        private const float SPACE_WIDTH = 5;

        private static Texture LockIcon;
        private static Texture UnlockIcon;
        private static GUIStyle ButtonStyle;
        private static GUIStyle TextFieldStyle;
        private static Color LineColor;

        static IntervalView()
        {
            LockIcon = EditorGUIUtility.IconContent("LockIcon").image;
            UnlockIcon = EditorGUIUtility.IconContent("LockIcon-On").image;
            LineColor = new Color32(94, 94, 94, 255);
        }

        private SerializedProperty serializedProperty;

        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            serializedProperty = serializedField.GetSerializedProperty();
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (ButtonStyle == null)
            {
                ButtonStyle = new GUIStyle("IconButton") {alignment = TextAnchor.MiddleCenter};

                TextFieldStyle = new GUIStyle(EditorStyles.textField) {fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};
            }

            position = EditorGUI.PrefixLabel(position, label);

            if (serializedProperty.propertyType == SerializedPropertyType.Vector2)
            {
                OnVector2GUI(ref position, in serializedProperty);
            }
            else if (serializedProperty.propertyType == SerializedPropertyType.Vector2Int)
            {
                OnVector2IntGUI(ref position, in serializedProperty);
            }
        }

        private void OnVector2GUI(ref Rect position, in SerializedProperty property)
        {
            float y = position.y;
            float totalWidth = position.width;
            float totalHeight = position.height;
            Vector2 vector = property.vector2Value;

            position.width = EditorGUIUtility.fieldWidth;
            if (vector.x == float.NegativeInfinity)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, "-∞", TextFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.x = EditorGUI.FloatField(position, vector.x);
            }

            position.x = position.xMax + EditorGUIUtility.standardVerticalSpacing;
            position.width = BUTTON_WIDTH;
            if (GUI.Button(position, vector.x == float.NegativeInfinity ? LockIcon : UnlockIcon, ButtonStyle))
            {
                vector.x = vector.x == float.NegativeInfinity ? 0 : float.NegativeInfinity;
            }

            position.x = position.xMax + SPACE_WIDTH;
            position.y += (position.height / 2) - 1;
            position.width = totalWidth - ((EditorGUIUtility.fieldWidth * 2) + (BUTTON_WIDTH * 2) + (SPACE_WIDTH * 2) + (EditorGUIUtility.standardVerticalSpacing * 2));
            position.height = 2;
            EditorGUI.DrawRect(position, LineColor);
            position.y = y;
            position.height = totalHeight;

            position.x = position.xMax + SPACE_WIDTH;
            position.width = BUTTON_WIDTH;
            if (GUI.Button(position, vector.y == float.PositiveInfinity ? LockIcon : UnlockIcon, ButtonStyle))
            {
                vector.y = vector.y == float.PositiveInfinity ? 1 : float.PositiveInfinity;
            }

            position.x = position.xMax + EditorGUIUtility.standardVerticalSpacing;
            position.width = EditorGUIUtility.fieldWidth;
            if (vector.y == float.PositiveInfinity)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, "∞", TextFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.y = EditorGUI.FloatField(position, vector.y);
            }

            property.vector2Value = vector;
        }

        private void OnVector2IntGUI(ref Rect position, in SerializedProperty property)
        {
            float y = position.y;
            float totalWidth = position.width;
            float totalHeight = position.height;
            Vector2Int vector = property.vector2IntValue;

            position.width = EditorGUIUtility.fieldWidth;
            if (vector.x == int.MinValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, "Min Value", TextFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.x = EditorGUI.IntField(position, vector.x);
            }

            position.x = position.xMax + EditorGUIUtility.standardVerticalSpacing;
            position.width = BUTTON_WIDTH;
            if (GUI.Button(position, vector.x == int.MinValue ? LockIcon : UnlockIcon, ButtonStyle))
            {
                vector.x = vector.x == int.MinValue ? 0 : int.MinValue;
            }

            position.x = position.xMax + SPACE_WIDTH;
            position.y += (position.height / 2) - 1;
            position.width = totalWidth - ((EditorGUIUtility.fieldWidth * 2) + (BUTTON_WIDTH * 2) + (SPACE_WIDTH * 2) + (EditorGUIUtility.standardVerticalSpacing * 2));
            position.height = 2;
            EditorGUI.DrawRect(position, LineColor);
            position.y = y;
            position.height = totalHeight;

            position.x = position.xMax + SPACE_WIDTH;
            position.width = BUTTON_WIDTH;
            if (GUI.Button(position, vector.y == int.MaxValue ? LockIcon : UnlockIcon, ButtonStyle))
            {
                vector.y = vector.y == int.MaxValue ? 1 : int.MaxValue;
            }

            position.x = position.xMax + EditorGUIUtility.standardVerticalSpacing;
            position.width = EditorGUIUtility.fieldWidth;
            if (vector.y == int.MaxValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, "Max Value", TextFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.y = EditorGUI.IntField(position, vector.y);
            }

            property.vector2IntValue = vector;
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Vector2 || property.propertyType == SerializedPropertyType.Vector2Int;
        }
    }
}