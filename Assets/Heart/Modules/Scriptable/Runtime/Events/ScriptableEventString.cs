using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_string.asset", menuName = "Pancake/Scriptable/Events/string")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventString : ScriptableEvent<string>
    {
    }
}