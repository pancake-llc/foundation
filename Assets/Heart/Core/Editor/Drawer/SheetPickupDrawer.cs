using Pancake;
using Pancake.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Editor = PancakeEditor.Common.Editor;

namespace PancakeEditor
{
    internal sealed class SheetPickupDrawer : OdinAttributeDrawer<SheetPickupAttribute, string>
    {
        private string _selectedId;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            string value = ValueEntry.SmartValue;
            var position = EditorGUILayout.GetControlRect();
            if (label != null)
            {
                var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                EditorGUI.LabelField(labelRect, ObjectNames.NicifyVariableName(label.text.ToCamelCase()));
            }

            float prev = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            _selectedId = string.IsNullOrEmpty(value) ? "Select type..." : value;

            var buttonRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.6f, position.height);
            var buttonColor = string.IsNullOrEmpty(value) ? Uniform.Error : new Color(0.93f, 0.78f, 0.22f, 0.31f);
            var defaultColor = GUI.backgroundColor;

            GUI.backgroundColor = buttonColor;

            if (GUI.Button(buttonRect, _selectedId))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"), string.IsNullOrEmpty(value), () => ValueEntry.SmartValue = string.Empty);

                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = GetSubClasses(type);
                    for (var i = 0; i < result.Count; i++)
                    {
                        int cachei = i;
                        bool isSelected = value == result[cachei].Name;
                        menu.AddItem(new GUIContent(result[cachei].Name), isSelected, () => ValueEntry.SmartValue = result[cachei].Name);
                    }
                }

                menu.DropDown(new Rect(Editor.CurrentEvent.MousePosition, Vector2.zero));
            }

            GUI.backgroundColor = defaultColor;
            EditorGUIUtility.labelWidth = prev;
        }

        private List<Type> GetSubClasses(Type baseType) { return baseType.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Sheet`1")); }

        private Type GetTypeByFullName()
        {
            TypeExtensions.FindTypeByFullName("Pancake.UI.Sheet", out var type);
            return type;
        }
    }
}