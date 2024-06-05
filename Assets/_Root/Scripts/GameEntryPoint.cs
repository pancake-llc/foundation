namespace Pancake.SceneFlow
{
    using Pancake;
    using Sound;
    using VContainer;
    using VContainer.Unity;

    [EditorIcon("icon_entry")]
    public class GameEntryPoint : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<AudioManager>().AsSelf();
        }
    }

}