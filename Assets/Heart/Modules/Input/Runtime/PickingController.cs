using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Pancake.Common;
using Sisus.Init;
using UnityEngine.Events;

namespace Pancake.MobileInput
{
    [EditorIcon("icon_manager")]
    [RequireComponent(typeof(TouchCamera))]
    public class PickingController : MonoBehaviour<TouchInput>
    {
        public enum SelectionAction
        {
            Select,
            Deselect,
        }

        #region Field

        [SerializeField] [Tooltip("When set to true, the position of dragged items snaps to discrete units.")]
        private bool snapToGrid = true;

        [SerializeField] [Tooltip("Size of the snap units when snapToGrid is enabled.")]
        private float snapUnitSize = 1;

        [SerializeField]
        [Tooltip(
            "When snapping is enabled, this value defines a position offset that is added to the center of the object when dragging. When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 snapOffset = Vector2.zero;

        [SerializeField]
        [Tooltip(
            "When set to Straight, picked items will be snapped to a perfectly horizontal and vertical grid in world space. Diagonal snaps the items on a 45 degree grid.")]
        private SnapAngle snapAngle = SnapAngle.Straight0Degrees;

        [Header("Advanced")] [SerializeField] [Tooltip("When this flag is enabled, more than one item can be selected and moved at the same time.")]
        private bool isMultiSelectionEnabled;

        [SerializeField] [Tooltip("When setting this variable to true, pickables can only be moved by long tapping on them first.")]
        private bool requireLongTapForMove;


#if UNITY_EDITOR
        [Space] [SerializeField] private bool advanceMode;
#endif

        [SerializeField, ShowIf("advanceMode"), Indent]
        [Tooltip(
            "When setting this to false, pickables will not become deselected when the user clicks somewhere on the screen, except when he clicks on another pickable.")]
        private bool deselectPreviousColliderOnClick = true;

        [SerializeField, ShowIf("advanceMode"), Indent]
        [Tooltip("When setting this to false, the OnPickableTransformSelect event will only be sent once when clicking on the same pickable repeatedly.")]
        private bool repeatEventSelectedOnClick = true;

        [SerializeField, ShowIf("advanceMode"), Indent] [Tooltip("May have fired the OnPickableTransformMoveStarted too early, when it hasn't actually been moved.")]
        private bool useLegacyTransformMoved;

        [Space] [SerializeField, FoldoutGroup("Pickable Callback")] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is selected.")]
        private TransformUnityEvent onTransformSelectedCallback;

        [SerializeField, FoldoutGroup("Pickable Callback")]
        [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is selected through a long tap.")]
        private PickableSelectedUnityEvent onTransformSelectedExtendedCallback;

        [SerializeField, FoldoutGroup("Pickable Callback")] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is deselected.")]
        private TransformUnityEvent onTransformDeselectedCallback;

        [SerializeField, FoldoutGroup("Pickable Callback")] [Tooltip("Here you can set up callbacks to be invoked when the moving of a pickable transform is started.")]
        private TransformUnityEvent onTransformMoveStartedCallback;

        [SerializeField, FoldoutGroup("Pickable Callback")] [Tooltip("Here you can set up callbacks to be invoked when a pickable transform is moved to a new position.")]
        private TransformUnityEvent onTransformMovedCallback;

        [SerializeField, FoldoutGroup("Pickable Callback")]
        [Tooltip(
            "Here you can set up callbacks to be invoked when the moving of a pickable transform is ended. The event requires 2 parameters. The first is the start position of the drag. The second is the dragged transform. The start position can be used to reset the transform in case the drag has ended on an invalid position.")]
        private Vector3TransformUnityEvent onTransformMoveEndedCallback;

        #endregion


        private TouchCamera _touchCam;
        private TouchInput _touchInput;

        private Component SelectedCollider => SelectedColliders.Count == 0 ? null : SelectedColliders[^1];

        public List<Component> SelectedColliders { get; private set; }

        private bool _isSelectedViaLongTap;

        public TouchPickable CurrentlyDraggedPickable { get; private set; }

        private Transform CurrentlyDraggedTransform => CurrentlyDraggedPickable != null ? CurrentlyDraggedPickable.PickableTransform : null;

        private Vector3 _draggedTransformOffset = Vector3.zero;

        private Vector3 _draggedTransformHeightOffset = Vector3.zero;

        private Vector3 _draggedItemCustomOffset = Vector3.zero;

        public bool SnapToGrid { get => snapToGrid; set => snapToGrid = value; }

        public SnapAngle SnapAngle { get => snapAngle; set => snapAngle = value; }

        public float SnapUnitSize { get => snapUnitSize; set => snapUnitSize = value; }

        public Vector2 SnapOffset { get => snapOffset; set => snapOffset = value; }

        public const float SNAP_ANGLE_DIAGONAL = 45 * Mathf.Deg2Rad;

        private Vector3 _currentlyDraggedTransformPosition = Vector3.zero;

        private const float TRANSFORM_MOVED_DISTANCE_THRESHOLD = 0.001f;

        private Vector3 _currentDragStartPos = Vector3.zero;

        private bool _invokeMoveStartedOnDrag;
        private bool _invokeMoveEndedOnDrag;

        private Vector3 _itemInitialDragOffsetWorld;
        private bool _isManualSelectionRequest;

        public bool IsMultiSelectionEnabled
        {
            get => isMultiSelectionEnabled;
            set
            {
                isMultiSelectionEnabled = value;
                if (value == false) DeselectAll();
            }
        }

        private Dictionary<Component, Vector3> _selectionPositionOffsets = new();

        protected override void Init(TouchInput argument) { _touchInput = argument; }

        public void Awake()
        {
            SelectedColliders = new List<Component>();
            _touchCam = GetComponent<TouchCamera>();
            if (_touchCam == null) Debug.LogError("No TouchCamera found in scene. This script will not work without this.");
        }

        public void Start()
        {
            TouchInput.OnClick += OnClick;
            TouchInput.OnFingerDown += InputOnFingerDown;
            TouchInput.OnFingerUp += InputOnFingerUp;
            TouchInput.OnStartDrag += InputOnDragStart;
            TouchInput.OnUpdateDrag += InputOnDragUpdate;
            TouchInput.OnStopDrag += InputOnDragStop;
        }

        public void OnDestroy()
        {
            TouchInput.OnClick -= OnClick;
            TouchInput.OnFingerDown -= InputOnFingerDown;
            TouchInput.OnFingerUp -= InputOnFingerUp;
            TouchInput.OnStartDrag -= InputOnDragStart;
            TouchInput.OnUpdateDrag -= InputOnDragUpdate;
            TouchInput.OnStopDrag -= InputOnDragStop;
        }

        public void LateUpdate()
        {
            if (_isManualSelectionRequest && TouchWrapper.TouchCount == 0) _isManualSelectionRequest = false;
        }

        #region public interface

        /// <summary>
        /// Method that allows to set the currently selected collider for the picking controller by code.
        /// Useful for example for auto-selecting newly spawned items or for selecting items via a menu button.
        /// Use this method when you want to select just one item.
        /// </summary>
        public void SelectCollider(Component collider)
        {
            if (IsMultiSelectionEnabled)
            {
                Select(collider, false, false);
            }
            else
            {
                SelectColliderInternal(collider, false, false);
                _isManualSelectionRequest = true;
            }
        }

        /// <summary>
        /// Method to deselect the last selected collider.
        /// </summary>
        public void DeselectSelectedCollider() { Deselect(SelectedCollider); }

        /// <summary>
        /// In case multi-selection is enabled, this method allows to deselect
        /// all colliders at once.
        /// </summary>
        public void DeselectAllSelectedColliders()
        {
            var collidersToRemove = new List<Component>(SelectedColliders);
            foreach (var colliderToRemove in collidersToRemove)
            {
                Deselect(colliderToRemove);
            }
        }

        /// <summary>
        /// Method to deselect the given collider.
        /// In case the collider hasn't been selected before, the method returns false.
        /// </summary>
        private bool Deselect(Component colliderComponent)
        {
            bool wasRemoved = SelectedColliders.Remove(colliderComponent);
            if (wasRemoved)
            {
                OnSelectedColliderChanged(SelectionAction.Deselect, colliderComponent.GetComponent<TouchPickable>());
            }

            return wasRemoved;
        }

        /// <summary>
        /// Method to deselect all currently selected colliders.
        /// </summary>
        /// <returns></returns>
        public int DeselectAll()
        {
            SelectedColliders.RemoveAll(item => item == null);
            int colliderCount = SelectedColliders.Count;
            foreach (Component colliderComponent in SelectedColliders)
            {
                OnSelectedColliderChanged(SelectionAction.Deselect, colliderComponent.GetComponent<TouchPickable>());
            }

            SelectedColliders.Clear();
            return colliderCount;
        }

        public Component GetClosestColliderAtScreenPoint(Vector3 screenPoint, out Vector3 intersectionPoint)
        {
            Component hitCollider = null;
            var hitDistance = float.MaxValue;
            var camRay = _touchCam.Cam.ScreenPointToRay(screenPoint);
            intersectionPoint = Vector3.zero;
            if (Physics.Raycast(camRay, out var hitInfo))
            {
                hitDistance = hitInfo.distance;
                hitCollider = hitInfo.collider;
                intersectionPoint = hitInfo.point;
            }

            var hitInfo2D = Physics2D.Raycast(camRay.origin, camRay.direction);
            if (hitInfo2D == true)
            {
                if (hitInfo2D.distance < hitDistance)
                {
                    hitCollider = hitInfo2D.collider;
                    intersectionPoint = hitInfo2D.point;
                }
            }

            return hitCollider;
        }

        public void RequestDragPickable(Component colliderComponent)
        {
            if (TouchWrapper.TouchCount == 1)
            {
                SelectColliderInternal(colliderComponent, false, false);
                _isManualSelectionRequest = true;
                Vector3 fingerDownPos = TouchWrapper.Touch0.Position;
                Ray dragRay = _touchCam.Cam.ScreenPointToRay(fingerDownPos);
                bool hitSuccess = _touchCam.RaycastGround(dragRay, out var intersectionPoint);
                if (hitSuccess == false)
                {
                    intersectionPoint = colliderComponent.transform.position;
                }

                if (requireLongTapForMove)
                {
                    _isSelectedViaLongTap = true; //This line ensures that dragging via scrip also works when 'requireLongTapForDrag' is set to true.
                }

                RequestDragPickable(colliderComponent, fingerDownPos, intersectionPoint);
                _invokeMoveEndedOnDrag = true;
            }
            else
            {
                Debug.LogError("A drag request can only be invoked when the user has placed exactly 1 finger on the screen.");
            }
        }

        public Vector3 GetFinger0PosWorld() { return _touchCam.GetFinger0PosWorld(); }

        #endregion

        private void SelectColliderInternal(Component colliderComponent, bool isDoubleClick, bool isLongTap)
        {
            if (deselectPreviousColliderOnClick == false)
            {
                //Skip selection change in case the user requested to deselect only in case another pickable is clicked.
                if (colliderComponent == null || colliderComponent.GetComponent<TouchPickable>() == null) return;
            }

            //Skip selection when the user has already requested a manual selection with the same click.
            if (_isManualSelectionRequest) return;

            Component previouslySelectedCollider = SelectedCollider;
            bool skipSelect = false;

            if (isMultiSelectionEnabled == false)
            {
                if (previouslySelectedCollider != null && previouslySelectedCollider != colliderComponent)
                {
                    Deselect(previouslySelectedCollider);
                }
            }
            else
            {
                skipSelect = Deselect(colliderComponent);
            }

            if (skipSelect == false)
            {
                if (colliderComponent != null)
                {
                    if (colliderComponent != previouslySelectedCollider || repeatEventSelectedOnClick)
                    {
                        Select(colliderComponent, isDoubleClick, isLongTap);
                        _isSelectedViaLongTap = isLongTap;
                    }
                }
            }
        }

        private void OnClick(Vector3 clickPosition, bool isDoubleClick, bool isLongTap)
        {
            var newCollider = GetClosestColliderAtScreenPoint(clickPosition, out _);
            SelectColliderInternal(newCollider, isDoubleClick, isLongTap);
        }

        private void RequestDragPickable(Vector3 fingerDownPosition)
        {
            var pickedCollider = GetClosestColliderAtScreenPoint(fingerDownPosition, out var intersectionPoint);
            if (pickedCollider != null && SelectedColliders.Contains(pickedCollider))
            {
                RequestDragPickable(pickedCollider, fingerDownPosition, intersectionPoint);
            }
        }

        private void RequestDragPickable(Component colliderComponent, Vector2 fingerDownPosition, Vector3 intersectionPoint)
        {
            if (requireLongTapForMove && _isSelectedViaLongTap == false) return;

            CurrentlyDraggedPickable = null;
            bool isDragStartedOnSelection = colliderComponent != null && SelectedColliders.Contains(colliderComponent);
            if (isDragStartedOnSelection)
            {
                TouchPickable touchPickable = colliderComponent.GetComponent<TouchPickable>();
                if (touchPickable != null)
                {
                    _touchCam.OnDragSceneObject(); //Lock camera movement.
                    CurrentlyDraggedPickable = touchPickable;
                    _currentlyDraggedTransformPosition = CurrentlyDraggedTransform.position;

                    _invokeMoveStartedOnDrag = true;
                    _currentDragStartPos = CurrentlyDraggedTransform.position;
                    _selectionPositionOffsets.Clear();
                    foreach (Component selectionComponent in SelectedColliders)
                    {
                        _selectionPositionOffsets.Add(selectionComponent, _currentDragStartPos - selectionComponent.transform.position);
                    }

                    _draggedTransformOffset = Vector3.zero;
                    _draggedTransformHeightOffset = Vector3.zero;
                    _draggedItemCustomOffset = Vector3.zero;

                    //Find offset of item transform relative to ground.
                    Ray groundScanRayCenter = new Ray(CurrentlyDraggedTransform.position, -_touchCam.RefPlane.normal);
                    bool rayHitSuccess = _touchCam.RaycastGround(groundScanRayCenter, out var groundPosCenter);
                    if (rayHitSuccess)
                    {
                        _draggedTransformHeightOffset = CurrentlyDraggedTransform.position - groundPosCenter;
                    }
                    else
                    {
                        groundPosCenter = CurrentlyDraggedTransform.position;
                    }

                    _draggedTransformOffset = groundPosCenter - intersectionPoint;
                    _itemInitialDragOffsetWorld = ComputeDragPosition(fingerDownPosition, SnapToGrid) - CurrentlyDraggedTransform.position;
                }
            }
        }

        private void InputOnFingerDown(Vector3 fingerDownPosition)
        {
            if (requireLongTapForMove == false || _isSelectedViaLongTap) RequestDragPickable(fingerDownPosition);
        }

        private void InputOnFingerUp() { EndPickableTransformMove(); }

        private Vector3 ComputeDragPosition(Vector3 dragPositionCurrent, bool clampToGrid)
        {
            Ray dragRay = _touchCam.Cam.ScreenPointToRay(dragPositionCurrent);

            dragRay.origin += _draggedTransformOffset;
            bool hitSuccess = _touchCam.RaycastGround(dragRay, out var dragPosWorld);
            if (hitSuccess == false)
            {
                //This case really should never be met. But in case it is for some unknown reason, return the current item position. That way at least it will remain static and not move somewhere into nirvana.
                return CurrentlyDraggedTransform.position;
            }

            dragPosWorld += _draggedTransformHeightOffset;
            dragPosWorld += _draggedItemCustomOffset;

            if (clampToGrid)
            {
                dragPosWorld = ClampDragPosition(CurrentlyDraggedPickable, dragPosWorld);
            }

            return dragPosWorld;
        }

        private void InputOnDragStart(Vector3 clickPosition, bool isLongTap)
        {
            if (isLongTap && _touchInput.LongTapStartsDrag)
            {
                var newCollider = GetClosestColliderAtScreenPoint(clickPosition, out _);
                if (newCollider != null)
                {
                    var newPickable = newCollider.GetComponent<TouchPickable>();
                    if (newPickable != null)
                    {
                        if (SelectedColliders.Contains(newCollider) == false)
                        {
                            SelectColliderInternal(newCollider, false, true);
                        }
                        else
                        {
                            _isSelectedViaLongTap = true;
                        }

                        RequestDragPickable(clickPosition);
                    }
                }
            }
        }

        private void InputOnDragUpdate(Vector3 dragPositionStart, Vector3 dragPositionCurrent, Vector3 correctionOffset, Vector3 delta)
        {
            if (CurrentlyDraggedTransform != null)
            {
                if (_invokeMoveStartedOnDrag && useLegacyTransformMoved) InvokePickableMoveStart();

                //Accomodate for custom movements by user code that happen while an item is being dragged. E.g. this allows users to lift items slightly during a drag.
                _draggedItemCustomOffset += CurrentlyDraggedTransform.position - _currentlyDraggedTransformPosition;
                Vector3 dragPosWorld = ComputeDragPosition(dragPositionCurrent, SnapToGrid);
                CurrentlyDraggedTransform.position = dragPosWorld - _itemInitialDragOffsetWorld;

                if (SelectedColliders.Count > 1)
                {
                    foreach (KeyValuePair<Component, Vector3> colliderOffsetPair in _selectionPositionOffsets)
                    {
                        if (colliderOffsetPair.Key != null && colliderOffsetPair.Key.transform != CurrentlyDraggedTransform)
                        {
                            colliderOffsetPair.Key.transform.position = CurrentlyDraggedTransform.position - colliderOffsetPair.Value;
                        }
                    }
                }

                bool hasMoved;
                if (_touchCam.CameraAxes == CameraPlaneAxes.XY2DSideScroll)
                {
                    hasMoved = ComputeDistance2d(CurrentlyDraggedTransform.position.x,
                        CurrentlyDraggedTransform.position.y,
                        _currentlyDraggedTransformPosition.x,
                        _currentlyDraggedTransformPosition.y) > TRANSFORM_MOVED_DISTANCE_THRESHOLD;
                }
                else
                {
                    hasMoved = ComputeDistance2d(CurrentlyDraggedTransform.position.x,
                        CurrentlyDraggedTransform.position.z,
                        _currentlyDraggedTransformPosition.x,
                        _currentlyDraggedTransformPosition.z) > TRANSFORM_MOVED_DISTANCE_THRESHOLD;
                }

                if (hasMoved)
                {
                    if (_invokeMoveStartedOnDrag && useLegacyTransformMoved == false) InvokePickableMoveStart();

                    InvokeTransformActionSafe(onTransformMovedCallback, CurrentlyDraggedTransform);
                }

                _currentlyDraggedTransformPosition = CurrentlyDraggedTransform.position;
            }
        }

        private void InvokePickableMoveStart()
        {
            InvokeTransformActionSafe(onTransformMoveStartedCallback, CurrentlyDraggedTransform);
            _invokeMoveStartedOnDrag = false;
            _invokeMoveEndedOnDrag = true;
        }

        private float ComputeDistance2d(float x0, float y0, float x1, float y1) { return Mathf.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0)); }

        private void InputOnDragStop(Vector3 dragStopPosition, Vector3 dragFinalMomentum) { EndPickableTransformMove(); }

        private void EndPickableTransformMove()
        {
            if (CurrentlyDraggedTransform != null)
            {
                if (_invokeMoveEndedOnDrag)
                {
                    onTransformMoveEndedCallback?.Invoke(_currentDragStartPos, CurrentlyDraggedTransform);
                }
            }

            CurrentlyDraggedPickable = null;
            _invokeMoveStartedOnDrag = false;
            _invokeMoveEndedOnDrag = false;
        }

        private Vector3 ClampDragPosition(TouchPickable draggedPickable, Vector3 position)
        {
            if (_touchCam.CameraAxes == CameraPlaneAxes.XY2DSideScroll)
            {
                if (snapAngle == SnapAngle.Diagonal45Degrees) RotateVector2(ref position.x, ref position.y, -SNAP_ANGLE_DIAGONAL);

                position.x = GetPositionSnapped(position.x, draggedPickable.LocalSnapOffset.x + snapOffset.x);
                position.y = GetPositionSnapped(position.y, draggedPickable.LocalSnapOffset.y + snapOffset.y);
                if (snapAngle == SnapAngle.Diagonal45Degrees) RotateVector2(ref position.x, ref position.y, SNAP_ANGLE_DIAGONAL);
            }
            else
            {
                if (snapAngle == SnapAngle.Diagonal45Degrees) RotateVector2(ref position.x, ref position.z, -SNAP_ANGLE_DIAGONAL);

                position.x = GetPositionSnapped(position.x, draggedPickable.LocalSnapOffset.x + snapOffset.x);
                position.z = GetPositionSnapped(position.z, draggedPickable.LocalSnapOffset.y + snapOffset.y);
                if (snapAngle == SnapAngle.Diagonal45Degrees) RotateVector2(ref position.x, ref position.z, SNAP_ANGLE_DIAGONAL);
            }

            return position;
        }

        private void RotateVector2(ref float x, ref float y, float degrees)
        {
            if (Mathf.Approximately(degrees, 0)) return;

            float newX = x * Mathf.Cos(degrees) - y * Mathf.Sin(degrees);
            float newY = x * Mathf.Sin(degrees) + y * Mathf.Cos(degrees);
            x = newX;
            y = newY;
        }

        private float GetPositionSnapped(float position, float snapOffset)
        {
            if (snapToGrid) return Mathf.RoundToInt(position / snapUnitSize) * snapUnitSize + snapOffset;

            return position;
        }

        private void OnSelectedColliderChanged(SelectionAction selectionAction, TouchPickable touchPickable)
        {
            if (touchPickable == null) return;

            switch (selectionAction)
            {
                case SelectionAction.Select:
                    InvokeTransformActionSafe(onTransformSelectedCallback, touchPickable.PickableTransform);
                    break;
                case SelectionAction.Deselect:
                    InvokeTransformActionSafe(onTransformDeselectedCallback, touchPickable.PickableTransform);
                    break;
            }
        }

        private void OnSelectedColliderChangedExtended(SelectionAction selectionAction, TouchPickable touchPickable, bool isDoubleClick, bool isLongTap)
        {
            if (touchPickable == null) return;
            if (selectionAction != SelectionAction.Select) return;

            var pickableSelected = new PickableSelected() {Selected = touchPickable.PickableTransform, IsDoubleClick = isDoubleClick, IsLongTap = isLongTap};
            InvokeGenericActionSafe(onTransformSelectedExtendedCallback, pickableSelected);
        }

        private void InvokeTransformActionSafe(TransformUnityEvent @event, Transform t) { @event?.Invoke(t); }

        private void InvokeGenericActionSafe<T1, T2>(T1 eventAction, T2 eventArgs) where T1 : UnityEvent<T2> { eventAction?.Invoke(eventArgs); }

        private void Select(Component colliderComponent, bool isDoubleClick, bool isLongTap)
        {
            TouchPickable touchPickable = colliderComponent.GetComponent<TouchPickable>();
            if (touchPickable != null)
            {
                if (SelectedColliders.Contains(colliderComponent) == false) SelectedColliders.Add(colliderComponent);
            }

            OnSelectedColliderChanged(SelectionAction.Select, touchPickable);
            OnSelectedColliderChangedExtended(SelectionAction.Select, touchPickable, isDoubleClick, isLongTap);
        }
    }
}