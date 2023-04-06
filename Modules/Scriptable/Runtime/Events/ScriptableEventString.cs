using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_string.asset", menuName = "Pancake/Scriptable/Events/string")]
    public class ScriptableEventString : ScriptableEvent<string>
    {
    }
}