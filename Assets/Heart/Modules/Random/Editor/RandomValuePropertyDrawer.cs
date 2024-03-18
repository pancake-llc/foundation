using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [CustomPropertyDrawer(typeof(RandomValue<>), true)]
    public class RandomValuePropertyDrawer : PropertyDrawer
    {
        private readonly string[] _popupOptions = {"Use Constant", "Use Random"};
        private GUIStyle _popupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _popupStyle ??= new GUIStyle(GUI.skin.GetStyle("PaneOptions")) {imagePosition = ImagePosition.ImageOnly};

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            var useConstant = property.FindPropertyRelative("useConstant");
            var min = property.FindPropertyRelative("min");
            var max = property.FindPropertyRelative("max");

            var buttonRect = new Rect(position);
            buttonRect.yMin += _popupStyle.margin.top;
            buttonRect.width = _popupStyle.fixedWidth + _popupStyle.margin.right;
            position.xMin = buttonRect.xMax + 15f;
            var minRect = useConstant.boolValue ? new Rect(position) : new Rect(position) {width = 80f};
            var maxRect = new Rect(position.x + minRect.width + 15f, position.y, minRect.width, buttonRect.height);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, _popupOptions, _popupStyle);

            useConstant.boolValue = result == 0;

            EditorGUI.PropertyField(minRect, min, GUIContent.none);
            if (!useConstant.boolValue) EditorGUI.PropertyField(maxRect, max, GUIContent.none);

            if (EditorGUI.EndChangeCheck()) property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}