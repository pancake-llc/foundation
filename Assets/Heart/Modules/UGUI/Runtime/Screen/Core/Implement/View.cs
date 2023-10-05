using Pancake.Threading.Tasks;

namespace Pancake.UI
{
    public abstract class View : GameComponent
    {
        private bool _isInitialized;

        public async UniTask InitializeAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            await Initialize();
        }

        protected abstract UniTask Initialize();

        public abstract void Refresh();
    }
}