using System.Threading.Tasks;
using Pancake.UI.Popup;
using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Sheet<TRootView, TViewState> : Sheet where TRootView : AppView<TViewState> where TViewState : AppViewState
    {
        public TRootView root;
        private bool _isInitialized;
        private TViewState _state;

        protected virtual ViewInitializationTiming RootInitializationTiming => ViewInitializationTiming.BeforeFirstEnter;

        public void Setup(TViewState state) { _state = state; }

        public override async Task Initialize()
        {
            Assert.IsNotNull(root);

            await base.Initialize();

            if (RootInitializationTiming == ViewInitializationTiming.Initialize && !_isInitialized)
            {
                await root.InitializeAsync(_state);
                _isInitialized = true;
            }
        }

        public override async Task WillEnter()
        {
            Assert.IsNotNull(root);

            await base.WillEnter();

            if (RootInitializationTiming == ViewInitializationTiming.BeforeFirstEnter && !_isInitialized)
            {
                await root.InitializeAsync(_state);
                _isInitialized = true;
            }
        }
    }
}