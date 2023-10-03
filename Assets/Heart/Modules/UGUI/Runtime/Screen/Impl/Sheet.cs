using System.Threading.Tasks;

using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Sheet<TView, TModel> : Sheet where TView : View<TModel> where TModel : Model
    {
        public TView root;
        private bool _isInitialized;
        private TModel _model;

        protected virtual ViewInitMode InitMode => ViewInitMode.BeforeFirstEnter;

        public void Setup(TModel model) { _model = model; }

        public override async Task Initialize()
        {
            Assert.IsNotNull(root);

            await base.Initialize();

            if (InitMode == ViewInitMode.Initialize && !_isInitialized)
            {
                await root.InitializeAsync(_model);
                _isInitialized = true;
            }
        }

        public override async Task WillEnter()
        {
            Assert.IsNotNull(root);

            await base.WillEnter();

            if (InitMode == ViewInitMode.BeforeFirstEnter && !_isInitialized)
            {
                await root.InitializeAsync(_model);
                _isInitialized = true;
            }
        }
    }
}