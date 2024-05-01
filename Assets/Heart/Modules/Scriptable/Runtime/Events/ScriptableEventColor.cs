using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_color.asset", menuName = "Pancake/Scriptable/Events/color")]
    [EditorIcon("so_blue_event")]
    public class ScriptableEventColor : ScriptableEvent<Color>
    {
    }
}