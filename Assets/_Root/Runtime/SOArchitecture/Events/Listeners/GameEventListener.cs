using UnityEngine;
using UnityEngine.Events;

namespace Pancake.SOA
{
    [AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "Game Event Listener")]
    [ExecuteInEditMode]
    public sealed class GameEventListener : BaseGameEventListener<GameEventBase, UnityEvent>
    {
    }
}