using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_int.asset", menuName = "Pancake/Scriptable/Events/int")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventInt : ScriptableEvent<int>
    {
    }
}