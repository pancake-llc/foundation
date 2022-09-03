using UnityEngine;

namespace Pancake.SOA
{
    [AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "int Event Listener")]
    public sealed class IntGameEventListener : BaseGameEventListener<int, IntGameEvent, IntUnityEvent>
    {
    }
}