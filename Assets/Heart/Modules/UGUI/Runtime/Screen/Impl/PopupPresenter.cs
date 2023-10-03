using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Pancake.UI
{
    public abstract class PopupPresenter<TPopup, TView, TModel> : PopupPresenter<TPopup>, IDisposableCollectionHolder
        where TPopup : Popup<TView, TModel> where TView : View<TModel> where TModel : Model, new()
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private TModel _model;

        protected PopupPresenter(TPopup view)
            : base(view)
        {
        }

        public ICollection<IDisposable> GetDisposableCollection() { return _disposables; }

        protected sealed override void Initialize(TPopup view) { base.Initialize(view); }

        protected sealed override async Task ViewDidLoad(TPopup view)
        {
            await base.ViewDidLoad(view);
            var model = new TModel();
            _model = model;
            _disposables.Add(model);
            view.Setup(model);
            await ViewDidLoad(view, _model);
        }

        protected sealed override async Task ViewWillPushEnter(TPopup view)
        {
            await base.ViewWillPushEnter(view);
            await ViewWillPushEnter(view, _model);
        }

        protected sealed override void ViewDidPushEnter(TPopup view)
        {
            base.ViewDidPushEnter(view);
            ViewDidPushEnter(view, _model);
        }

        protected sealed override async Task ViewWillPushExit(TPopup view)
        {
            await base.ViewWillPushExit(view);
            await ViewWillPushExit(view, _model);
        }

        protected sealed override void ViewDidPushExit(TPopup view)
        {
            base.ViewDidPushExit(view);
            ViewDidPushExit(view, _model);
        }

        protected sealed override async Task ViewWillPopEnter(TPopup view)
        {
            await base.ViewWillPopEnter(view);
            await ViewWillPopEnter(view, _model);
        }

        protected sealed override void ViewDidPopEnter(TPopup view)
        {
            base.ViewDidPopEnter(view);
            ViewDidPopEnter(view, _model);
        }

        protected sealed override async Task ViewWillPopExit(TPopup view)
        {
            await base.ViewWillPopExit(view);
            await ViewWillPopExit(view, _model);
        }

        protected sealed override void ViewDidPopExit(TPopup view)
        {
            base.ViewDidPopExit(view);
            ViewDidPopExit(view, _model);
        }

        protected override async Task ViewWillDestroy(TPopup view)
        {
            await base.ViewWillDestroy(view);
            await ViewWillDestroy(view, _model);
        }

        protected virtual Task ViewDidLoad(TPopup view, TModel model) { return Task.CompletedTask; }

        protected virtual Task ViewWillPushEnter(TPopup view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPushEnter(TPopup view, TModel model) { }

        protected virtual Task ViewWillPushExit(TPopup view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPushExit(TPopup view, TModel model) { }

        protected virtual Task ViewWillPopEnter(TPopup view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPopEnter(TPopup view, TModel model) { }

        protected virtual Task ViewWillPopExit(TPopup view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPopExit(TPopup view, TModel model) { }

        protected virtual Task ViewWillDestroy(TPopup view, TModel model) { return Task.CompletedTask; }

        protected sealed override void Dispose(TPopup view)
        {
            base.Dispose(view);
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}