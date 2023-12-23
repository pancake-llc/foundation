using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_gameobject.asset", menuName = "Pancake/Scriptable/Variables/gameobject")]
    [EditorIcon("scriptable_variable")]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
    }
}