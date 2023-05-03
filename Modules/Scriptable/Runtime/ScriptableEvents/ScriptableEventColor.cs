using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_color.asset", menuName = "Pancake/Scriptable/ScriptableEvents/color")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventColor : ScriptableEvent<Color>
    {
    }
}