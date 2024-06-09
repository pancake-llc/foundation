using Pancake.Sound;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_entry")]
    public class GameRootScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<AudioManager>().AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}