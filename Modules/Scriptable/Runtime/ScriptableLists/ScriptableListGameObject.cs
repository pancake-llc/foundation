using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_list_gameObject.asset", menuName = "Pancake/Scriptable/ScriptableLists/GameObject")]
    [EditorIcon("scriptable_list")]
    public class ScriptableListGameObject : ScriptableList<GameObject>
    {
    }
}