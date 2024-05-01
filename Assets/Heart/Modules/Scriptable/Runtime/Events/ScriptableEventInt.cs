using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_int.asset", menuName = "Pancake/Scriptable/Events/int")]
    [EditorIcon("so_blue_event")]
    public class ScriptableEventInt : ScriptableEvent<int>
    {
    }
}