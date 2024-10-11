using Pancake.Common;
using PancakeEditor.Common;
using Pancake.MobileInput;
using UnityEditor;
using UnityEngine;

namespace Pancake.MobileInputEditor
{
    [CustomEditor(typeof(TouchCamera), true)]
    public class TouchCameraEditor : UnityEditor.Editor
    {
        private const int SIZE_LABEL = 100;

        private SerializedProperty _cameraAxesProperty;
        private SerializedProperty _cameraProperty;
        private SerializedProperty _perspectiveZoomModeProperty;
        private SerializedProperty _camZoomMinProperty;
        private SerializedProperty _camZoomMaxProperty;
        private SerializedProperty _camOverzoomMarginProperty;
        private SerializedProperty _camOverdragMarginProperty;
        private SerializedProperty _boundaryMinProperty;
        private SerializedProperty _boundaryMaxProperty;
        private SerializedProperty _camFollowFactorProperty;
        private SerializedProperty _autoScrollDampModeProperty;
        private SerializedProperty _autoScrollDampProperty;
        private SerializedProperty _autoScrollDampCurveProperty;
        private SerializedProperty _groundLevelOffsetProperty;
        private SerializedProperty _enableRotationProperty;
        private SerializedProperty _enableTiltProperty;
        private SerializedProperty _tiltAngleMinProperty;
        private SerializedProperty _tiltAngleMaxProperty;
        private SerializedProperty _enableZoomTiltProperty;
        private SerializedProperty _zoomTiltAngleMinProperty;
        private SerializedProperty _zoomTiltAngleMaxProperty;
        private SerializedProperty _onPickItemProperty;
        private SerializedProperty _onPickItem2DProperty;
        private SerializedProperty _onPickItemDoubleClickProperty;
        private SerializedProperty _onPickItem2DDoubleClickProperty;
        private SerializedProperty _keyboardAndMousePlatformsProperty;
        private SerializedProperty _controlTweakablesEnabledProperty;
        private SerializedProperty _keyboardControlsModifiersProperty;
        private SerializedProperty _mouseRotationFactorProperty;
        private SerializedProperty _mouseTiltFactorProperty;
        private SerializedProperty _mouseZoomFactorProperty;
        private SerializedProperty _keyboardRotationFactorProperty;
        private SerializedProperty _keyboardTiltFactorProperty;
        private SerializedProperty _keyboardZoomFactorProperty;
        private SerializedProperty _keyboardRepeatFactorProperty;
        private SerializedProperty _keyboardRepeatDelayProperty;
        private SerializedProperty _advanceModeProperty;
        private SerializedProperty _zoomBackSpringFactorProperty;
        private SerializedProperty _dragBackSpringFactorProperty;
        private SerializedProperty _autoScrollVelocityMaxProperty;
        private SerializedProperty _dampFactorTimeMultiplierProperty;
        private SerializedProperty _isPinchModeExclusiveProperty;
        private SerializedProperty _customZoomSensitivityProperty;
        private SerializedProperty _terrainColliderProperty;
        private SerializedProperty _cameraTransformProperty;
        private SerializedProperty _is2dOverdragMarginEnabledProperty;
        private SerializedProperty _camOverdragMargin2dProperty;
        private SerializedProperty _rotationDetectionDeltaThresholdProperty;
        private SerializedProperty _rotationMinPinchDistanceProperty;
        private SerializedProperty _rotationLockThresholdProperty;
        private SerializedProperty _pinchModeDetectionMoveTresholdProperty;
        private SerializedProperty _pinchTiltModeThresholdProperty;
        private SerializedProperty _pinchTiltSpeedProperty;

        private void OnEnable()
        {
            _cameraAxesProperty = serializedObject.FindProperty("cameraAxes");
            _cameraProperty = serializedObject.FindProperty("cam");
            _perspectiveZoomModeProperty = serializedObject.FindProperty("perspectiveZoomMode");
            _camZoomMinProperty = serializedObject.FindProperty("camZoomMin");
            _camZoomMaxProperty = serializedObject.FindProperty("camZoomMax");
            _camOverzoomMarginProperty = serializedObject.FindProperty("camOverzoomMargin");
            _camOverdragMarginProperty = serializedObject.FindProperty("camOverdragMargin");
            _boundaryMinProperty = serializedObject.FindProperty("boundaryMin");
            _boundaryMaxProperty = serializedObject.FindProperty("boundaryMax");
            _camFollowFactorProperty = serializedObject.FindProperty("camFollowFactor");
            _autoScrollDampModeProperty = serializedObject.FindProperty("autoScrollDampMode");
            _autoScrollDampProperty = serializedObject.FindProperty("autoScrollDamp");
            _autoScrollDampCurveProperty = serializedObject.FindProperty("autoScrollDampCurve");
            _groundLevelOffsetProperty = serializedObject.FindProperty("groundLevelOffset");
            _enableRotationProperty = serializedObject.FindProperty("enableRotation");
            _enableTiltProperty = serializedObject.FindProperty("enableTilt");
            _tiltAngleMinProperty = serializedObject.FindProperty("tiltAngleMin");
            _tiltAngleMaxProperty = serializedObject.FindProperty("tiltAngleMax");
            _enableZoomTiltProperty = serializedObject.FindProperty("enableZoomTilt");
            _zoomTiltAngleMinProperty = serializedObject.FindProperty("zoomTiltAngleMin");
            _zoomTiltAngleMaxProperty = serializedObject.FindProperty("zoomTiltAngleMax");
            _onPickItemProperty = serializedObject.FindProperty("onPickItem");
            _onPickItem2DProperty = serializedObject.FindProperty("onPickItem2D");
            _onPickItemDoubleClickProperty = serializedObject.FindProperty("onPickItemDoubleClick");
            _onPickItem2DDoubleClickProperty = serializedObject.FindProperty("onPickItem2DDoubleClick");
            _keyboardAndMousePlatformsProperty = serializedObject.FindProperty("keyboardAndMousePlatforms");
            _controlTweakablesEnabledProperty = serializedObject.FindProperty("controlTweakablesEnabled");
            _keyboardControlsModifiersProperty = serializedObject.FindProperty("keyboardControlsModifiers");
            _mouseRotationFactorProperty = serializedObject.FindProperty("mouseRotationFactor");
            _mouseTiltFactorProperty = serializedObject.FindProperty("mouseTiltFactor");
            _mouseZoomFactorProperty = serializedObject.FindProperty("mouseZoomFactor");
            _keyboardRotationFactorProperty = serializedObject.FindProperty("keyboardRotationFactor");
            _keyboardTiltFactorProperty = serializedObject.FindProperty("keyboardTiltFactor");
            _keyboardZoomFactorProperty = serializedObject.FindProperty("keyboardZoomFactor");
            _keyboardRepeatFactorProperty = serializedObject.FindProperty("keyboardRepeatFactor");
            _keyboardRepeatDelayProperty = serializedObject.FindProperty("keyboardRepeatDelay");
            _advanceModeProperty = serializedObject.FindProperty("advanceMode");
            _zoomBackSpringFactorProperty = serializedObject.FindProperty("zoomBackSpringFactor");
            _dragBackSpringFactorProperty = serializedObject.FindProperty("dragBackSpringFactor");
            _autoScrollVelocityMaxProperty = serializedObject.FindProperty("autoScrollVelocityMax");
            _dampFactorTimeMultiplierProperty = serializedObject.FindProperty("dampFactorTimeMultiplier");
            _isPinchModeExclusiveProperty = serializedObject.FindProperty("isPinchModeExclusive");
            _customZoomSensitivityProperty = serializedObject.FindProperty("customZoomSensitivity");
            _terrainColliderProperty = serializedObject.FindProperty("terrainCollider");
            _cameraTransformProperty = serializedObject.FindProperty("cameraTransform");
            _is2dOverdragMarginEnabledProperty = serializedObject.FindProperty("is2dOverdragMarginEnabled");
            _camOverdragMargin2dProperty = serializedObject.FindProperty("camOverdragMargin2d");
            _rotationDetectionDeltaThresholdProperty = serializedObject.FindProperty("rotationDetectionDeltaThreshold");
            _rotationMinPinchDistanceProperty = serializedObject.FindProperty("rotationMinPinchDistance");
            _rotationLockThresholdProperty = serializedObject.FindProperty("rotationLockThreshold");
            _pinchModeDetectionMoveTresholdProperty = serializedObject.FindProperty("pinchModeDetectionMoveTreshold");
            _pinchTiltModeThresholdProperty = serializedObject.FindProperty("pinchTiltModeThreshold");
            _pinchTiltSpeedProperty = serializedObject.FindProperty("pinchTiltSpeed");
        }

        public void OnSceneGUI()
        {
            var mobileTouchCamera = (TouchCamera) target;

            if (UnityEngine.Event.current.rawType == EventType.MouseUp) CheckSwapBoundary(mobileTouchCamera);

            var boundaryMin = mobileTouchCamera.BoundaryMin;
            var boundaryMax = mobileTouchCamera.BoundaryMax;

            float offset = mobileTouchCamera.GroundLevelOffset;
            var pBottomLeft = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMin.x, boundaryMin.y), offset);
            var pBottomRight = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMax.x, boundaryMin.y), offset);
            var pTopLeft = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMin.x, boundaryMax.y), offset);
            var pTopRight = mobileTouchCamera.UnprojectVector2(new Vector2(boundaryMax.x, boundaryMax.y), offset);

            Handles.color = new Color(0, .4f, 1f, 1f);
            float handleSize = HandleUtility.GetHandleSize(mobileTouchCamera.Transform.position) * 0.1f;

            #region min/max handles

            pBottomLeft = DrawSphereHandle(pBottomLeft, handleSize);
            pTopRight = DrawSphereHandle(pTopRight, handleSize);
            boundaryMin = mobileTouchCamera.ProjectVector3(pBottomLeft);
            boundaryMax = mobileTouchCamera.ProjectVector3(pTopRight);

            #endregion

            #region min/max handles that need to be remapped

            var pBottomRightNew = DrawSphereHandle(pBottomRight, handleSize);
            var pTopLeftNew = DrawSphereHandle(pTopLeft, handleSize);

            if (Vector3.Distance(pBottomRight, pBottomRightNew) > 0)
            {
                var pBottomRight2d = mobileTouchCamera.ProjectVector3(pBottomRightNew);
                boundaryMin.y = pBottomRight2d.y;
                boundaryMax.x = pBottomRight2d.x;
            }

            if (Vector3.Distance(pTopLeft, pTopLeftNew) > 0)
            {
                var pTopLeftNew2d = mobileTouchCamera.ProjectVector3(pTopLeftNew);
                boundaryMin.x = pTopLeftNew2d.x;
                boundaryMax.y = pTopLeftNew2d.y;
            }

            #endregion

            #region one way handles

            Handles.color = new Color(1, 1, 1, 1);
            handleSize = HandleUtility.GetHandleSize(mobileTouchCamera.Transform.position) * 0.05f;
            boundaryMax.x = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(boundaryMax.x, 0.5f * (boundaryMax.y + boundaryMin.y)), offset).x;
            boundaryMax.y = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(0.5f * (boundaryMax.x + boundaryMin.x), boundaryMax.y), offset).y;
            boundaryMin.x = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(boundaryMin.x, 0.5f * (boundaryMax.y + boundaryMin.y)), offset).x;
            boundaryMin.y = DrawOneWayHandle(mobileTouchCamera, handleSize, new Vector2(0.5f * (boundaryMax.x + boundaryMin.x), boundaryMin.y), offset).y;

            #endregion

            if (Vector2.Distance(mobileTouchCamera.BoundaryMin, boundaryMin) > float.Epsilon ||
                Vector2.Distance(mobileTouchCamera.BoundaryMax, boundaryMax) > float.Epsilon)
            {
                Undo.RecordObject(target, "Mobile Touch Camera Boundary Modification");
                mobileTouchCamera.BoundaryMin = boundaryMin;
                mobileTouchCamera.BoundaryMax = boundaryMax;
                EditorUtility.SetDirty(mobileTouchCamera);
            }
        }

        private Vector3 DrawSphereHandle(Vector3 point, float handleSize) { return Handles.FreeMoveHandle(point, handleSize, Vector3.one, Handles.SphereHandleCap); }

        private Vector3 DrawOneWayHandle(TouchCamera mobileTouchCamera, float handleSize, Vector2 pRelative, float offset)
        {
            var point = mobileTouchCamera.UnprojectVector2(pRelative, offset);
            var pointNew = Handles.FreeMoveHandle(point, handleSize, Vector3.one, Handles.DotHandleCap);
            return mobileTouchCamera.ProjectVector3(pointNew);
        }

        /// <summary>
        /// Method to swap the boundary min/max values in case they aren't right.
        /// </summary>
        private void CheckSwapBoundary(TouchCamera mobileTouchCamera)
        {
            var boundaryMin = mobileTouchCamera.BoundaryMin;
            var boundaryMax = mobileTouchCamera.BoundaryMax;

            //Automatically swap min with max when necessary.
            var autoSwap = false;
            if (boundaryMax.x < boundaryMin.x)
            {
                Undo.RecordObject(target, "Touch Camera Boundary Auto Swap");
                (boundaryMax.x, boundaryMin.x) = (boundaryMin.x, boundaryMax.x);
                autoSwap = true;
            }

            if (boundaryMax.y < boundaryMin.y)
            {
                Undo.RecordObject(target, "Touch Camera Boundary Auto Swap");
                (boundaryMax.y, boundaryMin.y) = (boundaryMin.y, boundaryMax.y);
                autoSwap = true;
            }

            if (autoSwap) EditorUtility.SetDirty(mobileTouchCamera);

            mobileTouchCamera.BoundaryMin = boundaryMin;
            mobileTouchCamera.BoundaryMax = boundaryMax;
        }

        public override void OnInspectorGUI()
        {
            var touchCamera = (TouchCamera) target;
            EditorGUILayout.PropertyField(_cameraProperty, true);
            string camAxesError = touchCamera.CheckCameraAxesErrors();
            if (!string.IsNullOrEmpty(camAxesError)) EditorGUILayout.HelpBox(camAxesError, MessageType.Error);

            EditorGUILayout.PropertyField(_cameraAxesProperty, true);
            GUI.enabled = touchCamera.GetComponent<Camera>().orthographic == false;
            EditorGUILayout.PropertyField(_perspectiveZoomModeProperty, true);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_camZoomMinProperty, true);
            Uniform.DrawPropertyFieldValidate(_camZoomMaxProperty,
                error: _camZoomMaxProperty.floatValue < _camZoomMinProperty.floatValue,
                onError: () => EditorGUILayout.HelpBox("Cam Zoom Max must be bigger than Cam Zoom Min", MessageType.Error));

            Uniform.DrawPropertyFieldValidate(_camOverzoomMarginProperty, error: _camOverzoomMarginProperty.floatValue < 0);
            Uniform.DrawPropertyFieldValidate(_camOverdragMarginProperty, error: _camOverdragMarginProperty.floatValue < 0);


            #region boundary

            var boundaryMin = _boundaryMinProperty.vector2Value;
            var boundaryMax = _boundaryMaxProperty.vector2Value;

            EditorGUILayout.LabelField(new GUIContent("Boundary",
                    "These values define the scrolling borders for the camera. The camera will not scroll further than defined here. The boundary is drawn as yellow rectangular gizmo in the scene-view when the camera is selected."),
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Top", GUILayout.Width(SIZE_LABEL));
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            bool isBoundaryYValid = boundaryMax.y >= boundaryMin.y;
            bool isBoundaryXValid = boundaryMax.x >= boundaryMin.x;

            if (!isBoundaryYValid) GUI.color = Uniform.Rose_400;
            boundaryMax.y = EditorGUILayout.FloatField(boundaryMax.y, GUILayout.Width(70));
            GUI.color = Color.white;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!isBoundaryXValid) GUI.color = Uniform.Rose_400;
            Draw2FloatFields("Left/Right", ref boundaryMin.x, ref boundaryMax.x);
            GUI.color = Color.white;


            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bottom", GUILayout.Width(SIZE_LABEL));
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            if (!isBoundaryYValid) GUI.color = Uniform.Rose_400;
            boundaryMin.y = EditorGUILayout.FloatField(boundaryMin.y, GUILayout.Width(70));
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (isBoundaryYValid == false) EditorGUILayout.HelpBox("The value for Top needs to be bigger than the value for Bottom.", MessageType.Error);
            if (isBoundaryXValid == false) EditorGUILayout.HelpBox("The value for Right needs to be bigger than the value for Left.", MessageType.Error);

            _boundaryMinProperty.vector2Value = boundaryMin;
            _boundaryMaxProperty.vector2Value = boundaryMax;

            #endregion

            EditorGUILayout.PropertyField(_camFollowFactorProperty, true);

            #region auto scroll damp

            var selectedDampMode = (AutoScrollDampMode) _autoScrollDampModeProperty.enumValueIndex;
            if (selectedDampMode == AutoScrollDampMode.Default && !_autoScrollDampProperty.floatValue.Approximately(300))
            {
                _autoScrollDampModeProperty.enumValueIndex = (int) AutoScrollDampMode.Custom;
                selectedDampMode = AutoScrollDampMode.Custom;
            }

            EditorGUILayout.PropertyField(_autoScrollDampModeProperty, true);

            if (selectedDampMode == AutoScrollDampMode.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_autoScrollDampProperty, true);
                EditorGUILayout.PropertyField(_autoScrollDampCurveProperty, true);
                EditorGUI.indentLevel--;
            }

            #endregion

            EditorGUILayout.PropertyField(_groundLevelOffsetProperty, true);
            EditorGUILayout.PropertyField(_enableRotationProperty, true);
            EditorGUILayout.PropertyField(_enableTiltProperty, true);

            const float minTiltErrorAngle = 10;
            const float minTiltWarningAngle = 40;
            const float maxTiltErrorAngle = 90;

            if (_enableTiltProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                Uniform.DrawPropertyFieldValidate(_tiltAngleMinProperty,
                    error: _tiltAngleMinProperty.floatValue < minTiltErrorAngle,
                    warning: _tiltAngleMinProperty.floatValue < minTiltWarningAngle,
                    onError: () =>
                    {
                        EditorGUILayout.HelpBox("The minimum tilt angle must not be lower than " + minTiltErrorAngle +
                                                ". Otherwise the camera computation is guaranteed to become instable.",
                            MessageType.Error);
                    },
                    onWarning: () =>
                    {
                        EditorGUILayout.HelpBox("The minimum tilt angle\nshould not be lower than " + minTiltWarningAngle +
                                                ". Otherwise the camera computations may become instable.",
                            MessageType.Warning);
                    });

                Uniform.DrawPropertyFieldValidate(_tiltAngleMaxProperty,
                    error: _tiltAngleMaxProperty.floatValue > maxTiltErrorAngle,
                    onError: () =>
                    {
                        EditorGUILayout.HelpBox("The maximum tilt angle must not be higher than " + maxTiltErrorAngle +
                                                ". Otherwise the camera computation may become instable.",
                            MessageType.Error);
                    });

                if (_tiltAngleMaxProperty.floatValue < _tiltAngleMinProperty.floatValue)
                    EditorGUILayout.HelpBox("Tilt Angle Max must be bigger than Tilt Angle Min", MessageType.Error);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_enableZoomTiltProperty, true);
            if (_enableZoomTiltProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                Uniform.DrawPropertyFieldValidate(_zoomTiltAngleMinProperty,
                    _zoomTiltAngleMinProperty.floatValue < minTiltErrorAngle,
                    _zoomTiltAngleMinProperty.floatValue < minTiltWarningAngle,
                    onError: () =>
                    {
                        EditorGUILayout.HelpBox("The minimum tilt angle must not be lower than " + minTiltErrorAngle +
                                                ". Otherwise the camera computation is guaranteed to become instable.",
                            MessageType.Error);
                    },
                    onWarning: () =>
                    {
                        EditorGUILayout.HelpBox("The minimum tilt angle\nshould not be lower than " + minTiltWarningAngle +
                                                ". Otherwise the camera computations may become instable.",
                            MessageType.Warning);
                    });
                Uniform.DrawPropertyFieldValidate(_zoomTiltAngleMaxProperty,
                    _zoomTiltAngleMaxProperty.floatValue > maxTiltErrorAngle,
                    onError: () =>
                    {
                        EditorGUILayout.HelpBox("The maximum tilt angle must not be higher than " + maxTiltErrorAngle +
                                                ". Otherwise the camera computation may become instable.",
                            MessageType.Error);
                    });
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(_keyboardAndMousePlatformsProperty, true);
            EditorGUILayout.PropertyField(_controlTweakablesEnabledProperty, true);
            if (_controlTweakablesEnabledProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_keyboardControlsModifiersProperty, true);
                EditorGUILayout.PropertyField(_mouseRotationFactorProperty, true);
                EditorGUILayout.PropertyField(_mouseTiltFactorProperty, true);
                EditorGUILayout.PropertyField(_mouseZoomFactorProperty, true);
                EditorGUILayout.PropertyField(_keyboardRotationFactorProperty, true);
                EditorGUILayout.PropertyField(_keyboardTiltFactorProperty, true);
                EditorGUILayout.PropertyField(_keyboardZoomFactorProperty, true);
                EditorGUILayout.PropertyField(_keyboardRepeatFactorProperty, true);
                EditorGUILayout.PropertyField(_keyboardRepeatDelayProperty, true);
            }

            EditorGUILayout.PropertyField(_advanceModeProperty, true);
            if (_advanceModeProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_zoomBackSpringFactorProperty, true);
                EditorGUILayout.PropertyField(_dragBackSpringFactorProperty, true);
                EditorGUILayout.PropertyField(_autoScrollVelocityMaxProperty, true);
                EditorGUILayout.PropertyField(_dampFactorTimeMultiplierProperty, true);
                EditorGUILayout.PropertyField(_isPinchModeExclusiveProperty, true);
                EditorGUILayout.PropertyField(_customZoomSensitivityProperty, true);
                EditorGUILayout.PropertyField(_terrainColliderProperty, true);
                EditorGUILayout.PropertyField(_cameraTransformProperty, true);

                EditorGUILayout.PropertyField(_rotationDetectionDeltaThresholdProperty, true);
                EditorGUILayout.PropertyField(_rotationMinPinchDistanceProperty, true);
                EditorGUILayout.PropertyField(_rotationLockThresholdProperty, true);

                EditorGUILayout.PropertyField(_pinchModeDetectionMoveTresholdProperty, true);
                EditorGUILayout.PropertyField(_pinchTiltModeThresholdProperty, true);
                EditorGUILayout.PropertyField(_pinchTiltSpeedProperty, true);

                EditorGUILayout.PropertyField(_is2dOverdragMarginEnabledProperty, true);

                if (_is2dOverdragMarginEnabledProperty.boolValue) EditorGUILayout.PropertyField(_camOverdragMargin2dProperty, true);
                EditorGUI.indentLevel--;
            }

            Uniform.DrawGroupFoldout("touch_camera_callback", "Callback", DrawCallback);


            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();

                //Detect modified properties.
                var dampModeAfterApply = (AutoScrollDampMode) serializedObject.FindProperty("autoScrollDampMode").enumValueIndex;
                if (selectedDampMode != dampModeAfterApply) OnScrollDampModeChanged(dampModeAfterApply);
            }
        }

        private void DrawCallback()
        {
            EditorGUILayout.PropertyField(_onPickItemProperty, true);
            EditorGUILayout.PropertyField(_onPickItem2DProperty, true);
            EditorGUILayout.PropertyField(_onPickItemDoubleClickProperty, true);
            EditorGUILayout.PropertyField(_onPickItem2DDoubleClickProperty, true);
        }


        private void OnScrollDampModeChanged(AutoScrollDampMode dampMode)
        {
            var serializedScrollDamp = serializedObject.FindProperty("autoScrollDamp");
            var serializedScrollDampCurve = serializedObject.FindProperty("autoScrollDampCurve");
            switch (dampMode)
            {
                case AutoScrollDampMode.Default:
                    serializedScrollDamp.floatValue = 300;
                    serializedScrollDampCurve.animationCurveValue = new AnimationCurve(new Keyframe(0, 1, 0, 0),
                        new Keyframe(0.7f, 0.9f, -0.5f, -0.5f),
                        new Keyframe(1, 0.01f, -0.85f, -0.85f));
                    break;
                case AutoScrollDampMode.SlowFadeOut:
                    serializedScrollDamp.floatValue = 150;
                    serializedScrollDampCurve.animationCurveValue = new AnimationCurve(new Keyframe(0, 1, -1, -1), new Keyframe(1, 0.01f, -1, -1));
                    break;
            }

            if (dampMode != AutoScrollDampMode.Custom) serializedObject.ApplyModifiedProperties();
        }

        private void Draw2FloatFields(string caption, ref float valueA, ref float valueB)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(caption, GUILayout.Width(SIZE_LABEL));
            GUILayout.FlexibleSpace();
            valueA = EditorGUILayout.FloatField(valueA, GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            valueB = EditorGUILayout.FloatField(valueB, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();
        }
    }
}