using Pancake.Sound;
using VContainer;
using VContainer.Unity;

namespace Pancake.Game
{
    public class AppContext : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<AudioManager>().AsSelf();
        }
    }
}