using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_vector2.asset", menuName = "Pancake/Scriptable/Events/vector2")]
    public class ScriptableEventVector2 : ScriptableEvent<Vector2>
    {
    }
}