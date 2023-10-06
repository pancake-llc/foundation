using System.Threading.Tasks;

using UnityEngine.Assertions;

namespace Pancake.UI
{
    public abstract class Sheet<TView> : Sheet where TView : View
    {
        public TView root;
        private bool _isInitialized;

        protected virtual ViewInitMode InitMode => ViewInitMode.BeforeFirstEnter;

        public override async Task Initialize()
        {
            Assert.IsNotNull(root);

            await base.Initialize();

            if (InitMode == ViewInitMode.Initialize && !_isInitialized)
            {
                await root.InitializeAsync();
                _isInitialized = true;
            }
        }

        public override async Task WillEnter()
        {
            Assert.IsNotNull(root);

            await base.WillEnter();

            if (InitMode == ViewInitMode.BeforeFirstEnter && !_isInitialized)
            {
                await root.InitializeAsync();
                _isInitialized = true;
            }
        }
    }
}