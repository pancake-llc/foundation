using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_transform.asset", menuName = "Pancake/Scriptable/Events/transform")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventTransform : ScriptableEvent<Transform>
    {
    }
}