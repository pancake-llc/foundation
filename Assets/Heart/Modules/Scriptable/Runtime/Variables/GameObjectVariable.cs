using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_gameobject.asset", menuName = "Pancake/Scriptable/Variables/gameobject")]
    [EditorIcon("scriptable_variable_runtime")]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
#if UNITY_EDITOR
        protected override bool IgnoreDraw => true;
#endif
    }
}