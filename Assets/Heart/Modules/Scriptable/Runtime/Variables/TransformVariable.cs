using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_transform.asset", menuName = "Pancake/Scriptable/Variables/transform")]
    [EditorIcon("scriptable_variable_runtime")]
    public class TransformVariable : ScriptableVariable<Transform>
    {
#if UNITY_EDITOR
        protected override bool EditorDisableValue => true;
#endif
    }
}