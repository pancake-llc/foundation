using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.ExLibEditor
{
    public class ReorderableArray
    {
        public OnHeaderGUIDelegate onHeaderGUICallback;
        public OnElementGUIDelegate onElementGUICallback;
        public GetElementHeightDelegate getElementHeightCallback;
        public OnNoneElementGUIDelegate onNoneElementGUICallback;
        public OnAddClickDelegate onAddClickCallback;
        public OnRemoveClickDelegate onRemoveClickCallback;

        // Stored required properties.
        private SerializedProperty serializedProperty;
        private ReorderableList reorderableList;
        private GUIContent addButtonContent;
        private GUIContent removeButtonContent;
        private float headerHeight;
        private float buttonWidth;

        public ReorderableArray(SerializedObject serializedObject, SerializedProperty serializedProperty)
        {
            this.serializedProperty = serializedProperty;

            headerHeight = 22;
            buttonWidth = headerHeight;

            addButtonContent = EditorGUIUtility.IconContent("Toolbar Plus");
            removeButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");

            onHeaderGUICallback = (position) =>
            {
                position.x += 10;
                GUI.Label(position, this.serializedProperty.displayName, ExEditorStyles.LabelBold);
            };

            onElementGUICallback = (position, index, isActive, isFocused) =>
            {
                position.y += 3;
                position.height -= 1;
                SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(position, element, true);
            };

            getElementHeightCallback = (index) =>
            {
                SerializedProperty element = serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing;
            };

            onNoneElementGUICallback = (position) =>
            {
                position.x += 5;
                GUI.Label(position, $"Add {this.serializedProperty.displayName}...", ExEditorStyles.Label);
            };

            onAddClickCallback = (position) => { this.serializedProperty.arraySize++; };

            onRemoveClickCallback = (position, index) => { this.serializedProperty.DeleteArrayElementAtIndex(index); };

            reorderableList = new ReorderableList(serializedObject,
                serializedProperty,
                false,
                false,
                false,
                false)
            {
                headerHeight = 0,
                footerHeight = 0,
                showDefaultBackground = false,
                elementHeightCallback = getElementHeightCallback.Invoke,
                drawNoneElementCallback = onNoneElementGUICallback.Invoke,
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    position.x -= 3;
                    position.width += 7;
                    Rect elementPosition = new Rect(position.x, position.y, position.width - buttonWidth, position.height);
                    onElementGUICallback.Invoke(elementPosition, index, isActive, isFocused);

                    Rect buttonPosition = new Rect(elementPosition.xMax + 2, position.y, buttonWidth, position.height + 1);
                    if (GUI.Button(buttonPosition, removeButtonContent, ExEditorStyles.ArrayCenteredButton))
                    {
                        onRemoveClickCallback.Invoke(buttonPosition, index);
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                },
                drawElementBackgroundCallback = (position, index, isActive, isFocused) =>
                {
                    position.height += 1;
                    if (Event.current.type == EventType.Repaint)
                    {
                        if ((index + 1) % 2 == 0)
                        {
                            ExEditorStyles.ArrayEntryEven.Draw(position,
                                false,
                                isActive,
                                isActive,
                                isFocused);
                        }
                        else
                        {
                            ExEditorStyles.ArrayEntryOdd.Draw(position,
                                false,
                                isActive,
                                isActive,
                                isFocused);
                        }
                    }
                }
            };
        }

        public ReorderableArray(SerializedObject serializedObject, SerializedProperty serializedProperty, bool draggable)
            : this(serializedObject, serializedProperty)
        {
            reorderableList.draggable = draggable;
        }

        public void Draw(Rect position)
        {
            Rect headerPosition = new Rect(position.x, position.y, position.width - buttonWidth, headerHeight);
            if (GUI.Button(headerPosition, GUIContent.none, ExEditorStyles.ArrayButton))
            {
                serializedProperty.isExpanded = !serializedProperty.isExpanded;
            }

            onHeaderGUICallback.Invoke(headerPosition);

            Rect plusPosition = new Rect(headerPosition.xMax - 1, position.y, buttonWidth, headerHeight);
            if (GUI.Button(plusPosition, addButtonContent, ExEditorStyles.ArrayCenteredButton))
            {
                onAddClickCallback.Invoke(plusPosition);
            }

            if (serializedProperty.isExpanded)
            {
                Rect backgroundPosition = new Rect(position.x, headerPosition.yMax - 1, position.width - 1, position.height - headerPosition.height);
                GUI.Box(backgroundPosition, GUIContent.none, ExEditorStyles.ArrayEntryBkg);

                Rect listPosition = new Rect(backgroundPosition.x, backgroundPosition.y - 3f, backgroundPosition.width, backgroundPosition.height);
                reorderableList.DoList(listPosition);
            }
        }

        public void DrawLayout()
        {
            Rect position = GUILayoutUtility.GetRect(0, GetHeight());
            Draw(position);
        }

        public float GetHeight()
        {
            float height = headerHeight;
            if (serializedProperty.isExpanded)
            {
                height += reorderableList.GetHeight() - 8f;
            }

            return height;
        }

        #region [Delegates]

        /// <summary>
        /// Called to drawing array header.
        /// </summary>
        /// <param name="position">Header position.</param>
        public delegate void OnHeaderGUIDelegate(Rect position);

        /// <summary>
        /// Called to drawing element GUI.
        /// </summary>
        /// <param name="position">Element position.</param>
        /// <param name="index">Element array index.</param>
        /// <param name="isActive">Element is active.</param>
        /// <param name="isFocused">Element has been focused.</param>
        public delegate void OnElementGUIDelegate(Rect position, int index, bool isActive, bool isFocused);

        /// <summary>
        /// Called to calculate element height.
        /// </summary>
        /// <param name="index">Element array index.</param>
        public delegate float GetElementHeightDelegate(int index);

        /// <summary>
        /// Called to drawing placeholder when array is empty.
        /// </summary>
        public delegate void OnNoneElementGUIDelegate(Rect position);

        /// <summary>
        /// Called when add button is clicked.
        /// </summary>
        /// <param name="position">Button rectangle position.</param>
        public delegate void OnAddClickDelegate(Rect position);

        /// <summary>
        /// Called when remove button is clicked.
        /// </summary>
        /// <param name="position">Button rectangle position.</param>
        /// <param name="index">Remove element index.</param>
        public delegate void OnRemoveClickDelegate(Rect position, int index);

        #endregion

        #region [Getter / Setter]

        public GUIContent GetAddButtonContent() { return addButtonContent; }

        public void SetAddButtonContent(GUIContent value) { addButtonContent = value; }

        public GUIContent GetRemoveButtonContent() { return removeButtonContent; }

        public void SetRemoveButtonContent(GUIContent value) { removeButtonContent = value; }

        public float GetHeaderHeight() { return headerHeight; }

        public void SetHeaderHeight(float value) { headerHeight = value; }

        public float GetButtonWidth() { return buttonWidth; }

        public void SetButtonWidth(float value) { buttonWidth = value; }

        #endregion
    }
}