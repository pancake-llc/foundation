using UnityEngine;

namespace Pancake.MobileInput
{
    /// <summary>
    /// A little helper script that allows to focus the camera on a transform either
    /// via code, or by wiring it up with one of the many events of the touch camera
    /// or picking controller.
    /// </summary>
    [RequireComponent(typeof(TouchCamera))]
    public class FocusCameraOnItem : MonoBehaviour
    {
        [SerializeField] private float transitionDuration = 0.5f;

        [SerializeField] private AnimationCurve transitionCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));

        private TouchCamera TouchCamera { get; set; }

        private Vector3 _posTransitionStart;
        private Vector3 _posTransitionEnd;
        private Quaternion _rotTransitionStart;
        private Quaternion _rotTransitionEnd;
        private float _zoomTransitionStart;
        private float _zoomTransitionEnd;
        private float _timeTransitionStart;
        private bool _isTransitionStarted;

        public float TransitionDuration { get => transitionDuration; set => transitionDuration = value; }

        public void Awake()
        {
            TouchCamera = GetComponent<TouchCamera>();
            _isTransitionStarted = false;
        }

        public void LateUpdate()
        {
            if (TouchCamera.IsDragging || TouchCamera.IsPinching) _timeTransitionStart = Time.time - transitionDuration;

            if (_isTransitionStarted)
            {
                if (Time.time < _timeTransitionStart + transitionDuration) UpdateTransform();
                else
                {
                    SetTransform(_posTransitionEnd, _rotTransitionEnd, _zoomTransitionEnd);
                    _isTransitionStarted = false;
                }
            }
        }

        private void UpdateTransform()
        {
            float progress = (Time.time - _timeTransitionStart) / transitionDuration;
            float t = transitionCurve.Evaluate(progress);
            var positionNew = Vector3.Lerp(_posTransitionStart, _posTransitionEnd, t);
            var rotationNew = Quaternion.Lerp(_rotTransitionStart, _rotTransitionEnd, t);
            float zoomNew = Mathf.Lerp(_zoomTransitionStart, _zoomTransitionEnd, t);
            SetTransform(positionNew, rotationNew, zoomNew);
        }

        public void OnPickItem(RaycastHit hitInfo) { FocusCameraOnTransform(hitInfo.transform); }

        public void OnPickItem2D(RaycastHit2D hitInfo2D) { FocusCameraOnTransform(hitInfo2D.transform); }

        public void OnPickableTransformSelected(Transform pickableTransform) { FocusCameraOnTransform(pickableTransform); }

        public void FocusCameraOnTransform(Transform targetTransform)
        {
            if (targetTransform == null) return;

            FocusCameraOnTarget(targetTransform.position);
        }

        public void FocusCameraOnTransform(Vector3 targetPosition) { FocusCameraOnTarget(targetPosition); }

        public void FocusCameraOnTarget(Vector3 targetPosition) { FocusCameraOnTarget(targetPosition, transform.rotation, TouchCamera.CamZoom); }

        private float GetTiltFromRotation(Quaternion camRotation)
        {
            var camForwardDir = camRotation * Vector3.forward;
            var camUp = TouchCamera.CameraAxes == CameraPlaneAxes.XZTopDown ? Vector3.up : Vector3.back;
            var camRightEnd = Vector3.Cross(camUp, camForwardDir);
            var camForwardOnPlaneEnd = Vector3.Cross(camRightEnd, camUp);
            float camTilt = Vector3.Angle(camForwardOnPlaneEnd, camForwardDir);
            return camTilt;
        }

        public void FocusCameraOnTarget(Vector3 targetPosition, Quaternion targetRotation, float targetZoom)
        {
            _timeTransitionStart = Time.time;
            _posTransitionStart = transform.position;
            _rotTransitionStart = transform.rotation;
            _zoomTransitionStart = TouchCamera.CamZoom;
            _rotTransitionEnd = targetRotation;
            _zoomTransitionEnd = targetZoom;

            TouchCamera.Transform.rotation = targetRotation;
            TouchCamera.CamZoom = targetZoom;
            var intersectionScreenCenterEnd =
                TouchCamera.GetIntersectionPoint(TouchCamera.Cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
            var focusVector = targetPosition - intersectionScreenCenterEnd;
            _posTransitionEnd = TouchCamera.GetClampToBoundaries(_posTransitionStart + focusVector, true);
            TouchCamera.Transform.rotation = _rotTransitionStart;
            TouchCamera.CamZoom = _zoomTransitionStart;

            if (Mathf.Approximately(transitionDuration, 0))
            {
                SetTransform(_posTransitionEnd, targetRotation, targetZoom);
                return;
            }

            _isTransitionStarted = true;
        }

        private void SetTransform(Vector3 newPosition, Quaternion newRotation, float newZoom)
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
            TouchCamera.CamZoom = newZoom;
        }
    }
}