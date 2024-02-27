using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_const_float.asset", menuName = "Pancake/Scriptable/Constants/float")]
    [EditorIcon("scriptable_const")]
    [HideMonoScript]
    public class FloatConstant : ScriptableConstant<float>
    {
    }
}