using System.Threading.Tasks;

namespace Pancake.UI.Popup
{
    public class ModalPresenter<TModal> : Presenter<TModal>, IModalPresenter where TModal : Modal
    {
        private TModal View { get; }

        public ModalPresenter(TModal view)
            : base(view)
        {
            View = view;
        }

        public void InitPresenter() { ViewDidLoad(View); }
        
        Task IModalLifecycleEvent.Initialize() { return ViewDidLoad(View); }

        Task IModalLifecycleEvent.WillPushEnter() { return ViewWillPushEnter(View); }
        
        Task IModalLifecycleEvent.WillPushExit() { return ViewWillPushExit(View); }

        Task IModalLifecycleEvent.WillPopEnter() { return ViewWillPopEnter(View); }

        Task IModalLifecycleEvent.WillPopExit() { return ViewWillPopExit(View); }

        Task IModalLifecycleEvent.Cleanup() { return ViewWillDestroy(View); }

        void IModalLifecycleEvent.DidPushEnter() { ViewDidPushEnter(View); }

        void IModalLifecycleEvent.DidPushExit() { ViewDidPushExit(View); }

        void IModalLifecycleEvent.DidPopEnter() { ViewDidPopEnter(View); }

        void IModalLifecycleEvent.DidPopExit() { ViewDidPopExit(View); }

        protected virtual Task ViewDidLoad(TModal view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushEnter(TModal view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPushExit(TModal view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopEnter(TModal view) { return Task.CompletedTask; }
        protected virtual Task ViewWillPopExit(TModal view) { return Task.CompletedTask; }
        protected virtual Task ViewWillDestroy(TModal view) { return Task.CompletedTask; }
        protected virtual void ViewDidPushEnter(TModal view) { }
        protected virtual void ViewDidPushExit(TModal view) { }
        protected virtual void ViewDidPopEnter(TModal view) { }
        protected virtual void ViewDidPopExit(TModal view) { }
        
        protected override void Initialize(TModal view)
        {
            // The lifecycle event of the view will be added with priority 0.
            // Presenters should be processed after the view so set the priority to 1.
            view.AddLifecycleEvent(this, 1);
        }

        protected override void Dispose(TModal view) { view.RemoveLifecycleEvent(this); }
    }
}