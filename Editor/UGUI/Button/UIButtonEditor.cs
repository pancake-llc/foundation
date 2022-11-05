using System;
using UnityEditor;
using UnityEngine;
using Pancake.Editor;

namespace Pancake.UI.Editor
{
    [CustomEditor(typeof(UIButton), true)]
    [CanEditMultipleObjects]
    public class UIButtonEditor : UnityEditor.UI.ButtonEditor
    {
        protected const int DEFAULT_LABEL_WIDTH = 110;
        protected static readonly string[] ButtonMotion = {"Immediate", "Normal", "Uniform", "Late"};
        public static readonly string[] ButtonTypeClick = {"OnlySingleClick", "OnlyDoubleClick", "LongClick", "Instant", "Delayed"};
        private SerializedProperty _isMotion;
        private SerializedProperty _clickType;
        private SerializedProperty _onLongClick;
        private SerializedProperty _onDoubleClick;
        private SerializedProperty _onPointerUp;
        private SerializedProperty _isMotionUnableInteract;
        private SerializedProperty _isAffectToSelf;
        private SerializedProperty _allowMultipleClick;
        private SerializedProperty _timeDisableButton;
        private SerializedProperty _longClickInterval;
        private SerializedProperty _doubleClickInterval;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _affectObject;
        private SerializedProperty _motion;
        private SerializedProperty _scale;
        private SerializedProperty _durationDown;
        private SerializedProperty _durationUp;
        private SerializedProperty _interpolatorUp;
        private SerializedProperty _interpolatorDown;
        private SerializedProperty _motionUnableInteract;
        private SerializedProperty _scaleUnableInteract;
        private SerializedProperty _durationDownUnableInteract;
        private SerializedProperty _durationUpUnableInteract;
        private SerializedProperty _interpolatorUpInteract;
        private SerializedProperty _interpolatorDownInteract;

        protected override void OnEnable()
        {
            base.OnEnable();
            _clickType = serializedObject.FindProperty("clickType");
            _onLongClick = serializedObject.FindProperty("onLongClick");
            _onDoubleClick = serializedObject.FindProperty("onDoubleClick");
            _onPointerUp = serializedObject.FindProperty("onPointerUp");
            _allowMultipleClick = serializedObject.FindProperty("allowMultipleClick");
            _timeDisableButton = serializedObject.FindProperty("timeDisableButton");
            _longClickInterval = serializedObject.FindProperty("longClickInterval");
            _doubleClickInterval = serializedObject.FindProperty("doubleClickInterval");
            _ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            _isAffectToSelf = serializedObject.FindProperty("isAffectToSelf");
            _affectObject = serializedObject.FindProperty("affectObject");
            _isMotion = serializedObject.FindProperty("isMotion");
            _motion = serializedObject.FindProperty("motionData").FindPropertyRelative("motion");
            _scale = serializedObject.FindProperty("motionData").FindPropertyRelative("scale");
            _durationDown = serializedObject.FindProperty("motionData").FindPropertyRelative("durationDown");
            _durationUp = serializedObject.FindProperty("motionData").FindPropertyRelative("durationUp");
            _motionUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("motion");
            _scaleUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("scale");
            _durationDownUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("durationDown");
            _durationUpUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("durationUp");
            _isMotionUnableInteract = serializedObject.FindProperty("isMotionUnableInteract");
            _interpolatorUp = serializedObject.FindProperty("motionData").FindPropertyRelative("interpolatorUp");
            _interpolatorDown = serializedObject.FindProperty("motionData").FindPropertyRelative("interpolatorDown");     
            _interpolatorUpInteract = serializedObject.FindProperty("motionData").FindPropertyRelative("interpolatorUp");
            _interpolatorDownInteract = serializedObject.FindProperty("motionData").FindPropertyRelative("interpolatorDown");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawInspector();
        }

        protected virtual void DrawInspector() { Uniform.DrawUppercaseSection("UNIBUTTON_MOTION", "SETTING", () => Draw()); }

        protected void Draw(Action callback = null)
        {
            serializedObject.Update();

            if (callback != null)
            {
                callback.Invoke();
                GUILayout.Space(2);
            }

            Uniform.SpaceHalfLine();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Typle Click", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _clickType.enumValueIndex = EditorGUILayout.Popup(_clickType.enumValueIndex, ButtonTypeClick);
            EditorGUILayout.EndHorizontal();

            switch ((EButtonClickType) _clickType.enumValueIndex)
            {
                case EButtonClickType.OnlySingleClick:
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("  Multiple Click", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _allowMultipleClick.boolValue = GUILayout.Toggle(_allowMultipleClick.boolValue, "");
                    EditorGUILayout.EndHorizontal();

                    if (!_allowMultipleClick.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("  Duration(s)", "Second to trigger next click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _timeDisableButton.floatValue = EditorGUILayout.Slider(_timeDisableButton.floatValue, 0.05f, 1f);
                        EditorGUILayout.EndHorizontal();
                    }

                    break;
                }
                case EButtonClickType.LongClick:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Duration(s)", "Second to trigger long click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _longClickInterval.floatValue = EditorGUILayout.Slider(_longClickInterval.floatValue, 0.1f, 5f);
                    EditorGUILayout.EndHorizontal();
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Instant:
                case EButtonClickType.Delayed:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Duration(s)", "Second to trigger double click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _doubleClickInterval.floatValue = EditorGUILayout.Slider(_doubleClickInterval.floatValue, 0.05f, 1f);
                    EditorGUILayout.EndHorizontal();

                    break;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Ignore Time Scale", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _ignoreTimeScale.boolValue = GUILayout.Toggle(_ignoreTimeScale.boolValue, "");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Affect To Self", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _isAffectToSelf.boolValue = GUILayout.Toggle(_isAffectToSelf.boolValue, "");
            if (!_isAffectToSelf.boolValue)
                _affectObject.objectReferenceValue = EditorGUILayout.ObjectField("", _affectObject.objectReferenceValue, typeof(Transform), true) as Transform;
            EditorGUILayout.EndHorizontal();

            Uniform.SpaceTwoLine();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Use Motion", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _isMotion.boolValue = GUILayout.Toggle(_isMotion.boolValue, "");
            EditorGUILayout.EndHorizontal();

            if (_isMotion.boolValue)
            {
                Uniform.SpaceHalfLine();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("  Motion Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _motion.enumValueIndex = EditorGUILayout.Popup(_motion.enumValueIndex, ButtonMotion);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("  Scale(%)", GUILayout.Width(DEFAULT_LABEL_WIDTH - 50));
                GUILayout.FlexibleSpace();
                _scale.vector2Value = EditorGUILayout.Vector2Field("", _scale.vector2Value);

                EditorGUILayout.EndHorizontal();

                switch ((EButtonMotion) _motion.enumValueIndex)
                {
                    case EButtonMotion.Normal:
                    case EButtonMotion.Uniform:
                    case EButtonMotion.Late:
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("  Duration(Down)", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _durationDown.floatValue = EditorGUILayout.Slider(_durationDown.floatValue, 0.01f, 5f);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("  Duration(Up)", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _durationUp.floatValue = EditorGUILayout.Slider(_durationUp.floatValue, 0.01f, 5f);
                        EditorGUILayout.EndHorizontal();
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Interpolator Down"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorDown, new GUIContent(""));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Interpolator Up"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_interpolatorUp, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                Uniform.SpaceTwoLine();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Unable Interact", "Use motion when unable interact button"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _isMotionUnableInteract.boolValue = GUILayout.Toggle(_isMotionUnableInteract.boolValue, "");
                EditorGUILayout.EndHorizontal();

                if (_isMotionUnableInteract.boolValue)
                {
                    Uniform.SpaceHalfLine();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("  Motion Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _motionUnableInteract.enumValueIndex = EditorGUILayout.Popup(_motionUnableInteract.enumValueIndex, ButtonMotion);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("  Scale(%)", GUILayout.Width(DEFAULT_LABEL_WIDTH - 50));
                    _scaleUnableInteract.vector2Value = EditorGUILayout.Vector2Field("", _scaleUnableInteract.vector2Value);
                    EditorGUILayout.EndHorizontal();

                    switch ((EButtonMotion) _motionUnableInteract.enumValueIndex)
                    {
                        case EButtonMotion.Normal:
                        case EButtonMotion.Uniform:
                        case EButtonMotion.Late:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("  Duration(Down)", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                            _durationDownUnableInteract.floatValue = EditorGUILayout.Slider(_durationDownUnableInteract.floatValue, 0.01f, 5f);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("  Duration(Up)", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                            _durationUpUnableInteract.floatValue = EditorGUILayout.Slider(_durationUpUnableInteract.floatValue, 0.01f, 5f);
                            EditorGUILayout.EndHorizontal();
                            break;
                    }
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Interpolator Down"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    EditorGUILayout.PropertyField(_interpolatorDownInteract, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Interpolator Up"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    EditorGUILayout.PropertyField(_interpolatorUpInteract, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                    
                    Uniform.SpaceOneLine();
                }
            }


            switch ((EButtonClickType) _clickType.enumValueIndex)
            {
                case EButtonClickType.LongClick:
                    EditorGUILayout.PropertyField(_onLongClick);
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Instant:
                case EButtonClickType.Delayed:
                    EditorGUILayout.PropertyField(_onDoubleClick);
                    break;
            }

            EditorGUILayout.PropertyField(_onPointerUp);
            Uniform.HelpBox(
                "In case you move the mouse pointer away from the button while holding down the OnPointerUp event is still called. So be careful when using it",
                MessageType.Info);

            EditorGUILayout.Space();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}