using System;
using System.Collections.Generic;
using System.Linq;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using Sirenix.OdinInspector;
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
        [SerializeField] protected List<TransitionAnimation> pushEnterAnimations = new();
        [SerializeField] protected List<TransitionAnimation> pushExitAnimations = new();
        [SerializeField] protected List<TransitionAnimation> popEnterAnimations = new();
        [SerializeField] protected List<TransitionAnimation> popExitAnimations = new();

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

#if PANCAKE_UNITASK
        public virtual UniTask Initialize() { return UniTask.CompletedTask; }

        public virtual UniTask WillPushEnter() { return UniTask.CompletedTask; }

        public virtual UniTask WillPushExit() { return UniTask.CompletedTask; }

        public virtual UniTask WillPopEnter() { return UniTask.CompletedTask; }

        public virtual UniTask WillPopExit() { return UniTask.CompletedTask; }

        public virtual UniTask Cleanup() { return UniTask.CompletedTask; }
#endif

        public virtual void DidPushEnter() { }

        public virtual void DidPushExit() { }

        public virtual void DidPopEnter() { }

        public virtual void DidPopExit() { }

        #endregion

        public ITransitionAnimation GetTransitionAnimation(bool push, bool enter, string partnerId)
        {
            var anims = GetTransitionAnimations(push, enter);
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerId));
            var result = anim?.GetAnimation();
            return result;
        }

        private IReadOnlyList<TransitionAnimation> GetTransitionAnimations(bool push, bool enter)
        {
            if (push) return enter ? pushEnterAnimations : pushExitAnimations;

            return enter ? popEnterAnimations : popExitAnimations;
        }

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }

        public void AddLifecycleEvent(IPageLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.AddItem(lifecycleEvent, priority); }
        public void RemoveLifecycleEvent(IPageLifecycleEvent lifecycleEvent) { _lifecycleEvents.RemoveItem(lifecycleEvent); }

#if PANCAKE_UNITASK
        internal async UniTask AfterLoadAsync(RectTransform parentTransform)
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

            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Initialize());
        }

        internal async UniTask BeforeEnterAsync(bool push, Page partnerPage)
        {
            IsTransitioning = true;
            TransitionType = push ? PageTransitionType.PushEnter : PageTransitionType.PopEnter;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);
            _canvasGroup.alpha = 0.0f;

            if (push) await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushEnter());
            else await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopEnter());
        }

        internal async UniTask EnterAsync(bool push, bool playAnimation, Page partnerPage)
        {
            _canvasGroup.alpha = 1f;
            if (playAnimation)
            {
                var anim = GetTransitionAnimation(push, true, partnerPage?.Id) ?? DefaultNavigatorSetting.GetDefaultPageTransition(push, true);

                if (anim.Duration > 0f)
                {
                    anim.SetPartner(partnerPage?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    await anim.PlayWith(TransitionProgressReporter);
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

        internal async UniTask BeforeExitAsync(bool push, Page partnerPage)
        {
            IsTransitioning = true;
            TransitionType = push ? PageTransitionType.PushExit : PageTransitionType.PopExit;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);
            _canvasGroup.alpha = 1.0f;

            if (push) await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPushExit());
            else await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillPopExit());
        }

        internal async UniTask ExitAsync(bool push, bool playAnimation, Page partnerPage)
        {
            if (playAnimation)
            {
                var anim = GetTransitionAnimation(push, false, partnerPage?.Id) ?? DefaultNavigatorSetting.GetDefaultPageTransition(push, false);

                if (anim.Duration > 0.0f)
                {
                    anim.SetPartner(partnerPage?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    await anim.PlayWith(TransitionProgressReporter);
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

        internal void BeforeRelease() { _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()).Forget(); }

        internal async UniTask BeforeReleaseAsync() { await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()); }
#endif
    }
}