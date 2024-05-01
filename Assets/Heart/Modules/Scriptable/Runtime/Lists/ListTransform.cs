using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_transform.asset", menuName = "Pancake/Scriptable/Lists/transform")]
    [EditorIcon("so_blue_list")]
    public class ListTransform : ScriptableList<Transform>
    {
    }
}