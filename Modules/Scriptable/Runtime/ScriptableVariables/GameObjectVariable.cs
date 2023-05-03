using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_gameObject.asset", menuName = "Pancake/Scriptable/ScriptableVariables/gameObject")]
    [EditorIcon("scriptable_variable")]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
    }
}