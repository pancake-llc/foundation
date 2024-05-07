using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_float.asset", menuName = "Pancake/Scriptable/Constants/float")]
    [EditorIcon("so_blue_const")]
    [System.Serializable]
    public class FloatConstant : ScriptableConstant<float>
    {
    }
}