using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_int.asset", menuName = "Pancake/Scriptable/Events/int")]
    public class ScriptableEventInt : ScriptableEvent<int>
    {
    }
}