using System.Threading.Tasks;

namespace Pancake.UI.Popup
{
    public abstract class PagePresenter<TPage> : Presenter<TPage>, IPagePresenter where TPage : Page
    {
        private TPage View { get; }

        protected PagePresenter(TPage view)
            : base(view)
        {
            View = view;
        }

        public void InitPresenter() { ViewDidLoad(View); }

        Task IPageLifecycleEvent.Initialize() { return ViewDidLoad(View); }

        Task IPageLifecycleEvent.WillPushEnter() { return ViewWillPushEnter(View); }

        Task IPageLifecycleEvent.WillPushExit() { return ViewWillPushExit(View); }

        Task IPageLifecycleEvent.WillPopEnter() { return ViewWillPopEnter(View); }

        Task IPageLifecycleEvent.WillPopExit() { return ViewWillPopExit(View); }

        Task IPageLifecycleEvent.Cleanup() { return ViewWillDestroy(View); }

        void IPageLifecycleEvent.DidPushEnter() { ViewDidPushEnter(View); }

        void IPageLifecycleEvent.DidPushExit() { ViewDidPushExit(View); }

        void IPageLifecycleEvent.DidPopEnter() { ViewDidPopEnter(View); }

        void IPageLifecycleEvent.DidPopExit() { ViewDidPopExit(View); }

        protected virtual Task ViewDidLoad(TPage view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushEnter(TPage view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushExit(TPage view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopEnter(TPage view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopExit(TPage view) { return Task.CompletedTask; }
        protected virtual Task ViewWillDestroy(TPage view) { return Task.CompletedTask; }
        protected virtual void ViewDidPushEnter(TPage view) { }
        protected virtual void ViewDidPushExit(TPage view) { }
        protected virtual void ViewDidPopEnter(TPage view) { }
        protected virtual void ViewDidPopExit(TPage view) { }

        protected override void Initialize(TPage view)
        {
            // The lifecycle event of the view will be added with priority 0.
            // Presenters should be processed after the view so set the priority to 1.
            view.AddLifecycleEvent(this, 1);
        }

        protected override void Dispose(TPage view) { view.RemoveLifecycleEvent(this); }
    }
}