using UnityEngine;

namespace Pancake.SOA
{
    [AddComponentMenu(SOArchitecture_Utility.EVENT_LISTENER_SUBMENU + "byte Event Listener")]
    public sealed class ByteGameEventListener : BaseGameEventListener<byte, ByteGameEvent, ByteUnityEvent>
    {
    }
}