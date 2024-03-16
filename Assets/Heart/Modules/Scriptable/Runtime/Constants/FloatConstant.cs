using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_float.asset", menuName = "Pancake/Scriptable/Constants/float")]
    [EditorIcon("scriptable_const")]
    [HideMonoScript]
    [System.Serializable]
    public class FloatConstant : ScriptableConstant<float>
    {
    }
}