using Pancake.Notification;
using Pancake.Sound;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_entry")]
    public class GameRootScope : LifetimeScope
    {
        [SerializeField] private ScriptableNotification dailyNotification;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(dailyNotification);
            builder.RegisterEntryPoint<RequireScheduleNotification>();
            
            builder.RegisterEntryPoint<AudioManager>().AsSelf();
            builder.Register<SceneLoader>(Lifetime.Singleton);
        }
    }
}