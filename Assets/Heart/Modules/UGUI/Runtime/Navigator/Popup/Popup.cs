using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine;

#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class Popup : GameComponent, IPopupLifecycleEvent
    {
        [SerializeField] private bool usePrefabNameAsId = true;

        [field: SerializeField, HideIf(nameof(usePrefabNameAsId)), Indent]
        private string Id { get; set; }

        [SerializeField] protected List<TransitionAnimation> enterAnimations = new();
        [SerializeField] protected List<TransitionAnimation> exitAnimations = new();

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

#if PANCAKE_UNITASK
        public virtual UniTask Initialize() { return UniTask.CompletedTask; }

        public virtual UniTask WillPushEnter() { return UniTask.CompletedTask; }

        public virtual UniTask WillPushExit() { return UniTask.CompletedTask; }

        public virtual UniTask WillPopEnter() { return UniTask.CompletedTask; }

        public virtual UniTask WillPopExit() { return UniTask.CompletedTask; }

        public virtual UniTask Cleanup() { return UniTask.CompletedTask; }

        public void AddLifecycleEvent(IPopupLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.AddItem(lifecycleEvent, priority); }

        public void RemoveLifecycleEvent(IPopupLifecycleEvent lifecycleEvent) { _lifecycleEvents.RemoveItem(lifecycleEvent); }

        internal async UniTask AfterLoadAsync(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.AddItem(this, 0);
            Id = usePrefabNameAsId ? gameObject.name.Replace("(Clone)", string.Empty) : Id;
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);
            _canvasGroup.alpha = 0.0f;

            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Initialize());
        }

        internal async UniTask BeforeEnterAsync(bool push, Popup partnerPopup)
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

            if (push) await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushEnter());
            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopEnter());
        }

        internal async UniTask EnterAsync(bool push, bool playAnimation, Popup partnerPopup)
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
                        await anim.PlayWith(TransitionProgressReporter);
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

        internal async UniTask BeforeExitAsync(bool push, Popup partnerPopup)
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

            if (push) await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushExit());
            else await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopExit());
        }

        internal async UniTask ExitAsync(bool push, bool playAnimation, Popup partnerPopup)
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
                        await anim.PlayWith(TransitionProgressReporter);
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

        internal void BeforeRelease() { _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()).Forget(); }

        internal async UniTask BeforeReleaseAsync() { await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()); }
#endif

        public virtual void DidPushEnter() { }
        public virtual void DidPushExit() { }
        public virtual void DidPopEnter() { }
        public virtual void DidPopExit() { }

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }
    }
}