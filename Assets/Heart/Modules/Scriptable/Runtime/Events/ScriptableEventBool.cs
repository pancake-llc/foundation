using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_bool.asset", menuName = "Pancake/Scriptable/Events/bool")]
    [EditorIcon("so_blue_event")]
    public class ScriptableEventBool : ScriptableEvent<bool>
    {
    }
}