using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Pancake.UI
{
    public abstract class PagePresenter<TPage, TView, TModel> : PagePresenter<TPage>, IDisposableCollectionHolder
        where TPage : Page<TView, TModel> where TView : View<TModel> where TModel : Model, new()
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private TModel _model;

        protected PagePresenter(TPage view)
            : base(view)
        {
        }

        public ICollection<IDisposable> GetDisposableCollection() { return _disposables; }
        protected sealed override void Initialize(TPage view) { base.Initialize(view); }

        protected sealed override async Task ViewDidLoad(TPage view)
        {
            await base.ViewDidLoad(view);
            var model = new TModel();
            _model = model;
            _disposables.Add(model);
            view.Setup(model);
            await ViewDidLoad(view, _model);
        }

        protected sealed override async Task ViewWillPushEnter(TPage view)
        {
            await base.ViewWillPushEnter(view);
            await ViewWillPushEnter(view, _model);
        }

        protected sealed override void ViewDidPushEnter(TPage view)
        {
            base.ViewDidPushEnter(view);
            ViewDidPushEnter(view, _model);
        }

        protected sealed override async Task ViewWillPushExit(TPage view)
        {
            await base.ViewWillPushExit(view);
            await ViewWillPushExit(view, _model);
        }

        protected sealed override void ViewDidPushExit(TPage view)
        {
            base.ViewDidPushExit(view);
            ViewDidPushExit(view, _model);
        }

        protected sealed override async Task ViewWillPopEnter(TPage view)
        {
            await base.ViewWillPopEnter(view);
            await ViewWillPopEnter(view, _model);
        }

        protected sealed override void ViewDidPopEnter(TPage view)
        {
            base.ViewDidPopEnter(view);
            ViewDidPopEnter(view, _model);
        }

        protected sealed override async Task ViewWillPopExit(TPage view)
        {
            await base.ViewWillPopExit(view);
            await ViewWillPopExit(view, _model);
        }

        protected sealed override void ViewDidPopExit(TPage view)
        {
            base.ViewDidPopExit(view);
            ViewDidPopExit(view, _model);
        }

        protected override async Task ViewWillDestroy(TPage view)
        {
            await base.ViewWillDestroy(view);
            await ViewWillDestroy(view, _model);
        }

        protected virtual Task ViewDidLoad(TPage view, TModel model) { return Task.CompletedTask; }

        protected virtual Task ViewWillPushEnter(TPage view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPushEnter(TPage view, TModel model) { }

        protected virtual Task ViewWillPushExit(TPage view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPushExit(TPage view, TModel model) { }

        protected virtual Task ViewWillPopEnter(TPage view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPopEnter(TPage view, TModel model) { }

        protected virtual Task ViewWillPopExit(TPage view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidPopExit(TPage view, TModel model) { }

        protected virtual Task ViewWillDestroy(TPage view, TModel model) { return Task.CompletedTask; }

        protected sealed override void Dispose(TPage view)
        {
            base.Dispose(view);
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}