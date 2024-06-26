using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alchemy.Inspector;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class Popup : GameComponent, IPopupLifecycleEvent
    {
        [SerializeField] private bool usePrefabNameAsId = true;
        [field: SerializeField, ShowIf(nameof(usePrefabNameAsId))] private string Id { get; set; }
        [SerializeField] private List<TransitionAnimation> enterAnimations = new();
        [SerializeField] private List<TransitionAnimation> exitAnimations = new();


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

        private readonly CompositeLifecycleEvent<IPopupLifecycleEvent> _lifecycleEvents = new();

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

        public ITransitionAnimation GetTransitionAnimation(bool enter, string partnerId)
        {
            var anims = enter ? enterAnimations : exitAnimations;
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerId));
            var result = anim?.GetAnimation();
            return result;
        }

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


        public void AddLifecycleEvent(IPopupLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.AddItem(lifecycleEvent, priority); }

        public void RemoveLifecycleEvent(IPopupLifecycleEvent lifecycleEvent) { _lifecycleEvents.RemoveItem(lifecycleEvent); }

        internal AsyncProcessHandle AfterLoad(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.AddItem(this, 0);
            Id = usePrefabNameAsId ? gameObject.name.Replace("(Clone)", string.Empty) : Id;
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 0.0f;

            var task = _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Initialize());
            return App.StartCoroutine(CreateCoroutine(task));
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

            var task = push
                ? _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushEnter())
                : _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopEnter());
            var handle = App.StartCoroutine(CreateCoroutine(task));

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
                    var anim = GetTransitionAnimation(true, partnerPopup?.Id);
                    if (anim == null)
                        anim = DefaultNavigatorSetting.GetDefaultPopupTransition(true);

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
            if (push) _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPushEnter());
            else _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPopEnter());

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

            var task = push
                ? _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushExit())
                : _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopExit());
            var handle = App.StartCoroutine(CreateCoroutine(task));

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
                    var anim = GetTransitionAnimation(false, partnerPopup?.Id);
                    if (anim == null) anim = DefaultNavigatorSetting.GetDefaultPopupTransition(false);

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
            if (push) _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPushExit());
            else _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidPopExit());

            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal void BeforeReleaseAndForget() { _ = _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()); }

        internal AsyncProcessHandle BeforeRelease()
        {
            // Evaluate here because users may add/remove lifecycle events within the lifecycle events.
            return App.StartCoroutine(CreateCoroutine(_lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup())));
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