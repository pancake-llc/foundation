using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_const_int.asset", menuName = "Pancake/Scriptable/Constants/int")]
    [EditorIcon("scriptable_const")]
    [HideMonoScript]
    public class IntConstant : ScriptableConstant<int>
    {
    }
}