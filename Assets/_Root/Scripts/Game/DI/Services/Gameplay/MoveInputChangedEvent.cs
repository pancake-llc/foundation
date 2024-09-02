using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    /// <summary>
    /// An event that is invoked when move input given for the player has changed.
    /// <para>
    /// Whenever the event is <see cref="Event.Trigger">triggered</see> all methods
    /// that are listening for the event are invoked.
    /// </para>
    /// </summary>
    [Service(typeof(IMoveInputChangedEvent), AddressableKey = "move_input_changed_event")]
    [Service(typeof(MoveInputChangedEvent), AddressableKey = "move_input_changed_event")]
    [EditorIcon("so_blue_event")]
    [CreateAssetMenu(menuName = CREATE_ASSET_MENU + "Move Input Changed Event")]
    public sealed class MoveInputChangedEvent : Event<Vector2>, IMoveInputChangedEvent
    {
    }
}