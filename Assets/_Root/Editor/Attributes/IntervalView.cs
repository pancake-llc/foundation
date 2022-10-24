using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(IntervalAttribute))]
    sealed class IntervalView : FieldView, ITypeValidationCallback
    {
        private GUIContent lockContent;
        private GUIContent unlockContent;
        private GUIStyle buttonStyle;
        private GUIStyle textFieldStyle;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            lockContent = EditorGUIUtility.IconContent("LockIcon");
            unlockContent = EditorGUIUtility.IconContent("LockIcon-On");
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle("IconButton") {alignment = TextAnchor.MiddleCenter};

                textFieldStyle = new GUIStyle(EditorStyles.textField) {fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter};
            }

            position = EditorGUI.PrefixLabel(position, label);

            switch (serializedField.GetSerializedProperty().propertyType)
            {
                case SerializedPropertyType.Vector2:
                    OnVector2FloatGUI(position, serializedField);
                    break;
                case SerializedPropertyType.Vector2Int:
                    OnVector2IntGUI(position, serializedField);
                    break;
            }
        }

        private void OnVector2FloatGUI(Rect position, SerializedField serializedField)
        {
            Vector2 vector = serializedField.GetSerializedProperty().vector2Value;

            position.width -= 8;
            Rect[] splitPosition = HorizontalContainer.SplitRectangle(position, 2);

            Rect leftValuePosition = splitPosition[0];
            Rect leftButtonPosition = new Rect(leftValuePosition.x, leftValuePosition.y + 1, 15, 15);
            if (GUI.Button(leftButtonPosition, vector.x == float.NegativeInfinity ? lockContent : unlockContent, buttonStyle))
            {
                vector.x = vector.x == float.NegativeInfinity ? 0 : float.NegativeInfinity;
            }

            Rect leftFieldPosition = new Rect(leftButtonPosition.xMax + 2, leftValuePosition.y, leftValuePosition.width - 15, leftValuePosition.height);
            if (vector.x == float.NegativeInfinity)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(leftFieldPosition, "-∞", textFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.x = EditorGUI.FloatField(leftFieldPosition, vector.x);
            }


            Rect rightValuePosition = splitPosition[1];
            rightValuePosition.x += 5;
            Rect rightFieldPosition = new Rect(rightValuePosition.x, rightValuePosition.y, rightValuePosition.width - 15, rightValuePosition.height);
            if (vector.y == float.PositiveInfinity)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rightFieldPosition, "∞", textFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.y = EditorGUI.FloatField(rightFieldPosition, vector.y);
            }

            Rect rightButtonPosition = new Rect(rightFieldPosition.xMax + 1, rightValuePosition.y + 1, 15, 15);
            if (GUI.Button(rightButtonPosition, vector.y == float.PositiveInfinity ? lockContent : unlockContent, buttonStyle))
            {
                vector.y = vector.y == float.PositiveInfinity ? 1 : float.PositiveInfinity;
            }

            serializedField.GetSerializedProperty().vector2Value = vector;
        }

        private void OnVector2IntGUI(Rect position, SerializedField serializedField)
        {
            Vector2Int vector = serializedField.GetSerializedProperty().vector2IntValue;

            position.width -= 8;
            Rect[] splitPosition = HorizontalContainer.SplitRectangle(position, 2);

            Rect leftValuePosition = splitPosition[0];
            Rect leftButtonPosition = new Rect(leftValuePosition.x, leftValuePosition.y + 1, 15, 15);
            if (GUI.Button(leftButtonPosition, vector.x == int.MinValue ? lockContent : unlockContent, buttonStyle))
            {
                vector.x = vector.x == int.MinValue ? 0 : int.MinValue;
            }

            Rect leftFieldPosition = new Rect(leftButtonPosition.xMax + 2, leftValuePosition.y, leftValuePosition.width - 15, leftValuePosition.height);
            if (vector.x == int.MinValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(leftFieldPosition, "Min Value", textFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.x = EditorGUI.IntField(leftFieldPosition, vector.x);
            }


            Rect rightValuePosition = splitPosition[1];
            rightValuePosition.x += 5;
            Rect rightFieldPosition = new Rect(rightValuePosition.x, rightValuePosition.y, rightValuePosition.width - 15, rightValuePosition.height);
            if (vector.y == int.MaxValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rightFieldPosition, "Max Value", textFieldStyle);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                vector.y = EditorGUI.IntField(rightFieldPosition, vector.y);
            }

            Rect rightButtonPosition = new Rect(rightFieldPosition.xMax + 1, rightValuePosition.y + 1, 15, 15);
            if (GUI.Button(rightButtonPosition, vector.y == int.MaxValue ? lockContent : unlockContent, buttonStyle))
            {
                vector.y = vector.y == int.MaxValue ? 1 : int.MaxValue;
            }

            serializedField.GetSerializedProperty().vector2IntValue = vector;
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