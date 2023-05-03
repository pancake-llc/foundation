using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_float.asset", menuName = "Pancake/Scriptable/ScriptableEvents/float")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventFloat : ScriptableEvent<float>
    {
    }
}