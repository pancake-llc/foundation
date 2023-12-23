using Pancake.Apex;
using Pancake.ExLibEditor.Windows;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(SearchableEnumAttribute))]
    public class SearchableEnumView : BaseEnumView
    {
        private float height;
        private bool useToggleIcons;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="field">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField field, ViewAttribute viewAttribute, GUIContent label)
        {
            base.Initialize(field, viewAttribute, label);
            SearchableEnumAttribute attribute = (SearchableEnumAttribute) viewAttribute;
            height = attribute.Height;
            useToggleIcons = FlagMode() && attribute.ToggleIcons;
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="field">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField field, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            if (GUI.Button(position, GetValueName(position.width), EditorStyles.popup))
            {
                Texture toggleIcon = null;
                Texture emptyIcon = null;

                if (useToggleIcons)
                {
                    toggleIcon = EditorGUIUtility.IconContent("d_Toggle Icon").image;

                    Texture2D emptyTexture = new Texture2D(1, 1);
                    emptyTexture.SetPixel(0, 0, Color.clear);
                    emptyTexture.Apply();
                    emptyIcon = emptyTexture;
                }

                ExSearchWindow searchWindow = ExSearchWindow.Create(ObjectNames.NicifyVariableName(GetEnumType().Name));
                searchWindow.SetSortType(ExSearchWindow.SortType.None);

                List<MenuItem> menuItems = GetMenuItems();
                for (int i = 0; i < menuItems.Count; i++)
                {
                    MenuItem menuItem = menuItems[i];
                    if (useToggleIcons)
                    {
                        menuItem.content.image = menuItem.isOn ? toggleIcon : emptyIcon;
                    }
                    else if (menuItem.isOn)
                    {
                        menuItem.content.text += " •";
                    }

                    searchWindow.AddEntry(menuItem.content, menuItem.bit, SetEnumFunction);
                }

                searchWindow.Open(position, 0, height);
            }
        }
    }
}