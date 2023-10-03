using System.Threading.Tasks;

namespace Pancake.UI
{
    public abstract class SheetPresenter<TView> : Presenter<TView>, ISheetPresenter where TView : Sheet
    {
        private TView View { get; }

        protected SheetPresenter(TView view)
            : base(view)
        {
            View = view;
        }

        public void InitPresenter() { ViewDidLoad(View); }

        Task ISheetLifecycleEvent.Initialize() { return ViewDidLoad(View); }

        Task ISheetLifecycleEvent.WillEnter() { return ViewWillEnter(View); }

        Task ISheetLifecycleEvent.WillExit() { return ViewWillExit(View); }

        Task ISheetLifecycleEvent.Cleanup() { return ViewWillDestroy(View); }

        public void DidEnter() { ViewDidEnter(View); }

        public void DidExit() { ViewDidExit(View); }
        
        protected virtual Task ViewDidLoad(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillEnter(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillExit(TView view) { return Task.CompletedTask; }
        protected virtual Task ViewWillDestroy(TView view) { return Task.CompletedTask; }
        protected virtual void ViewDidEnter(TView view) { }
        protected virtual void ViewDidExit(TView view) { }

        protected override void Initialize(TView view)
        {
            // The lifecycle event of the view will be added with priority 0.
            // Presenters should be processed after the view so set the priority to 1.
            view.AddLifecycleEvent(this, 1);
        }

        protected override void Dispose(TView view) { view.RemoveLifecycleEvent(this); }
    }
}