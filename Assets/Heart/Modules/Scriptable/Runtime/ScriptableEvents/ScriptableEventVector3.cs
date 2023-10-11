using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_vector3.asset", menuName = "Pancake/Scriptable/Events/vector3")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventVector3 : ScriptableEvent<Vector3>
    {
    }
}