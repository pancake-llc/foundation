using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_vector2int.asset", menuName = "Pancake/Scriptable/Events/vector2int")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventVector2Int : ScriptableEvent<Vector2Int>
    {
    }
}