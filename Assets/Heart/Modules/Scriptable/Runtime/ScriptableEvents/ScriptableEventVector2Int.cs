using UnityEngine;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventVector2.
    /// </summary>
    [CreateAssetMenu(fileName = "scriptable_event_vector2int.asset", menuName = "Pancake/Scriptable/Events/vector2int")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventVector2Int : ScriptableEvent<Vector2Int>
    {
    }
}