using UnityEngine;
using UnityEngine.EventSystems;

namespace Pancake.Joystick
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float Horizontal => (snapX) ? SnapFloat(_input.x, AxisOptions.Horizontal) : _input.x;
        public float Vertical => (snapY) ? SnapFloat(_input.y, AxisOptions.Vertical) : _input.y;
        public Vector2 Direction => new Vector2(Horizontal, Vertical);

        public float HandleRange { get => handleRange; set => handleRange = Mathf.Abs(value); }

        public float DeadZone { get => deadZone; set => deadZone = Mathf.Abs(value); }

        public AxisOptions AxisOptions { get => AxisOptions; set => axisOptions = value; }
        public bool SnapX { get => snapX; set => snapX = value; }
        public bool SnapY { get => snapY; set => snapY = value; }

        [SerializeField] private float handleRange = 1;
        [SerializeField] private float deadZone;
        [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
        [SerializeField] private bool snapX;
        [SerializeField] private bool snapY;

        [SerializeField] internal RectTransform background;
        [SerializeField] internal RectTransform handle;
        private RectTransform _baseRect;

        private Canvas _canvas;
        private Camera _cam;

        private Vector2 _input = Vector2.zero;

        protected virtual void Start()
        {
            HandleRange = handleRange;
            DeadZone = deadZone;
            _baseRect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null) Debug.LogError("The Joystick is not placed inside a canvas");

            Vector2 center = new Vector2(0.5f, 0.5f);
            background.pivot = center;
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
        }

        public virtual void OnPointerDown(PointerEventData eventData) { OnDrag(eventData); }

        public void OnDrag(PointerEventData eventData)
        {
            _cam = null;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera) _cam = _canvas.worldCamera;

            var position = RectTransformUtility.WorldToScreenPoint(_cam, background.position);
            var radius = background.sizeDelta / 2;
            _input = (eventData.position - position) / (radius * _canvas.scaleFactor);
            FormatInput();
            HandleInput(_input.magnitude, _input.normalized, radius, _cam);
            handle.anchoredPosition = _input * radius * handleRange;
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > deadZone)
            {
                if (magnitude > 1) _input = normalised;
            }
            else _input = Vector2.zero;
        }

        private void FormatInput()
        {
            if (axisOptions == AxisOptions.Horizontal) _input = new Vector2(_input.x, 0f);
            else if (axisOptions == AxisOptions.Vertical) _input = new Vector2(0f, _input.y);
        }

        private float SnapFloat(float value, AxisOptions snapAxis)
        {
            if (value == 0) return value;

            if (axisOptions == AxisOptions.Both)
            {
                float angle = Vector2.Angle(_input, Vector2.up);
                if (snapAxis == AxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f) return 0;
                    return (value > 0) ? 1 : -1;
                }
                else if (snapAxis == AxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f) return 0;
                    return (value > 0) ? 1 : -1;
                }

                return value;
            }

            if (value > 0) return 1;
            if (value < 0) return -1;

            return 0;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out var localPoint))
            {
                var pivotOffset = _baseRect.pivot * _baseRect.sizeDelta;
                return localPoint - (background.anchorMax * _baseRect.sizeDelta) + pivotOffset;
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