using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_gameobject.asset", menuName = "Pancake/Scriptable/Lists/gameobject")]
    [EditorIcon("so_blue_list")]
    public class ListGameObject : ScriptableList<GameObject>
    {
    }
}