using System.Threading.Tasks;

namespace Pancake.UI
{
    public abstract class PagePresenter<TView> : Presenter<TView>, IPagePresenter where TView : Page
    {
        private TView View { get; }

        protected PagePresenter(TView view)
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

        protected virtual Task ViewDidLoad(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushEnter(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushExit(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopEnter(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopExit(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillDestroy(TView view) { return Task.CompletedTask; }
        protected virtual void ViewDidPushEnter(TView view) { }
        protected virtual void ViewDidPushExit(TView view) { }
        protected virtual void ViewDidPopEnter(TView view) { }
        protected virtual void ViewDidPopExit(TView view) { }

        protected override void Initialize(TView view)
        {
            // The lifecycle event of the view will be added with priority 0.
            // Presenters should be processed after the view so set the priority to 1.
            view.AddLifecycleEvent(this, 1);
        }

        protected override void Dispose(TView view) { view.RemoveLifecycleEvent(this); }
    }
}