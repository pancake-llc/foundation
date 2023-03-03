using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_gameObject.asset", menuName = "Pancake/Scriptable/Variables/gameObject")]
    [System.Serializable]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
    }
}