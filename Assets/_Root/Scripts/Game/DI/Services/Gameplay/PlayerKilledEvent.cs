using Pancake.Game.Interfaces;
using Sisus.Init;
using UnityEngine;

namespace Pancake.Game
{
    [Service(typeof(IPlayerKilledEvent))]
    [Service(typeof(PlayerKilledEvent))]
    [CreateAssetMenu(menuName = CREATE_ASSET_MENU + "PlayerKilledEvent")]
    public class PlayerKilledEvent : Event, IPlayerKilledEvent
    {
    }
}