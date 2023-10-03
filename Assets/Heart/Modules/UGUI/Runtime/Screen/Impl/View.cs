using Pancake.Threading.Tasks;

namespace Pancake.UI
{
    public abstract class View<TModel> : GameComponent where TModel : Model
    {
        private bool _isInitialized;

        public async UniTask InitializeAsync(TModel model)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            await Initialize(model);
        }

        protected abstract UniTask Initialize(TModel model);
    }
}