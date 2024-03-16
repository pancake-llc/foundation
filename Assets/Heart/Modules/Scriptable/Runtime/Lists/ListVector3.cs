using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_vector3.asset", menuName = "Pancake/Scriptable/Lists/vector3")]
    [EditorIcon("scriptable_list")]
    public class ListVector3 : ScriptableList<Vector3>
    {
    }
}