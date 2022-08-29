using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.Database
{
    [CustomPropertyDrawer(typeof(EntityDropdownAttribute))]
    public class EntityDropdownDrawer : PropertyDrawer
    {
        public static List<Entity> content = new List<Entity>();
        public static SerializedProperty property;
        public static Type type;

        private static EntityDropdownAttribute dropdownAttribute;
        private static string[] contentNames;
        private static bool isClean;

        private static void RefreshContent(SerializedProperty property, Entity currentTarget)
        {
            EntityDropdownDrawer.property = property;
            if (!isClean) content = AllDataEntity(dropdownAttribute);
            isClean = true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null) return;

            EntityDropdownDrawer.property = property;

            dropdownAttribute = (EntityDropdownAttribute) attribute;
            type = dropdownAttribute.SourceType;

            // find the target object of the property field in the list of items in the project
            string pathToCurrentObj = AssetDatabase.GetAssetPath(property.objectReferenceValue);
            var currentTarget = AssetDatabase.LoadAssetAtPath<Entity>(pathToCurrentObj);

            float leftPadding = 14 * EditorGUI.indentLevel;
            const int buttonSizeX = 40;

            var left = new Rect(new Vector2(position.x + leftPadding, position.y), new Vector2(EditorGUIUtility.labelWidth - leftPadding, position.size.y));
            var mid = new Rect(new Vector2(position.x + left.size.x, position.y), new Vector2(position.size.x - left.size.x - buttonSizeX * 2, position.size.y));
            var right1New = new Rect(new Vector2(left.x + left.size.x + mid.size.x - leftPadding, position.y), new Vector2(buttonSizeX, left.size.y));
            var right2Edit = new Rect(new Vector2(left.x + left.size.x + mid.size.x + buttonSizeX - leftPadding, position.y), new Vector2(buttonSizeX, left.size.y));

            // put content titles into an array
            contentNames = new string[content.Count];
            for (var i = 0; i < content.Count; i++)
            {
                contentNames[i] = content[i].Title;
            }

            // build the field label on the left.
            GUI.Label(left, property.displayName);

            // insert the fancy dropdown
            var obj = (Entity) EntityDropdownDrawer.property.objectReferenceValue;
            string frontlabel = obj == null ? "(None)" : obj.Title;
            if (GUI.Button(mid, new GUIContent(frontlabel), EditorStyles.popup))
            {
                RefreshContent(property, currentTarget);
                var dropdown = new AdvancedDropdown(new AdvancedDropdownState()) {targetProperty = property};
                dropdown.Show(mid);
            }
            else isClean = false;

            if (type.IsAbstract) GUI.enabled = false;
            if (GUI.Button(right1New, "New"))
            {
                MakeNew(Dashboard.CreateNewEntity(type));
            }

            GUI.enabled = true;

            if (GUI.Button(right2Edit, "Edit"))
            {
                Dashboard.InspectAssetRemote(currentTarget, type);
            }
        }

        // Callback for the Dropdown being used to change the Property.
        public static void ItemSelected(SerializedProperty targetObject, Entity newValue) { PushUpdate(targetObject, newValue); }

        private static void PushUpdate(SerializedProperty targetProperty, Entity newValue)
        {
            if (newValue == null || newValue.Title == "(None)") targetProperty.objectReferenceValue = null;
            else targetProperty.objectReferenceValue = newValue;

            property.serializedObject.ApplyModifiedProperties();
        }

        private static void MakeNew(Entity value)
        {
            property.objectReferenceValue = value == null || value.Title == "(None)" ? null : value;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static List<Entity> AllDataEntity(EntityDropdownAttribute att)
        {
            var list = new List<Entity> {ScriptableObject.CreateInstance<None>()};
            string[] guids = AssetDatabase.FindAssets($"t:{att.SourceType}");
            list.AddRange(guids.Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), att.SourceType) as Entity));
            return list.OrderBy(x => x.Title).ToList();
        }

        /// <summary>
        /// This is strictly only used to represent the "None" or Null state of the dropdowns. It's instantiated in the background and never used in the game.
        /// The PopupField can't handle Null stuff, so this is easiest workaround.
        /// </summary>
        // ReSharper disable once ClassNeverInstantiated.Local
        private class None : Entity
        {
            public None()
            {
                Title = "(None)";
                Description = "Null";
            }

            protected override void Reset()
            {
                Title = "(None)";
                Description = "Null";
            }
        }
    }
}