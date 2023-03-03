using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_list")]
    [CreateAssetMenu(fileName = "scriptable_list_vector3.asset", menuName = "Pancake/Scriptable/Lists/vector3")]
    public class ScriptableListVector3 : ScriptableList<Vector3>
    {
    }
}