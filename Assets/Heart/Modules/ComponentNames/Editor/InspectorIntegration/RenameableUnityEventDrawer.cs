using System;
using System.Reflection;
using System.Text;
using Sisus.Shared.EditorOnly;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Sisus.ComponentNames.Editor
{
    [CustomPropertyDrawer(typeof(UnityEventBase), true)]
    public class RenameableUnityEventDrawer : PropertyDrawer
    {
        private static readonly FieldInfo attributeField;
        private static readonly FieldInfo fieldInfoField;
        private static readonly FieldInfo textField;

        private UnityEventDrawer wrappedDrawer;

        static RenameableUnityEventDrawer()
        {
            attributeField = typeof(PropertyDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            fieldInfoField = typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            textField = typeof(UnityEventDrawer).GetField("m_Text", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

#if DEV_MODE
            Debug.Assert(attributeField != null);
            Debug.Assert(fieldInfoField != null);
            Debug.Assert(textField != null);
#endif
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                OnLeftMouseDown(position, property);
            }

            GetOrCreateWrappedDrawer(label.text).OnGUI(position, property, label);

#if DEV_MODE
            if(Event.current.control)
			{
                var c = Color.red;
                c.a = 0.5f;

                var functionDropdownRect = GetFirstFunctionDropdownRect(position);
                float elementHeight = GetElementHeight();
                float yMax = position.yMax - elementHeight;

                for(float y = functionDropdownRect.y; y < yMax; y += elementHeight)
                {
                    functionDropdownRect.y = y;
                    EditorGUI.DrawRect(functionDropdownRect, c);
                }
			}
#endif
        }

        private Rect GetFirstFunctionDropdownRect(Rect propertyRect)
        {
            var functionDropdownRect = propertyRect;
            functionDropdownRect.y += 27f;
            float indent = EditorGUI.IndentedRect(propertyRect).x;
            float firstDropdownWidth = (propertyRect.width - indent) * 0.3f;
            functionDropdownRect.xMin = indent + firstDropdownWidth + 23f;
            functionDropdownRect.width -= 7f;
            functionDropdownRect.height = EditorGUIUtility.singleLineHeight;
            return functionDropdownRect;
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var rootVisualElement = GetOrCreateWrappedDrawer(property.displayName).CreatePropertyGUI(property);
            if (rootVisualElement is null)
            {
                return null;
            }

            foreach (var child in rootVisualElement.Children())
            {
                if (child is Label label)
                {
                    label.text = property.displayName + GetNameSuffix(property);
                    return rootVisualElement;
                }
            }

            return rootVisualElement;
        }

        private string GetNameSuffix(SerializedProperty property)
        {
            if (property.GetMemberType() is not { } eventType) return "";

            for (var type = eventType; type != null; type = type.BaseType)
            {
                if (!type.IsGenericType) continue;

                var typeDefinition = type.GetGenericTypeDefinition();
                if (typeDefinition != typeof(UnityEvent<>) && typeDefinition != typeof(UnityEvent<,>) && typeDefinition != typeof(UnityEvent<,,>) &&
                    typeDefinition != typeof(UnityEvent<,,,>))
                {
                    continue;
                }

                var sb = new StringBuilder();
                sb.Append(" <color=grey>(");
                var genericArguments = type.GetGenericArguments();
                sb.Append(ObjectNames.NicifyVariableName(genericArguments[0].ToHumanReadableString()));
                int argumentCount = genericArguments.Length;
                for (var i = 1; i < argumentCount; i++)
                {
                    sb.Append(", ");
                    sb.Append(ObjectNames.NicifyVariableName(genericArguments[i].ToHumanReadableString()));
                }

                sb.Append(")</color>");
                return sb.ToString();
            }

            return "";
        }

        private UnityEventDrawer GetOrCreateWrappedDrawer(string label)
        {
            if (wrappedDrawer is null)
            {
                wrappedDrawer = new UnityEventDrawer();
                attributeField?.SetValue(wrappedDrawer, attribute);
                fieldInfoField?.SetValue(wrappedDrawer, fieldInfo);
                textField?.SetValue(wrappedDrawer, label);
            }

            return wrappedDrawer;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetOrCreateWrappedDrawer(label.text).GetPropertyHeight(property, label);
        }

        private void OnLeftMouseDown(Rect propertyRect, SerializedProperty property)
        {
            float elementHeight = GetElementHeight();
            var functionDropdownRect = GetFirstFunctionDropdownRect(propertyRect);
            float yMax = propertyRect.yMax - elementHeight;

            var index = 0;
            for (float y = functionDropdownRect.y; y < yMax; y += elementHeight)
            {
                functionDropdownRect.y = y;
                if (functionDropdownRect.Contains(Event.current.mousePosition))
                {
                    var genericMenu = UnityEventDrawerUtility.BuildFunctionSelectDropdownMenu(property, index);
                    if (genericMenu != null)
                    {
                        Event.current.Use();
                        genericMenu.DropDown(functionDropdownRect);
                        GUIUtility.ExitGUI();
                        return;
                    }
                }

                index++;
            }
        }

        private float GetElementHeight()
        {
            const float extraSpacing = 9f;
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing + extraSpacing;
        }
    }
}