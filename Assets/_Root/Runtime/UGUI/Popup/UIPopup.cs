#pragma warning disable 0649
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Tween;
using Pancake.UIQuery;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pancake.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(UICache))]
    public abstract class UIPopup : MonoBehaviour, IPopup
    {
        #region implementation

        [SerializeField] protected bool ignoreTimeScale;
        [SerializeField] protected UnityEvent onBeforeShow;
        [SerializeField] protected UnityEvent onAfterShow;
        [SerializeField] protected UnityEvent onBeforeClose;
        [SerializeField] protected UnityEvent onAfterClose;
        [SerializeField] protected bool closeByClickContainer;
        [SerializeField] protected bool closeByClickBackground;
        [SerializeField] protected bool closeByBackButton;
        [SerializeField] private List<Button> closeButtons = new List<Button>();
        [SerializeField] protected Vector2 startScale;
        [SerializeField] private string uniqueId;

        public EMotionAffect motionAffectDisplay = EMotionAffect.Scale;
        public Vector2 endValueDisplay = Vector2.one;
        public Vector2 positionToDisplay;
        public Vector2 positionFromDisplay;
        [Range(0.01f, 3f)] public float durationDisplay = 0.25f;
        public Interpolator interpolatorDisplay;

        public EMotionAffect motionAffectHide = EMotionAffect.Scale;
        public Vector2 endValueHide = Vector2.zero;
        public Vector2 positionToHide;
        [Range(0.01f, 3f)] public float durationHide = 0.25f;
        public Interpolator interpolatorHide;

        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private RectTransform backgroundTransform;
        [SerializeField] private CanvasGroup backgroundCanvasGroup;
        [SerializeField] private RectTransform containerTransform;
        [SerializeField] private CanvasGroup containerCanvasGroup;

        private UICache _uiCache;
        private bool _canActuallyClose;
        private Vector3 _defaultContainerScale;
        private CancellationTokenSource _tokenSourceCheckPressButton;

        public UICache UIRoot
        {
            get
            {
                if (_uiCache == null) _uiCache = GetComponent<UICache>();
                return _uiCache;
            }
        }

        public string Id => uniqueId;
        public GameObject GameObject => gameObject;
        public bool BackButtonPressed { get; private set; }

        public Canvas Canvas
        {
            get
            {
                if (canvas == null) canvas = GetComponent<Canvas>();
                return canvas;
            }
        }

        public bool CloseByBackButton => closeByBackButton;
        public bool CloseByClickBackground => closeByClickBackground;
        public bool CloseByClickContainer => closeByClickContainer;

        public GraphicRaycaster GraphicRaycaster
        {
            get
            {
                if (graphicRaycaster == null) graphicRaycaster = GetComponent<GraphicRaycaster>();
                return graphicRaycaster;
            }
        }

        public RectTransform BackgroundTransform
        {
            get
            {
                if (backgroundTransform == null) backgroundTransform = UIRoot.Get<RectTransform>("Background");
                return backgroundTransform;
            }
        }

        public CanvasGroup BackgroundCanvasGroup
        {
            get
            {
                if (backgroundCanvasGroup == null) backgroundCanvasGroup = UIRoot.Get<CanvasGroup>("Background");
                return backgroundCanvasGroup;
            }
        }

        public RectTransform ContainerTransform
        {
            get
            {
                if (containerTransform == null) containerTransform = UIRoot.Get<RectTransform>("Container");
                return containerTransform;
            }
        }

        public CanvasGroup ContainerCanvasGroup
        {
            get
            {
                if (containerCanvasGroup == null) containerCanvasGroup = UIRoot.Get<CanvasGroup>("Container");
                return containerCanvasGroup;
            }
        }

        public bool Active { get; protected set; }
        public CancellationTokenSource TokenSourceCheckPressButton => _tokenSourceCheckPressButton ?? (_tokenSourceCheckPressButton = new CancellationTokenSource());


#if UNITY_EDITOR
        /// <summary>
        /// do not use this
        /// it only avaiable on editor
        /// </summary>
        [Obsolete] public List<Button> CloseButtons => closeButtons;
#endif

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
                    ContainerTransform.localScale = startScale;
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
                    ContainerTransform.localScale = startScale;
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
    }
}