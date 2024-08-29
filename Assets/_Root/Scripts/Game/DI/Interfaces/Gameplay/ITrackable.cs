using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Game.Interfaces
{
    /// <summary>
    /// Represents an object whose position can be tracked.
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Event that is invoked whenever the position of the trackable object has changed.
        /// </summary>
        event UnityAction PositionChanged;

        /// <summary>
        /// The current position of the object in world space.
        /// </summary>
        Vector2 Position { get; }
    }
}