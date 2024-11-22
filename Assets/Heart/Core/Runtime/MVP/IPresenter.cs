namespace Pancake
{
    public abstract class BasePresenter<TData, TModel, TView> where TModel : IModel<TData> where TView : IView<TData>
    {
        protected TModel Model { get; }
        protected TView View { get; }

        protected BasePresenter(TModel model, TView view)
        {
            Model = model;
            View = view;
            Initialize();
        }

        protected void Initialize()
        {
            Model.Initialize();
            View.Initialize();
            ConnectModel();
            ConnectView();
        }

        protected virtual void ConnectModel() { Model.OnDataChanged += HandleDataChange; }

        protected virtual void ConnectView() { }

        public virtual void Cleanup()
        {
            View.Cleanup();
            Model.OnDataChanged -= HandleDataChange;
        }

        public virtual void HandleDataChange() { UpdateView(); }

        protected void UpdateView() { View.UpdateView(Model.GetData()); }
    }
}