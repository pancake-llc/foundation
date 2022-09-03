using UnityEngine;

namespace Pancake.SOA
{
    [AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "double Event Listener")]
    public sealed class DoubleGameEventListener : BaseGameEventListener<double, DoubleGameEvent, DoubleUnityEvent>
    {
    }
}