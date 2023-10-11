using System.Collections.Generic;
using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(EnumAttribute))]
    public class EnumView : BaseEnumView
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="field">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField field, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginDisabledGroup(IsEmpty());
            if (GUI.Button(position, GetValueName(position.width), EditorStyles.popup))
            {
                GenericMenu genericMenu = new GenericMenu();
                List<MenuItem> menuItems = GetMenuItems();
                for (int i = 0; i < menuItems.Count; i++)
                {
                    MenuItem menuItem = menuItems[i];
                    genericMenu.AddItem(menuItem.content, menuItem.isOn, SetEnumFunction, menuItem.bit);
                }

                genericMenu.DropDown(position);
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}