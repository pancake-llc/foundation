using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    /// <summary>
    /// An event that is invoked when the player collects a collectable.
    /// <para>
    /// Whenever the event is <see cref="Event.Trigger">triggered</see> all methods
    /// that are listening for the event are invoked.
    /// </para>
    /// </summary>
    [Service(typeof(IItemCollectedEvent), AddressableKey = "item_collected_event")]
    [EditorIcon("so_blue_event")]
    [CreateAssetMenu(menuName = CREATE_ASSET_MENU + "Item Collected Event")]
    public sealed class ItemCollectedEvent : Event, IItemCollectedEvent
    {
    }
}