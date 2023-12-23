using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_vector2.asset", menuName = "Pancake/Scriptable/Events/vector2")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventVector2 : ScriptableEvent<Vector2>
    {
    }
}