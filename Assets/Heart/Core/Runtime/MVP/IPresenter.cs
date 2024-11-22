namespace Pancake
{
    public interface IPresenter<in TData>
    {
        void Initialize();
        void Cleanup();
        void HandleDataChange(TData data);
    }

    public abstract class BasePresenter<TData, TModel, TView> : IPresenter<TData> where TModel : IModel<TData> where TView : IView<TData>
    {
        protected TModel Model { get; }
        protected TView View { get; }

        protected BasePresenter(TModel model, TView view)
        {
            Model = model;
            View = view;
        }

        public virtual void Initialize()
        {
            Model.Initialize();
            View.Initialize();
            View.OnDataChanged += HandleDataChange;
            UpdateView();
        }

        public virtual void Cleanup()
        {
            View.Cleanup();
            View.OnDataChanged -= HandleDataChange;
        }

        public virtual void HandleDataChange(TData data)
        {
            Model.SetData(data);
            UpdateView();
        }

        protected void UpdateView() { View.UpdateView(Model.GetData()); }
    }
}