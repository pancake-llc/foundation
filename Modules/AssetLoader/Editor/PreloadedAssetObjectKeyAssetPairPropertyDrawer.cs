using System.Collections.Generic;
using Pancake.AssetLoader;
using UnityEditor;
using UnityEngine;

namespace Pancake.AssetLoaderEditor
{
    [CustomPropertyDrawer(typeof(PreloadedAssetLoaderObject.KeyAssetPair))]
    internal sealed class PreloadedAssetObjectKeyAssetPairPropertyDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, PropertyData> _dataList = new Dictionary<string, PropertyData>();
        private PropertyData _property;

        private void Init(SerializedProperty property)
        {
            if (_dataList.TryGetValue(property.propertyPath, out _property)) return;

            _property = new PropertyData
            {
                keySourceProperty = property.FindPropertyRelative("keySource"),
                keyProperty = property.FindPropertyRelative("key"),
                assetProperty = property.FindPropertyRelative("asset")
            };
            _dataList.Add(property.propertyPath, _property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);
            var fieldRect = position;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

            using (new EditorGUI.PropertyScope(fieldRect, label, property))
            {
                property.isExpanded = EditorGUI.Foldout(new Rect(fieldRect), property.isExpanded, label, true);
                if (property.isExpanded)
                    using (new EditorGUI.IndentLevelScope())
                    {
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(new Rect(fieldRect), _property.keySourceProperty);

                        var keySource = (PreloadedAssetLoaderObject.KeyAssetPair.KeySourceType) _property.keySourceProperty.intValue;
                        GUI.enabled = keySource == PreloadedAssetLoaderObject.KeyAssetPair.KeySourceType.InputField;
                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(new Rect(fieldRect), _property.keyProperty);
                        GUI.enabled = true;

                        fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(new Rect(fieldRect), _property.assetProperty);
                    }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);
            var height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded) height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;

            return height;
        }

        private class PropertyData
        {
            public SerializedProperty assetProperty;
            public SerializedProperty keyProperty;
            public SerializedProperty keySourceProperty;
        }
    }
}