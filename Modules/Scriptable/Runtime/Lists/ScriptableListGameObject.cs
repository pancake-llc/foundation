using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_list")]
    [CreateAssetMenu(fileName = "scriptable_list_gameObject.asset", menuName = "Pancake/Scriptable/Lists/gameObject")]
    public class ScriptableListGameObject : ScriptableList<GameObject>
    {
    }
}