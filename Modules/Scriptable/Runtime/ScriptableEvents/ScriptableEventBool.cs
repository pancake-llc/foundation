using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_bool.asset", menuName = "Pancake/Scriptable/Events/bool")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventBool : ScriptableEvent<bool>
    {
    }
}