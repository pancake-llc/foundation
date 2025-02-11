using Pancake;
using Pancake.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace PancakeEditor
{
    internal sealed class PagePickupDrawer : OdinAttributeDrawer<PagePickupAttribute, string>
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

            _selectedId = string.IsNullOrEmpty(value) ? "Select Page..." : value;
            var buttonRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.6f, position.height);
            var buttonColor = string.IsNullOrEmpty(value) ? Uniform.Error : new Color(0f, 0.18f, 0.53f, 0.31f);
            var defaultColor = GUI.backgroundColor;

            GUI.backgroundColor = buttonColor;

            if (GUI.Button(buttonRect, _selectedId))
            {
                var result = new List<Type>();

                var type = GetTypeByFullName();
                if (type != null)
                {
                    result.Clear();
                    result.AddRange(GetSubClasses(type));
                }

                var selector = new GenericSelector<Type>("Select Page", result, false, item => item.Name);

                selector.SetSelection(result.Filter(t => t.Name == _selectedId));
                selector.SelectionConfirmed += selection =>
                {
                    var datas = selection as Type[] ?? selection.ToArray();
                    if (datas.IsNullOrEmpty()) return;

                    var data = datas[0];
                    ValueEntry.SmartValue = data.Name;
                    _selectedId = data.Name;
                    GUIHelper.RequestRepaint();
                };

                selector.ShowInPopup();
            }

            GUI.backgroundColor = defaultColor;
            EditorGUIUtility.labelWidth = prev;
        }

        private List<Type> GetSubClasses(Type baseType) { return baseType.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Page`1")); }

        private Type GetTypeByFullName()
        {
            TypeExtensions.FindTypeByFullName("Pancake.UI.Page", out var type);
            return type;
        }
    }
}