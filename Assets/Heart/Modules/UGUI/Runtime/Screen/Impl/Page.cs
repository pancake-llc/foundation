using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Page<TView, TModel> : Page where TView : View<TModel> where TModel : Model
    {
        public TView view;

        private bool _isInitialized;
        private TModel _model;

        protected virtual ViewInitMode InitMode => ViewInitMode.BeforeFirstEnter;

        public void Setup(TModel model) { _model = model; }

        public override async Task Initialize()
        {
            Assert.IsNotNull(view);

            await base.Initialize();

            if (InitMode == ViewInitMode.Initialize && !_isInitialized)
            {
                await view.InitializeAsync(_model);
                _isInitialized = true;
            }
        }

        public override async Task WillPushEnter()
        {
            Assert.IsNotNull(view);

            await base.WillPushEnter();

            if (InitMode == ViewInitMode.BeforeFirstEnter && !_isInitialized)
            {
                await view.InitializeAsync(_model);
                _isInitialized = true;
            }
        }
    }
}