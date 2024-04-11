using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_int.asset", menuName = "Pancake/Scriptable/Constants/int")]
    [EditorIcon("scriptable_const")]

    [System.Serializable]
    public class IntConstant : ScriptableConstant<int>
    {
    }
}