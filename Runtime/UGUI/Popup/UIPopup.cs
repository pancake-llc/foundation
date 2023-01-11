#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Tween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [DeclareFoldoutGroup("basic", Title = "BASIC")]
    [DeclareFoldoutGroup("event", Title = "EVENT")]
    [DeclareFoldoutGroup("close", Title = "CLOSE")]
    [DeclareFoldoutGroup("display", Title = "DISPLAY")]
    [DeclareHorizontalGroup("display/button")]
    [DeclareFoldoutGroup("hide", Title = "HIDE")]
    [DeclareHorizontalGroup("hide/button")]
    public abstract class UIPopup : MonoBehaviour, IPopup
    {
        #region implementation

        #region prop

        #region basic

        [SerializeField, Group("basic"), Ulid, PropertyOrder(0)] private string uniqueId;
        [SerializeField, Group("basic")] protected bool ignoreTimeScale;

        #endregion

        #region event

        [SerializeField, Group("event"), PropertyOrder(0)] protected UnityEvent onBeforeShow;
        [SerializeField, Group("event")] protected UnityEvent onAfterShow;
        [SerializeField, Group("event")] protected UnityEvent onBeforeClose;
        [SerializeField, Group("event")] protected UnityEvent onAfterClose;

        #endregion

        #region close

        [SerializeField, Group("close"), OnValueChanged(nameof(OnCloseByClickContainerChanged)), LabelText("By Click Container"), PropertyOrder(0)]
        protected bool closeByClickContainer;

        [SerializeField, Group("close"), OnValueChanged(nameof(OnCloseByClickBackgroundChanged)), LabelText("By Click Background")]
        protected bool closeByClickBackground;

        [SerializeField, Group("close"), LabelText("By Back Button")]
        protected bool closeByBackButton;

        [SerializeField, Group("close")] private List<Button> closeButtons = new List<Button>();

        #endregion

        #region display

        [SerializeField, Group("display"), LabelText("Type"), PropertyOrder(1)]
        protected EMotionAffect motionAffectDisplay = EMotionAffect.Scale;

        [SerializeField, Group("display"), LabelText("Scale"), HideIf(nameof(motionAffectDisplay), EMotionAffect.Position)]
        protected Vector2 endValueDisplay = Vector2.one;

        [Button("Save From"), ShowIf(nameof(ConditionDisplayPosition)), Group("display/button"), PropertyOrder(2)]
        private void SaveFromPosition()
        {
            positionFromDisplay = ContainerTransform.localPosition;
            ContainerTransform.localPosition = Vector3.zero;
        }

        [Button("Save To"), ShowIf(nameof(ConditionDisplayPosition)), Group("display/button"), PropertyOrder(2)]
        private void SaveToPosition()
        {
            positionToDisplay = ContainerTransform.localPosition;
            ContainerTransform.localPosition = Vector3.zero;
        }

        [Button("Clear"), ShowIf(nameof(ConditionDisplayPosition)), Group("display/button"), PropertyOrder(2)]
        private void ClearPosition()
        {
            positionToDisplay = Vector2.zero;
            positionFromDisplay = Vector2.zero;
            ContainerTransform.localPosition = Vector3.zero;
        }

        [SerializeField, Group("display"), ShowIf(nameof(ConditionDisplayPosition)), ReadOnly]
        protected Vector2 positionToDisplay;

        [SerializeField, Group("display"), ShowIf(nameof(ConditionDisplayPosition)), ReadOnly]
        protected Vector2 positionFromDisplay;

        [SerializeField, Group("display"), LabelText("Duration")] [Range(0.01f, 3f)]
        protected float durationDisplay = 0.25f;

        [SerializeField, Group("display"), LabelText("Interpolate")] protected Interpolator interpolatorDisplay;

        #endregion

        #region hide

        [SerializeField, Group("hide"), LabelText("Type"), PropertyOrder(2)]
        protected EMotionAffect motionAffectHide = EMotionAffect.Scale;

        [Button("Save To"), ShowIf(nameof(ConditionHidePosition)), Group("hide/button"), PropertyOrder(3)]
        private void SaveHideToPosition()
        {
            positionToHide = ContainerTransform.localPosition;
            ContainerTransform.localPosition = Vector3.zero;
        }

        [Button("Clear"), ShowIf(nameof(ConditionHidePosition)), Group("hide/button"), PropertyOrder(3)]
        private void ClearHidePosition()
        {
            positionToHide = Vector2.zero;
            ContainerTransform.localPosition = Vector3.zero;
        }

        [SerializeField, Group("hide"), LabelText("Scale"), HideIf(nameof(motionAffectHide), EMotionAffect.Position)]
        protected Vector2 endValueHide = Vector2.zero;

        [SerializeField, Group("hide"), ShowIf(nameof(ConditionHidePosition)), ReadOnly]
        protected Vector2 positionToHide;

        [SerializeField, Group("hide"), LabelText("Duration")] [Range(0.01f, 3f)]
        protected float durationHide = 0.25f;

        [SerializeField, Group("hide"), LabelText("Interpolate")] protected Interpolator interpolatorHide;

        #endregion

        #endregion


        private Vector2 _startScale;
        private Canvas _canvas;
        private GraphicRaycaster _graphicRaycaster;
        private RectTransform _backgroundTransform;
        private CanvasGroup _backgroundCanvasGroup;
        private RectTransform _containerTransform;
        private CanvasGroup _containerCanvasGroup;
        private bool _canActuallyClose;
        private Vector3 _defaultContainerScale;
        private CancellationTokenSource _tokenSourceCheckPressButton;

        public string Id => uniqueId;
        public GameObject GameObject => gameObject;
        public bool BackButtonPressed { get; private set; }

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null) _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }

        public bool CloseByBackButton => closeByBackButton;
        public bool CloseByClickBackground => closeByClickBackground;
        public bool CloseByClickContainer => closeByClickContainer;

        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (_graphicRaycaster == null) _graphicRaycaster = GetComponent<GraphicRaycaster>();
                return _graphicRaycaster;
            }
        }

        public RectTransform BackgroundTransform
        {
            get
            {
                if (_backgroundTransform == null) _backgroundTransform = transform.Find("Background").GetComponent<RectTransform>();
                return _backgroundTransform;
            }
        }

        public CanvasGroup BackgroundCanvasGroup
        {
            get
            {
                if (_backgroundCanvasGroup == null) _backgroundCanvasGroup = transform.Find("Background").GetComponent<CanvasGroup>();
                return _backgroundCanvasGroup;
            }
        }

        public RectTransform ContainerTransform
        {
            get
            {
                if (_containerTransform == null) _containerTransform = transform.Find("Container").GetComponent<RectTransform>();
                return _containerTransform;
            }
        }

        public CanvasGroup ContainerCanvasGroup
        {
            get
            {
                if (_containerCanvasGroup == null) _containerCanvasGroup = transform.Find("Container").GetComponent<CanvasGroup>();
                return _containerCanvasGroup;
            }
        }

        public bool Active { get; protected set; }
        public CancellationTokenSource TokenSourceCheckPressButton => _tokenSourceCheckPressButton ?? (_tokenSourceCheckPressButton = new CancellationTokenSource());

        private void OnCloseByClickBackgroundChanged()
        {
            BackgroundTransform.TryGetComponent<Button>(out var btn);

            if (CloseByClickBackground)
            {
                if (btn == null) btn = BackgroundTransform.gameObject.AddBlankButtonComponent();
                if (!closeButtons.Contains(btn)) closeButtons.Add(btn);
            }
            else
            {
                if (btn == null) return;
                DestroyImmediate(btn);
                closeButtons?.Remove(btn);
            }
        }

        private void OnCloseByClickContainerChanged()
        {
            ContainerTransform.TryGetComponent<Button>(out var btn);

            if (CloseByClickContainer)
            {
                if (btn == null) btn = ContainerTransform.gameObject.AddBlankButtonComponent();
                if (!closeButtons.Contains(btn)) closeButtons.Add(btn);
            }
            else
            {
                if (btn == null) return;
                DestroyImmediate(btn);
                closeButtons?.Remove(btn);
            }
        }

        private bool ConditionDisplayPosition => motionAffectDisplay == EMotionAffect.Position || motionAffectDisplay == EMotionAffect.PositionAndScale;
        private bool ConditionHidePosition => motionAffectHide == EMotionAffect.Position || motionAffectHide == EMotionAffect.PositionAndScale;

        private void Awake() { _defaultContainerScale = ContainerTransform.localScale; }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) BackButtonPressed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async void Show(CancellationToken token = default)
        {
            var btns = GetClosePopupButtons();
            OnBeforeShow();
            ActivePopup();
            MotionDisplay();

            using (_tokenSourceCheckPressButton = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                try
                {
                    var linkedToken = TokenSourceCheckPressButton.Token;
                    var buttonTask = PopupHelper.SelectButton(linkedToken, btns);
                    Task finishedTask;
                    if (closeByBackButton)
                    {
                        var pressBackButtonTask = PopupHelper.WaitForPressBackButton(this, linkedToken);
                        //_tokenSourceCheckPressButton.Token.ThrowIfCancellationRequested();
                        finishedTask = await Task.WhenAny(buttonTask, pressBackButtonTask);
                    }
                    else
                    {
                        //_tokenSourceCheckPressButton.Token.ThrowIfCancellationRequested();
                        finishedTask = await Task.WhenAny(buttonTask);
                    }

                    await finishedTask; // Propagate exception if the task finished because of exceptio
                    if (_tokenSourceCheckPressButton != null && !_tokenSourceCheckPressButton.IsCancellationRequested) _tokenSourceCheckPressButton.Cancel();
                }
                finally
                {
                    _tokenSourceCheckPressButton?.Dispose();
                    if (Application.isPlaying) Popup.Close();
                }
            }

            OnAfterShow();
        }

        /// <summary>
        /// get close button
        /// </summary>
        /// <returns></returns>
        protected virtual Button[] GetClosePopupButtons() { return closeButtons.ToArray(); }

        /// <summary>
        /// close popup
        /// </summary>
        public virtual void Close()
        {
            OnBeforeClose();
            MotionHide();
            ActuallyClose();
        }

        protected virtual async void ActuallyClose()
        {
            while (!_canActuallyClose)
            {
                await Task.Yield();
            }

            DeActivePopup();
            OnAfterClose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sortingOrder"></param>
        public virtual void UpdateSortingOrder(int sortingOrder) { Canvas.sortingOrder = sortingOrder; }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Refresh() { }

        /// <summary>
        /// 
        /// </summary>
        public void ActivePopup()
        {
            Active = true;
            gameObject.SetActive(true);
            BackButtonPressed = false;
            _canActuallyClose = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void DeActivePopup()
        {
            Active = false;
            gameObject.SetActive(false);
        }

        public virtual void Rise()
        {
            if (BackgroundCanvasGroup != null) BackgroundCanvasGroup.alpha = 1;
        }

        public virtual void Collapse()
        {
            if (BackgroundCanvasGroup != null) BackgroundCanvasGroup.alpha = 0;
        }

        #endregion

        #region abstract behaviour

        /// <summary>
        /// before popup showing
        /// </summary>
        protected virtual void OnBeforeShow() { onBeforeShow?.Invoke(); }

        /// <summary>
        /// after popup showing
        /// </summary>
        protected virtual void OnAfterShow() { onAfterShow?.Invoke(); }

        /// <summary>
        /// before popup closed
        /// </summary>
        protected virtual void OnBeforeClose() { onBeforeClose?.Invoke(); }

        /// <summary>
        /// after popup closed
        /// </summary>
        protected virtual void OnAfterClose()
        {
            onAfterClose?.Invoke();
            _tokenSourceCheckPressButton?.Dispose();
        }

        private void OnApplicationQuit()
        {
            _tokenSourceCheckPressButton?.Cancel();
            _tokenSourceCheckPressButton?.Dispose();
        }

        #endregion

        #region motion

        /// <summary>
        /// 
        /// </summary>
        protected virtual void MotionDisplay()
        {
            ContainerCanvasGroup.blocksRaycasts = false;
            ContainerTransform.gameObject.SetActive(true);
            switch (motionAffectDisplay)
            {
                case EMotionAffect.Scale:
                    ContainerTransform.pivot = new Vector2(0.5f, 0.5f);
                    ContainerTransform.localScale = _startScale;
                    ContainerTransform.TweenLocalScale(endValueDisplay, durationDisplay)
                        .SetEase(interpolatorDisplay)
                        .OnComplete(() => ContainerCanvasGroup.blocksRaycasts = true)
                        .Play();
                    break;
                case EMotionAffect.Position:
                    ContainerTransform.localScale = _defaultContainerScale;
                    ContainerTransform.localPosition = positionFromDisplay;
                    ContainerTransform.TweenLocalPosition(positionToDisplay, durationDisplay)
                        .SetEase(interpolatorDisplay)
                        .OnComplete(() => ContainerCanvasGroup.blocksRaycasts = true)
                        .Play();
                    break;
                case EMotionAffect.PositionAndScale:
                    ContainerTransform.pivot = new Vector2(0.5f, 0.5f);
                    ContainerTransform.localScale = _startScale;
                    ContainerTransform.localPosition = positionFromDisplay;
                    var sequense = TweenManager.Sequence();
                    sequense.Join(ContainerTransform.TweenLocalScale(endValueDisplay, durationDisplay).SetEase(interpolatorDisplay));
                    sequense.Join(ContainerTransform.TweenLocalPosition(positionToDisplay, durationDisplay).SetEase(interpolatorDisplay));
                    sequense.OnComplete(() => ContainerCanvasGroup.blocksRaycasts = true);
                    sequense.Play();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void MotionHide()
        {
            ContainerCanvasGroup.blocksRaycasts = false;

            void End()
            {
                ContainerTransform.gameObject.SetActive(false);
                _canActuallyClose = true;
            }

            switch (motionAffectHide)
            {
                case EMotionAffect.Scale:
                    ContainerTransform.pivot = new Vector2(0.5f, 0.5f);
                    ContainerTransform.TweenLocalScale(endValueHide, durationHide).SetEase(interpolatorHide).OnComplete(End).Play();
                    break;
                case EMotionAffect.Position:
                    ContainerTransform.TweenLocalPosition(positionToHide, durationHide).SetEase(interpolatorHide).OnComplete(End).Play();
                    break;
                case EMotionAffect.PositionAndScale:
                    ContainerTransform.pivot = new Vector2(0.5f, 0.5f);
                    var sequense = TweenManager.Sequence();
                    sequense.Join(ContainerTransform.TweenLocalScale(endValueHide, durationHide).SetEase(interpolatorHide));
                    sequense.Join(ContainerTransform.TweenLocalPosition(positionToHide, durationHide).SetEase(interpolatorHide));
                    sequense.OnComplete(End);
                    sequense.Play();
                    break;
            }
        }

        #endregion

        #region mark popup

#if PANCAKE_ADDRESSABLE && UNITY_EDITOR
        [HideIf(nameof(IsMarkAsPopup))]
        [InfoBox("Click the toogle below to mark the popup as can be loaded by addressable", EMessageType.Warning)]
        [Button(ButtonSize.Medium)]
        private void MarkAsPopup()
        {
            GameObject.MarkAddressableWithLabel(PopupHelper.POPUP_LABEL);
        }

        private bool IsMarkAsPopup => GameObject.IsAddressableWithLabel(PopupHelper.POPUP_LABEL);

#endif

        #endregion
    }
}