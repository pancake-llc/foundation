using System;
using Pancake.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI.Editor
{
    [CustomEditor(typeof(UIPopup), true)]
    [CanEditMultipleObjects]
    public class UIPopupEditor : UnityEditor.Editor
    {
        protected const int DEFAULT_LABEL_WIDTH = 110;
        protected static readonly string[] PopupMotionType = {"Scale", "Position", "PositionAndScale"};
        private SerializedProperty _closeButtons;
        private SerializedProperty _closeByBackButton;
        private SerializedProperty _closeByClickBackground;
        private SerializedProperty _closeByClickContainer;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _motionAffectDisplay;
        private SerializedProperty _motionAffectHide;
        private SerializedProperty _positionToHide;
        private SerializedProperty _positionToDisplay;
        private SerializedProperty _positionFromDisplay;
        private SerializedProperty _interpolatorDisplay;
        private SerializedProperty _interpolatorHide;
        private SerializedProperty _endValueHide;
        private SerializedProperty _endValueDisplay;
        private SerializedProperty _durationHide;
        private SerializedProperty _durationDisplay;
        private ReorderableList _closeButtonList;
        private SerializedProperty _uniqueId;

        public UIPopup popup;

        protected virtual void OnEnable()
        {
            popup = target as UIPopup;
            _closeByBackButton = serializedObject.FindProperty("closeByBackButton");
            _closeByClickBackground = serializedObject.FindProperty("closeByClickBackground");
            _closeByClickContainer = serializedObject.FindProperty("closeByClickContainer");
            _ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            _closeButtons = serializedObject.FindProperty("closeButtons");
            _motionAffectDisplay = serializedObject.FindProperty("motionAffectDisplay");
            _motionAffectHide = serializedObject.FindProperty("motionAffectHide");
            _positionToHide = serializedObject.FindProperty("positionToHide");
            _positionToDisplay = serializedObject.FindProperty("positionToDisplay");
            _positionFromDisplay = serializedObject.FindProperty("positionFromDisplay");
            _interpolatorDisplay = serializedObject.FindProperty("interpolatorDisplay");
            _interpolatorHide = serializedObject.FindProperty("interpolatorHide");
            _endValueHide = serializedObject.FindProperty("endValueHide");
            _endValueDisplay = serializedObject.FindProperty("endValueDisplay");
            _durationHide = serializedObject.FindProperty("durationHide");
            _durationDisplay = serializedObject.FindProperty("durationDisplay");
            _uniqueId = serializedObject.FindProperty("uniqueId");
            _closeButtonList = new ReorderableList(serializedObject,
                _closeButtons,
                true,
                true,
                true,
                true);
            _closeButtonList.drawElementCallback = DrawListButtonItem;
            _closeButtonList.drawHeaderCallback = DrawHeader;
        }

        private void DrawHeader(Rect rect) { EditorGUI.LabelField(rect, "Close Button"); }

        private void DrawListButtonItem(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty element = _closeButtonList.serializedProperty.GetArrayElementAtIndex(index); //The element in the list
            EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName), element.isExpanded);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUIUtility.labelWidth = 110;

            if (PrefabUtility.IsPartOfAnyPrefab(popup.gameObject))
            {
                if (!popup.gameObject.IsAddressableWithLabel(Pancake.UI.PopupHelper.POPUP_LABEL))
                {
                    Uniform.HelpBox("Click the toogle below to mark the popup as can be loaded by addressable", MessageType.Warning);
                    if (GUILayout.Button("Mark Popup")) popup.gameObject.MarkAddressableWithLabel(PopupHelper.POPUP_LABEL);
                }
                else
                {
                    Uniform.HelpBox("Marked as popup", MessageType.Info);
                }
            }

            Uniform.SpaceOneLine();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.BeginHorizontal();
            if (string.IsNullOrEmpty(_uniqueId.stringValue)) _uniqueId.stringValue = Ulid.NewUlid().ToString();
            GUILayout.Label("Id", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _uniqueId.stringValue = GUILayout.TextField(_uniqueId.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            Uniform.SpaceOneLine();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Ignore Time Scale", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _ignoreTimeScale.boolValue = GUILayout.Toggle(_ignoreTimeScale.boolValue, "");
            EditorGUILayout.EndHorizontal();
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("UIPOPUP_CLOSE", "CLOSE BY", DrawCloseSetting);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_DISPLAY", "DISPLAY", DrawDisplaySetting);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("UIPOPUP_SETTING_HIDE", "HIDE", DrawHideSetting);
            OnDrawExtraSetting(); // Draw custom field of inherit class


            void DrawCloseSetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Back Button", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByBackButton.boolValue = GUILayout.Toggle(_closeByBackButton.boolValue, "");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Background", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByClickBackground.boolValue = GUILayout.Toggle(_closeByClickBackground.boolValue, "");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Container", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _closeByClickContainer.boolValue = GUILayout.Toggle(_closeByClickContainer.boolValue, "");
                EditorGUILayout.EndHorizontal();

#pragma warning disable 612
                if (popup.BackgroundTransform != null)
                {
                    popup.BackgroundTransform.TryGetComponent<Button>(out var btn);
                    if (_closeByClickBackground.boolValue)
                    {
                        if (btn == null) btn = AddBlankButtonComponent(popup.BackgroundTransform.gameObject);
                        if (!popup.CloseButtons.Contains(btn)) popup.CloseButtons.Add(btn);
                    }
                    else
                    {
                        if (btn != null)
                        {
                            DestroyImmediate(btn);
                            _closeButtons?.RemoveEmptyArrayElements();
                        }
                    }
                }
                
                if (popup.ContainerTransform != null)
                {
                    popup.ContainerTransform.TryGetComponent<Button>(out var btn);
                    if (_closeByClickContainer.boolValue)
                    {
                        if (btn == null) btn = AddBlankButtonComponent(popup.ContainerTransform.gameObject);
                        if (!popup.CloseButtons.Contains(btn)) popup.CloseButtons.Add(btn);
                    }
                    else
                    {
                        if (btn != null)
                        {
                            DestroyImmediate(btn);
                            _closeButtons?.RemoveEmptyArrayElements();
                        }
                    }
                }
#pragma warning restore 612

                _closeButtonList.DoLayoutList();
            }

            void DrawDisplaySetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _motionAffectDisplay.enumValueIndex = EditorGUILayout.Popup(_motionAffectDisplay.enumValueIndex, PopupMotionType);
                EditorGUILayout.EndHorizontal();
                if (_motionAffectDisplay.enumValueIndex == (int) EMotionAffect.Position || _motionAffectDisplay.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    if (GUILayout.Button("Save From", GUILayout.Width(90)))
                    {
                        _positionFromDisplay.vector2Value = popup.ContainerTransform.localPosition;
                        popup.ContainerTransform.localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Save To", GUILayout.Width(90)))
                    {
                        _positionToDisplay.vector2Value = popup.ContainerTransform.localPosition;
                        popup.ContainerTransform.localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        _positionFromDisplay.vector2Value = Vector2.zero;
                        _positionToDisplay.vector2Value = Vector2.zero;
                        popup.ContainerTransform.localPosition = Vector3.zero;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   From", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    popup.positionToHide = EditorGUILayout.Vector2Field("", _positionFromDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   To", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    popup.positionToDisplay = EditorGUILayout.Vector2Field("", _positionToDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Interpolator"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorDisplay, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (_motionAffectDisplay.enumValueIndex == (int) EMotionAffect.Scale || _motionAffectDisplay.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Value", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _endValueDisplay.vector2Value = EditorGUILayout.Vector2Field("", _endValueDisplay.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                }

                _durationDisplay.floatValue = EditorGUILayout.FloatField("Duration", _durationDisplay.floatValue);
            }

            void DrawHideSetting()
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _motionAffectHide.enumValueIndex = EditorGUILayout.Popup(_motionAffectHide.enumValueIndex, PopupMotionType);
                EditorGUILayout.EndHorizontal();

                if (_motionAffectHide.enumValueIndex == (int) EMotionAffect.Position || _motionAffectHide.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("", GUILayout.Width(DEFAULT_LABEL_WIDTH));

                    if (GUILayout.Button("Save To", GUILayout.Width(90)))
                    {
                        _positionToHide.vector2Value = popup.ContainerTransform.localPosition;
                        popup.ContainerTransform.localPosition = Vector3.zero;
                    }

                    if (GUILayout.Button("Clear", GUILayout.Width(90)))
                    {
                        _positionToHide.vector2Value = Vector2.zero;
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("   To", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _positionToHide.vector2Value = EditorGUILayout.Vector2Field("", _positionToHide.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                    GUI.enabled = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Interpolator"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorHide, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                if (_motionAffectHide.enumValueIndex == (int) EMotionAffect.Scale || _motionAffectHide.enumValueIndex == (int) EMotionAffect.PositionAndScale)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Value", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _endValueHide.vector2Value = EditorGUILayout.Vector2Field("", _endValueHide.vector2Value, GUILayout.Height(18));
                    GUILayout.EndHorizontal();
                }

                _durationHide.floatValue = EditorGUILayout.FloatField("Duration", _durationHide.floatValue);
            }

            EditorGUILayout.Space();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        /// <summary>
        /// Method to implement drawing properties for classes that inherit from <see cref="UIPopupEditor"/>
        /// </summary>
        protected virtual void OnDrawExtraSetting() { }

        /// <summary>
        /// add blank button
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static Button AddBlankButtonComponent(GameObject target)
        {
            var button = target.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            return button;
        }
    }
}