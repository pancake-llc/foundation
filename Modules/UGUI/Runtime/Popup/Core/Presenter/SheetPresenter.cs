using System.Threading.Tasks;

namespace Pancake.UI.Popup
{
    public abstract class SheetPresenter<TSheet> : Presenter<TSheet>, ISheetPresenter where TSheet : Sheet
    {
        private TSheet View { get; }

        protected SheetPresenter(TSheet view)
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
        
        protected virtual Task ViewDidLoad(TSheet view) { return Task.CompletedTask; }
        protected virtual Task ViewWillEnter(TSheet view) { return Task.CompletedTask; }
        protected virtual Task ViewWillExit(TSheet view) { return Task.CompletedTask; }
        protected virtual Task ViewWillDestroy(TSheet view) { return Task.CompletedTask; }
        protected virtual void ViewDidEnter(TSheet view) { }
        protected virtual void ViewDidExit(TSheet view) { }

        protected override void Initialize(TSheet view)
        {
            // The lifecycle event of the view will be added with priority 0.
            // Presenters should be processed after the view so set the priority to 1.
            view.AddLifecycleEvent(this, 1);
        }

        protected override void Dispose(TSheet view) { view.RemoveLifecycleEvent(this); }
    }
}