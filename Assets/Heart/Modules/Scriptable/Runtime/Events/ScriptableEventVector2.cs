using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_vector2.asset", menuName = "Pancake/Scriptable/Events/vector2")]
    [EditorIcon("so_blue_event")]
    public class ScriptableEventVector2 : ScriptableEvent<Vector2>
    {
    }
}