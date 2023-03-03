using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_gameObject.asset", menuName = "Pancake/Scriptable/Events/gameObject")]
    public class ScriptableEventGameObject : ScriptableEvent<GameObject>
    {
    }
}