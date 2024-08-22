using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "const_int.asset", menuName = "Pancake/Scriptable/const int")]
    [EditorIcon("so_blue_const")]
    [System.Serializable]
    public class IntConstant : ScriptableConstant<int>
    {
    }
}