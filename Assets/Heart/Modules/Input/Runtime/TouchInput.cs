using UnityEngine;
using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;

namespace Pancake.MobileInput
{
    [EditorIcon("script_input")]
    public class TouchInput : MonoBehaviour
    {
        private const float DRAG_DURATION_THRESHOLD = 0.01f;
        private const int MOMENTUM_SAMPLES_COUNT = 5;

        [SerializeField] private BoolVariable statusTouchOnLockedArea;

        [Group("Event Drag")] public ScriptableInputStartDrag onStartDrag;
        [Group("Event Drag")] public ScriptableInputUpdateDrag onUpdateDrag;
        [Group("Event Drag")] public ScriptableInputStopDrag onStopDrag;
        [Group("Event Finger")] public ScriptableInputFingerDown onFingerDown;
        [Group("Event Finger")] public ScriptableInputFingerUp onFingerUp;
        [Group("Event Finger")] public ScriptableInputClick onClick;
        [Group("Event Finger")] public ScriptableInputLongTapUpdate onLongTapUpdate;
        [Group("Event Pinch")] public ScriptableInputStartPinch onStartPinch;
        [Group("Event Pinch")] public ScriptableInputUpdatePinch onUpdatePinch;
        [Group("Event Pinch")] public ScriptableInputUpdateExtendPinch onUpdateExtendPinch;
        [Group("Event Pinch")] public ScriptableInputStopPinch onStopPinch;

#if UNITY_EDITOR
        [SerializeField] private bool isCustom;
#endif

        [SerializeField, ShowIf("isCustom")]
        [Tooltip("When the finger is held on an item for at least this duration without moving, the gesture is recognized as a long tap.")]
        private FloatReference clickDurationThreshold ; //0.7f

        [SerializeField, ShowIf("isCustom")] [Tooltip("A double click gesture is recognized when the time between two consecutive taps is shorter than this duration.")]
        private FloatReference doubleClickThreshold; //0.5f

        [SerializeField, ShowIf("isCustom")]
        [Tooltip("This value controls how close to a vertical line the user has to perform a tilt gesture for it to be recognized as such.")]
        private FloatReference tiltVerticalThreshold; //0.7f

        [SerializeField, ShowIf("isCustom")]
        [Tooltip(
            "Threshold value for detecting whether the fingers are horizontal enough for starting the tilt. Using this value you can prevent vertical finger placement to be counted as tilt gesture.")]
        private FloatReference tiltHorizontalThreshold; // 0.5f

        [SerializeField, ShowIf("isCustom")]
        [Tooltip(
            "A drag is started as soon as the user moves his finger over a longer distance than this value. The value is defined as normalized value. Dragging the entire width of the screen equals 1. Dragging the entire height of the screen also equals 1.")]
        private FloatReference dragThreshold; // 0.05f

        [SerializeField, ShowIf("isCustom")] [Tooltip("When this flag is enabled the drag started event is invoked immediately when the long tap time is succeeded.")]
        private BoolVariable longTapStartsDrag;

        private float _realTimeOfLastFinderDown;
        private float _realTimeOfLastClick;
        private bool _wasFingerDownLastFrame;
        private Vector3 _lastFinger0DownPosition;
        private bool _isDragging;
        private Vector3 _dragStartPosition;
        private Vector3 _dragPreviousFramePosition;
        private Vector3 _dragStartOffset;
        private List<Vector3> _dragFinalMomentumVector;
        private float _pinchStartDistance;
        private List<Vector3> _pinchStartPositions;
        private List<Vector3> _touchPositionsLastFrame;
        private Vector3 _pinchVectorStart = Vector3.zero;
        private Vector3 _pinchVectorLastFrame = Vector3.zero;
        private float _totalFingerMovement;
        private bool _wasDraggingLastFrame;
        private bool _wasPinchingLastFrame;
        private bool _isPinching;

        private float _timeSinceDragStart;
        private bool _isClickPrevented;
        private bool _isFingerDown;


        public bool LongTapStartsDrag => longTapStartsDrag;

        public bool IsTouchOnLockedArea { get => statusTouchOnLockedArea.Value; set => statusTouchOnLockedArea.Value = value; }

        public void Awake()
        {
            _realTimeOfLastFinderDown = 0;
            _realTimeOfLastClick = 0;
            _lastFinger0DownPosition = Vector3.zero;
            _dragStartPosition = Vector3.zero;
            _dragPreviousFramePosition = Vector3.zero;
            _isDragging = false;
            _wasFingerDownLastFrame = false;
            _dragFinalMomentumVector = new List<Vector3>();
            _pinchStartPositions = new List<Vector3> {Vector3.zero, Vector3.zero};
            _touchPositionsLastFrame = new List<Vector3> {Vector3.zero, Vector3.zero};
            _pinchStartDistance = 1;
            _isPinching = false;
            _isClickPrevented = false;
        }

        public void Update()
        {
            if (TouchWrapper.IsFingerDown == false) statusTouchOnLockedArea.Value = false;

            var pinchToDragCurrentFrame = false;

            if (statusTouchOnLockedArea.Value == false)
            {
                #region pinch

                if (_isPinching == false)
                {
                    if (TouchWrapper.TouchCount == 2)
                    {
                        StartPinch();
                        _isPinching = true;
                    }
                }
                else
                {
                    switch (TouchWrapper.TouchCount)
                    {
                        case < 2:
                            StopPinch();
                            _isPinching = false;
                            break;
                        case 2:
                            UpdatePinch();
                            break;
                    }
                }

                #endregion

                #region drag

                if (_isPinching == false)
                {
                    if (_wasPinchingLastFrame == false)
                    {
                        if (_wasFingerDownLastFrame && TouchWrapper.IsFingerDown)
                        {
                            if (_isDragging == false)
                            {
                                float dragDistance = GetRelativeDragDistance(TouchWrapper.Touch0.Position, _dragStartPosition);
                                float dragTime = Time.realtimeSinceStartup - _realTimeOfLastFinderDown;

                                bool isLongTap = dragTime > clickDurationThreshold;
                                if (onLongTapUpdate != null)
                                {
                                    float longTapProgress = 0;
                                    if (Mathf.Approximately(clickDurationThreshold, 0) == false) longTapProgress = Mathf.Clamp01(dragTime / clickDurationThreshold);
                                    onLongTapUpdate.Raise(longTapProgress);
                                }

                                if ((dragDistance >= dragThreshold.Value && dragTime >= DRAG_DURATION_THRESHOLD) || (longTapStartsDrag && isLongTap))
                                {
                                    _isDragging = true;
                                    _dragStartOffset = _lastFinger0DownPosition - _dragStartPosition;
                                    _dragStartPosition = _lastFinger0DownPosition;
                                    _dragPreviousFramePosition = _lastFinger0DownPosition;
                                    DragStart(_dragStartPosition, isLongTap);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (TouchWrapper.IsFingerDown)
                        {
                            _isDragging = true;
                            _dragStartPosition = TouchWrapper.Touch0.Position;
                            _dragPreviousFramePosition = TouchWrapper.Touch0.Position;
                            DragStart(_dragStartPosition, false);
                            pinchToDragCurrentFrame = true;
                        }
                    }

                    if (_isDragging && TouchWrapper.IsFingerDown)
                    {
                        DragUpdate(TouchWrapper.Touch0.Position, TouchWrapper.Touch0.Position - _dragPreviousFramePosition);
                        _dragPreviousFramePosition = TouchWrapper.Touch0.Position;
                    }

                    if (_isDragging && TouchWrapper.IsFingerDown == false)
                    {
                        _isDragging = false;
                        DragStop(_lastFinger0DownPosition);
                    }
                }

                #endregion

                #region click

                if (_isPinching == false && _isDragging == false && _wasPinchingLastFrame == false && _wasDraggingLastFrame == false && _isClickPrevented == false)
                {
                    if (_wasFingerDownLastFrame == false && TouchWrapper.IsFingerDown)
                    {
                        _realTimeOfLastFinderDown = Time.realtimeSinceStartup;
                        _dragStartPosition = TouchWrapper.Touch0.Position;
                        _dragPreviousFramePosition = TouchWrapper.Touch0.Position;
                        FingerDown(TouchWrapper.AverageTouchPos);
                    }

                    if (_wasFingerDownLastFrame && TouchWrapper.IsFingerDown == false)
                    {
                        float fingerDownUpDuration = Time.realtimeSinceStartup - _realTimeOfLastFinderDown;

                        if (_wasDraggingLastFrame == false && _wasPinchingLastFrame == false)
                        {
                            float clickDuration = Time.realtimeSinceStartup - _realTimeOfLastClick;
                            bool isDoubleClick = clickDuration < doubleClickThreshold;
                            bool isLongTap = fingerDownUpDuration > clickDurationThreshold;
                            if (onClick != null) onClick.Raise(_lastFinger0DownPosition, isDoubleClick, isLongTap);
                            _realTimeOfLastClick = Time.realtimeSinceStartup;
                        }
                    }
                }

                #endregion
            }

            if (_isDragging && TouchWrapper.IsFingerDown && pinchToDragCurrentFrame == false)
            {
                _dragFinalMomentumVector.Add(TouchWrapper.Touch0.Position - _lastFinger0DownPosition);
                if (_dragFinalMomentumVector.Count > MOMENTUM_SAMPLES_COUNT) _dragFinalMomentumVector.RemoveAt(0);
            }

            if (statusTouchOnLockedArea.Value == false) _wasFingerDownLastFrame = TouchWrapper.IsFingerDown;

            if (_wasFingerDownLastFrame) _lastFinger0DownPosition = TouchWrapper.Touch0.Position;

            _wasDraggingLastFrame = _isDragging;
            _wasPinchingLastFrame = _isPinching;

            if (TouchWrapper.TouchCount == 0)
            {
                _isClickPrevented = false;
                if (_isFingerDown) FingerUp();
            }
        }

        private void StartPinch()
        {
            _pinchStartPositions[0] = _touchPositionsLastFrame[0] = TouchWrapper.Touches[0].Position;
            _pinchStartPositions[1] = _touchPositionsLastFrame[1] = TouchWrapper.Touches[1].Position;

            _pinchStartDistance = GetPinchDistance(_pinchStartPositions[0], _pinchStartPositions[1]);
            if (onStartPinch != null) onStartPinch.Raise((_pinchStartPositions[0] + _pinchStartPositions[1]) * 0.5f, _pinchStartDistance);

            _isClickPrevented = true;
            _pinchVectorStart = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            _pinchVectorLastFrame = _pinchVectorStart;
            _totalFingerMovement = 0;
        }

        private void UpdatePinch()
        {
            float pinchDistance = GetPinchDistance(TouchWrapper.Touches[0].Position, TouchWrapper.Touches[1].Position);
            var pinchVector = TouchWrapper.Touches[1].Position - TouchWrapper.Touches[0].Position;
            float pinchAngleSign = Vector3.Cross(_pinchVectorLastFrame, pinchVector).z < 0 ? -1 : 1;
            float pinchAngleDelta = 0;
            if (Mathf.Approximately(Vector3.Distance(_pinchVectorLastFrame, pinchVector), 0) == false)
            {
                pinchAngleDelta = Vector3.Angle(_pinchVectorLastFrame, pinchVector) * pinchAngleSign;
            }

            float pinchVectorDeltaMag = Mathf.Abs(_pinchVectorLastFrame.magnitude - pinchVector.magnitude);
            float pinchAngleDeltaNormalized = 0;
            if (Mathf.Approximately(pinchVectorDeltaMag, 0) == false) pinchAngleDeltaNormalized = pinchAngleDelta / pinchVectorDeltaMag;

            var pinchCenter = (TouchWrapper.Touches[0].Position + TouchWrapper.Touches[1].Position) * 0.5f;

            #region tilting gesture

            float pinchTiltDelta = 0;
            var touch0DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[0].Position - _touchPositionsLastFrame[0]);
            var touch1DeltaRelative = GetTouchPositionRelative(TouchWrapper.Touches[1].Position - _touchPositionsLastFrame[1]);
            float touch0DotUp = Vector2.Dot(touch0DeltaRelative.normalized, Vector2.up);
            float touch1DotUp = Vector2.Dot(touch1DeltaRelative.normalized, Vector2.up);
            float pinchVectorDotHorizontal = Vector3.Dot(pinchVector.normalized, Vector3.right);
            if (Mathf.Sign(touch0DotUp).Approximately(Mathf.Sign(touch1DotUp)))
            {
                if (Mathf.Abs(touch0DotUp) > tiltVerticalThreshold && Mathf.Abs(touch1DotUp) > tiltVerticalThreshold)
                {
                    if (Mathf.Abs(pinchVectorDotHorizontal) >= tiltHorizontalThreshold) pinchTiltDelta = 0.5f * (touch0DeltaRelative.y + touch1DeltaRelative.y);
                }
            }

            _totalFingerMovement += touch0DeltaRelative.magnitude + touch1DeltaRelative.magnitude;

            #endregion

            if (onUpdatePinch != null) onUpdatePinch.Raise(pinchCenter, pinchDistance, _pinchStartDistance);

            if (onUpdateExtendPinch != null)
            {
                onUpdateExtendPinch.Raise(new PinchData
                {
                    pinchCenter = pinchCenter,
                    pinchDistance = pinchDistance,
                    pinchStartDistance = _pinchStartDistance,
                    pinchAngleDelta = pinchAngleDelta,
                    pinchAngleDeltaNormalized = pinchAngleDeltaNormalized,
                    pinchTiltDelta = pinchTiltDelta,
                    pinchTotalFingerMovement = _totalFingerMovement
                });
            }

            _pinchVectorLastFrame = pinchVector;
            _touchPositionsLastFrame[0] = TouchWrapper.Touches[0].Position;
            _touchPositionsLastFrame[1] = TouchWrapper.Touches[1].Position;
        }

        private float GetPinchDistance(Vector3 pos0, Vector3 pos1)
        {
            float distanceX = Mathf.Abs(pos0.x - pos1.x) / Screen.width;
            float distanceY = Mathf.Abs(pos0.y - pos1.y) / Screen.height;
            return Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
        }

        private void StopPinch()
        {
            _dragStartOffset = Vector3.zero;
            if (onStopPinch != null) onStopPinch.Raise();
        }

        private void DragStart(Vector3 position, bool isLongTap)
        {
            if (onStartDrag != null) onStartDrag.Raise(position, isLongTap);

            _isClickPrevented = true;
            _timeSinceDragStart = 0;
            _dragFinalMomentumVector.Clear();
        }

        private void DragUpdate(Vector3 currentPosition, Vector3 delta)
        {
            if (onUpdateDrag != null)
            {
                _timeSinceDragStart += Time.unscaledDeltaTime;
                var offset = Vector3.Lerp(Vector3.zero, _dragStartOffset, Mathf.Clamp01(_timeSinceDragStart * 10.0f));
                onUpdateDrag.Raise(_dragStartPosition, currentPosition, offset, delta);
            }
        }

        private void DragStop(Vector3 stopPosition)
        {
            if (onStopDrag != null)
            {
                var momentum = Vector3.zero;
                if (_dragFinalMomentumVector.Count > 0)
                {
                    for (var i = 0; i < _dragFinalMomentumVector.Count; ++i) momentum += _dragFinalMomentumVector[i];

                    momentum /= _dragFinalMomentumVector.Count;
                }

                onStopDrag.Raise(stopPosition, momentum);
            }

            _dragFinalMomentumVector.Clear();
        }

        private void FingerDown(Vector3 position)
        {
            _isFingerDown = true;
            if (onFingerDown != null) onFingerDown.Raise(position);
        }

        private void FingerUp()
        {
            _isFingerDown = false;
            if (onFingerUp != null) onFingerUp.Raise();
        }

        private Vector3 GetTouchPositionRelative(Vector3 touchPosScreen)
        {
            return new Vector3(touchPosScreen.x / Screen.width, touchPosScreen.y / Screen.height, touchPosScreen.z);
        }

        private float GetRelativeDragDistance(Vector3 pos0, Vector3 pos1)
        {
            Vector2 dragVector = pos0 - pos1;
            float dragDistance = new Vector2(dragVector.x / Screen.width, dragVector.y / Screen.height).magnitude;
            return dragDistance;
        }
    }
}