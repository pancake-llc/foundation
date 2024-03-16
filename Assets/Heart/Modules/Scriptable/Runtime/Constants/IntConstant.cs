using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_int.asset", menuName = "Pancake/Scriptable/Constants/int")]
    [EditorIcon("scriptable_const")]
    [HideMonoScript]
    [System.Serializable]
    public class IntConstant : ScriptableConstant<int>
    {
    }
}