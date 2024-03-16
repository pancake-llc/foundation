using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_transform.asset", menuName = "Pancake/Scriptable/Lists/transform")]
    [EditorIcon("scriptable_list")]
    public class ListTransform : ScriptableList<Transform>
    {
    }
}