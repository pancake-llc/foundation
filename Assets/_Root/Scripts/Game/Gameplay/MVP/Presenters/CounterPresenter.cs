namespace Pancake.Game
{
    public class CounterPresenter : BasePresenter<int, CounterModel, CounterView>
    {
        private CounterPresenter(CounterModel model, CounterView view)
            : base(model, view)
        {
        }

        protected override void ConnectView()
        {
            base.ConnectView();

            View.IncreaseCounterEvent += OnIncreaseCounter;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            View.IncreaseCounterEvent -= OnIncreaseCounter;
        }

        private void OnIncreaseCounter() { Model.SetData(Model.GetData() + 1); }

        public class Builder
        {
            private readonly CounterModel _model = new();

            public Builder WithData(int data)
            {
                _model.SetData(data);
                return this;
            }

            public CounterPresenter Build(CounterView view)
            {
                var presenter = new CounterPresenter(_model, view);
                presenter.UpdateView(); // update data for WithData
                return presenter;
            }
        }
    }
}