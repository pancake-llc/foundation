using Pancake.Game.Interfaces;

namespace Pancake.Game
{
    /// <summary>
    /// Component that invokes an <see cref="UnityEngine.Events.UnityEvent"/> whenever the
    /// <see cref="IPlayerKilledEvent"/> event is triggered.
    /// </summary>
    [EditorIcon("icon_event_listener")]
    public class OnPlayerKilled : OnEvent<IPlayerKilledEvent>
    {
    }
}