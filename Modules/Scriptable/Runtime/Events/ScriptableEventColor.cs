using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_color.asset", menuName = "Pancake/Scriptable/Events/color")]
    public class ScriptableEventColor : ScriptableEvent<Color>
    {
    }
}