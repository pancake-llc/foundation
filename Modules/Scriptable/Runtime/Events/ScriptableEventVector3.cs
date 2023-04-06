using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_vector3.asset", menuName = "Pancake/Scriptable/Events/vector3")]
    public class ScriptableEventVector3 : ScriptableEvent<Vector3>
    {
    }
}