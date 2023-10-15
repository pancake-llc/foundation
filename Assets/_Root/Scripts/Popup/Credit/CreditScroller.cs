using Pancake.UI;
using PrimeTween;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class CreditScroller : MonoBehaviour
    {
        [SerializeField] private Scrollbar scrollbar;
        [SerializeField, Range(0f, 500f)] private float scrollSpeed;
        [SerializeField] private float initialDelay;
        [SerializeField] private ScrollRect scrollRect;

        private bool _flag;
        private float _tweenValue;
        private bool _autoScroll;
        private bool _touchInside;
        private Tween _tweenScrollBar;
        private Camera _camera;
        private float _currentStart;
        private float _time;
        private float _initialTime;
        private float _percentage;
        private float _easeStart;
        private float _easeInA;
        private float _easeInB;
        private float _mathHelperA;
        private float _mathHelperB;

        private void Awake() { Input.multiTouchEnabled = false; }

        private void Start() { _camera = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER).GetComponentInParent<Canvas>().worldCamera; }

        private void OnEnable()
        {
            _flag = false;

            //Invokes a wait for 0.1 seconds before proceeding to setup.
            //This is a workaround for the content height not being updated before the first frame.

            App.Delay(0.1f, DelayedSetup);
            scrollbar.enabled = true;
            _tweenValue = 1f;
            _autoScroll = true;
        }

        private void OnDisable()
        {
            scrollRect.content.transform.SetPositionY(0);
            _tweenScrollBar.Complete();
            _tweenScrollBar.Stop();
        }

        private void Update()
        {
            if (_flag && _touchInside == false && (scrollRect.velocity.y - 0).Abs() <= 0.1f)
            {
                _flag = false;
                PressReleased();
            }

            if (_autoScroll) scrollbar.value = _tweenValue;

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray))
                {
                    _tweenScrollBar.Stop();
                    _autoScroll = false;
                    _touchInside = true;
                }
                else
                {
                    _touchInside = false;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (_touchInside)
                {
                    _tweenValue = scrollbar.value;
                    _flag = true;
                    _touchInside = false;
                }
            }
#endif

#if UNITY_ANDROID ||UNITY_IOS
            for (var i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Ray ray = _camera.ScreenPointToRay(Input.GetTouch(i).position);

                    if (Physics.Raycast(ray))
                    {
                        _tweenScrollBar.Stop();
                        _autoScroll = false;
                        _touchInside = true;
                    }
                    else
                    {
                        _touchInside = false;
                    }
                }
            }

            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    if (_touchInside)
                    {
                        _tweenValue = scrollbar.value;
                        _flag = true;
                        _touchInside = false;
                    }
                }
            }
#endif
        }

        private void DelayedSetup()
        {
            App.Delay(initialDelay, InitialEaseIn);

            var rtScrollRect = scrollRect.GetComponent<RectTransform>();
            var size = new Vector3(rtScrollRect.rect.width, rtScrollRect.rect.height, 1);
            var box = scrollRect.GetOrAddComponent<BoxCollider>();
            box.size = size;

            var content = scrollRect.content;
            _mathHelperA = content.rect.height - rtScrollRect.rect.height;
            _initialTime = _mathHelperA / scrollSpeed;
            _mathHelperA /= scrollSpeed / 2;
            _mathHelperB = _mathHelperA - 1;
            _easeInA = _mathHelperB / _mathHelperA;
            _easeInB = 1 - _easeInA;

            _time = _initialTime;
            _easeStart = 1;
            _currentStart = 1;
            _percentage = 0;

            var verticalLayout = content.GetComponent<VerticalLayoutGroup>();
            verticalLayout.enabled = false;
            var contentSizeFilter = content.GetComponent<ContentSizeFitter>();
            contentSizeFilter.enabled = false;
        }

        private void InitialEaseIn()
        {
            _tweenValue = _currentStart;
            _tweenScrollBar = Tween.Custom(_tweenValue,
                    _easeInA,
                    1,
                    value => _tweenValue = value,
                    Ease.InQuad)
                .OnComplete(InitialScroll);
        }

        private void InitialScroll()
        {
            _tweenValue = _easeInA;
            _tweenScrollBar = Tween.Custom(_tweenValue,
                    0,
                    _time,
                    value => _tweenValue = value,
                    Ease.Linear)
                .OnComplete(AutoScrollEnd);
        }

        private void PressReleased()
        {
            _currentStart = scrollbar.value;
            _time = _initialTime;
            _percentage = _currentStart;
            _time *= _percentage;
            _easeStart = _currentStart - _easeInB;
            _tweenValue = _currentStart;
            _autoScroll = true;
            if (_tweenValue != 0)
            {
                _tweenScrollBar = Tween.Custom(_tweenValue,
                        _easeStart,
                        1,
                        value => _tweenValue = value,
                        Ease.InQuad)
                    .OnComplete(AutoScroll);
            }
        }

        private void AutoScroll()
        {
            _tweenScrollBar = Tween.Custom(_tweenValue,
                    0,
                    _time,
                    value => _tweenValue = value,
                    Ease.Linear)
                .OnComplete(AutoScrollEnd);
        }

        private void AutoScrollEnd() { _autoScroll = false; }
    }
}