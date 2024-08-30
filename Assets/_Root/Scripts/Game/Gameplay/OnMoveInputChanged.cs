using Pancake.Game.Interfaces;
using UnityEngine;

namespace Pancake.Game
{
    /// <summary>
    /// Component that invokes an <see cref="UnityEngine.Events.UnityEvent"/> whenever the
    /// <see cref="IMoveInputChangedEvent"/> event is triggered.
    /// </summary>
    [EditorIcon("icon_event_listener")]
    public class OnMoveInputChanged : OnEvent<IMoveInputChangedEvent, Vector2>
    {
        
    }
}