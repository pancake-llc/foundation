using System;
using System.Collections.Generic;
using Pancake;
using Pancake.AssetLoader;
using Pancake.Common;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Pancake.UI;


namespace Pancake.DebugView
{
    [DefaultExecutionOrder(int.MinValue)]
    public sealed class DebugSheet : MonoBehaviour, IPageContainerCallbackReceiver
    {
        private const float THRESHOLD_INCH = 0.24f;

        private static readonly Dictionary<int, DebugSheet> InstanceCacheByTransform = new Dictionary<int, DebugSheet>();
        [SerializeField] private FlickToOpenMode flickToOpen = FlickToOpenMode.Edge;

        [SerializeField] private KeyboardShortcut keyboardShortcut = new KeyboardShortcut();
        [SerializeField] private List<GameObject> cellPrefabs = new List<GameObject>();
        [SerializeField] private StatefulDrawer drawer;
        [SerializeField] private StatefulDrawerController drawerController;
        [SerializeField] private Button backButton;
        [SerializeField] private Text exitTitleText;
        [SerializeField] private Text enterTitleText;
        [SerializeField] private Button closeButton;
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private GameObject pagePrefab;
        [SerializeField] private InputBasedFlickEvent flickEvent;
        [SerializeField] private BalloonButton balloonButton;
        [SerializeField] private FloatingButton floatingButton;
        [SerializeField] private Canvas canvas;
        private float _dpi;
        private bool _isInitialized;
        private PreloadedAssetLoader _preloadedAssetLoader;

        public static DebugSheet Instance { get; private set; }

        public string InitialPageId { get; private set; }
        public DebugPageBase InitialDebugPage { get; private set; }
        public DebugPageBase CurrentDebugPage { get; private set; }
        public DebugPageBase EnteringDebugPage { get; private set; }
        public DebugPageBase ExitingDebugPage { get; private set; }
        public IReadOnlyDictionary<string, Page> Pages => pageContainer.Pages;
        public IList<GameObject> CellPrefabs => cellPrefabs;

        public FlickToOpenMode FlickToOpen { get => flickToOpen; set => flickToOpen = value; }

        public KeyboardShortcut KeyboardShortcut => keyboardShortcut;

        public BalloonButton BalloonButton => balloonButton;

        public FloatingButton FloatingButton => floatingButton;

        private void Awake()
        {
            var dpi = Screen.dpi;
            if (dpi == 0) dpi = 326;
            _dpi = dpi;

            backButton.interactable = false;
            SetBackButtonVisibility(0.0f);
            balloonButton.Initialize(canvas);

            if (Instance == null)
            {
                Instance = this;
                return;
            }

            foreach (var cellPrefab in cellPrefabs)
            {
                if (!Instance.CellPrefabs.Contains(cellPrefab)) Instance.CellPrefabs.Add(cellPrefab);
            }

            Destroy(gameObject);
        }

        private void Update()
        {
            if (keyboardShortcut.Evaluate())
            {
                var isClosed = Mathf.Approximately(drawer.Progress, drawer.MinProgress);
                var targetState = isClosed ? DrawerState.Max : DrawerState.Min;
                drawerController.SetStateWithAnimation(targetState);
            }
        }

        private void OnEnable()
        {
            flickEvent.flicked.AddListener(OnFlicked);
            backButton.onClick.AddListener(OnBackButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            flickEvent.flicked.RemoveListener(OnFlicked);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void IPageContainerCallbackReceiver.BeforePush(Page enterPage, Page exitPage)
        {
            backButton.interactable = false;
            EnteringDebugPage = enterPage.GetComponent<DebugPageBase>();
            ExitingDebugPage = exitPage == null ? null : exitPage.GetComponent<DebugPageBase>();
            enterTitleText.text = EnteringDebugPage.GetTitle();
            exitTitleText.text = ExitingDebugPage == null ? "" : ExitingDebugPage.GetTitle();
            CurrentDebugPage = EnteringDebugPage;
            enterPage.TransitionAnimationProgressChanged += OnTransitionProgressChanged;
        }

        void IPageContainerCallbackReceiver.AfterPush(Page enterPage, Page exitPage)
        {
            backButton.interactable = pageContainer.Pages.Count >= 2;
            enterPage.TransitionAnimationProgressChanged -= OnTransitionProgressChanged;
            EnteringDebugPage = null;
            ExitingDebugPage = null;
        }

        void IPageContainerCallbackReceiver.BeforePop(Page enterPage, Page exitPage)
        {
            backButton.interactable = false;
            EnteringDebugPage = enterPage.GetComponent<DebugPageBase>();
            ExitingDebugPage = exitPage.GetComponent<DebugPageBase>();
            enterTitleText.text = EnteringDebugPage.GetTitle();
            exitTitleText.text = ExitingDebugPage.GetTitle();
            CurrentDebugPage = EnteringDebugPage;
            enterPage.TransitionAnimationProgressChanged += OnTransitionProgressChanged;
        }

        void IPageContainerCallbackReceiver.AfterPop(Page enterPage, Page exitPage)
        {
            backButton.interactable = pageContainer.Pages.Count >= 2;
            enterPage.TransitionAnimationProgressChanged -= OnTransitionProgressChanged;
            EnteringDebugPage = null;
            ExitingDebugPage = null;
        }

        /// <summary>
        ///     Get the <see cref="PageContainer" /> that manages the debug page to which <see cref="transform" /> belongs.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="transform" />.</param>
        /// <returns></returns>
        public static DebugSheet Of(Transform transform, bool useCache = true) { return Of((RectTransform) transform, useCache); }

        /// <summary>
        ///     Get the <see cref="DebugSheet" /> that manages the debug page to which <see cref="rectTransform" /> belongs.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="useCache">Use the previous result for the <see cref="rectTransform" />.</param>
        /// <returns></returns>
        public static DebugSheet Of(RectTransform rectTransform, bool useCache = true)
        {
            var id = rectTransform.GetInstanceID();

            if (useCache && InstanceCacheByTransform.TryGetValue(id, out var container)) return container;

            container = rectTransform.GetComponentInParent<DebugSheet>();
            if (container != null)
            {
                InstanceCacheByTransform.Add(id, container);
                return container;
            }

            return null;
        }

        public TInitialPage Initialize<TInitialPage>(string titleOverride = null, Action<(string pageId, TInitialPage page)> onLoad = null, string pageId = null)
            where TInitialPage : DebugPageBase
        {
            if (_isInitialized)
                throw new InvalidOperationException($"{nameof(DebugSheet)} is already initialized.");

            backButton.interactable = false;
            SetBackButtonVisibility(0.0f);
            pageContainer.AddCallbackReceiver(this);
            var preloadedAssetLoader = new PreloadedAssetLoader();
            _preloadedAssetLoader = preloadedAssetLoader;
            preloadedAssetLoader.AddObject(pagePrefab.gameObject);
            pageContainer.AssetLoader = preloadedAssetLoader;

            PushPage<TInitialPage>(false,
                titleOverride,
                x =>
                {
                    InitialPageId = x.pageId;
                    InitialDebugPage = x.page;
                    onLoad?.Invoke((x.pageId, x.page));
                },
                pageId);
            _isInitialized = true;
            return (TInitialPage) InitialDebugPage;
        }

        public TInitialPage GetOrCreateInitialPage<TInitialPage>(
            string titleOverride = null,
            Action<(string pageId, TInitialPage page)> onLoad = null,
            string pageId = null) where TInitialPage : DebugPageBase
        {
            if (_isInitialized)
                return (TInitialPage) InitialDebugPage;

            return Initialize(titleOverride, onLoad, pageId);
        }

        public DebugPage GetOrCreateInitialPage(string titleOverride = null, string pageId = null, Action<(string pageId, DebugPage page)> onLoad = null)
        {
            return GetOrCreateInitialPage(titleOverride, onLoad, pageId);
        }

        public AsyncProcessHandle PushPage(
            Type pageType,
            DebugPageBase prefab,
            bool playAnimation,
            string titleOverride = null,
            Action<(string pageId, DebugPageBase page)> onLoad = null,
            string pageId = null)
        {
            if (!_preloadedAssetLoader.PreloadedObjects.ContainsValue(prefab.gameObject))
                _preloadedAssetLoader.AddObject(prefab.gameObject);

            return PushPage(pageType,
                prefab.gameObject.name,
                playAnimation,
                titleOverride,
                onLoad,
                pageId);
        }

        public AsyncProcessHandle PushPage<TPage>(
            TPage prefab,
            bool playAnimation,
            string titleOverride = null,
            Action<(string pageId, TPage page)> onLoad = null,
            string pageId = null) where TPage : DebugPageBase
        {
            if (!_preloadedAssetLoader.PreloadedObjects.ContainsValue(prefab.gameObject))
                _preloadedAssetLoader.AddObject(prefab.gameObject);

            return PushPage(typeof(TPage),
                prefab.gameObject.name,
                playAnimation,
                titleOverride,
                x => onLoad?.Invoke((pageId, (TPage) x.page)),
                pageId);
        }

        public AsyncProcessHandle PushPage(
            Type pageType,
            bool playAnimation,
            string titleOverride = null,
            Action<(string pageId, DebugPageBase page)> onLoad = null,
            string pageId = null)
        {
            return PushPage(pageType,
                pagePrefab.gameObject.name,
                playAnimation,
                titleOverride,
                onLoad,
                pageId);
        }

        public AsyncProcessHandle PushPage<TPage>(
            bool playAnimation,
            string titleOverride = null,
            Action<(string pageId, TPage page)> onLoad = null,
            string pageId = null) where TPage : DebugPageBase
        {
            return PushPage(typeof(TPage),
                pagePrefab.gameObject.name,
                playAnimation,
                titleOverride,
                x => onLoad?.Invoke((x.pageId, (TPage) x.page)),
                pageId);
        }

        private AsyncProcessHandle PushPage(
            Type pageType,
            string prefabName,
            bool playAnimation,
            string titleOverride = null,
            Action<(string pageId, DebugPageBase page)> onLoad = null,
            string pageId = null)
        {
            return pageContainer.Push(pageType,
                prefabName,
                playAnimation,
                pageId: pageId,
                onLoad: x =>
                {
                    var debugPage = (DebugPageBase) x.page;
                    if (titleOverride != null)
                        debugPage.SetTitle(titleOverride);

                    var prefabContainer = debugPage.GetComponent<PrefabContainer>();
                    prefabContainer.Prefabs.AddRange(cellPrefabs);

                    onLoad?.Invoke((x.pageId, debugPage));
                },
                loadAsync: false);
        }

        public AsyncProcessHandle PopPage(bool playAnimation, int popCount = 1) { return pageContainer.Pop(playAnimation, popCount); }

        public AsyncProcessHandle PopPage(bool playAnimation, string destinationPageId) { return pageContainer.Pop(playAnimation, destinationPageId); }

        public void Show() { drawerController.SetStateWithAnimation(DrawerState.Max); }

        public void Hide() { drawerController.SetStateWithAnimation(DrawerState.Min); }

        private void OnBackButtonClicked()
        {
            if (pageContainer.IsInTransition) return;

            pageContainer.Pop(false);
        }

        private void OnCloseButtonClicked() { drawerController.SetStateWithAnimation(DrawerState.Min); }

        private void OnTransitionProgressChanged(float progress)
        {
            var exitTitleCol = exitTitleText.color;
            exitTitleCol.a = 1.0f - progress;
            exitTitleText.color = exitTitleCol;

            var enterTitleCol = enterTitleText.color;
            enterTitleCol.a = progress;
            enterTitleText.color = enterTitleCol;

            if (EnteringDebugPage.TransitionType == PageTransitionType.PushEnter && ExitingDebugPage != null && ExitingDebugPage == InitialDebugPage)
                SetBackButtonVisibility(progress);

            if (EnteringDebugPage.TransitionType == PageTransitionType.PopEnter && EnteringDebugPage == InitialDebugPage)
                SetBackButtonVisibility(1.0f - progress);
        }

        private void SetBackButtonVisibility(float visibility)
        {
            var color = backButton.image.color;
            color.a = visibility;
            backButton.image.color = color;
        }

        private void OnFlicked(Flick flick)
        {
            if (flickToOpen == FlickToOpenMode.Off)
                return;

            // If it is horizontal flick, ignore it.
            var isVertical = Mathf.Abs(flick.DeltaInchPosition.y) > Mathf.Abs(flick.DeltaInchPosition.x);
            if (!isVertical)
                return;

            // Determines whether flicking is valid or not by the global control mode.
            var startPosXInch = flick.TouchStartScreenPosition.x / _dpi;
            var totalInch = Screen.width / _dpi;
            var leftSafeAreaInch = Screen.safeArea.xMin / _dpi;
            var isLeftEdge = startPosXInch <= THRESHOLD_INCH + leftSafeAreaInch;
            var rightSafeAreaInch = (Screen.width - Screen.safeArea.xMax) / _dpi;
            var isRightEdge = startPosXInch >= totalInch - (THRESHOLD_INCH + rightSafeAreaInch);
            var isValid = false;
            switch (flickToOpen)
            {
                case FlickToOpenMode.Edge:
                    isValid = isLeftEdge || isRightEdge;
                    break;
                case FlickToOpenMode.LeftEdge:
                    isValid = isLeftEdge;
                    break;
                case FlickToOpenMode.RightEdge:
                    isValid = isRightEdge;
                    break;
                case FlickToOpenMode.Off:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!isValid)
                return;

            // Apply the flick.
            var isUp = flick.DeltaInchPosition.y >= 0;
            var state = isUp ? drawer.GetUpperState() : drawer.GetLowerState();
            drawerController.SetStateWithAnimation(state);
        }
    }
}