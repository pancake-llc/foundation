using System;
using System.Threading.Tasks;
using Pancake.Apex;
using Pancake.ExLib;
using UnityEngine;

namespace Pancake.UI
{
    [DisallowMultipleComponent]
    [EditorIcon("")]
    public class Page : GameComponent, IPageLifecycleEvent
    {
        [SerializeField] private bool usePrefabNameAsId = true;
        [field: SerializeField, ShowIf(nameof(usePrefabNameAsId))] private string Id { get; set; }

        [SerializeField] private int order;
        [SerializeField] private PageTransitionContainer animationContainer = new PageTransitionContainer();
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

        private readonly PriorityList<IPageLifecycleEvent> _lifecycleEvents = new PriorityList<IPageLifecycleEvent>();

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

        public void AddLifecycleEvent(IPageLifecycleEvent lifecycleEvent, int priority = 0) { _lifecycleEvents.Add(lifecycleEvent, priority); }
        public void RemoveLifecycleEvent(IPageLifecycleEvent lifecycleEvent) { _lifecycleEvents.Remove(lifecycleEvent); }
    }
}