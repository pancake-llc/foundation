using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_gameObject.asset", menuName = "Pancake/Scriptable/ScriptableEvents/GameObject")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventGameObject : ScriptableEvent<GameObject>
    {
    }
}