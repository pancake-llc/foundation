using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "const_float.asset", menuName = "Pancake/Scriptable/const float")]
    [EditorIcon("so_blue_const")]
    [System.Serializable]
    public class FloatConstant : ScriptableConstant<float>
    {
    }
}