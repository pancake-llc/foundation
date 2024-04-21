using System;
using UnityEngine;
#if PANCAKE_LITMOTION
using LitMotion;
#endif

namespace Pancake.DebugView
{
    [ExecuteAlways]
    public class Drawer : MonoBehaviour
    {
        [SerializeField] private DrawerDirection direction;

        [SerializeField] [Range(0.0f, 1.0f)] [Tooltip("Size of the drawer. When 1, it is displayed on the full screen.")]
        private float size = 0.5f;

        [SerializeField]
        [Tooltip("If true, the drawer will be located at the edge of the Safe Area when progress is zero. " + "Otherwise, it will be located at the edge of the screen.")]
        private bool moveInsideSafeArea;

        [SerializeField] private bool openOnStart;

        protected Canvas canvas;
        private bool _isProgressDirty;
        private bool _isTransformDirty;
        private Vector2 _lastFullSize;
        private float _progress = 1.0f;
#if PANCAKE_LITMOTION
        private MotionHandle _handle;
#endif

        public DrawerDirection Direction
        {
            get => direction;
            set
            {
                direction = value;
                _isTransformDirty = true;
            }
        }

        /// <summary>
        ///     Size of the drawer. When 1, it is displayed on the full screen.
        /// </summary>
        public float Size
        {
            get => size;
            set
            {
                size = value;
                _isTransformDirty = true;
            }
        }

        public bool MoveInsideSafeArea
        {
            get => moveInsideSafeArea;
            set
            {
                moveInsideSafeArea = value;
                _isTransformDirty = true;
            }
        }

        public bool OpenOnStart { get => openOnStart; set => openOnStart = value; }

        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                _isProgressDirty = true;
            }
        }

        public bool IsInAnimation
        {
            get
            {
#if PANCAKE_LITMOTION
                return _handle.IsActive();
#else
                return false;
#endif
            }
        }

        protected virtual void Start()
        {
            if (Application.isPlaying)
                Progress = openOnStart ? 1.0f : 0.0f;
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            else
            {
                if (canvas == null)
                    canvas = GetComponentInParent<Canvas>();
            }

            var scaleFactor = canvas.scaleFactor;
            var fullSize = new Vector2(Screen.width, Screen.height) / scaleFactor;
            var safeArea = Screen.safeArea;
            safeArea.position /= scaleFactor;
            safeArea.size /= scaleFactor;

            // Update Transform because the screen size or canvas scale has been changed.
            if (_lastFullSize != fullSize)
            {
                _isTransformDirty = true;
                _lastFullSize = fullSize;
            }

            if (!Application.isPlaying)
            {
                // In EditMode, update in every frame.
                UpdateTransform(fullSize, safeArea);
                UpdateProgress(fullSize, safeArea);
            }
            else
            {
                // In PlayMode, update as needed.
                if (_isTransformDirty)
                {
                    UpdateTransform(fullSize, safeArea);
                    UpdateProgress(fullSize, safeArea);
                    ProgressUpdated?.Invoke(Progress);
                    _isTransformDirty = false;
                    _isProgressDirty = false;
                }

                if (_isProgressDirty)
                {
                    UpdateProgress(fullSize, safeArea);
                    ProgressUpdated?.Invoke(Progress);
                    _isProgressDirty = false;
                }
            }
        }

        protected virtual void OnEnable() { _isTransformDirty = true; }

        public event Action<float> ProgressUpdated;

        public void PlayProgressAnimation(float toProgress, float durationSec, Ease ease, Action completed = null)
        {
            PlayProgressAnimation(Progress,
                toProgress,
                durationSec,
                ease,
                completed);
        }

        public void PlayProgressAnimation(float fromProgress, float toProgress, float durationSec, Ease ease, Action completed = null)
        {
            if (IsInAnimation) throw new Exception("Progress Animation is now playing. If you want to play new animation, call StopProgressAnimation first.");

#if PANCAKE_LITMOTION
            LMotion.Create(fromProgress, toProgress, durationSec).WithEase(ease).WithOnComplete(() => completed?.Invoke()).Bind(ValueChanged);
#endif
            return;

            void ValueChanged(float value) { Progress = value; }
        }

        public void StopProgressAnimation()
        {
#if PANCAKE_LITMOTION
            if (_handle.IsActive()) _handle.Cancel();
#endif
        }

        public float GetProgressFromDistance(float distance)
        {
            float scaleFactor = canvas.scaleFactor;
            var fullSize = new Vector2(Screen.width, Screen.height) / scaleFactor;
            var safeArea = Screen.safeArea;
            safeArea.position /= scaleFactor;
            safeArea.size /= scaleFactor;
            float minPos = GetStartPos(fullSize, safeArea, direction, moveInsideSafeArea);
            float maxPos = GetEndPos(fullSize, safeArea, direction);
            float length = Mathf.Abs(maxPos - minPos);
            return distance / length;
        }

        private void UpdateTransform(Vector2 fullSize, Rect safeArea)
        {
            var rectTransform = (RectTransform) transform;
            rectTransform.anchorMin = GetAnchorMin(direction, rectTransform.anchorMin);
            rectTransform.anchorMax = GetAnchorMax(direction, rectTransform.anchorMax);
            rectTransform.pivot = GetPivot(direction, rectTransform.pivot);
            rectTransform.sizeDelta = GetSizeDelta(rectTransform.sizeDelta,
                fullSize,
                safeArea,
                size,
                moveInsideSafeArea,
                direction);
        }

        private void UpdateProgress(Vector2 fullSize, Rect safeArea)
        {
            var rectTransform = (RectTransform) transform;
            float minPos = GetStartPos(fullSize, safeArea, direction, moveInsideSafeArea);
            float maxPos = GetEndPos(fullSize, safeArea, direction);
            float normalizedSize = Mathf.Lerp(0.0f, size, _progress);
            var anchoredPosition = rectTransform.anchoredPosition;
            if (direction == DrawerDirection.LeftToRight || direction == DrawerDirection.RightToLeft)
                anchoredPosition.x = Mathf.Lerp(minPos, maxPos, normalizedSize);
            else
                anchoredPosition.y = Mathf.Lerp(minPos, maxPos, normalizedSize);
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static Vector2 GetAnchorMin(DrawerDirection direction, Vector2 source)
        {
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    source.x = 0.0f;
                    break;
                case DrawerDirection.RightToLeft:
                    source.x = 1.0f;
                    break;
                case DrawerDirection.BottomToTop:
                    source.y = 0.0f;
                    break;
                case DrawerDirection.TopToBottom:
                    source.y = 1.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return source;
        }

        private static Vector2 GetAnchorMax(DrawerDirection direction, Vector2 source)
        {
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    source.x = 0.0f;
                    break;
                case DrawerDirection.RightToLeft:
                    source.x = 1.0f;
                    break;
                case DrawerDirection.BottomToTop:
                    source.y = 0.0f;
                    break;
                case DrawerDirection.TopToBottom:
                    source.y = 1.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return source;
        }

        private static Vector2 GetPivot(DrawerDirection direction, Vector2 source)
        {
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    source.x = 1.0f;
                    break;
                case DrawerDirection.RightToLeft:
                    source.x = 0.0f;
                    break;
                case DrawerDirection.BottomToTop:
                    source.y = 1.0f;
                    break;
                case DrawerDirection.TopToBottom:
                    source.y = 0.0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return source;
        }

        private static Vector2 GetSizeDelta(Vector2 source, Vector2 fullAreaSize, Rect safeArea, float drawerScale, bool moveInsideSafeArea, DrawerDirection direction)
        {
            float maxSize;
            float minSize;
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    minSize = moveInsideSafeArea ? safeArea.xMin : 0.0f;
                    maxSize = safeArea.xMax;
                    break;
                case DrawerDirection.RightToLeft:
                    minSize = moveInsideSafeArea ? fullAreaSize.x - safeArea.xMax : 0.0f;
                    maxSize = fullAreaSize.x - safeArea.xMin;
                    break;
                case DrawerDirection.BottomToTop:
                    minSize = moveInsideSafeArea ? safeArea.yMin : 0.0f;
                    maxSize = safeArea.yMax;
                    break;
                case DrawerDirection.TopToBottom:
                    minSize = moveInsideSafeArea ? fullAreaSize.y - safeArea.yMax : 0.0f;
                    maxSize = fullAreaSize.y - safeArea.yMin;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var value = Mathf.Lerp(minSize, maxSize, drawerScale);
            if (direction == DrawerDirection.LeftToRight || direction == DrawerDirection.RightToLeft)
                source.x = value;
            else
                source.y = value;

            return source;
        }

        private static float GetStartPos(Vector2 fullSize, Rect safeArea, DrawerDirection direction, bool insideSafeArea)
        {
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    return insideSafeArea ? safeArea.xMin : 0.0f;
                case DrawerDirection.RightToLeft:
                    return insideSafeArea ? (fullSize.x - safeArea.width - safeArea.xMin) * -1.0f : 0.0f;
                case DrawerDirection.BottomToTop:
                    return insideSafeArea ? safeArea.yMin : 0.0f;
                case DrawerDirection.TopToBottom:
                    return insideSafeArea ? (fullSize.y - safeArea.height - safeArea.yMin) * -1.0f : 0.0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private static float GetEndPos(Vector2 fullSize, Rect localSafeArea, DrawerDirection direction)
        {
            switch (direction)
            {
                case DrawerDirection.LeftToRight:
                    return localSafeArea.xMax;
                case DrawerDirection.RightToLeft:
                    return (fullSize.x - localSafeArea.xMin) * -1.0f;
                case DrawerDirection.BottomToTop:
                    return localSafeArea.yMax;
                case DrawerDirection.TopToBottom:
                    return (fullSize.y - localSafeArea.yMin) * -1.0f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}