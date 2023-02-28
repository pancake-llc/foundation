using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_gameObject.asset", menuName = "Pancake/ScriptableEvents/GameObject")]
    public class ScriptableEventGameObject : ScriptableEvent<GameObject>
    {
    }
}