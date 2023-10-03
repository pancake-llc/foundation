using System.Threading.Tasks;

namespace Pancake.UI
{
    public class PopupPresenter<TView> : Presenter<TView>, IPopupPresenter where TView : Popup
    {
        private TView View { get; }

        public PopupPresenter(TView view)
            : base(view)
        {
            View = view;
        }

        public void InitPresenter() { ViewDidLoad(View); }
        
        Task IPopupLifecycleEvent.Initialize() { return ViewDidLoad(View); }

        Task IPopupLifecycleEvent.WillPushEnter() { return ViewWillPushEnter(View); }
        
        Task IPopupLifecycleEvent.WillPushExit() { return ViewWillPushExit(View); }

        Task IPopupLifecycleEvent.WillPopEnter() { return ViewWillPopEnter(View); }

        Task IPopupLifecycleEvent.WillPopExit() { return ViewWillPopExit(View); }

        Task IPopupLifecycleEvent.Cleanup() { return ViewWillDestroy(View); }

        void IPopupLifecycleEvent.DidPushEnter() { ViewDidPushEnter(View); }

        void IPopupLifecycleEvent.DidPushExit() { ViewDidPushExit(View); }

        void IPopupLifecycleEvent.DidPopEnter() { ViewDidPopEnter(View); }

        void IPopupLifecycleEvent.DidPopExit() { ViewDidPopExit(View); }

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