using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_list_vector3.asset", menuName = "Pancake/Scriptable/ScriptableLists/Vector3")]
    [EditorIcon("scriptable_list")]
    public class ScriptableListVector3 : ScriptableList<Vector3>
    {
    }
}