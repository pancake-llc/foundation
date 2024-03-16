using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_transform.asset", menuName = "Pancake/Scriptable/Variables/transform")]
    [EditorIcon("scriptable_variable")]
    public class TransformVariable : ScriptableVariable<Transform>
    {
    }
}