using UnityEngine;

namespace Pancake.Game.Interfaces
{
    /// <summary>
    /// Represents an event that is invoked when move input given for the player has changed.
    /// </summary>
    public interface IMoveInputChangedEvent : IEvent<Vector2>
    {
    }
}