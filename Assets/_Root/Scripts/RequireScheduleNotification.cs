using Pancake.Notification;
using VContainer;
using VContainer.Unity;

namespace Pancake.SceneFlow
{
    public class RequireScheduleNotification : IInitializable
    {
        [Inject] private readonly ScriptableNotification _dailyNotification;

        public void Initialize() { _dailyNotification.Schedule(); }
    }
}