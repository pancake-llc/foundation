using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Pancake.Common;

namespace Pancake.MobileInput
{
    [RequireComponent(typeof(Camera))]
    [EditorIcon("icon_input")]
    public class TouchCamera : CacheGameComponent<TouchCamera>
    {
        #region Field

        [SerializeField] private Camera cam;

        [SerializeField]
        [Tooltip(
            "You need to define whether your camera is a side-view camera (which is the default when using the 2D mode of unity) or if you chose a top-down looking camera. This parameter tells the system whether to scroll in XY direction, or in XZ direction.")]
        private CameraPlaneAxes cameraAxes = CameraPlaneAxes.XY2DSideScroll;

        [SerializeField]
        [Tooltip("When using a perspective camera, the zoom can either be performed by changing the field of view, or by moving the camera closer to the scene.")]
        private PerspectiveZoomMode perspectiveZoomMode = PerspectiveZoomMode.FieldOfView;

        [SerializeField]
        [Tooltip(
            "For perspective cameras this value denotes the min field of view used for zooming (field of view zoom), or the min distance to the ground (translation zoom). For orthographic cameras it denotes the min camera size.")]
        private float camZoomMin = 4;

        [SerializeField]
        [Tooltip(
            "For perspective cameras this value denotes the max field of view used for zooming (field of view zoom), or the max distance to the ground (translation zoom). For orthographic cameras it denotes the max camera size.")]
        private float camZoomMax = 12;

        [SerializeField] [Tooltip("The cam will overzoom the min/max values by this amount and spring back when the user releases the zoom.")]
        private float camOverzoomMargin = 1;

        [SerializeField]
        [Tooltip(
            "When dragging the camera close to the defined border, it will spring back when the user stops dragging. This value defines the distance from the border where the camera will spring back to.")]
        private float camOverdragMargin = 5.0f;

        [SerializeField]
        [Tooltip(
            "These values define the scrolling borders for the camera. The camera will not scroll further than defined here. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 boundaryMin = new(-1000, -1000);

        [SerializeField]
        [Tooltip(
            "These values define the scrolling borders for the camera. The camera will not scroll further than defined here. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 boundaryMax = new(1000, 1000);

        [Header("Advanced")]
        [SerializeField]
        [Tooltip(
            "The lower the value, the slower the camera will follow. The higher the value, the more direct the camera will follow movement updates. Necessary for keeping the camera smooth when the framerate is not in sync with the touch input update rate.")]
        private float camFollowFactor = 15.0f;

        [SerializeField] [Tooltip("Set the behaviour of the damping (e.g. the slow-down) at the end of auto-scrolling.")]
#pragma warning disable 414 //NOTE: This field is actually used by the custom inspector. The pragma disables the warning that appears because the variable isn't accessed directly, but only through reflection.
        private AutoScrollDampMode autoScrollDampMode = AutoScrollDampMode.Default;
#pragma warning restore 414
        [SerializeField]
        [Tooltip(
            "When dragging quickly, the camera will keep autoscrolling in the last direction. The autoscrolling will slowly come to a halt. This value defines how fast the camera will come to a halt.")]
        private float autoScrollDamp = 300;

        [SerializeField] [Tooltip("This curve allows to modulate the auto scroll damp value over time.")]
        private AnimationCurve autoScrollDampCurve = new(new Keyframe(0, 1, 0, 0), new Keyframe(0.7f, 0.9f, -0.5f, -0.5f), new Keyframe(1, 0.01f, -0.85f, -0.85f));

        [SerializeField]
        [Tooltip(
            "The camera assumes that the scrollable content of your scene (e.g. the ground of your game-world) is located at y = 0 for top-down cameras or at z = 0 for side-scrolling cameras. In case this is not valid for your scene, you may adjust this property to the correct offset.")]
        private float groundLevelOffset;

        [SerializeField] [Tooltip("When enabled, the camera can be rotated using a 2-finger rotation gesture.")]
        private bool enableRotation;

        [SerializeField] [Tooltip("When enabled, the camera can be tilted using a synced 2-finger up or down motion.")]
        private bool enableTilt;

        [SerializeField] [Tooltip("The minimum tilt angle for the camera.")]
        private float tiltAngleMin = 45;

        [SerializeField] [Tooltip("The maximum tilt angle for the camera.")]
        private float tiltAngleMax = 90;

        [SerializeField] [Tooltip("When enabled, the camera is tilted automatically when zooming.")]
        private bool enableZoomTilt;

        [SerializeField] [Tooltip("The minimum tilt angle for the camera when using zoom tilt.")]
        private float zoomTiltAngleMin = 45;

        [SerializeField] [Tooltip("The maximum tilt angle for the camera when using zoom tilt.")]
        private float zoomTiltAngleMax = 90;

        [Header("Event Callbacks")]
#pragma warning disable 649
        [SerializeField]
        [Tooltip("Here you can set up callbacks to be invoked when an item with Collider is tapped on.")]
        private RaycastHitUnityEvent onPickItem;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when an item with Collider2D is tapped on.")]
        private RaycastHit2DUnityEvent onPickItem2D;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when an item with Collider is double-tapped on.")]
        private RaycastHitUnityEvent onPickItemDoubleClick;

        [SerializeField] [Tooltip("Here you can set up callbacks to be invoked when an item with Collider2D is double-tapped on.")]
        private RaycastHit2DUnityEvent onPickItem2DDoubleClick;
#pragma warning restore 649

        [Header("Keyboard & Mouse Input")]
        [Tooltip("Here you can define all platforms that should support keyboard and mouse input for controlling the camera.")]
        [SerializeField]
        private List<RuntimePlatform> keyboardAndMousePlatforms = new()
        {
            RuntimePlatform.WindowsEditor,
            RuntimePlatform.WindowsPlayer,
            RuntimePlatform.OSXEditor,
            RuntimePlatform.OSXPlayer,
            RuntimePlatform.LinuxEditor,
            RuntimePlatform.LinuxPlayer,
            RuntimePlatform.WebGLPlayer
        };

        [SerializeField] private bool controlTweakablesEnabled;

        [SerializeField] [Tooltip("When any of these modifiers is pressed the arrow keys can be used for rotating the camera.")]
        private List<KeyCode> keyboardControlsModifiers = new() {KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl};

        [SerializeField] private float mouseRotationFactor = 0.01f;
        [SerializeField] private float mouseTiltFactor = 0.005f;
        [SerializeField] private float mouseZoomFactor = 1;
        [SerializeField] private float keyboardRotationFactor = 1;
        [SerializeField] private float keyboardTiltFactor = 1;
        [SerializeField] private float keyboardZoomFactor = 1;
        [SerializeField] private float keyboardRepeatFactor = 60;

        [SerializeField]
        [Tooltip(
            "When holding down a key for rotate/tilt/zoom there's a small delay between the first press action and the repeated input action. This value allows to tweak that delay.")]
        private float keyboardRepeatDelay = 0.5f;

        #endregion

        public CameraPlaneAxes CameraAxes { get => cameraAxes; set => cameraAxes = value; }
        public Camera Cam => cam;

        private Vector3 _dragStartCamPos;
        private Vector3 _cameraScrollVelocity;

        private float _pinchStartCamZoomSize;
        private Vector3 _pinchStartIntersectionCenter;
        private Vector3 _pinchCenterCurrent;
        private float _pinchDistanceCurrent;
        private float _pinchAngleCurrent;
        private float _pinchDistanceStart;
        private Vector3 _pinchCenterCurrentLerp;
        private float _pinchDistanceCurrentLerp;
        private float _pinchAngleCurrentLerp;
        private bool _isRotationLock = true;
        private bool _isRotationActivated;
        private float _pinchAngleLastFrame;
        private float _pinchTiltCurrent;
        private float _pinchTiltAccumulated;
        private bool _isTiltModeEvaluated;
        private float _pinchTiltLastFrame;
        private bool _isPinchTiltMode;
        private Vector3 _camVelocity = Vector3.zero;
        private Vector3 _posLastFrame = Vector3.zero;

        private float _timeRealDragStop;

        public bool IsAutoScrolling => _cameraScrollVelocity.sqrMagnitude > float.Epsilon;

        public bool IsPinching { get; private set; }
        public bool IsDragging { get; private set; }

        #region expert mode tweakables

        [Space] [SerializeField] private bool advanceMode;

        [SerializeField]
        [Tooltip(
            "Depending on your settings the camera allows to zoom slightly over the defined value. When releasing the zoom the camera will spring back to the defined value. This variable defines the speed of the spring back.")]
        private float zoomBackSpringFactor = 20;

        [SerializeField]
        [Tooltip("When close to the border the camera will spring back if the margin is bigger than 0. This variable defines the speed of the spring back.")]
        private float dragBackSpringFactor = 10;

        [SerializeField]
        [Tooltip(
            "When swiping over the screen the camera will keep scrolling a while before coming to a halt. This variable limits the maximum velocity of the auto scroll.")]
        private float autoScrollVelocityMax = 60;

        [SerializeField] [Tooltip("This value defines how quickly the camera comes to a halt when auto scrolling.")]
        private float dampFactorTimeMultiplier = 2;

        [SerializeField]
        [Tooltip(
            "When setting this flag to true, the camera will behave like a popular tower defense game. It will either go into an exclusive tilt mode, or into a combined zoom/rotate mode. When set to false, the camera will behave like a popular city building game. The camera won't pan with 2 fingers, and instead zoom, rotate and tilt are done in parallel.")]
        private bool isPinchModeExclusive = true;

        [SerializeField]
        [Tooltip(
            "This value should be kept at 1 for pixel perfect zoom. In case you need a non-pixel perfect, slower or faster zoom however, you can change this value. 0.5f for example will make the camera zoom half as fast as in pixel perfect mode. This value is currently tested only in perspective camera mode with translation based zoom.")]
        private float customZoomSensitivity = 1.0f;

        [SerializeField] [Tooltip("Optional. When assigned, the terrain collider will be used to align items on the ground following the terrain.")]
        private TerrainCollider terrainCollider;

        [SerializeField] [Tooltip("Optional. When assigned, the given transform will be moved and rotated instead of the one where this component is located on.")]
        private Transform cameraTransform;

        [SerializeField] [Tooltip("When setting this flag to true, the uniform camOverdragMargin will be overrided with the values set in camOverdragMargin2d.")]
        private bool is2dOverdragMarginEnabled;

        [SerializeField] [Tooltip("Using this field allows to set the horizontal overdrag to a different value than the vertical one.")]
        private Vector2 camOverdragMargin2d = Vector2.one * 5.0f;

        #endregion

        #region cam rotation tweakables

        [SerializeField]
        [Tooltip("A gesture may be interpreted as intended rotation in case the relative rotation angle between 2 frames becomes bigger than this value.")]
        private float rotationDetectionDeltaThreshold = 0.25f;

        [SerializeField]
        [Tooltip(
            "Relative pinch distance must be bigger than this value in order to detect a rotation. This is to prevent errors that occur when the fingers are too close together to properly detect a clean rotation.")]
        private float rotationMinPinchDistance = 0.125f;

        [SerializeField]
        [Tooltip(
            "The rotation mode is enabled as soon as the rotation by the user becomes bigger than this value (in degrees). The value is used to prevent micro rotations from regular jittering of the fingers to be interpreted as rotation and helps keeping the camera more steady and less jittery.")]
        private float rotationLockThreshold = 2.5f;

        #endregion

        #region tilt tweakables

        [SerializeField]
        [Tooltip("After this amount of finger-movement (relative to screen size), the pinch mode is decided. E.g. whether tilt mode or regular mode is used.")]
        private float pinchModeDetectionMoveTreshold = 0.025f;

        [SerializeField] [Tooltip("A threshold used to detect the up or down tilting motion.")]
        private float pinchTiltModeThreshold = 0.0075f;

        [SerializeField] [Tooltip("The tilt sensitivity once the tilt mode has started.")]
        private float pinchTiltSpeed = 180;

        #endregion

        private bool _isStarted;

        private bool IsTranslationZoom => cam.orthographic == false && perspectiveZoomMode == PerspectiveZoomMode.Translation;

        public float CamZoom
        {
            get
            {
                if (cam.orthographic) return cam.orthographicSize;

                if (IsTranslationZoom)
                {
                    var camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                    return Vector3.Distance(camCenterIntersection, CachedTransform.position);
                }

                return cam.fieldOfView;
            }
            set
            {
                if (cam.orthographic) cam.orthographicSize = value;
                else
                {
                    if (IsTranslationZoom)
                    {
                        var camCenterIntersection = GetIntersectionPoint(GetCamCenterRay());
                        CachedTransform.position = camCenterIntersection - CachedTransform.forward * value;
                    }
                    else cam.fieldOfView = value;
                }

                ComputeCamBoundaries();
            }
        }

        public float CamZoomMin { get => camZoomMin; set => camZoomMin = value; }
        public float CamZoomMax { get => camZoomMax; set => camZoomMax = value; }
        public float CamOverzoomMargin { get => camOverzoomMargin; set => camOverzoomMargin = value; }
        public float CamFollowFactor { get => camFollowFactor; set => camFollowFactor = value; }
        public float AutoScrollDamp { get => autoScrollDamp; set => autoScrollDamp = value; }
        public AnimationCurve AutoScrollDampCurve { get => autoScrollDampCurve; set => autoScrollDampCurve = value; }

        public float GroundLevelOffset
        {
            get => groundLevelOffset;
            set
            {
                groundLevelOffset = value;
                ComputeCamBoundaries();
            }
        }

        public Vector2 BoundaryMin { get => boundaryMin; set => boundaryMin = value; }
        public Vector2 BoundaryMax { get => boundaryMax; set => boundaryMax = value; }
        public PerspectiveZoomMode PerspectiveZoomMode { get => perspectiveZoomMode; set => perspectiveZoomMode = value; }
        public bool EnableRotation { get => enableRotation; set => enableRotation = value; }
        public bool EnableTilt { get => enableTilt; set => enableTilt = value; }
        public float TiltAngleMin { get => tiltAngleMin; set => tiltAngleMin = value; }
        public float TiltAngleMax { get => tiltAngleMax; set => tiltAngleMax = value; }
        public bool EnableZoomTilt { get => enableZoomTilt; set => enableZoomTilt = value; }
        public float ZoomTiltAngleMin { get => zoomTiltAngleMin; set => zoomTiltAngleMin = value; }
        public float ZoomTiltAngleMax { get => zoomTiltAngleMax; set => zoomTiltAngleMax = value; }
        public List<KeyCode> KeyboardControlsModifiers { get => keyboardControlsModifiers; set => keyboardControlsModifiers = value; }
        public List<RuntimePlatform> KeyboardAndMousePlatforms { get => keyboardAndMousePlatforms; set => keyboardAndMousePlatforms = value; }
        public float MouseRotationFactor { get => mouseRotationFactor; set => mouseRotationFactor = value; }
        public float MouseTiltFactor { get => mouseTiltFactor; set => mouseTiltFactor = value; }
        public float MouseZoomFactor { get => mouseZoomFactor; set => mouseZoomFactor = value; }
        public float KeyboardRotationFactor { get => keyboardRotationFactor; set => keyboardRotationFactor = value; }
        public float KeyboardTiltFactor { get => keyboardTiltFactor; set => keyboardTiltFactor = value; }
        public float KeyboardZoomFactor { get => keyboardZoomFactor; set => keyboardZoomFactor = value; }
        public float KeyboardRepeatFactor { get => keyboardRepeatFactor; set => keyboardRepeatFactor = value; }
        public float KeyboardRepeatDelay { get => keyboardRepeatDelay; set => keyboardRepeatDelay = value; }


        public Vector2 CamOverdragMargin2d
        {
            get
            {
                if (is2dOverdragMarginEnabled) return camOverdragMargin2d;

                return Vector2.one * camOverdragMargin;
            }
            set
            {
                camOverdragMargin2d = value;
                camOverdragMargin = value.x;
            }
        }

        private bool _isDraggingSceneObject;

        private Plane _refPlaneXY = new(new Vector3(0, 0, -1), 0);
        private Plane _refPlaneXZ = new(new Vector3(0, 1, 0), 0);

        public Plane RefPlane
        {
            get
            {
                if (CameraAxes == CameraPlaneAxes.XZTopDown) return _refPlaneXZ;

                return _refPlaneXY;
            }
        }

        private List<Vector3> DragCameraMoveVector { get; set; }
        private const int MOMENTUM_SAMPLES_COUNT = 5;

        private const float PINCH_DISTANCE_FOR_TILT_BREAKOUT = 0.05f;
        private const float PINCH_ACCUM_BREAKOUT = 0.025f;

        private Vector3 _targetPositionClamped = Vector3.zero;

        public bool IsSmoothingEnabled { get; set; }

        private Vector2 CamPosMin { get; set; }
        private Vector2 CamPosMax { get; set; }

        public TerrainCollider TerrainCollider { get => terrainCollider; set => terrainCollider = value; }

        public Vector2 CameraScrollVelocity { get => _cameraScrollVelocity; set => _cameraScrollVelocity = value; }

        private bool _showHorizonError = true;

        #region work in progress //Features that are currently being worked on, but not fully polished and documented yet. Use them at your own risk.

        private bool _enableOvertiltSpring = false; //Allows to enable the camera to spring being when being tilted over the limits.
        private float _camOvertiltMargin = 5.0f;
        private float _tiltBackSpringFactor = 30;

        //This value is necessary to reposition the camera and do boundary update computations while the auto spring back from overtilt is active and larger than this value.
        private float _minOvertiltSpringPositionThreshold = 0.1f;

        private bool _useUntransformedCamBoundary = false;
        private float _maxHorizonFallbackDistance = 10000; //This value may need to be tweaked when using the camera in a low-tilt, see above the horizon scenario.
        private bool _useOldScrollDamp = false;

        #endregion

        #region Keyboard & Mouse Input

        private Vector3 _mousePosLastFrame = Vector3.zero;
        private Vector3 _mouseRotationCenter = Vector3.zero;
        private float _timeKeyDown;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            IsSmoothingEnabled = true;
            _dragStartCamPos = Vector3.zero;
            _cameraScrollVelocity = Vector3.zero;
            _timeRealDragStop = 0;
            _pinchStartCamZoomSize = 0;
            IsPinching = false;
            IsDragging = false;
            DragCameraMoveVector = new List<Vector3>();
            _refPlaneXY = new Plane(new Vector3(0, 0, -1), groundLevelOffset);
            _refPlaneXZ = new Plane(new Vector3(0, 1, 0), -groundLevelOffset);
            if (EnableZoomTilt) ResetZoomTilt();

            ComputeCamBoundaries();

            if (CamZoomMax < CamZoomMin)
            {
                Debug.LogWarning($"The defined max camera zoom ({CamZoomMax}) is smaller than the defined min ({CamZoomMin}). Automatically switching the values.");
                (CamZoomMin, CamZoomMax) = (CamZoomMax, CamZoomMin);
            }

            //Errors for certain incorrect settings.
            string cameraAxesError = CheckCameraAxesErrors();
            if (string.IsNullOrEmpty(cameraAxesError) == false) Debug.LogError(cameraAxesError);
        }

        public void Start()
        {
            TouchInput.OnClick += OnClick;
            TouchInput.OnStartDrag += InputOnDragStart;
            TouchInput.OnUpdateDrag += InputOnDragUpdate;
            TouchInput.OnStopDrag += InputOnDragStop;
            TouchInput.OnFingerDown += InputOnFingerDown;
            TouchInput.OnFingerUp += InputOnFingerUp;
            TouchInput.OnStartPinch += InputOnPinchStart;
            TouchInput.OnUpdateExtendPinch += InputOnPinchUpdate;
            TouchInput.OnStopPinch += InputOnPinchStop;
            _isStarted = true;
            StartCoroutine(InitCamBoundariesDelayed());
        }

        public void OnDestroy()
        {
            if (_isStarted)
            {
                TouchInput.OnClick -= OnClick;
                TouchInput.OnStartDrag -= InputOnDragStart;
                TouchInput.OnUpdateDrag -= InputOnDragUpdate;
                TouchInput.OnStopDrag -= InputOnDragStop;
                TouchInput.OnFingerDown -= InputOnFingerDown;
                TouchInput.OnFingerUp -= InputOnFingerUp;
                TouchInput.OnStartPinch -= InputOnPinchStart;
                TouchInput.OnUpdateExtendPinch -= InputOnPinchUpdate;
                TouchInput.OnStopPinch -= InputOnPinchStop;
            }
        }

        #region public interface

        /// <summary>
        /// Method for resetting the camera boundaries. This method may need to be invoked
        /// when resetting the camera transform (rotation, tilt) by code for example.
        /// </summary>
        public void ResetCameraBoundaries() { ComputeCamBoundaries(); }

        /// <summary>
        /// This method tilts the camera based on the values
        /// defined for the zoom tilt mode.
        /// </summary>
        public void ResetZoomTilt() { UpdateTiltForAutoTilt(CamZoom); }

        /// <summary>
        /// Helper method for retrieving the world position of the
        /// finger with id 0. This method may only return a valid value when
        /// there is at least 1 finger touching the device.
        /// </summary>
        public Vector3 GetFinger0PosWorld()
        {
            var posWorld = Vector3.zero;
            if (TouchWrapper.TouchCount > 0)
            {
                var fingerPos = TouchWrapper.Touch0.Position;
                RaycastGround(cam.ScreenPointToRay(fingerPos), out posWorld);
            }

            return posWorld;
        }

        /// <summary>
        /// Method for performing a raycast against either the refplane, or
        /// against a terrain-collider in case the collider is set.
        /// </summary>
        public bool RaycastGround(Ray ray, out Vector3 hitPoint)
        {
            bool hitSuccess;
            hitPoint = Vector3.zero;
            if (TerrainCollider != null)
            {
                hitSuccess = TerrainCollider.Raycast(ray, out var hitInfo, Mathf.Infinity);
                if (hitSuccess)
                {
                    hitPoint = hitInfo.point;
                }
            }
            else
            {
                hitSuccess = RefPlane.Raycast(ray, out float hitDistance);
                if (hitSuccess)
                {
                    hitPoint = ray.GetPoint(hitDistance);
                }
            }

            return hitSuccess;
        }

        /// <summary>
        /// Method for retrieving the intersection-point between the given ray and the ref plane.
        /// </summary>
        public Vector3 GetIntersectionPoint(Ray ray)
        {
            bool success = RefPlane.Raycast(ray, out float distance);
            if (success == false || (cam.orthographic == false && distance > _maxHorizonFallbackDistance))
            {
                if (_showHorizonError)
                {
                    Debug.LogError("Failed to compute intersection between camera ray and reference plane. Make sure the camera Axes are set up correctly.");
                    _showHorizonError = false;
                }

                //Fallback: Compute a sphere-cap on the ground and use the border point at the direction of the ray as maximum point in the distance.
                var rayOriginProjected = UnprojectVector2(ProjectVector3(ray.origin));
                var rayDirProjected = UnprojectVector2(ProjectVector3(ray.direction));
                return rayOriginProjected + rayDirProjected.normalized * _maxHorizonFallbackDistance;
            }

            return ray.origin + ray.direction * distance;
        }

        /// <summary>
        /// Custom planet intersection method that doesn't take into account rays parallel to the plane or rays shooting in the wrong direction and thus never hitting.
        /// May yield slightly better performance however and should be safe for use when the camera setup is correct (e.g. axes set correctly in this script, and camera actually pointing towards floor).
        /// </summary>
        public Vector3 GetIntersectionPointUnsafe(Ray ray)
        {
            float distance = Vector3.Dot(RefPlane.normal, Vector3.zero - ray.origin) / Vector3.Dot(RefPlane.normal, ray.origin + ray.direction - ray.origin);
            return ray.origin + ray.direction * distance;
        }

        /// <summary>
        /// Returns whether the camera is at the defined boundary.
        /// </summary>
        public bool GetIsBoundaryPosition(Vector3 testPosition) { return GetIsBoundaryPosition(testPosition, Vector2.zero); }

        /// <summary>
        /// Returns whether the camera is at the defined boundary.
        /// </summary>
        public bool GetIsBoundaryPosition(Vector3 testPosition, Vector2 margin)
        {
            var isBoundaryPosition = false;
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY2DSideScroll:
                    isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
                    isBoundaryPosition |= testPosition.y <= CamPosMin.y + margin.y;
                    isBoundaryPosition |= testPosition.y >= CamPosMax.y - margin.y;
                    break;
                case CameraPlaneAxes.XZTopDown:
                    isBoundaryPosition = testPosition.x <= CamPosMin.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= CamPosMax.x - margin.x;
                    isBoundaryPosition |= testPosition.z <= CamPosMin.y + margin.y;
                    isBoundaryPosition |= testPosition.z >= CamPosMax.y - margin.y;
                    break;
            }

            return isBoundaryPosition;
        }

        /// <summary>
        /// Returns a position that is clamped to the defined boundary.
        /// </summary>
        public Vector3 GetClampToBoundaries(Vector3 newPosition, bool includeSpringBackMargin = false)
        {
            var margin = Vector2.zero;
            if (includeSpringBackMargin)
            {
                margin = CamOverdragMargin2d;
            }

            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY2DSideScroll:
                    newPosition.x = Mathf.Clamp(newPosition.x, CamPosMin.x + margin.x, CamPosMax.x - margin.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, CamPosMin.y + margin.y, CamPosMax.y - margin.y);
                    break;
                case CameraPlaneAxes.XZTopDown:
                    newPosition.x = Mathf.Clamp(newPosition.x, CamPosMin.x + margin.x, CamPosMax.x - margin.x);
                    newPosition.z = Mathf.Clamp(newPosition.z, CamPosMin.y + margin.y, CamPosMax.y - margin.y);
                    break;
            }

            return newPosition;
        }

        public void OnDragSceneObject() { _isDraggingSceneObject = true; }

        public string CheckCameraAxesErrors()
        {
            var error = "";
            if (Transform.forward == Vector3.down && cameraAxes != CameraPlaneAxes.XZTopDown)
            {
                error = "Camera is pointing down but the cameraAxes is not set to TOP_DOWN. Make sure to set the cameraAxes variable properly.";
            }

            if (Transform.forward == Vector3.forward && cameraAxes != CameraPlaneAxes.XY2DSideScroll)
            {
                error = "Camera is pointing sidewards but the cameraAxes is not set to 2D_SIDESCROLL. Make sure to set the cameraAxes variable properly.";
            }

            return error;
        }

        /// <summary>
        /// Helper method that unprojects the given Vector2 to a Vector3
        /// according to the camera axes setting.
        /// </summary>
        public Vector3 UnprojectVector2(Vector2 v2, float offset = 0)
        {
            if (CameraAxes == CameraPlaneAxes.XY2DSideScroll) return new Vector3(v2.x, v2.y, offset);

            return new Vector3(v2.x, offset, v2.y);
        }

        public Vector2 ProjectVector3(Vector3 v3)
        {
            if (CameraAxes == CameraPlaneAxes.XY2DSideScroll) return new Vector2(v3.x, v3.y);

            return new Vector2(v3.x, v3.z);
        }

        #endregion

        private IEnumerator InitCamBoundariesDelayed()
        {
            yield return null;
            ComputeCamBoundaries();
        }

        /// <summary>
        /// MonoBehaviour method override to assign proper default values depending on
        /// the camera parameters and orientation.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();

            //Compute camera tilt to find out the camera orientation.
            var camForwardOnPlane = Vector3.Cross(Vector3.up, GetTiltRotationAxis());
            float tiltAngle = Vector3.Angle(camForwardOnPlane, -Transform.forward);
            CameraAxes = tiltAngle < 45 ? CameraPlaneAxes.XY2DSideScroll : CameraPlaneAxes.XZTopDown;

            //Compute zoom default values based on the camera type.
            var cameraComponent = GetComponent<Camera>();
            if (cameraComponent.orthographic)
            {
                CamZoomMin = 4;
                CamZoomMax = 13;
                CamOverzoomMargin = 1;
            }
            else
            {
                CamZoomMin = 5;
                CamZoomMax = 40;
                CamOverzoomMargin = 3;
                PerspectiveZoomMode = PerspectiveZoomMode.Translation;
            }
        }

        /// <summary>
        /// Method that does all the computation necessary when the pinch gesture of the user
        /// has changed.
        /// </summary>
        private void UpdatePinch(float deltaTime)
        {
            if (IsPinching)
            {
                if (_isTiltModeEvaluated)
                {
                    if (_isPinchTiltMode || isPinchModeExclusive == false)
                    {
                        //Tilt
                        float pinchTiltDelta = _pinchTiltLastFrame - _pinchTiltCurrent;
                        UpdateCameraTilt(pinchTiltDelta * pinchTiltSpeed);
                        _pinchTiltLastFrame = _pinchTiltCurrent;
                    }

                    if (_isPinchTiltMode == false || isPinchModeExclusive == false)
                    {
                        if (_isRotationActivated && _isRotationLock && Mathf.Abs(_pinchAngleCurrent) >= rotationLockThreshold) _isRotationLock = false;

                        if (IsSmoothingEnabled)
                        {
                            float lerpFactor = Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor);
                            _pinchDistanceCurrentLerp = Mathf.Lerp(_pinchDistanceCurrentLerp, _pinchDistanceCurrent, lerpFactor);
                            _pinchCenterCurrentLerp = Vector3.Lerp(_pinchCenterCurrentLerp, _pinchCenterCurrent, lerpFactor);
                            if (_isRotationLock == false) _pinchAngleCurrentLerp = Mathf.Lerp(_pinchAngleCurrentLerp, _pinchAngleCurrent, lerpFactor);
                        }
                        else
                        {
                            _pinchDistanceCurrentLerp = _pinchDistanceCurrent;
                            _pinchCenterCurrentLerp = _pinchCenterCurrent;
                            if (_isRotationLock == false) _pinchAngleCurrentLerp = _pinchAngleCurrent;
                        }

                        //Rotation
                        if (_isRotationActivated && _isRotationLock == false)
                        {
                            float pinchAngleDelta = _pinchAngleCurrentLerp - _pinchAngleLastFrame;
                            var rotationAxis = GetRotationAxis();
                            CachedTransform.RotateAround(_pinchCenterCurrent, rotationAxis, pinchAngleDelta);
                            _pinchAngleLastFrame = _pinchAngleCurrentLerp;
                            ComputeCamBoundaries();
                        }

                        //Zoom
                        float zoomFactor = _pinchDistanceStart /
                                           Mathf.Max((_pinchDistanceCurrentLerp - _pinchDistanceStart) * customZoomSensitivity + _pinchDistanceStart, 0.0001f);
                        float cameraSize = _pinchStartCamZoomSize * zoomFactor;
                        cameraSize = Mathf.Clamp(cameraSize, camZoomMin - camOverzoomMargin, camZoomMax + camOverzoomMargin);
                        if (enableZoomTilt) UpdateTiltForAutoTilt(cameraSize);

                        CamZoom = cameraSize;
                    }

                    //Position update.
                    DoPositionUpdateForTilt(false);
                }
            }
            else
            {
                //Spring back.
                if (EnableTilt && _enableOvertiltSpring)
                {
                    float overtiltSpringValue = ComputeOvertiltSpringBackFactor(_camOvertiltMargin);
                    if (Mathf.Abs(overtiltSpringValue) > _minOvertiltSpringPositionThreshold)
                    {
                        UpdateCameraTilt(overtiltSpringValue * deltaTime * _tiltBackSpringFactor);
                        DoPositionUpdateForTilt(true);
                    }
                }
            }
        }

        private void UpdateTiltForAutoTilt(float newCameraSize)
        {
            float zoomProgress = Mathf.Clamp01((newCameraSize - camZoomMin) / (camZoomMax - camZoomMin));
            float tiltTarget = Mathf.Lerp(zoomTiltAngleMin, zoomTiltAngleMax, zoomProgress);
            float tiltAngleDiff = tiltTarget - GetCurrentTiltAngleDeg(GetTiltRotationAxis());
            UpdateCameraTilt(tiltAngleDiff);
        }

        /// <summary>
        /// Method that computes the updated camera position when the user tilts the camera.
        /// </summary>
        private void DoPositionUpdateForTilt(bool isSpringBack)
        {
            //Position update.
            Vector3 intersectionDragCurrent;
            if (isSpringBack || (_isPinchTiltMode && isPinchModeExclusive))
            {
                intersectionDragCurrent = GetIntersectionPoint(GetCamCenterRay()); //In exclusive tilt mode always rotate around the screen center.
            }
            else
            {
                intersectionDragCurrent = GetIntersectionPoint(cam.ScreenPointToRay(_pinchCenterCurrentLerp));
            }

            var dragUpdateVector = intersectionDragCurrent - _pinchStartIntersectionCenter;
            if (isSpringBack && isPinchModeExclusive == false) dragUpdateVector = Vector3.zero;

            var targetPos = GetClampToBoundaries(CachedTransform.position - dragUpdateVector);

            CachedTransform.position = targetPos; //Disable smooth follow for the pinch-move update to prevent oscillation during the zoom phase.
            SetTargetPosition(targetPos);
        }

        /// <summary>
        /// Helper method for computing the tilt spring back.
        /// </summary>
        private float ComputeOvertiltSpringBackFactor(float margin)
        {
            float springBackValue = 0;
            var rotationAxis = GetTiltRotationAxis();
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
            if (tiltAngle < tiltAngleMin + margin)
            {
                springBackValue = tiltAngleMin + margin - tiltAngle;
            }
            else if (tiltAngle > tiltAngleMax - margin)
            {
                springBackValue = tiltAngleMax - margin - tiltAngle;
            }

            return springBackValue;
        }

        /// <summary>
        /// Method that computes all necessary parameters for a tilt update caused by the user's tilt gesture.
        /// </summary>
        private void UpdateCameraTilt(float angle)
        {
            var rotationAxis = GetTiltRotationAxis();
            var rotationPoint = GetIntersectionPoint(new Ray(CachedTransform.position, CachedTransform.forward));
            CachedTransform.RotateAround(rotationPoint, rotationAxis, angle);
            ClampCameraTilt(rotationPoint, rotationAxis);
            ComputeCamBoundaries();
        }

        /// <summary>
        /// Method that ensures that all limits are met when the user tilts the camera.
        /// </summary>
        private void ClampCameraTilt(Vector3 rotationPoint, Vector3 rotationAxis)
        {
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
            if (tiltAngle < tiltAngleMin)
            {
                float tiltClampDiff = tiltAngleMin - tiltAngle;
                CachedTransform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
            else if (tiltAngle > tiltAngleMax)
            {
                float tiltClampDiff = tiltAngleMax - tiltAngle;
                CachedTransform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
        }

        /// <summary>
        /// Method to get the current tilt angle of the camera.
        /// </summary>
        private float GetCurrentTiltAngleDeg(Vector3 rotationAxis)
        {
            var camForwardOnPlane = Vector3.Cross(RefPlane.normal, rotationAxis);
            float tiltAngle = Vector3.Angle(camForwardOnPlane, -CachedTransform.forward);
            return tiltAngle;
        }

        /// <summary>
        /// Returns the rotation axis of the camera. This purely depends
        /// on the defined camera axis.
        /// </summary>
        private Vector3 GetRotationAxis() { return RefPlane.normal; }

        /// <summary>
        /// Returns the rotation of the camera.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private float GetRotationDeg()
        {
            return CameraAxes == CameraPlaneAxes.XY2DSideScroll ? CachedTransform.rotation.eulerAngles.z : CachedTransform.rotation.eulerAngles.y;
        }

        /// <summary>
        /// Returns the tilt rotation axis.
        /// </summary>
        private Vector3 GetTiltRotationAxis()
        {
#if UNITY_EDITOR
            return Transform.right;
#else
            return CachedTransform.right;
#endif
        }

        /// <summary>
        /// Method to compute all the necessary updates when the user moves the camera.
        /// </summary>
        private void UpdatePosition(float deltaTime)
        {
            if (IsPinching && _isPinchTiltMode) return;

            if (IsDragging || IsPinching)
            {
                var posOld = CachedTransform.position;
                if (IsSmoothingEnabled)
                {
                    CachedTransform.position = Vector3.Lerp(CachedTransform.position, _targetPositionClamped, Mathf.Clamp01(Time.unscaledDeltaTime * camFollowFactor));
                }
                else
                {
                    CachedTransform.position = _targetPositionClamped;
                }

                DragCameraMoveVector.Add((posOld - CachedTransform.position) / Time.unscaledDeltaTime);
                if (DragCameraMoveVector.Count > MOMENTUM_SAMPLES_COUNT)
                {
                    DragCameraMoveVector.RemoveAt(0);
                }
            }

            Vector2 autoScrollVector = -_cameraScrollVelocity * deltaTime;
            var camPos = CachedTransform.position;
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY2DSideScroll:
                    camPos.x += autoScrollVector.x;
                    camPos.y += autoScrollVector.y;
                    break;
                case CameraPlaneAxes.XZTopDown:
                    camPos.x += autoScrollVector.x;
                    camPos.z += autoScrollVector.y;
                    break;
            }

            if (IsDragging == false && IsPinching == false)
            {
                var overdragSpringVector = ComputeOverdragSpringBackVector(camPos, CamOverdragMargin2d, ref _cameraScrollVelocity);
                if (overdragSpringVector.magnitude > float.Epsilon)
                {
                    camPos += overdragSpringVector * (Time.unscaledDeltaTime * dragBackSpringFactor);
                }
            }

            CachedTransform.position = GetClampToBoundaries(camPos);
        }

        /// <summary>
        /// Computes the camera drag spring back when the user is close to a boundary.
        /// </summary>
        private Vector3 ComputeOverdragSpringBackVector(Vector3 camPos, Vector2 margin, ref Vector3 currentCamScrollVelocity)
        {
            var springBackVector = Vector3.zero;
            if (camPos.x < CamPosMin.x + margin.x)
            {
                springBackVector.x = CamPosMin.x + margin.x - camPos.x;
                currentCamScrollVelocity.x = 0;
            }
            else if (camPos.x > CamPosMax.x - margin.x)
            {
                springBackVector.x = CamPosMax.x - margin.x - camPos.x;
                currentCamScrollVelocity.x = 0;
            }

            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY2DSideScroll:
                    if (camPos.y < CamPosMin.y + margin.y)
                    {
                        springBackVector.y = CamPosMin.y + margin.y - camPos.y;
                        currentCamScrollVelocity.y = 0;
                    }
                    else if (camPos.y > CamPosMax.y - margin.y)
                    {
                        springBackVector.y = CamPosMax.y - margin.y - camPos.y;
                        currentCamScrollVelocity.y = 0;
                    }

                    break;
                case CameraPlaneAxes.XZTopDown:
                    if (camPos.z < CamPosMin.y + margin.y)
                    {
                        springBackVector.z = CamPosMin.y + margin.y - camPos.z;
                        currentCamScrollVelocity.z = 0;
                    }
                    else if (camPos.z > CamPosMax.y - margin.y)
                    {
                        springBackVector.z = CamPosMax.y - margin.y - camPos.z;
                        currentCamScrollVelocity.z = 0;
                    }

                    break;
            }

            return springBackVector;
        }

        /// <summary>
        /// Internal helper method for setting the desired cam position.
        /// </summary>
        private void SetTargetPosition(Vector3 newPositionClamped) { _targetPositionClamped = newPositionClamped; }

        /// <summary>
        /// Method that computes the cam boundaries used for the current rotation and tilt of the camera.
        /// This computation is complex and needs to be invoked when the camera is rotated or tilted.
        /// </summary>
        private void ComputeCamBoundaries()
        {
            _refPlaneXY = new Plane(Vector3.back, groundLevelOffset);
            _refPlaneXZ = new Plane(Vector3.up, -groundLevelOffset);

            if (_useUntransformedCamBoundary)
            {
                CamPosMin = boundaryMin;
                CamPosMax = boundaryMax;
            }
            else
            {
                //float camRotation = GetRotationDeg();

                var camProjectedCenter =
                    GetIntersection2d(new Ray(CachedTransform.position,
                        -RefPlane.normal)); //Get camera position projected vertically onto the ref plane. This allows to compute the offset that arises from camera tilt.

                //Fetch camera boundary as world-space coordinates projected to the ground.
                var camRight = GetIntersection2d(cam.ScreenPointToRay(new Vector3(Screen.width, Screen.height * 0.5f, 0)));
                var camLeft = GetIntersection2d(cam.ScreenPointToRay(new Vector3(0, Screen.height * 0.5f, 0)));
                var camUp = GetIntersection2d(cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height, 0)));
                var camDown = GetIntersection2d(cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, 0, 0)));
                var camProjectedMin = GetVector2Min(camRight, camLeft, camUp, camDown);
                var camProjectedMax = GetVector2Max(camRight, camLeft, camUp, camDown);

                var projectionCorrectionMin = camProjectedCenter - camProjectedMin;
                var projectionCorrectionMax = camProjectedCenter - camProjectedMax;

                CamPosMin = boundaryMin + projectionCorrectionMin;
                CamPosMax = boundaryMax + projectionCorrectionMax;

                var margin = CamOverdragMargin2d;
                if (CamPosMax.x - CamPosMin.x < margin.x * 2)
                {
                    float midPoint = (CamPosMax.x + CamPosMin.x) * 0.5f;
                    CamPosMax = new Vector2(midPoint + margin.x, CamPosMax.y);
                    CamPosMin = new Vector2(midPoint - margin.x, CamPosMin.y);
                }

                if (CamPosMax.y - CamPosMin.y < margin.y * 2)
                {
                    float midPoint = (CamPosMax.y + CamPosMin.y) * 0.5f;
                    CamPosMax = new Vector2(CamPosMax.x, midPoint + margin.y);
                    CamPosMin = new Vector2(CamPosMin.x, midPoint - margin.y);
                }
            }
        }

        /// <summary>
        /// Method for retrieving the intersection of the given ray with the defined ground
        /// in 2d space.
        /// </summary>
        private Vector2 GetIntersection2d(Ray ray)
        {
            var intersection3d = GetIntersectionPoint(ray);
            var intersection2d = new Vector2(intersection3d.x, 0);
            switch (cameraAxes)
            {
                case CameraPlaneAxes.XY2DSideScroll:
                    intersection2d.y = intersection3d.y;
                    break;
                case CameraPlaneAxes.XZTopDown:
                    intersection2d.y = intersection3d.z;
                    break;
            }

            return intersection2d;
        }

        private Vector2 GetVector2Min(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return new Vector2(Mathf.Min(v0.x, v1.x, v2.x, v3.x), Mathf.Min(v0.y, v1.y, v2.y, v3.y));
        }

        private Vector2 GetVector2Max(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            return new Vector2(Mathf.Max(v0.x, v1.x, v2.x, v3.x), Mathf.Max(v0.y, v1.y, v2.y, v3.y));
        }

        protected void Update()
        {
            #region auto scroll code

            if (_cameraScrollVelocity.sqrMagnitude > float.Epsilon)
            {
                float timeSinceDragStop = Time.realtimeSinceStartup - _timeRealDragStop;
                float dampFactor = Mathf.Clamp01(timeSinceDragStop * dampFactorTimeMultiplier);
                float camScrollVel = _cameraScrollVelocity.magnitude;
                float camScrollVelRelative = camScrollVel / autoScrollVelocityMax;
                var camVelDamp = _cameraScrollVelocity.normalized * (dampFactor * autoScrollDamp * Time.unscaledDeltaTime);
                camVelDamp *= EvaluateAutoScrollDampCurve(Mathf.Clamp01(1.0f - camScrollVelRelative));
                if (camVelDamp.sqrMagnitude >= _cameraScrollVelocity.sqrMagnitude) _cameraScrollVelocity = Vector3.zero;
                else _cameraScrollVelocity -= camVelDamp;
            }

            #endregion
        }

        protected void LateUpdate()
        {
            //Pinch.
            UpdatePinch(Time.unscaledDeltaTime);

            //Translation.
            UpdatePosition(Time.unscaledDeltaTime);

            #region Keyboard & Mouse Input

            if (keyboardAndMousePlatforms.Contains(Application.platform) && Input.touchCount == 0)
            {
                float mouseScrollDelta = Input.GetAxis("Mouse ScrollWheel") * mouseZoomFactor;
                var mousePosDelta = _mousePosLastFrame - Input.mousePosition;
                var isEditorInputRotate = false;
                var isEditorInputTilt = false;

                if (Input.GetMouseButtonDown(1))
                {
                    _mouseRotationCenter = Input.mousePosition;
                }

                if (Input.GetMouseButton(1))
                {
                    mouseScrollDelta = mousePosDelta.x * mouseRotationFactor;
                    isEditorInputRotate = true;
                }
                else if (Input.GetMouseButton(2))
                {
                    mouseScrollDelta = mousePosDelta.y * mouseTiltFactor;
                    isEditorInputTilt = true;
                }

                bool anyModifierPressed = keyboardControlsModifiers.Exists(item => Input.GetKey(item));
                if (anyModifierPressed)
                {
                    if (GetKeyWithRepeat(KeyCode.KeypadPlus, out float timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardZoomFactor * timeFactor;
                    }
                    else if (GetKeyWithRepeat(KeyCode.KeypadMinus, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardZoomFactor * timeFactor;
                    }
                    else if (GetKeyWithRepeat(KeyCode.LeftArrow, out timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardRotationFactor * timeFactor;
                        isEditorInputRotate = true;
                        _mouseRotationCenter = Input.mousePosition;
                    }
                    else if (GetKeyWithRepeat(KeyCode.RightArrow, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardRotationFactor * timeFactor;
                        isEditorInputRotate = true;
                        _mouseRotationCenter = Input.mousePosition;
                    }
                    else if (GetKeyWithRepeat(KeyCode.UpArrow, out timeFactor))
                    {
                        mouseScrollDelta = 0.05f * keyboardTiltFactor * timeFactor;
                        isEditorInputTilt = true;
                    }
                    else if (GetKeyWithRepeat(KeyCode.DownArrow, out timeFactor))
                    {
                        mouseScrollDelta = -0.05f * keyboardTiltFactor * timeFactor;
                        isEditorInputTilt = true;
                    }
                }

                if (Mathf.Approximately(mouseScrollDelta, 0) == false)
                {
                    if (isEditorInputRotate)
                    {
                        if (EnableRotation)
                        {
                            var rotationAxis = GetRotationAxis();
                            var intersectionScreenCenter = GetIntersectionPoint(cam.ScreenPointToRay(_mouseRotationCenter));
                            CachedTransform.RotateAround(intersectionScreenCenter, rotationAxis, mouseScrollDelta * 100);
                            ComputeCamBoundaries();
                        }
                    }
                    else if (isEditorInputTilt)
                    {
                        if (EnableTilt)
                        {
                            UpdateCameraTilt(Mathf.Sign(mouseScrollDelta) * Mathf.Min(25, Mathf.Abs(mouseScrollDelta * 100)));
                        }
                    }
                    else
                    {
                        float editorZoomFactor;
                        if (cam.orthographic) editorZoomFactor = 15;
                        else editorZoomFactor = IsTranslationZoom ? 30 : 100;

                        float zoomAmount = mouseScrollDelta * editorZoomFactor;
                        float camSizeDiff = DoEditorCameraZoom(zoomAmount);
                        var intersectionScreenCenter = GetIntersectionPoint(cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
                        var pinchFocusVector = GetIntersectionPoint(cam.ScreenPointToRay(Input.mousePosition)) - intersectionScreenCenter;
                        float multiplier = 1.0f / CamZoom * camSizeDiff;
                        CachedTransform.position += pinchFocusVector * multiplier;
                    }
                }

                for (var i = 0; i < 3; ++i)
                {
                    if (Input.GetKeyDown((KeyCode) ((int) KeyCode.Alpha1 + i)))
                    {
                        StartCoroutine(ZoomToTargetValueCoroutine(Mathf.Lerp(CamZoomMin, CamZoomMax, i / 2.0f)));
                    }
                }
            }

            #endregion

            //When the camera is zoomed in further than the defined normal value, it will snap back to normal using the code below.
            if (IsPinching == false && IsDragging == false)
            {
                float camZoomDeltaToNormal = 0;
                if (CamZoom > camZoomMax)
                {
                    camZoomDeltaToNormal = CamZoom - camZoomMax;
                }
                else if (CamZoom < camZoomMin)
                {
                    camZoomDeltaToNormal = CamZoom - camZoomMin;
                }

                if (Mathf.Approximately(camZoomDeltaToNormal, 0) == false)
                {
                    float cameraSizeCorrection = Mathf.Lerp(0, camZoomDeltaToNormal, zoomBackSpringFactor * Time.unscaledDeltaTime);
                    if (Mathf.Abs(cameraSizeCorrection) > Mathf.Abs(camZoomDeltaToNormal))
                    {
                        cameraSizeCorrection = camZoomDeltaToNormal;
                    }

                    CamZoom -= cameraSizeCorrection;
                }
            }

            _mousePosLastFrame = Input.mousePosition;
            _camVelocity = (CachedTransform.position - _posLastFrame) / Time.unscaledDeltaTime;
            _posLastFrame = CachedTransform.position;
        }

        /// <summary>
        /// Editor helper code.
        /// </summary>
        private float DoEditorCameraZoom(float amount)
        {
            float newCamZoom = CamZoom - amount;
            newCamZoom = Mathf.Clamp(newCamZoom, camZoomMin, camZoomMax);
            float camSizeDiff = CamZoom - newCamZoom;
            if (enableZoomTilt)
            {
                UpdateTiltForAutoTilt(newCamZoom);
            }

            CamZoom = newCamZoom;
            return camSizeDiff;
        }

        /// <summary>
        /// Helper method used for auto scroll.
        /// </summary>
        private float EvaluateAutoScrollDampCurve(float t)
        {
            if (autoScrollDampCurve == null || autoScrollDampCurve.length == 0)
            {
                return 1;
            }

            return autoScrollDampCurve.Evaluate(t);
        }

        private void InputOnFingerDown(Vector3 pos) { _cameraScrollVelocity = Vector3.zero; }

        private void InputOnFingerUp() { _isDraggingSceneObject = false; }

        private Vector3 GetDragVector(Vector3 dragPosStart, Vector3 dragPosCurrent)
        {
            var intersectionDragStart = GetIntersectionPoint(cam.ScreenPointToRay(dragPosStart));
            var intersectionDragCurrent = GetIntersectionPoint(cam.ScreenPointToRay(dragPosCurrent));
            return intersectionDragCurrent - intersectionDragStart;
        }

        /// <summary>
        /// Helper method that computes the suggested auto cam velocity from
        /// the last few frames of the user drag.
        /// </summary>
        private Vector3 GetVelocityFromMoveHistory()
        {
            var momentum = Vector3.zero;
            if (DragCameraMoveVector.Count > 0)
            {
                for (var i = 0; i < DragCameraMoveVector.Count; ++i)
                {
                    momentum += DragCameraMoveVector[i];
                }

                momentum /= DragCameraMoveVector.Count;
            }

            if (CameraAxes == CameraPlaneAxes.XZTopDown)
            {
                momentum.y = momentum.z;
                momentum.z = 0;
            }

            return momentum;
        }

        private void InputOnDragStart(Vector3 dragPosStart, bool isLongTap)
        {
            if (TerrainCollider != null)
            {
                UpdateRefPlaneForTerrain(dragPosStart);
            }

            if (_isDraggingSceneObject == false)
            {
                _cameraScrollVelocity = Vector3.zero;
                _dragStartCamPos = CachedTransform.position;
                IsDragging = true;
                DragCameraMoveVector.Clear();
                SetTargetPosition(CachedTransform.position);
            }
        }

        private void InputOnDragUpdate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset, Vector3 delta)
        {
            if (_isDraggingSceneObject == false)
            {
                var dragVector = GetDragVector(dragPosStart, dragPosCurrent + correctionOffset);
                var posNewClamped = GetClampToBoundaries(_dragStartCamPos - dragVector);
                SetTargetPosition(posNewClamped);
            }
            else
            {
                IsDragging = false;
            }
        }

        private void InputOnDragStop(Vector3 dragStopPos, Vector3 dragFinalMomentum)
        {
            if (_isDraggingSceneObject == false)
            {
                if (_useOldScrollDamp)
                {
                    _cameraScrollVelocity = GetVelocityFromMoveHistory();
                    if (_cameraScrollVelocity.sqrMagnitude >= autoScrollVelocityMax * autoScrollVelocityMax)
                    {
                        _cameraScrollVelocity = _cameraScrollVelocity.normalized * autoScrollVelocityMax;
                    }
                }
                else
                {
                    _cameraScrollVelocity = -ProjectVector3(_camVelocity) * 0.5f;
                }

                _timeRealDragStop = Time.realtimeSinceStartup;
                DragCameraMoveVector.Clear();
            }

            IsDragging = false;
        }

        private void InputOnPinchStart(Vector3 pinchCenter, float pinchDistance)
        {
            if (TerrainCollider != null)
            {
                UpdateRefPlaneForTerrain(pinchCenter);
            }

            _pinchStartCamZoomSize = CamZoom;
            _pinchStartIntersectionCenter = GetIntersectionPoint(cam.ScreenPointToRay(pinchCenter));

            _pinchCenterCurrent = pinchCenter;
            _pinchDistanceCurrent = pinchDistance;
            _pinchDistanceStart = pinchDistance;

            _pinchCenterCurrentLerp = pinchCenter;
            _pinchDistanceCurrentLerp = pinchDistance;

            SetTargetPosition(CachedTransform.position);
            IsPinching = true;
            _isRotationActivated = false;
            ResetPinchRotation(0);

            _pinchTiltCurrent = 0;
            _pinchTiltAccumulated = 0;
            _pinchTiltLastFrame = 0;
            _isTiltModeEvaluated = false;
            _isPinchTiltMode = false;

            if (EnableTilt == false)
            {
                _isTiltModeEvaluated = true; //Early out of this evaluation in case tilt is not enabled.
            }
        }

        private void InputOnPinchUpdate(PinchData pinchData)
        {
            if (EnableTilt)
            {
                _pinchTiltCurrent += pinchData.pinchTiltDelta;
                _pinchTiltAccumulated += Mathf.Abs(pinchData.pinchTiltDelta);

                if (_isTiltModeEvaluated == false && pinchData.pinchTotalFingerMovement > pinchModeDetectionMoveTreshold)
                {
                    _isPinchTiltMode = Mathf.Abs(_pinchTiltCurrent) > pinchTiltModeThreshold;
                    _isTiltModeEvaluated = true;
                    if (_isPinchTiltMode && isPinchModeExclusive)
                    {
                        _pinchStartIntersectionCenter = GetIntersectionPoint(GetCamCenterRay());
                    }
                }
            }

            if (_isTiltModeEvaluated)
            {
#pragma warning disable 162
                if (isPinchModeExclusive)
                {
                    _pinchCenterCurrent = pinchData.pinchCenter;

                    if (_isPinchTiltMode)
                    {
                        //Evaluate a potential break-out from a tilt. Under certain tweak-settings the tilt may trigger prematurely and needs to be overrided.
                        if (_pinchTiltAccumulated < PINCH_ACCUM_BREAKOUT)
                        {
                            bool breakoutZoom = Mathf.Abs(_pinchDistanceStart - pinchData.pinchDistance) > PINCH_DISTANCE_FOR_TILT_BREAKOUT;
                            bool breakoutRot = enableRotation && Mathf.Abs(_pinchAngleCurrent) > rotationLockThreshold;
                            if (breakoutZoom || breakoutRot)
                            {
                                InputOnPinchStart(pinchData.pinchCenter, pinchData.pinchDistance);
                                _isTiltModeEvaluated = true;
                                _isPinchTiltMode = false;
                            }
                        }
                    }
                }
#pragma warning restore 162
                _pinchDistanceCurrent = pinchData.pinchDistance;

                if (enableRotation)
                {
                    if (Mathf.Abs(pinchData.pinchAngleDeltaNormalized) > rotationDetectionDeltaThreshold)
                    {
                        _pinchAngleCurrent += pinchData.pinchAngleDelta;
                    }

                    if (_pinchDistanceCurrent > rotationMinPinchDistance)
                    {
                        if (_isRotationActivated == false)
                        {
                            ResetPinchRotation(0);
                            _isRotationActivated = true;
                        }
                    }
                    else
                    {
                        _isRotationActivated = false;
                    }
                }
            }
        }

        private void ResetPinchRotation(float currentPinchRotation)
        {
            _pinchAngleCurrent = currentPinchRotation;
            _pinchAngleCurrentLerp = currentPinchRotation;
            _pinchAngleLastFrame = currentPinchRotation;
            _isRotationLock = true;
        }

        private void InputOnPinchStop()
        {
            IsPinching = false;
            DragCameraMoveVector.Clear();
            _isPinchTiltMode = false;
            _isTiltModeEvaluated = false;
        }

        private void OnClick(Vector3 clickPosition, bool isDoubleClick, bool isLongTap)
        {
            if (isLongTap)
            {
                return;
            }

            var camRay = cam.ScreenPointToRay(clickPosition);
            if (onPickItem != null || onPickItemDoubleClick != null)
            {
                if (Physics.Raycast(camRay, out var hitInfo))
                {
                    onPickItem?.Invoke(hitInfo);

                    if (isDoubleClick)
                    {
                        onPickItemDoubleClick?.Invoke(hitInfo);
                    }
                }
            }

            if (onPickItem2D != null || onPickItem2DDoubleClick != null)
            {
                var hitInfo2D = Physics2D.Raycast(camRay.origin, camRay.direction);
                if (hitInfo2D == true)
                {
                    onPickItem2D?.Invoke(hitInfo2D);

                    if (isDoubleClick) onPickItem2DDoubleClick?.Invoke(hitInfo2D);
                }
            }
        }

        private IEnumerator ZoomToTargetValueCoroutine(float target)
        {
            if (Mathf.Approximately(target, CamZoom) == false)
            {
                float startValue = CamZoom;
                const float duration = 0.3f;
                float timeStart = Time.time;
                while (Time.time < timeStart + duration)
                {
                    float progress = (Time.time - timeStart) / duration;
                    CamZoom = Mathf.Lerp(startValue, target, Mathf.Sin(-Mathf.PI * 0.5f + progress * Mathf.PI) * 0.5f + 0.5f);
                    yield return null;
                }

                CamZoom = target;
            }
        }

        private Ray GetCamCenterRay() { return cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)); }

        private void UpdateRefPlaneForTerrain(Vector3 touchPosition)
        {
            var dragRay = cam.ScreenPointToRay(touchPosition);
            if (TerrainCollider.Raycast(dragRay, out var hitInfo, float.MaxValue))
            {
                _refPlaneXZ = new Plane(new Vector3(0, 1, 0), -hitInfo.point.y);
            }
        }

        private bool GetKeyWithRepeat(KeyCode keyCode, out float factor)
        {
            var isDown = false;
            factor = 1;
            if (Input.GetKeyDown(keyCode))
            {
                _timeKeyDown = Time.realtimeSinceStartup;
                isDown = true;
            }
            else if (Input.GetKey(keyCode) && Time.realtimeSinceStartup > _timeKeyDown + keyboardRepeatDelay)
            {
                isDown = true;
                factor = Time.unscaledDeltaTime * keyboardRepeatFactor;
            }

            return isDown;
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var boundaryCenter2d = 0.5f * (boundaryMin + boundaryMax);
            var boundarySize2d = boundaryMax - boundaryMin;
            var boundaryCenter = UnprojectVector2(boundaryCenter2d, groundLevelOffset);
            var boundarySize = UnprojectVector2(boundarySize2d);
            Gizmos.DrawWireCube(boundaryCenter, boundarySize);
        }
    }
}