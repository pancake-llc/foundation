using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Popup<TView> : Popup where TView : View
    {
        public TView view;

        private bool _isInitialized;

        protected virtual ViewInitMode InitMode => ViewInitMode.BeforeFirstEnter;

        public override async Task Initialize()
        {
            Assert.IsNotNull(view);

            await base.Initialize();

            if (InitMode == ViewInitMode.Initialize && !_isInitialized)
            {
                await view.InitializeAsync();
                _isInitialized = true;
            }
        }

        public override async Task WillPushEnter()
        {
            Assert.IsNotNull(view);

            await base.WillPushEnter();

            if (InitMode == ViewInitMode.BeforeFirstEnter && !_isInitialized)
            {
                await view.InitializeAsync();
                _isInitialized = true;
            }
        }
    }
}