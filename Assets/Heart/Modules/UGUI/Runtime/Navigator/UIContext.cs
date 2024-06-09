#if PANCAKE_UNITASK && PANCAKE_VCONTAINER
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer.Unity;

namespace Pancake.UI
{
    public abstract class UIContext : LifetimeScope
    {
        #region Fields

        [SerializeField] private EAnimationSetting animationSetting = EAnimationSetting.Container;
        [SerializeField] private EAnimationType animationType = EAnimationType.Scale;
        [SerializeField] private ViewShowAnimation showAnimation = new();
        [SerializeField] private ViewHideAnimation hideAnimation = new();

        private CanvasGroup _canvasGroup;

        private readonly Subject<Unit> _preInitializeEvent = new();
        private readonly Subject<Unit> _postInitializeEvent = new();
        private readonly Subject<Unit> _appearEvent = new();
        private readonly Subject<Unit> _appearedEvent = new();
        private readonly Subject<Unit> _disappearEvent = new();
        private readonly Subject<Unit> _disappearedEvent = new();

        #endregion

        #region Properties

        public static List<UIContext> ActiveViews { get; } = new();
        public float LastShowTime { get; private set; }

        public static UIContext FocusContext
        {
            get
            {
                var activeViews = ActiveViews.Where(view => view.gameObject.activeInHierarchy)
                    .Where(view => view is not Sheet)
                    .Where(view =>
                    {
                        if (view is not Page page) return true;
                        if (page.UIContainer is PageContainer pageContainer) return pageContainer.DefaultPage != page;
                        return true;
                    });

                if (activeViews.Any()) return activeViews.Aggregate((prev, current) => prev.LastShowTime > current.LastShowTime ? prev : current);

                return null;
            }
        }

        public UIContainer UIContainer { get; set; }
        public CanvasGroup CanvasGroup => _canvasGroup ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();
        public EVisibleState VisibleState { get; private set; } = EVisibleState.Disappeared;

        /// <summary>
        /// Events called before Awake
        /// </summary>
        public Observable<Unit> OnPreInitialize => _preInitializeEvent.Share();

        /// <summary>
        /// Event called immediately after Awake
        /// </summary>
        public Observable<Unit> OnPostInitialize => _postInitializeEvent.Share();

        /// <summary>
        /// Event that occurs when a UI View begins activation
        /// </summary>
        public Observable<Unit> OnAppear => _appearEvent.Share();

        /// <summary>
        /// An event that occurs every frame when the UI View is active and animation is in progress.
        /// </summary>
        public Observable<Unit> OnAppearing => OnChangingVisibleState(OnAppear, OnAppeared);

        /// <summary>
        /// Event that occurs when the UI View is completely activated
        /// </summary>
        public Observable<Unit> OnAppeared => _appearedEvent.Share();

        /// <summary>
        /// Event that occurs every frame while the UI View is active
        /// </summary>
        public Observable<Unit> OnUpdate => OnChangingVisibleState(OnAppeared, OnDisappear);

        /// <summary>
        /// Event that fires when a UI View starts deactivating
        /// </summary>
        public Observable<Unit> OnDisappear => _disappearEvent.Share();

        /// <summary>
        /// An event that occurs every frame when the UI View deactivation animation is in progress.
        /// </summary>
        public Observable<Unit> OnDisappearing => OnChangingVisibleState(OnDisappear, OnDisappeared);

        /// <summary>
        /// Event that occurs when the UI View is completely deactivated
        /// </summary>
        public Observable<Unit> OnDisappeared => _disappearedEvent.Share();

        #endregion

        #region Unity Lifecycle

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _preInitializeEvent.Dispose();
            _postInitializeEvent.Dispose();
            _appearEvent.Dispose();
            _appearedEvent.Dispose();
            _disappearEvent.Dispose();
            _disappearedEvent.Dispose();

            ActiveViews.Remove(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This logic is executed when the UI View is activated. <br/>
        /// Before activating the UI View, set the anchor and pivot to Stretch Stretch and initialize alpha to 1. <br/>
        /// During this process, 1 frame is consumed due to limitations of the Unity UI system. <br/>
        /// Then, an event is generated before and after the UI View is activated and the current VisibleState is updated. <br/>
        /// <br/>
        /// The rest consists of a template method pattern and defines specific logic in sub-objects. <br/>
        /// <br/>
        /// The overall logic flow is as follows. <br/>
        /// Activate game object after initializing RectTransform and CanvasGroup -> Change to Appearing State -> Send AppearEvent -> Wait for pre-processing logic -> Proceed with Show animation -> Change to Appeared State -> Send AppearedEvent <br/>
        /// <br/>
        /// Caution - API that does not need to be used by the client; find a way to avoid exposing it to the outside in the future.
        /// </summary>
        /// <param name="useAnimation"> Whether to use animation or not, the animation is executed only when isUseAnimation, which is determined in the inspector, is both true. </param>
        internal async UniTask ShowAsync(bool useAnimation = true)
        {
            LastShowTime = Time.time;
            ActiveViews.Add(this);

            var rectTransform = (RectTransform) transform;
            await InitializeRectTransformAsync(rectTransform);
            CanvasGroup.alpha = 1;

            await WhenPreAppearAsync();
            _preInitializeEvent.OnNext(Unit.Default);
            gameObject.SetActive(true);
            _postInitializeEvent.OnNext(Unit.Default);

            VisibleState = EVisibleState.Appearing;
            _appearEvent.OnNext(Unit.Default);

            if (useAnimation)
            {
                if (animationSetting == EAnimationSetting.Custom) await showAnimation.AnimateAsync(rectTransform, CanvasGroup);
                else await UIContainer.ShowAnimation.AnimateAsync(transform, CanvasGroup);
            }

            await WhenPostAppearAsync();

            VisibleState = EVisibleState.Appeared;
            _appearedEvent.OnNext(Unit.Default);
        }

        /// <summary>
        /// This logic is executed when the UI View is deactivated. <br/>
        /// Then, an event is generated before and after the UI View is deactivated and the current VisibleState is updated. <br/>
        /// <br/>
        /// The rest consists of a template method pattern and defines specific logic in sub-objects. <br/>
        /// <br/>
        /// The overall logic flow is as follows. <br/>
        /// Change to Disappearing State -> Send DisappearEvent -> Proceed with Hide animation -> Wait for post-processing logic -> Change to Disappeared State -> Send DisappearedEvent <br/>
        /// <br/>
        /// Caution - API that does not need to be used by the client; find a way to avoid exposing it to the outside in the future.
        /// </summary>
        /// <param name="useAnimation"> Whether to use animation or not, the animation is executed only when isUseAnimation, which is determined in the inspector, is both true. </param>
        internal async UniTask HideAsync(bool useAnimation = true)
        {
            ActiveViews.Remove(this);

            VisibleState = EVisibleState.Disappearing;
            _disappearEvent.OnNext(Unit.Default);

            await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());

            await WhenPreDisappearAsync();

            if (useAnimation)
            {
                if (animationSetting == EAnimationSetting.Custom) await hideAnimation.AnimateAsync(transform, CanvasGroup);
                else await UIContainer.HideAnimation.AnimateAsync(transform, CanvasGroup);
            }

            gameObject.SetActive(false);
            await WhenPostDisappearAsync();

            VisibleState = EVisibleState.Disappeared;
            _disappearedEvent.OnNext(Unit.Default);
        }

        #endregion

        #region Virtual Methods

        protected virtual UniTask WhenPreAppearAsync() => UniTask.CompletedTask;
        protected virtual UniTask WhenPostAppearAsync() => UniTask.CompletedTask;

        protected virtual UniTask WhenPreDisappearAsync() => UniTask.CompletedTask;
        protected virtual UniTask WhenPostDisappearAsync() => UniTask.CompletedTask;

        #endregion

        #region Private Methods

        Observable<Unit> OnChangingVisibleState(Observable<Unit> begin, Observable<Unit> end) => this.UpdateAsObservable().SkipUntil(begin).TakeUntil(end).Share();

        async UniTask InitializeRectTransformAsync(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            await UniTask.Yield();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
        }

        #endregion
    }
}
#endif