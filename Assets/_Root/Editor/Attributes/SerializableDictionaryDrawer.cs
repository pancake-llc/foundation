using Pancake.Serialization;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [DrawerTarget(typeof(SerializableDictionaryBase), Subclasses = true)]
    sealed class SerializableDictionaryDrawer : FieldDrawer
    {
        private const float SpaceBetweenElements = 5.0f;
        private const float RemoveButtonWidth = 26.0f;

        private SerializedField keys;
        private SerializedField values;
        private EntityContainer container;

        private GUIContent keyContent;
        private GUIContent valueContent;
        private GUIContent removeButtonContent;

        /// <summary>
        /// Called once when initializing serialized field drawer.
        /// </summary>
        /// <param name="serializedField">Serialized field with DrawerAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override void Initialize(SerializedField serializedField, GUIContent label)
        {
            keys = serializedField.GetChild(0) as SerializedField;
            values = serializedField.GetChild(1) as SerializedField;

            container = new FoldoutContainer(label.text, "Group", null)
            {
                onChildrenGUI = OnChildrenGUI,
                getChildrenHeight = GetChildrenHeight,
                onMenuButtonClick = OnPlusClick,
                menuIconContent = EditorGUIUtility.IconContent("Toolbar Plus")
            };

            keyContent = new GUIContent("Key");
            valueContent = new GUIContent("Value");
            removeButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");
        }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the serialized field drawer GUI.</param>
        /// <param name="serializedField">Reference of serialized field with drawer attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label) { container.OnGUI(position); }


        /// <summary>
        /// Get height which needed to serialized field drawer.
        /// </summary>
        /// <param name="serializedField">Serialized field with DrawerAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public override float GetHeight(SerializedField serializedField, GUIContent label) { return container.GetHeight(); }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the serialized field drawer GUI.</param>
        private void OnChildrenGUI(Rect position)
        {
            position.height = 0;

            float space = EditorGUIUtility.standardVerticalSpacing;
            int count = keys.GetArrayLength();
            for (int i = 0; i < count; i++)
            {
                SerializedField key = keys.GetArrayElement(i);
                SerializedField value = values.GetArrayElement(i);

                float keyHeight = key.GetHeight();
                float valueHeight = value.GetHeight();
                float totalHeight = keyHeight + valueHeight;

                Rect boxPosition = new Rect(position.x, position.y - 3, position.width + 4, totalHeight + 8);
                GUI.Box(boxPosition, GUIContent.none, Uniform.ContentBackground);

                Rect keyPosition = new Rect(position.x, position.y, position.width - RemoveButtonWidth, keyHeight);
                key.SetLabel(keyContent);
                key.OnGUI(keyPosition);

                Rect valuePosition = new Rect(position.x, keyPosition.yMax + space, keyPosition.width, valueHeight);
                value.SetLabel(valueContent);
                value.OnGUI(valuePosition);

                Rect removeButtonPosition = new Rect(boxPosition.xMax - RemoveButtonWidth, boxPosition.y, RemoveButtonWidth, boxPosition.height);
                if (GUI.Button(removeButtonPosition, removeButtonContent, Uniform.ActionButton))
                {
                    keys.RemoveArrayElement(i);
                    values.RemoveArrayElement(i);
                    break;
                }

                position.y = valuePosition.yMax + SpaceBetweenElements;
            }
        }

        /// <summary>
        /// Get height which needed to serialized field drawer.
        /// </summary>
        private float GetChildrenHeight()
        {
            float height = 0;
            float space = EditorGUIUtility.standardVerticalSpacing;
            int count = keys.GetArrayLength();
            int lastIndex = count - 1;
            for (int i = 0; i < count; i++)
            {
                SerializedField key = keys.GetArrayElement(i);
                SerializedField value = values.GetArrayElement(i);

                height += key.GetHeight() + value.GetHeight() + space;
                if (i < lastIndex)
                {
                    height += SpaceBetweenElements;
                }
            }

            return height;
        }

        /// <summary>
        /// Called when pressed container button.
        /// </summary>
        private void OnPlusClick(Rect position)
        {
            keys.IncreaseArraySize();
            values.IncreaseArraySize();
        }
    }
}