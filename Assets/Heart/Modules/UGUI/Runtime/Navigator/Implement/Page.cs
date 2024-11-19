#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;
#endif

namespace Pancake.UI
{
    public abstract class Page<TView> : Page where TView : View
    {
        public TView view;

        private bool _isInitialized;

        protected virtual ViewInitMode InitMode => ViewInitMode.BeforeFirstEnter;

#if PANCAKE_UNITASK
        public override async UniTask Initialize()
        {
            Assert.IsNotNull(view);

            await base.Initialize();

            if (InitMode == ViewInitMode.Initialize && !_isInitialized)
            {
                await view.InitializeAsync();
                _isInitialized = true;
            }
        }

        public override async UniTask WillPushEnter()
        {
            Assert.IsNotNull(view);

            await base.WillPushEnter();

            if (InitMode == ViewInitMode.BeforeFirstEnter && !_isInitialized)
            {
                await view.InitializeAsync();
                _isInitialized = true;
            }
        }
#endif
    }
}