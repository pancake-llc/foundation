using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_float.asset", menuName = "Pancake/Scriptable/Events/float")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventFloat : ScriptableEvent<float>
    {
    }
}