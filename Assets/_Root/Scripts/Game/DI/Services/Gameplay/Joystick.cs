using Pancake.Game.Interfaces;
using UnityEngine.EventSystems;

namespace Pancake.Game
{
    using UnityEngine;

    public abstract class Joystick : GameUnit, IPointerDownHandler, IDragHandler, IPointerUpHandler, IJoystick
    {
        [SerializeField] private float handleRange = 1;
        [SerializeField] private float deadZone;
        [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
        [SerializeField] private bool snapX;
        [SerializeField] private bool snapY;
        [SerializeField] protected RectTransform visual;
        [SerializeField] private RectTransform handle;

        private Vector2 _direction;

        public float Horizontal => snapX ? SnapFloat(_direction.x, AxisOptions.Horizontal) : _direction.x;

        public float Vertical => snapY ? SnapFloat(_direction.y, AxisOptions.Vertical) : _direction.y;

        public Vector2 Direction => new(Horizontal, Vertical);
        public bool Dragging { get; private set; }

        public float HandleRange { get => handleRange; set => handleRange = Mathf.Abs(value); }

        public float DeadZone { get => deadZone; set => deadZone = Mathf.Abs(value); }

        public AxisOptions AxisOptions { get => AxisOptions; set => axisOptions = value; }

        public bool SnapX { get => snapX; set => snapX = value; }

        public bool SnapY { get => snapY; set => snapY = value; }

        private Canvas _canvas;
        private RectTransform _baseRect;
        private Camera _cam;

        private void Start() { Initialize(); }

        public virtual void Initialize()
        {
            HandleRange = handleRange;
            DeadZone = deadZone;
            _baseRect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("The Joystick is not placed inside a canvas");
            }

            var center = new Vector2(0.5f, 0.5f);
            visual.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
            Dragging = false;

            _cam = null;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _cam = _canvas.worldCamera;
            }
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ResetData();
        }

        public virtual void OnPointerDown(PointerEventData eventData) { OnDrag(eventData); }

        public void OnDrag(PointerEventData eventData)
        {
            var position = RectTransformUtility.WorldToScreenPoint(_cam, visual.position);
            var radius = visual.sizeDelta / 2;
            _direction = (eventData.position - position) / (radius * _canvas.scaleFactor);
            FormatInput();
            HandleInput(_direction.magnitude, _direction.normalized, radius, _cam);
            handle.anchoredPosition = _direction * radius * handleRange;
            Dragging = true;
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1) _direction = normalised;
            }
            else
            {
                _direction = Vector2.zero;
            }
        }

        private void FormatInput()
        {
            if (axisOptions == AxisOptions.Horizontal)
            {
                _direction = new Vector2(_direction.x, 0f);
            }
            else if (axisOptions == AxisOptions.Vertical)
            {
                _direction = new Vector2(0f, _direction.y);
            }
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (axisOptions == AxisOptions.Both)
            {
                var angle = Vector2.Angle(_direction, Vector2.up);
                if (snapAxis == AxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f)
                    {
                        return 0;
                    }

                    return value > 0 ? 1 : -1;
                }

                if (snapAxis == AxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f) return 0;

                    return value > 0 ? 1 : -1;
                }

                return value;
            }

            if (value > 0) return 1;

            if (value < 0) return -1;

            return 0;
        }

        public virtual void OnPointerUp(PointerEventData eventData) { ResetData(); }

        private void ResetData()
        {
            _direction = Vector2.zero;

            handle.anchoredPosition = Vector2.zero;
            Dragging = false;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            var localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
            {
                var size = new Vector2(_baseRect.rect.width, _baseRect.rect.height);
                var pivotOffset = _baseRect.pivot * size;
                return localPoint - visual.anchorMax * size + pivotOffset;
            }

            return Vector2.zero;
        }
    }

    public enum AxisOptions
    {
        Both,
        Horizontal,
        Vertical
    }
}