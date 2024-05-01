using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_gameobject.asset", menuName = "Pancake/Scriptable/Variables/gameobject")]
    [EditorIcon("so_blue_variable2")]
    public class GameObjectVariable : ScriptableVariable<GameObject>
    {
#if UNITY_EDITOR
        protected override bool IgnoreDraw => true;
#endif
    }
}