using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_float.asset", menuName = "Pancake/Scriptable/Events/float")]
    public class ScriptableEventFloat : ScriptableEvent<float>
    {
    }
}