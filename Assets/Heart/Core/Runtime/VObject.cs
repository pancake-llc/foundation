using VContainer;
using VContainer.Unity;

namespace Pancake
{
    public class VObject<T> where T : LifetimeScope
    {
        [Inject] protected LifetimeScope context;

        protected T Context => context as T;
    }
}