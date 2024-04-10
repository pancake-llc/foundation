using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pancake.Apex;
using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class Page : GameComponent, IPageLifecycleEvent
    {
        [SerializeField] private bool usePrefabNameAsId = true;
        [field: SerializeField, ShowIf(nameof(usePrefabNameAsId))] private string Id { get; set; }

        [SerializeField] private int order;
        [SerializeField, InlineEditor] private PageTransitionContainer animationContainer = new();
        private CanvasGroup _canvasGroup;
        private RectTransform _parentTransform;
        private RectTransform _rectTransform;
        private Progress<float> _transitionProgressReporter;

        private Progress<float> TransitionProgressReporter
        {
            get
            {
                if (_transitionProgressReporter == null) _transitionProgressReporter = new Progress<float>(SetTransitionProgress);
                return _transitionProgressReporter;
            }
        }

        private readonly CompositeLifecycleEvent<IPageLifecycleEvent> _lifecycleEvents = new();

        /// <summary>
        ///     Progress of the transition animation.
        /// </summary>
        public float TransitionAnimationProgress { get; private set; }

        public bool IsTransitioning { get; private set; }

        /// <summary>
        ///     Return the transition animation type currently playing.
        ///     If not in transition, return null.
        /// </summary>
        public PageTransitionType? TransitionType { get; private set; }

        /// <summary>
        ///     Event when the transition animation progress changes.
        /// </summary>
        public event Action<float> TransitionAnimationProgressChanged;


        #region Implement

        public virtual Task Initialize() { return Task.CompletedTask; }

        public virtual Task WillPushEnter() { return Task.CompletedTask; }

        public virtual Task WillPushExit() { return Task.CompletedTask; }

        public virtual Task WillPopEnter() { return Task.CompletedTask; }

        public virtual Task WillPopExit() { return Task.CompletedTask; }

        public virtual Task Cleanup() { return Task.CompletedTask; }

        public virtual void DidPushEnter() { }

        public virtual void DidPushExit() { }

        public virtual void DidPopEnter() { }

        public virtual void DidPopExit() { }

        #endregion

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }

        public void AddLifecycleEvent(IPageLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.AddItem(lifecycleEvent, priority); }
        public void RemoveLifecycleEvent(IPageLifecycleEvent lifecycleEvent) { _lifecycleEvents.RemoveItem(lifecycleEvent); }

        internal AsyncProcessHandle AfterLoad(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.AddItem(this, 0);
            Id = usePrefabNameAsId ? gameObject.name.Replace("(Clone)", string.Empty) : Id;
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);

            // Set order of rendering.
            var siblingIndex = 0;
            for (var i = 0; i < _parentTransform.childCount; i++)
            {
                var child = _parentTransform.GetChild(i);
                var childPage = child.GetComponent<Page>();
                siblingIndex = i;
                if (order >= childPage.order) continue;

                break;
            }

            _rectTransform.SetSiblingIndex(siblingIndex);

            _canvasGroup.alpha = 0.0f;

            return App.StartCoroutine(CreateCoroutine(_lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Initialize())));
        }

        internal AsyncProcessHandle BeforeEnter(bool push, Page partnerPage) { return App.StartCoroutine(BeforeEnterRoutine(push, partnerPage)); }

        private IEnumerator BeforeEnterRoutine(bool push, Page partnerPage)
        {
            IsTransitioning = true;
            TransitionType = push ? PageTransitionType.PushEnter : PageTransitionType.PopEnter;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);
            _canvasGroup.alpha = 0.0f;

            var task = push
                ? _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushEnter())
                : _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopEnter());
            var handle = App.StartCoroutine(CreateCoroutine(task));

            while (!handle.IsTerminated) yield return null;
        }

        internal AsyncProcessHandle Enter(bool push, bool playAnimation, Page partnerPage) { return App.StartCoroutine(EnterRoutine(push, playAnimation, partnerPage)); }

        private IEnumerator EnterRoutine(bool push, bool playAnimation, Page partnerPage)
        {
            _canvasGroup.alpha = 1f;
            if (playAnimation)
            {
                var anim = animationContainer.GetAnimation(push, true, partnerPage?.Id);
                if (anim == null) anim = DefaultTransitionSetting.GetDefaultPageTransition(push, true);

                if (anim.Duration > 0f)
                {
                    anim.SetPartner(partnerPage?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    yield return App.StartCoroutine(anim.CreateRoutine(TransitionProgressReporter));
                }
            }

            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(1f);
        }

        internal void AfterEnter(bool push, Page partnerPage)
        {
            if (push) _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPushEnter());
            else _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPopEnter());

            IsTransitioning = false;
            TransitionType = null;
        }

        internal AsyncProcessHandle BeforeExit(bool push, Page partnerPage) { return App.StartCoroutine(BeforeExitRoutine(push, partnerPage)); }

        private IEnumerator BeforeExitRoutine(bool push, Page partnerPage)
        {
            IsTransitioning = true;
            TransitionType = push ? PageTransitionType.PushExit : PageTransitionType.PopExit;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);
            _canvasGroup.alpha = 1.0f;

            var task = push
                ? _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushExit())
                : _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopExit());
            var handle = App.StartCoroutine(CreateCoroutine(task));

            while (!handle.IsTerminated) yield return null;
        }

        internal AsyncProcessHandle Exit(bool push, bool playAnimation, Page partnerPage) { return App.StartCoroutine(ExitRountine(push, playAnimation, partnerPage)); }

        private IEnumerator ExitRountine(bool push, bool playAnimation, Page partnerPage)
        {
            if (playAnimation)
            {
                var anim = animationContainer.GetAnimation(push, false, partnerPage?.Id);
                if (anim == null) anim = DefaultTransitionSetting.GetDefaultPageTransition(push, false);

                if (anim.Duration > 0.0f)
                {
                    anim.SetPartner(partnerPage?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    yield return App.StartCoroutine(anim.CreateRoutine(TransitionProgressReporter));
                }
            }

            _canvasGroup.alpha = 0.0f;
            SetTransitionProgress(1.0f);
        }

        internal void AfterExit(bool push, Page partnerPage)
        {
            if (push) _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPushExit());
            else _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPopExit());

            gameObject.SetActive(false);
            IsTransitioning = false;
            TransitionType = null;
        }

        internal void BeforeReleaseAndForget() { _ = _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()); }

        internal AsyncProcessHandle BeforeRelease() { return App.StartCoroutine(CreateCoroutine(_lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()))); }

        private IEnumerator CreateCoroutine(IEnumerable<Task> targets)
        {
            foreach (var target in targets)
            {
                var handle = App.StartCoroutine(CreateCoroutine(target));
                if (!handle.IsTerminated) yield return handle;
            }
        }

        private IEnumerator CreateCoroutine(Task target)
        {
            var isCompleted = false;
            WaitTaskAndCallback(target, () => { isCompleted = true; });

            return new WaitUntil(() => isCompleted);

            async void WaitTaskAndCallback(Task task, Action callback)
            {
                await task;
                callback?.Invoke();
            }
        }
    }
}