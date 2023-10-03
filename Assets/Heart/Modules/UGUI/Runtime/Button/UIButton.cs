using System;
using System.Collections;
using System.Threading;
using Pancake.Sound;
using Pancake.Threading.Tasks;
using Pancake.Tracking;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Pancake.UI
{
    [EditorIcon("script_button")]
    [RequireComponent(typeof(Image))]
    public class UIButton : Button, IButton, IButtonAffect
    {
        private const float DOUBLE_CLICK_TIME_INTERVAL = 0.2f;
        private const float LONG_CLICK_TIME_INTERVAL = 0.5f;

        /// <summary>
        /// Function definition for a button click event.
        /// </summary>
        [Serializable]
        public class ButtonHoldEvent : UnityEngine.Events.UnityEvent<float>
        {
        }

        [Serializable]
        public class MotionData
        {
            public Vector2 scale;
            public EButtonMotion motion = EButtonMotion.Normal;
            public float duration = 0.1f;
            public Ease ease = Ease.Default;
        }

        #region Property

        [SerializeField] private EButtonClickType clickType = EButtonClickType.OnlySingleClick;
        [SerializeField] private bool allowMultipleClick; // if true, button can spam clicked and it not get disabled
        [SerializeField] private float timeDisableButton = DOUBLE_CLICK_TIME_INTERVAL; // time disable button when not multiple click
        [Range(0.05f, 1f)] [SerializeField] private float doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL; // time detected double click
        [Range(0.1f, 10f)] [SerializeField] private float longClickInterval = LONG_CLICK_TIME_INTERVAL; // time detected long click
        [Range(0.2f, 1f)] [SerializeField] private float delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL; // time detected hold
        [SerializeField] private ButtonClickedEvent onDoubleClick = new();
        [SerializeField] private ButtonClickedEvent onLongClick = new();
        [SerializeField] private ButtonClickedEvent onPointerUp = new();
        [SerializeField] private ButtonHoldEvent onHold = new();
        [SerializeField] private bool isMotion = true;
        [SerializeField] private bool ignoreTimeScale;
        [SerializeField] private bool isMotionUnableInteract;
        [SerializeField] private bool isAffectToSelf = true;
        [SerializeField] private Transform affectObject;
        [SerializeField] private bool enabledSound;
        [SerializeField] private Audio audioClick;
        [SerializeField] private ScriptableEventAudio audioPlayEvent;
        [SerializeField] private bool enabledTracking;
        [SerializeField] private ScriptableTrackingNoParam trackingEvent;
        [SerializeField] private MotionData motionData = new MotionData {scale = new Vector2(0.92f, 0.92f), motion = EButtonMotion.Uniform};
        [SerializeField] private MotionData motionDataUnableInteract = new MotionData {scale = new Vector2(1.15f, 1.15f), motion = EButtonMotion.Late};

        private Coroutine _routineLongClick;
        private Coroutine _routineHold;
        private AsyncProcessHandle _handleMultipleClick;
        private bool _clickedOnce; // marked as true after one click. (only check for double click)
        private bool _longClickDone; // marks as true after long click or hold up
        private bool _holdDone;
        private bool _holding;
        private float _doubleClickTimer; // calculate the time interval between two sequential clicks. (use for double click)
        private float _longClickTimer; // calculate how long was the button pressed
        private float _holdTimer; // calculate how long was the button pressed
        private Vector2 _endValue;
        private bool _isCompletePhaseDown;
        private readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
        private Tween _tweenUp;
        private Tween _tweenDown;
        private CancellationTokenSource _tokenSource;

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
            _tokenSource = new CancellationTokenSource();
            DefaultScale = AffectObject.localScale;
            onClick.AddListener(PlaySound);
        }

        private void PlaySound()
        {
            if (enabledSound) audioPlayEvent.Raise(audioClick);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _doubleClickTimer = 0;
            doubleClickInterval = DOUBLE_CLICK_TIME_INTERVAL;
            _longClickTimer = 0;
            longClickInterval = LONG_CLICK_TIME_INTERVAL;
            delayDetectHold = DOUBLE_CLICK_TIME_INTERVAL;
            _clickedOnce = false;
            _longClickDone = false;
            _holdDone = false;
        }
#endif

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_handleMultipleClick != null)
            {
#if UNITY_EDITOR
                // avoid case app be destroy soon than other component
                try
                {
#endif
                    App.StopCoroutine(_handleMultipleClick);
#if UNITY_EDITOR
                }
                catch (Exception)
                {
                    // ignored
                }
#endif
            }

            if (_routineLongClick != null) StopCoroutine(_routineLongClick);
            if (_routineHold != null) StopCoroutine(_routineHold);
            interactable = true;
            _clickedOnce = false;
            _longClickDone = false;
            _holdDone = false;
            _doubleClickTimer = 0;
            _longClickTimer = 0;
            if (AffectObject != null) AffectObject.localScale = DefaultScale;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_tweenUp.isAlive) _tweenUp.Stop();
            if (_tweenDown.isAlive) _tweenDown.Stop();
            _tokenSource?.Cancel();
        }

        #region Overrides of Button

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            IsRelease = false;
            IsPrevent = false;
            if (IsDetectLongCLick && interactable) RegisterLongClick();
            if (IsDetectHold && interactable) RegisterHold();

            RunMotionPointerDown();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsRelease) return;
            base.OnPointerUp(eventData);
            IsRelease = true;
            onPointerUp.Invoke();
            if (IsDetectLongCLick) CancelLongClick();
            if (IsDetectHold)
            {
                if (_holding)
                {
                    _holding = false;
                    _holdDone = true;
                }

                CancelHold();
            }

            RunMotionPointerUp();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsRelease && IsPrevent || !interactable) return;

            StartClick(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsRelease) return;
            base.OnPointerExit(eventData);
            IsPrevent = true;
            if (IsDetectLongCLick) ResetLongClick();
            if (IsDetectHold)
            {
                if (_holding)
                {
                    _holding = false;
                    _holdDone = true;
                }

                ResetHold();
            }

            OnPointerUp(eventData);
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

        private IEnumerator IeDisableButton(float duration)
        {
            interactable = false;
            yield return new WaitForSeconds(duration);
            interactable = true;
        }

        #region single click

        private bool IsDetectSingleClick =>
            clickType is EButtonClickType.OnlySingleClick or EButtonClickType.Instant || clickType == EButtonClickType.LongClick || clickType == EButtonClickType.Hold;

        private void StartClick(PointerEventData eventData)
        {
            if (IsDetectLongCLick && _longClickDone)
            {
                ResetLongClick();
                return;
            }

            if (IsDetectHold && _holdDone)
            {
                ResetHold();
                return;
            }

            if ((clickType == EButtonClickType.OnlySingleClick || clickType == EButtonClickType.Instant) && enabledTracking) trackingEvent.Track();
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
                
                _handleMultipleClick = App.StartCoroutine(IeDisableButton(timeDisableButton));
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
            PlaySound();
            onDoubleClick.Invoke();
        }

        #endregion

        #region long click

        /// <summary>
        /// button is allow long click
        /// </summary>
        /// <returns></returns>
        private bool IsDetectLongCLick => clickType == EButtonClickType.LongClick;

        /// <summary>
        /// waiting check long click done
        /// </summary>
        /// <returns></returns>
        private IEnumerator IeExcuteLongClick()
        {
            while (_longClickTimer < longClickInterval)
            {
                if (ignoreTimeScale) _longClickTimer += Time.unscaledDeltaTime;
                else _longClickTimer += Time.deltaTime;
                yield return _waitForEndOfFrame;
            }

            ExecuteLongClick();
            _longClickDone = true;
        }

        /// <summary>
        /// execute for long click button
        /// </summary>
        private void ExecuteLongClick()
        {
            if (!IsActive() || !IsInteractable() || !IsDetectLongCLick) return;
            PlaySound();
            onLongClick.Invoke();
        }

        /// <summary>
        /// reset
        /// </summary>
        private void ResetLongClick()
        {
            if (!IsDetectLongCLick) return;
            _longClickDone = false;
            _longClickTimer = 0;
            StopCoroutine(_routineLongClick);
        }

        /// <summary>
        /// register
        /// </summary>
        private void RegisterLongClick()
        {
            if (_longClickDone || !IsDetectLongCLick) return;
            ResetLongClick();
            _routineLongClick = StartCoroutine(IeExcuteLongClick());
        }

        /// <summary>
        /// reset long click and stop sheduler wait detect long click
        /// </summary>
        private void CancelLongClick()
        {
            if (_longClickDone || !IsDetectLongCLick) return;
            ResetLongClick();
        }

        #endregion

        #region hold

        /// <summary>
        /// button is allow hold
        /// </summary>
        /// <returns></returns>
        private bool IsDetectHold => clickType == EButtonClickType.Hold;

        /// <summary>
        /// update hold
        /// </summary>
        /// <returns></returns>
        private IEnumerator IeExcuteHold()
        {
            _holding = false;
            yield return new WaitForSeconds(delayDetectHold);
            _holding = true;
            while (true)
            {
                if (ignoreTimeScale) _holdTimer += Time.unscaledDeltaTime;
                else _holdTimer += Time.deltaTime;
                ExecuteHold(_holdTimer);
                yield return _waitForEndOfFrame;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        /// <summary>
        /// execute hold
        /// </summary>
        private void ExecuteHold(float time)
        {
            if (!IsActive() || !IsInteractable() || !IsDetectHold) return;
            onHold.Invoke(time);
        }

        /// <summary>
        /// reset
        /// </summary>
        private void ResetHold()
        {
            if (!IsDetectHold) return;
            _holdDone = false;
            _holdTimer = 0;
            StopCoroutine(_routineHold);
        }

        /// <summary>
        /// register
        /// </summary>
        private void RegisterHold()
        {
            if (_holdDone || !IsDetectHold) return;
            ResetHold();
            _routineHold = StartCoroutine(IeExcuteHold());
        }

        /// <summary>
        /// reset hold and stop sheduler wait detect hold
        /// </summary>
        private void CancelHold()
        {
            if (_holdDone || !IsDetectHold) return;
            ResetHold();
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
                    try
                    {
                        await UniTask.WaitUntil(() => _isCompletePhaseDown, cancellationToken: _tokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    _tweenUp = Tween.Scale(AffectObject, DefaultScale, motionData.duration, motionData.ease);
                    break;
                case EButtonMotion.Uniform:
                    break;
                case EButtonMotion.Late:
                    _isCompletePhaseDown = false;
                    _endValue = new Vector3(DefaultScale.x * motionData.scale.x, DefaultScale.y * motionData.scale.y);
                    _tweenDown = Tween.Scale(AffectObject, _endValue, motionData.duration, motionData.ease).OnComplete(() => _isCompletePhaseDown = true);

                    try
                    {
                        await UniTask.WaitUntil(() => _isCompletePhaseDown, cancellationToken: _tokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    _tweenUp = Tween.Scale(AffectObject, DefaultScale, motionData.duration, motionData.ease);

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
                    _tweenDown = Tween.Scale(AffectObject, _endValue, motionData.duration, motionData.ease).OnComplete(() => _isCompletePhaseDown = true);
                    break;
                case EButtonMotion.Uniform:
                    _isCompletePhaseDown = false;
                    _tweenDown = Tween.Scale(AffectObject, _endValue, motionData.duration, motionData.ease).OnComplete(() => _isCompletePhaseDown = true);
                    try
                    {
                        await UniTask.WaitUntil(() => _isCompletePhaseDown, cancellationToken: _tokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    _tweenUp = Tween.Scale(AffectObject, DefaultScale, motionData.duration, motionData.ease);
                    break;
                case EButtonMotion.Late:
                    break;
            }
        }

        #endregion

        #endregion
    }
}