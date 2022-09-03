using UnityEngine;

namespace Pancake.SOA
{
    [AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "ulong Event Listener")]
    public sealed class ULongGameEventListener : BaseGameEventListener<ulong, ULongGameEvent, ULongUnityEvent>
    {
    }
}