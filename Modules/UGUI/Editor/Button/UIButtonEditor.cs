using System;
using Pancake.ExLibEditor;
using Pancake.Sound;
using Pancake.Tracking;
using UnityEditor;
using UnityEngine;
using Pancake.UI;

namespace PancakeEditor
{
    [CustomEditor(typeof(UIButton), true)]
    [CanEditMultipleObjects]
    public class UIButtonEditor : UnityEditor.UI.ButtonEditor
    {
        protected const int DEFAULT_LABEL_WIDTH = 115;
        protected static readonly string[] ButtonMotion = {"Immediate", "Normal", "Uniform", "Late"};
        public static readonly string[] ButtonTypeClick = {"OnlySingleClick", "OnlyDoubleClick", "LongClick", "Instant", "Delayed", "Hold"};
        private SerializedProperty _isMotion;
        private SerializedProperty _clickType;
        private SerializedProperty _onLongClick;
        private SerializedProperty _onDoubleClick;
        private SerializedProperty _onPointerUp;
        private SerializedProperty _onHold;
        private SerializedProperty _isMotionUnableInteract;
        private SerializedProperty _isAffectToSelf;
        private SerializedProperty _allowMultipleClick;
        private SerializedProperty _timeDisableButton;
        private SerializedProperty _longClickInterval;
        private SerializedProperty _delayDetectHold;
        private SerializedProperty _doubleClickInterval;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _affectObject;
        private SerializedProperty _enabledSound;
        private SerializedProperty _audioClick;
        private SerializedProperty _audioPlayEvent;
        private SerializedProperty _enabledTracking;
        private SerializedProperty _trackingEvent;
        private SerializedProperty _motion;
        private SerializedProperty _scale;
        private SerializedProperty _duration;
        private SerializedProperty _ease;
        private SerializedProperty _motionUnableInteract;
        private SerializedProperty _scaleUnableInteract;
        private SerializedProperty _durationUnableInteract;
        private SerializedProperty _easeInteract;

        protected override void OnEnable()
        {
            base.OnEnable();
            _clickType = serializedObject.FindProperty("clickType");
            _onLongClick = serializedObject.FindProperty("onLongClick");
            _onDoubleClick = serializedObject.FindProperty("onDoubleClick");
            _onPointerUp = serializedObject.FindProperty("onPointerUp");
            _onHold = serializedObject.FindProperty("onHold");
            _allowMultipleClick = serializedObject.FindProperty("allowMultipleClick");
            _timeDisableButton = serializedObject.FindProperty("timeDisableButton");
            _longClickInterval = serializedObject.FindProperty("longClickInterval");
            _delayDetectHold = serializedObject.FindProperty("delayDetectHold");
            _doubleClickInterval = serializedObject.FindProperty("doubleClickInterval");
            _ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            _isAffectToSelf = serializedObject.FindProperty("isAffectToSelf");
            _affectObject = serializedObject.FindProperty("affectObject");
            _enabledSound = serializedObject.FindProperty("enabledSound");
            _audioClick = serializedObject.FindProperty("audioClick");
            _audioPlayEvent = serializedObject.FindProperty("audioPlayEvent");
            _enabledTracking = serializedObject.FindProperty("enabledTracking");
            _trackingEvent = serializedObject.FindProperty("trackingEvent");
            _isMotion = serializedObject.FindProperty("isMotion");
            _motion = serializedObject.FindProperty("motionData").FindPropertyRelative("motion");
            _scale = serializedObject.FindProperty("motionData").FindPropertyRelative("scale");
            _duration = serializedObject.FindProperty("motionData").FindPropertyRelative("duration");
            _motionUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("motion");
            _scaleUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("scale");
            _durationUnableInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("duration");
            _isMotionUnableInteract = serializedObject.FindProperty("isMotionUnableInteract");
            _ease = serializedObject.FindProperty("motionData").FindPropertyRelative("ease");
            _easeInteract = serializedObject.FindProperty("motionDataUnableInteract").FindPropertyRelative("ease");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawInspector();
        }

        protected virtual void DrawInspector() { Uniform.DrawGroupFoldout("uibutton_setting", "Settings", () => Draw()); }

        protected void Draw(Action callback = null)
        {
            serializedObject.Update();

            if (callback != null)
            {
                callback.Invoke();
                GUILayout.Space(2);
            }

            EditorGUILayout.Space();
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
                        GUILayout.Label(new GUIContent("  Duration(s)", "Number of seconds to trigger next click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _timeDisableButton.floatValue = EditorGUILayout.Slider(_timeDisableButton.floatValue, 0.05f, 1f);
                        EditorGUILayout.EndHorizontal();
                    }

                    break;
                }
                case EButtonClickType.LongClick:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Duration(s)", "Number of seconds to trigger long click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _longClickInterval.floatValue = EditorGUILayout.Slider(_longClickInterval.floatValue, 0.1f, 5f);
                    EditorGUILayout.EndHorizontal();
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Instant:
                case EButtonClickType.Delayed:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Duration(s)", "Number of seconds to trigger double click"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _doubleClickInterval.floatValue = EditorGUILayout.Slider(_doubleClickInterval.floatValue, 0.05f, 1f);
                    EditorGUILayout.EndHorizontal();
                    break;
                case EButtonClickType.Hold:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Delay Detect(s)", "Number of seconds to detect hold"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _delayDetectHold.floatValue = EditorGUILayout.Slider(_delayDetectHold.floatValue, 0.2f, 1f);
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

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Is Play Sound", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _enabledSound.boolValue = GUILayout.Toggle(_enabledSound.boolValue, "");
            EditorGUILayout.EndHorizontal();


            if (_enabledSound.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Audio", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _audioClick.objectReferenceValue = EditorGUILayout.ObjectField("", _audioClick.objectReferenceValue, typeof(Audio), false) as Audio;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("   Channel", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _audioPlayEvent.objectReferenceValue =
                    EditorGUILayout.ObjectField("", _audioPlayEvent.objectReferenceValue, typeof(ScriptableEventAudio), false) as ScriptableEventAudio;
                EditorGUILayout.EndHorizontal();
            }

            switch ((EButtonClickType) _clickType.enumValueIndex)
            {
                case EButtonClickType.OnlySingleClick:
                case EButtonClickType.Instant:
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Enable Tracking", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _enabledTracking.boolValue = GUILayout.Toggle(_enabledTracking.boolValue, "");
                    EditorGUILayout.EndHorizontal();

                    if (_enabledTracking.boolValue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("   Event", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _trackingEvent.objectReferenceValue =
                            EditorGUILayout.ObjectField("", _trackingEvent.objectReferenceValue, typeof(ScriptableTrackingNoParam), false) as ScriptableTrackingNoParam;
                        EditorGUILayout.EndHorizontal();
                    }

                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Use Motion", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _isMotion.boolValue = GUILayout.Toggle(_isMotion.boolValue, "");
            EditorGUILayout.EndHorizontal();

            if (_isMotion.boolValue)
            {
                EditorGUILayout.Space();
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
                        GUILayout.Label("  Duration", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                        _duration.floatValue = EditorGUILayout.Slider(_duration.floatValue, 0.01f, 5f);
                        EditorGUILayout.EndHorizontal();
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Ease"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                EditorGUILayout.PropertyField(_ease, new GUIContent(""));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("  Unable Interact", "Use motion when unable interact button"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                _isMotionUnableInteract.boolValue = GUILayout.Toggle(_isMotionUnableInteract.boolValue, "");
                EditorGUILayout.EndHorizontal();

                if (_isMotionUnableInteract.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("  Motion Type", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    _motionUnableInteract.enumValueIndex = EditorGUILayout.Popup(_motionUnableInteract.enumValueIndex, ButtonMotion);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("  Scale(%)", GUILayout.Width(DEFAULT_LABEL_WIDTH - 50));
                    GUILayout.FlexibleSpace();
                    _scaleUnableInteract.vector2Value = EditorGUILayout.Vector2Field("", _scaleUnableInteract.vector2Value);
                    EditorGUILayout.EndHorizontal();

                    switch ((EButtonMotion) _motionUnableInteract.enumValueIndex)
                    {
                        case EButtonMotion.Normal:
                        case EButtonMotion.Uniform:
                        case EButtonMotion.Late:
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("  Duration", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                            _durationUnableInteract.floatValue = EditorGUILayout.Slider(_durationUnableInteract.floatValue, 0.01f, 5f);
                            EditorGUILayout.EndHorizontal();
                            break;
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Ease"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    EditorGUILayout.PropertyField(_easeInteract, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }
            }

            switch ((EButtonClickType) _clickType.enumValueIndex)
            {
                case EButtonClickType.LongClick:
                    EditorGUILayout.PropertyField(_onLongClick);
                    break;
                case EButtonClickType.Hold:
                    EditorGUILayout.PropertyField(_onHold);
                    break;
                case EButtonClickType.OnlyDoubleClick:
                case EButtonClickType.Instant:
                case EButtonClickType.Delayed:
                    EditorGUILayout.PropertyField(_onDoubleClick);
                    break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "In case you move the mouse pointer away from the button while holding down the OnPointerUp event is still called. So be careful when using it",
                MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onPointerUp);

            EditorGUILayout.Space();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}