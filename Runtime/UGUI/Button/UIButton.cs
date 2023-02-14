using System;
using System.Collections;
using System.Collections.Generic;
using Pancake.Threading.Tasks;
using Pancake.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Pancake.UI
{
    [RequireComponent(typeof(Image))]
    public class UIButton : Button, IButton, IButtonAffect
    {
        private const float DOUBLE_CLICK_TIME_INTERVAL = 0.2f;
        private const float LONG_CLICK_TIME_INTERVAL = 0.5f;
        
        /// <summary>
        /// Function definition for a button click event.
        /// </summary>
        [Serializable]
        public class ButtonHoldEvent : UnityEngine.Events.UnityEvent<float> { }

        [Serializable]
        public class MotionData
        {
            public Vector2 scale;
            public EButtonMotion motion = EButtonMotion.Normal;
            public float durationDown = 0.1f;
            public float durationUp = 0.1f;
            public Interpolator interpolatorDown;
            public Interpolator interpolatorUp;
        }

        #region Property

        [SerializeField] private EButtonClickType clickType = EButtonClickType.OnlySingleClick;
        [SerializeField] private bool allowMultipleClick = true; // if true, button can spam clicked and it not get disabled
        [SerializeField] private float timeDisableButton = DOUBLE_CLICK_TIME_INTERVAL; // time disable button when not multiple click
        [Range(0.05f, 1f)] [SerializeField] private float doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL; // time detected double click
        [Range(0.1f, 10f)] [SerializeField] private float longClickInterval = LONG_CLICK_TIME_INTERVAL; // time detected long click
        [Range(0.2f, 1f)] [SerializeField] private float delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL; // time detected hold
        [SerializeField] private ButtonClickedEvent onDoubleClick = new();
        [SerializeField] private ButtonClickedEvent onLongClick = new();
        [SerializeField] private ButtonClickedEvent onPointerUp = new();
        [SerializeField] private ButtonHoldEvent onHold = new();
        [SerializeField] private bool isMotion;
        [SerializeField] private bool ignoreTimeScale;
        [SerializeField] private bool isMotionUnableInteract;
        [SerializeField] private bool isAffectToSelf = true;
        [SerializeField] private Transform affectObject;
        [SerializeField] private MotionData motionData = new MotionData {scale = new Vector2(0.92f, 0.92f), motion = EButtonMotion.Uniform};
        [SerializeField] private MotionData motionDataUnableInteract = new MotionData {scale = new Vector2(1.15f, 1.15f), motion = EButtonMotion.Late};

        private CoroutineHandle _routineHold;
        private CoroutineHandle _routineMultiple;
        private bool _clickedOnce; // marked as true after one click. (only check for double click)
        private bool _holdDone; // marks as true after long click or hold up
        private bool _holding;
        private float _doubleClickTimer; // calculate the time interval between two sequential clicks. (use for double click)
        private float _holdTimer; // calculate how long was the button pressed
        private Vector2 _endValue;
        private bool _isCompletePhaseUp;
        private bool _isCompletePhaseDown;

        #endregion

        #region Implementation of IButton

#if UNITY_EDITOR
        /// <summary>
        /// Editor only
        /// </summary>
        public bool IsMotion { get => isMotion; set => isMotion = value; }
#endif

        /// <summary>
        /// is release only set true when OnPointerUp called
        /// </summary>
        private bool IsRelease { get; set; } = true;

        /// <summary>
        /// make sure OnPointerClick is called on the condition of IsRelease, only set true when OnPointerExit called
        /// </summary>
        private bool IsPrevent { get; set; }

        #endregion

        #region Implementation of IAffect

        public Vector3 DefaultScale { get; set; }
        public bool IsAffectToSelf => isAffectToSelf;

        public Transform AffectObject => IsAffectToSelf ? targetGraphic.rectTransform : affectObject;

        #endregion

        #region Overrides of UIBehaviour

        protected override void Awake()
        {
            base.Awake();
            DefaultScale = AffectObject.localScale;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _doubleClickTimer = 0;
            doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL;
            _holdTimer = 0;
            longClickInterval = LONG_CLICK_TIME_INTERVAL;
            delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL;
            _clickedOnce = false;
            _holdDone = false;
            _holding = false;
        }
#endif

        protected override void OnDisable()
        {
            base.OnDisable();
            Timing.KillCoroutines(_routineMultiple);
            Timing.KillCoroutines(_routineHold);
            interactable = true;
            _clickedOnce = false;
            _holdDone = false;
            _holding = false;
            _doubleClickTimer = 0;
            _holdTimer = 0;
            if (AffectObject != null) AffectObject.localScale = DefaultScale;
        }

        #region Overrides of Selectable

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsRelease) return;
            base.OnPointerExit(eventData);
            IsPrevent = true;
            OnPointerUp(eventData);
        }

        #endregion

        #region Overrides of Button

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            IsRelease = false;
            IsPrevent = false;
            if (IsDetectHold && interactable) RegisterHold();

            RunMotionPointerDown();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsRelease) return;
            base.OnPointerUp(eventData);
            IsRelease = true;
            onPointerUp.Invoke();
            if (IsDetectHold) CancelHold();

            RunMotionPointerUp();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsRelease && IsPrevent || !interactable) return;

            StartClick(eventData);
        }

        /// <summary>
        /// run motion when pointer down.
        /// </summary>
        private void RunMotionPointerDown()
        {
            if (!isMotion) return;
            if (!interactable && isMotionUnableInteract)
            {
                MotionDown(motionDataUnableInteract);
                return;
            }

            MotionDown(motionData);
        }

        /// <summary>
        /// run motion when pointer up.
        /// </summary>
        private void RunMotionPointerUp()
        {
            if (!isMotion) return;
            if (!interactable && isMotionUnableInteract)
            {
                MotionUp(motionDataUnableInteract);
                return;
            }

            MotionUp(motionData);
        }

        private IEnumerator<float> IeDisableButton(float duration)
        {
            interactable = false;
            yield return Timing.WaitForSeconds(duration);
            interactable = true;
        }

        #region single click

        private bool IsDetectSingleClick => clickType is EButtonClickType.OnlySingleClick or EButtonClickType.Instant || clickType == EButtonClickType.LongClick || clickType == EButtonClickType.Hold;

        private void StartClick(PointerEventData eventData)
        {
            if (IsDetectHold && _holdDone)
            {
                ResetHold();
                return;
            }

            StartCoroutine(IeExecute(eventData));
        }

        /// <summary>
        /// execute for click button
        /// </summary>
        /// <returns></returns>
        private IEnumerator IeExecute(PointerEventData eventData)
        {
            if (IsDetectSingleClick) base.OnPointerClick(eventData);
            if (!allowMultipleClick && clickType == EButtonClickType.OnlySingleClick)
            {
                if (!interactable) yield break;

                _routineMultiple = Timing.RunCoroutine(IeDisableButton(timeDisableButton));
                yield break;
            }

            if (clickType == EButtonClickType.OnlySingleClick || clickType == EButtonClickType.LongClick || clickType == EButtonClickType.Hold) yield break;

            if (!_clickedOnce && _doubleClickTimer < doubleClickInterval)
            {
                _clickedOnce = true;
            }
            else
            {
                _clickedOnce = false;
                yield break;
            }

            yield return null;

            while (_doubleClickTimer < doubleClickInterval)
            {
                if (!_clickedOnce)
                {
                    ExecuteDoubleClick();
                    _doubleClickTimer = 0;
                    _clickedOnce = false;
                    yield break;
                }

                if (ignoreTimeScale) _doubleClickTimer += Time.unscaledDeltaTime;
                else _doubleClickTimer += Time.deltaTime;
                yield return null;
            }

            if (clickType == EButtonClickType.Delayed) base.OnPointerClick(eventData);

            _doubleClickTimer = 0;
            _clickedOnce = false;
        }

        #endregion

        #region double click

        private bool IsDetectDoubleClick =>
            clickType == EButtonClickType.OnlyDoubleClick || clickType == EButtonClickType.Instant || clickType == EButtonClickType.Delayed;

        /// <summary>
        /// execute for double click button
        /// </summary>
        private void ExecuteDoubleClick()
        {
            if (!IsActive() || !IsInteractable() || !IsDetectDoubleClick) return;
            onDoubleClick.Invoke();
        }

        #endregion

        #region LongClick

        /// <summary>
        /// button is allow hold
        /// </summary>
        /// <returns></returns>
        private bool IsDetectHold => clickType == EButtonClickType.LongClick || clickType == EButtonClickType.Hold;

        /// <summary>
        /// waiting check long click done
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> IeExcuteLongClick()
        {
            while (_holdTimer < longClickInterval)
            {
                if (ignoreTimeScale) _holdTimer += Time.unscaledDeltaTime;
                else _holdTimer += Time.deltaTime;
                yield return Timing.WaitForOneFrame;
            }

            ExecuteLongClick();
            _holdDone = true;
        }

        /// <summary>
        /// waiting check long click done
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> IeExcuteHold()
        {
            yield return Timing.WaitForSeconds(delayDetectHold);
            _holding = true;
            while (true)
            {
                if (ignoreTimeScale) _holdTimer += Time.unscaledDeltaTime;
                else _holdTimer += Time.deltaTime;
                ExecuteHold(_holdTimer);
                yield return Timing.WaitForOneFrame;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        /// <summary>
        /// execute for long click button
        /// </summary>
        private void ExecuteHold(float time)
        {
            if (!IsActive() || !IsInteractable() || !IsDetectHold) return;
            onHold.Invoke(time);
        }

        /// <summary>
        /// execute for long click button
        /// </summary>
        private void ExecuteLongClick()
        {
            if (!IsActive() || !IsInteractable() || !IsDetectHold) return;
            onLongClick.Invoke();
        }

        /// <summary>
        /// reset
        /// </summary>
        private void ResetHold(bool isDone = false)
        {
            if (!IsDetectHold) return;
            _holdTimer = 0;
            _holdDone = false;
            Timing.KillCoroutines(_routineHold);
        }

        /// <summary>
        /// register
        /// </summary>
        private void RegisterHold()
        {
            _holding = false;
            if (_holdDone || !IsDetectHold) return;
            ResetHold();
            _routineHold = clickType switch
            {
                EButtonClickType.LongClick => Timing.RunCoroutine(IeExcuteLongClick()),
                EButtonClickType.Hold => Timing.RunCoroutine(IeExcuteHold()),
                _ => _routineHold
            };
        }

        /// <summary>
        /// reset long click and stop sheduler wait detect long click
        /// </summary>
        private void CancelHold()
        {
            if (_holdDone || !IsDetectHold) return;
            if (_holding) ResetHold(true);
            else ResetHold();
        }

        #endregion

        #endregion

        #region Motion

        public async void MotionUp(MotionData data)
        {
            _endValue = DefaultScale;
            switch (data.motion)
            {
                case EButtonMotion.Immediate:
                    AffectObject.localScale = DefaultScale;
                    break;
                case EButtonMotion.Normal:
                    _isCompletePhaseUp = false;
                    AffectObject.TweenLocalScale(DefaultScale, motionData.durationUp)
                        .SetEase(motionData.interpolatorUp)
                        .OnComplete(() => _isCompletePhaseUp = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseUp);
                    break;
                case EButtonMotion.Uniform:
                    break;
                case EButtonMotion.Late:
                    _isCompletePhaseUp = false;
                    _isCompletePhaseDown = false;
                    _endValue = new Vector3(DefaultScale.x * motionData.scale.x, DefaultScale.y * motionData.scale.y);

                    AffectObject.TweenLocalScale(_endValue, motionData.durationDown)
                        .SetEase(motionData.interpolatorDown)
                        .OnComplete(() => _isCompletePhaseDown = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseDown);

                    AffectObject.TweenLocalScale(DefaultScale, motionData.durationUp)
                        .SetEase(motionData.interpolatorUp)
                        .OnComplete(() => _isCompletePhaseUp = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseUp);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public async void MotionDown(MotionData data)
        {
            _endValue = new Vector3(DefaultScale.x * motionData.scale.x, DefaultScale.y * motionData.scale.y);
            switch (data.motion)
            {
                case EButtonMotion.Immediate:
                    AffectObject.localScale = _endValue;
                    break;
                case EButtonMotion.Normal:
                    _isCompletePhaseDown = false;
                    AffectObject.TweenLocalScale(_endValue, motionData.durationDown)
                        .SetEase(motionData.interpolatorDown)
                        .OnComplete(() => _isCompletePhaseDown = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseDown);
                    break;
                case EButtonMotion.Uniform:
                    _isCompletePhaseUp = false;
                    _isCompletePhaseDown = false;
                    AffectObject.TweenLocalScale(_endValue, motionData.durationDown)
                        .SetEase(motionData.interpolatorDown)
                        .OnComplete(() => _isCompletePhaseDown = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseDown);

                    AffectObject.TweenLocalScale(DefaultScale, motionData.durationUp)
                        .SetEase(motionData.interpolatorUp)
                        .OnComplete(() => _isCompletePhaseUp = true)
                        .Play();
                    await UniTask.WaitUntil(() => _isCompletePhaseUp);
                    break;
                case EButtonMotion.Late:
                    break;
            }
        }

        #endregion

        #endregion
    }
}