using Pancake.Threading.Tasks;

namespace Pancake.UI
{
    public abstract class AppView<TState> : GameComponent where TState : AppViewState
    {
        private bool _isInitialized;

        public async UniTask InitializeAsync(TState state)
        {
            if (_isInitialized) return;
            _isInitialized = true;
            await Initialize(state);
        }

        protected abstract UniTask Initialize(TState state);
    }
}