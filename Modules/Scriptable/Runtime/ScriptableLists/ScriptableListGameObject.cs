using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_list_gameobject.asset", menuName = "Pancake/Scriptable/Lists/gameobject")]
    [EditorIcon("scriptable_list")]
    public class ScriptableListGameObject : ScriptableList<GameObject>
    {
    }
}