using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pancake.ExLib;
using UnityEngine;
using Pancake.Apex;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class Popup : GameComponent, IPopupLifecycleEvent
    {
        [SerializeField] private bool usePrefabNameAsId = true;
        [field: SerializeField, ShowIf(nameof(usePrefabNameAsId))] private string Id { get; set; }

        [SerializeField, InlineEditor] private PopupTransitionContainer animationContainer = new PopupTransitionContainer();

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

        private readonly PriorityList<IPopupLifecycleEvent> _lifecycleEvents = new PriorityList<IPopupLifecycleEvent>();

        public PopupTransitionContainer AnimationContainer => animationContainer;

        public bool IsTransitioning { get; private set; }

        /// <summary>
        ///     Return the transition animation type currently playing.
        ///     If not in transition, return null.
        /// </summary>
        public PopupTransitionType? TransitionAnimationType { get; private set; }

        /// <summary>
        ///     Progress of the transition animation.
        /// </summary>
        public float TransitionAnimationProgress { get; private set; }

        /// <summary>
        ///     Event when the transition animation progress changes.
        /// </summary>
        public event Action<float> TransitionAnimationProgressChanged;

        public virtual Task Initialize() { return Task.CompletedTask; }

        public virtual Task WillPushEnter() { return Task.CompletedTask; }

        public virtual void DidPushEnter() { }

        public virtual Task WillPushExit() { return Task.CompletedTask; }

        public virtual void DidPushExit() { }

        public virtual Task WillPopEnter() { return Task.CompletedTask; }

        public virtual void DidPopEnter() { }

        public virtual Task WillPopExit() { return Task.CompletedTask; }

        public virtual void DidPopExit() { }

        public virtual Task Cleanup() { return Task.CompletedTask; }


        public void AddLifecycleEvent(IPopupLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.Add(lifecycleEvent, priority); }

        public void RemoveLifecycleEvent(IPopupLifecycleEvent lifecycleEvent) { _lifecycleEvents.Remove(lifecycleEvent); }

        internal AsyncProcessHandle AfterLoad(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.Add(this, 0);
            Id = usePrefabNameAsId ? gameObject.name.Replace("(Clone)", string.Empty) : Id;
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 0.0f;

            return App.StartCoroutine(CreateCoroutine(_lifecycleEvents.Select(x => x.Initialize())));
        }

        internal AsyncProcessHandle BeforeEnter(bool push, Popup partnerPopup) { return App.StartCoroutine(BeforeEnterRoutine(push, partnerPopup)); }

        private IEnumerator BeforeEnterRoutine(bool push, Popup partnerPopup)
        {
            IsTransitioning = true;
            if (push)
            {
                TransitionAnimationType = PopupTransitionType.Enter;
                gameObject.SetActive(true);
                _rectTransform.FillWithParent(_parentTransform);
                _canvasGroup.alpha = 0.0f;
            }

            SetTransitionProgress(0.0f);

            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            var routines = push ? _lifecycleEvents.Select(x => x.WillPushEnter()).ToArray() : _lifecycleEvents.Select(x => x.WillPopEnter()).ToArray();
            var handle = App.StartCoroutine(CreateCoroutine(routines));

            while (!handle.IsTerminated)
                yield return null;
        }

        internal AsyncProcessHandle Enter(bool push, bool playAnimation, Popup partnerPopup)
        {
            return App.StartCoroutine(EnterRoutine(push, playAnimation, partnerPopup));
        }

        private IEnumerator EnterRoutine(bool push, bool playAnimation, Popup partnerPopup)
        {
            if (push)
            {
                _canvasGroup.alpha = 1.0f;

                if (playAnimation)
                {
                    var anim = animationContainer.GetAnimation(true, partnerPopup?.Id);
                    if (anim == null)
                        anim = DefaultTransitionSetting.GetDefaultPopupTransition(true);

                    if (anim.Duration > 0.0f)
                    {
                        anim.SetPartner(partnerPopup?.transform as RectTransform);
                        anim.Setup(_rectTransform);
                        yield return App.StartCoroutine(anim.CreateRoutine(TransitionProgressReporter));
                    }
                }

                _rectTransform.FillWithParent(_parentTransform);
            }

            SetTransitionProgress(1.0f);
        }

        internal void AfterEnter(bool push, Popup partnerPopup)
        {
            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            var lifecycleEvents = _lifecycleEvents.ToArray();
            if (push)
                foreach (var lifecycleEvent in lifecycleEvents)
                    lifecycleEvent.DidPushEnter();
            else
                foreach (var lifecycleEvent in lifecycleEvents)
                    lifecycleEvent.DidPopEnter();

            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal AsyncProcessHandle BeforeExit(bool push, Popup partnerPopup) { return App.StartCoroutine(BeforeExitRoutine(push, partnerPopup)); }

        private IEnumerator BeforeExitRoutine(bool push, Popup partnerPopup)
        {
            IsTransitioning = true;
            if (!push)
            {
                TransitionAnimationType = PopupTransitionType.Exit;
                gameObject.SetActive(true);
                _rectTransform.FillWithParent(_parentTransform);
                _canvasGroup.alpha = 1.0f;
            }

            SetTransitionProgress(0.0f);

            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            var routines = push ? _lifecycleEvents.Select(x => x.WillPushExit()).ToArray() : _lifecycleEvents.Select(x => x.WillPopExit()).ToArray();
            var handle = App.StartCoroutine(CreateCoroutine(routines));

            while (!handle.IsTerminated)
                yield return null;
        }

        internal AsyncProcessHandle Exit(bool push, bool playAnimation, Popup partnerPopup) { return App.StartCoroutine(ExitRoutine(push, playAnimation, partnerPopup)); }

        private IEnumerator ExitRoutine(bool push, bool playAnimation, Popup partnerPopup)
        {
            if (!push)
            {
                if (playAnimation)
                {
                    var anim = animationContainer.GetAnimation(false, partnerPopup?.Id);
                    if (anim == null) anim = DefaultTransitionSetting.GetDefaultPopupTransition(false);

                    if (anim.Duration > 0.0f)
                    {
                        anim.SetPartner(partnerPopup?.transform as RectTransform);
                        anim.Setup(_rectTransform);
                        yield return App.StartCoroutine(anim.CreateRoutine(TransitionProgressReporter));
                    }
                }

                _canvasGroup.alpha = 0.0f;
            }

            SetTransitionProgress(1.0f);
        }

        internal void AfterExit(bool push, Popup partnerPopup)
        {
            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            var lifecycleEvents = _lifecycleEvents.ToArray();
            if (push)
            {
                foreach (var lifecycleEvent in lifecycleEvents)
                    lifecycleEvent.DidPushExit();
            }
            else
            {
                foreach (var lifecycleEvent in lifecycleEvents)
                    lifecycleEvent.DidPopExit();
            }

            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal AsyncProcessHandle BeforeRelease()
        {
            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            return App.StartCoroutine(CreateCoroutine(_lifecycleEvents.Select(x => x.Cleanup()).ToArray()));
        }

        private IEnumerator CreateCoroutine(IEnumerable<Task> targets)
        {
            foreach (var target in targets)
            {
                var handle = App.StartCoroutine(CreateCoroutine(target));
                if (!handle.IsTerminated)
                    yield return handle;
            }
        }

        private IEnumerator CreateCoroutine(Task target)
        {
            async void WaitTaskAndCallback(Task task, Action callback)
            {
                await task;
                callback?.Invoke();
            }

            var isCompleted = false;
            WaitTaskAndCallback(target, () => { isCompleted = true; });
            return new WaitUntil(() => isCompleted);
        }

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }
    }
}