using System.Threading.Tasks;
using Pancake.UI.Popup;
using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Modal<TRootView, TViewState> : Modal where TRootView : AppView<TViewState> where TViewState : AppViewState
    {
        public TRootView root;

        private bool _isInitialized;
        private TViewState _state;

        protected virtual ViewInitializationTiming RootInitializationTiming => ViewInitializationTiming.BeforeFirstEnter;

        public void Setup(TViewState state)
        {
            Assert.IsNotNull(root);
            _state = state;
        }

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

        public override async Task WillPushEnter()
        {
            Assert.IsNotNull(root);

            await base.WillPushEnter();

            if (RootInitializationTiming == ViewInitializationTiming.BeforeFirstEnter && !_isInitialized)
            {
                await root.InitializeAsync(_state);
                _isInitialized = true;
            }
        }
    }
}