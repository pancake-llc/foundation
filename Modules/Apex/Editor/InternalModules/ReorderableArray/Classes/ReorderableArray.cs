using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public class ReorderableArray
    {
        private const float VERTICAL_SPACE = 2;

        // Stored delegates.
        public OnHeaderGUIDelegate onHeaderGUICallback;
        public OnElementGUIDelegate onElementGUICallback;
        public GetElementHeightDelegate getElementHeightCallback;
        public OnNoneElementGUIDelegate onNoneElementGUICallback;
        public OnAddClickDelegate onAddClickCallback;
        public OnRemoveClickDelegate onRemoveClickCallback;

        // Stored required properties.
        private string headerName;
        private float headerHeight;
        private float buttonWidth;
        private bool showCount;
        private bool hasHover;
        private SerializedField serializedField;
        private ReorderableList reorderableList;
        private GUIContent addButtonContent;
        private GUIContent removeButtonContent;

        public ReorderableArray(SerializedField serializedField)
        {
            this.serializedField = serializedField;

            headerName = serializedField.GetLabel().text;
            headerHeight = 22;
            buttonWidth = headerHeight;
            showCount = true;

            addButtonContent = EditorGUIUtility.IconContent("Toolbar Plus");
            removeButtonContent = EditorGUIUtility.IconContent("Toolbar Minus");

            onHeaderGUICallback = (position) =>
            {
                if (Event.current.type == EventType.Repaint)
                {
                    position.x += 4;
                    ApexStyles.BoldFoldout.Draw(position,
                        headerName,
                        false,
                        false,
                        serializedField.IsExpanded(),
                        false);
                    position.x -= 4;
                }
            };

            onElementGUICallback = (position, index, isActive, isFocused) => { serializedField.GetArrayElement(index).OnGUI(position); };

            getElementHeightCallback = (index) => { return serializedField.GetArrayElement(index).GetHeight(); };

            onNoneElementGUICallback = (position) =>
            {
                position.x += 5;
                GUI.Label(position, $"Add {serializedField.GetLabel().text}...", ApexStyles.Label);
            };

            onAddClickCallback = (position) => { serializedField.IncreaseArraySize(); };

            onRemoveClickCallback = (position, index) => { serializedField.RemoveArrayElement(index); };

            reorderableList = new ReorderableList(serializedField.GetSerializedObject(),
                serializedField.GetSerializedProperty(),
                false,
                false,
                false,
                false)
            {
                headerHeight = 0,
                footerHeight = 0,
                showDefaultBackground = false,
                drawNoneElementCallback = onNoneElementGUICallback.Invoke,
                drawElementCallback = (position, index, isActive, isFocused) =>
                {
                    position.x -= 3;
                    position.width += 7;

                    EditorGUIUtility.labelWidth -= 17;
                    position.y += VERTICAL_SPACE;
                    position.width -= buttonWidth;
                    onElementGUICallback.Invoke(position, index, isActive, isFocused);
                    EditorGUIUtility.labelWidth += 17;

                    position.x = position.xMax + 2;
                    position.y -= VERTICAL_SPACE;
                    position.width = buttonWidth;
                    position.height += 1;
                    if (GUI.Button(position, removeButtonContent, ApexStyles.BoxCenteredButton))
                    {
                        onRemoveClickCallback.Invoke(position, index);
                        GUIUtility.ExitGUI();
                    }
                },
                elementHeightCallback = (index) => { return getElementHeightCallback.Invoke(index) + VERTICAL_SPACE; },
                drawElementBackgroundCallback = (position, index, isActive, isFocused) =>
                {
                    position.height += 1;
                    if (Event.current.type == EventType.Repaint)
                    {
                        if ((index + 1) % 2 == 0)
                        {
                            ApexStyles.BoxEntryEven.Draw(position,
                                false,
                                isActive,
                                isActive,
                                isFocused);
                        }
                        else
                        {
                            ApexStyles.BoxEntryOdd.Draw(position,
                                false,
                                isActive,
                                isActive,
                                isFocused);
                        }
                    }
                },
                onReorderCallback = (list) =>
                {
                    serializedField.GetSerializedObject().ApplyModifiedProperties();
                    serializedField.ApplyChildren();
                }
            };
        }

        public ReorderableArray(SerializedField serializedField, bool draggable)
            : this(serializedField)
        {
            reorderableList.draggable = draggable;
        }

        public void Draw(Rect position)
        {
            position.width += 1;

            float x = position.x;
            float contentHeight = position.height - headerHeight;
            float totalWidth = position.width;

            position.height = headerHeight;
            position.width -= buttonWidth;
            if (GUI.Button(position, GUIContent.none, ApexStyles.BoxButton))
            {
                serializedField.IsExpanded(!serializedField.IsExpanded());
            }

            onHeaderGUICallback.Invoke(position);

            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.MouseMove && hasHover != isHover)
            {
                serializedField.Repaint();
                hasHover = isHover;
            }

            if (showCount)
            {
                position.width -= 5;
                GUI.Label(position, $"{serializedField.GetArrayLength()} items", ApexStyles.SuffixMessage);
                position.width += 5;
            }

            position.x = position.xMax - 1;
            position.width = buttonWidth;
            if (GUI.Button(position, addButtonContent, ApexStyles.BoxCenteredButton))
            {
                onAddClickCallback.Invoke(position);
            }


            if (serializedField.IsExpanded() && contentHeight > 0)
            {
                position.x = x;
                position.y = position.yMax - 1;
                position.width = totalWidth - 1;
                position.height = contentHeight;
                GUI.Box(position, GUIContent.none, ApexStyles.BoxEntryBkg);


                position.y -= 3;

                reorderableList.DoList(position);
            }
        }

        public void DrawLayout() { Draw(ApexGUILayout.GetControlRect(GetHeight())); }

        public float GetHeight()
        {
            float height = headerHeight;
            if (serializedField.IsExpanded())
            {
                height += reorderableList.GetHeight() - 6f;
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

        public string GetHeaderName() { return headerName; }

        public void SetHeaderName(string value) { headerName = value; }

        public float GetHeaderHeight() { return headerHeight; }

        public void SetHeaderHeight(float value) { headerHeight = value; }

        public float GetButtonWidth() { return buttonWidth; }

        public void SetButtonWidth(float value) { buttonWidth = value; }

        public bool GetShowCount() { return showCount; }

        public void SetShowCount(bool value) { showCount = value; }

        public GUIContent GetAddButtonContent() { return addButtonContent; }

        public void SetAddButtonContent(GUIContent value) { addButtonContent = value; }

        public GUIContent GetRemoveButtonContent() { return removeButtonContent; }

        public void SetRemoveButtonContent(GUIContent value) { removeButtonContent = value; }

        #endregion
    }
}