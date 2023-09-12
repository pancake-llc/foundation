using UnityEngine;

namespace Pancake.Scriptable
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "event_get_gameobject.asset", menuName = "Pancake/Scriptable/Events/func gameobject")]
    public class ScriptableEventGetGameObject : ScriptableEventFunc<GameObject>
    {
    }
}