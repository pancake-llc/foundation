using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "event_gameobject.asset", menuName = "Pancake/Scriptable/Events/gameobject")]
    [EditorIcon("scriptable_event")]
    public class ScriptableEventGameObject : ScriptableEvent<GameObject>
    {
    }
}