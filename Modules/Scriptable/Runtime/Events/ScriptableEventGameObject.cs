using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_event_gameObject.asset", menuName = "Pancake/Scriptable/Events/gameObject")]
    public class ScriptableEventGameObject : ScriptableEvent<GameObject>
    {
    }
}