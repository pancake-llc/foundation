using System;
using System.Collections.Generic;
using Pancake.Linq;
using UnityEngine;

#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using Pancake.Common;
#endif

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    public class Sheet : MonoBehaviour, ISheetLifecycleEvent
    {
        [field: SerializeField] private string Id { get; set; }

        [SerializeField] private int order;

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

        private readonly CompositeLifecycleEvent<ISheetLifecycleEvent> _lifecycleEvents = new();

        public bool IsTransitioning { get; private set; }

        /// <summary>
        ///     Return the transition animation type currently playing.
        ///     If not in transition, return null.
        /// </summary>
        public SheetTransitionType? TransitionAnimationType { get; private set; }

        /// <summary>
        ///     Progress of the transition animation.
        /// </summary>
        public float TransitionAnimationProgress { get; private set; }

        /// <summary>
        ///     Event when the transition animation progress changes.
        /// </summary>
        public event Action<float> TransitionAnimationProgressChanged;

        public ITransitionAnimation GetTransitionAnimation(bool enter, string partnerTransitionIdentifier)
        {
            var anims = enter ? enterAnimations : exitAnimations;
            var anim = anims.FirstOrDefault(x => x.IsValid(partnerTransitionIdentifier));
            var result = anim?.GetAnimation();
            return result;
        }

#if PANCAKE_UNITASK
        public virtual UniTask Initialize() { return UniTask.CompletedTask; }

        public virtual UniTask WillEnter() { return UniTask.CompletedTask; }


        public virtual UniTask WillExit() { return UniTask.CompletedTask; }


        public virtual UniTask Cleanup() { return UniTask.CompletedTask; }

        public void AddLifecycleEvent(ISheetLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.AddItem(lifecycleEvent, priority); }

        public void RemoveLifecycleEvent(ISheetLifecycleEvent lifecycleEvent) { _lifecycleEvents.RemoveItem(lifecycleEvent); }

        internal async UniTask AfterLoadAsync(RectTransform parentTransform)
        {
            _rectTransform = (RectTransform) transform;
            _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
            _lifecycleEvents.AddItem(this, 0);
            _parentTransform = parentTransform;
            _rectTransform.FillWithParent(_parentTransform);

            // Set order of rendering.
            var siblingIndex = 0;
            for (var i = 0; i < _parentTransform.childCount; i++)
            {
                var child = _parentTransform.GetChild(i);
                var childPage = child.GetComponent<Sheet>();
                siblingIndex = i;
                if (order >= childPage.order) continue;

                break;
            }

            _rectTransform.SetSiblingIndex(siblingIndex);

            gameObject.SetActive(false);

            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Initialize());
        }

        internal async UniTask BeforeEnterAsync(Sheet partnerSheet)
        {
            IsTransitioning = true;
            TransitionAnimationType = SheetTransitionType.Enter;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);

            _canvasGroup.alpha = 0.0f;

            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillEnter());
        }

        internal async UniTask EnterAsync(bool playAnimation, Sheet partnerSheet)
        {
            _canvasGroup.alpha = 1.0f;

            if (playAnimation)
            {
                var anim = GetTransitionAnimation(true, partnerSheet?.Id);
                if (anim == null) anim = DefaultNavigatorSetting.GetDefaultSheetTransition(true);

                if (anim.Duration > 0.0f)
                {
                    anim.SetPartner(partnerSheet?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    await anim.PlayWith(TransitionProgressReporter);
                }
            }

            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(1.0f);
        }

        internal async UniTask BeforeExitAsync(Sheet partnerSheet)
        {
            IsTransitioning = true;
            TransitionAnimationType = SheetTransitionType.Exit;
            gameObject.SetActive(true);
            _rectTransform.FillWithParent(_parentTransform);
            SetTransitionProgress(0.0f);

            _canvasGroup.alpha = 1.0f;

            await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.WillExit());
        }

        internal async UniTask ExitAsync(bool playAnimation, Sheet partnerSheet)
        {
            if (playAnimation)
            {
                var anim = GetTransitionAnimation(false, partnerSheet?.Id);
                if (anim == null) anim = DefaultNavigatorSetting.GetDefaultSheetTransition(false);

                if (anim.Duration > 0.0f)
                {
                    anim.SetPartner(partnerSheet?.transform as RectTransform);
                    anim.Setup(_rectTransform);
                    await anim.PlayWith(TransitionProgressReporter);
                }
            }

            _canvasGroup.alpha = 0.0f;
            SetTransitionProgress(1.0f);
        }

        internal void BeforeRelease() { _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()).Forget(); }

        internal async UniTask BeforeReleaseAsync() { await _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.Cleanup()); }
#endif

        public virtual void DidEnter() { }

        public virtual void DidExit() { }

        internal void AfterEnter(Sheet partnerSheet)
        {
            _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidEnter());
            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        internal void AfterExit(Sheet partnerSheet)
        {
            _lifecycleEvents.ExecuteLifecycleEventsSequentially(x => x.DidExit());
            gameObject.SetActive(false);
            IsTransitioning = false;
            TransitionAnimationType = null;
        }

        private void SetTransitionProgress(float progress)
        {
            TransitionAnimationProgress = progress;
            TransitionAnimationProgressChanged?.Invoke(progress);
        }
    }
}