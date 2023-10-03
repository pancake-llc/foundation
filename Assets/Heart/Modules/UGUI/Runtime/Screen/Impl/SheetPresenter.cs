using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Pancake.UI
{
    public abstract class SheetPresenter<TSheet, TView, TModel> : SheetPresenter<TSheet>, IDisposableCollectionHolder
        where TSheet : Sheet<TView, TModel> where TView : View<TModel> where TModel : Model, new()
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private TModel _model;

        protected SheetPresenter(TSheet view)
            : base(view)
        {
        }

        public ICollection<IDisposable> GetDisposableCollection() { return _disposables; }
        protected sealed override void Initialize(TSheet view) { base.Initialize(view); }

        protected sealed override async Task ViewDidLoad(TSheet view)
        {
            await base.ViewDidLoad(view);
            var model = new TModel();
            _model = model;
            _disposables.Add(model);
            view.Setup(model);
            await ViewDidLoad(view, _model);
        }

        protected sealed override async Task ViewWillEnter(TSheet view)
        {
            await base.ViewWillEnter(view);
            await ViewWillEnter(view, _model);
        }

        protected sealed override void ViewDidEnter(TSheet view)
        {
            base.ViewDidEnter(view);
            ViewDidEnter(view, _model);
        }

        protected sealed override async Task ViewWillExit(TSheet view)
        {
            await base.ViewWillExit(view);
            await ViewWillExit(view, _model);
        }

        protected sealed override void ViewDidExit(TSheet view)
        {
            base.ViewDidExit(view);
            ViewDidExit(view, _model);
        }

        protected override async Task ViewWillDestroy(TSheet view)
        {
            await base.ViewWillDestroy(view);
            await ViewWillDestroy(view, _model);
        }

        protected virtual Task ViewDidLoad(TSheet view, TModel model) { return Task.CompletedTask; }

        protected virtual Task ViewWillEnter(TSheet view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidEnter(TSheet view, TModel model) { }

        protected virtual Task ViewWillExit(TSheet view, TModel model) { return Task.CompletedTask; }

        protected virtual void ViewDidExit(TSheet view, TModel model) { }

        protected virtual Task ViewWillDestroy(TSheet view, TModel model) { return Task.CompletedTask; }

        protected sealed override void Dispose(TSheet view)
        {
            base.Dispose(view);
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}