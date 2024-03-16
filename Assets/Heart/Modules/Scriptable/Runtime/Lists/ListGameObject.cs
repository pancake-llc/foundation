using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_gameobject.asset", menuName = "Pancake/Scriptable/Lists/gameobject")]
    [EditorIcon("scriptable_list")]
    public class ListGameObject : ScriptableList<GameObject>
    {
    }
}