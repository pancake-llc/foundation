using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_string.asset", menuName = "Pancake/Scriptable/Events/string")]
    [EditorIcon("so_blue_event")]
    public class ScriptableEventString : ScriptableEvent<string>
    {
    }
}