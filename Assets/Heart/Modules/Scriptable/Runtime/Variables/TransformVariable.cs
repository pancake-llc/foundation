using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_transform.asset", menuName = "Pancake/Scriptable/Variables/transform")]
    [EditorIcon("so_blue_variable2")]
    public class TransformVariable : ScriptableVariable<Transform>
    {
#if UNITY_EDITOR
        protected override bool IgnoreDraw => true;
#endif
    }
}