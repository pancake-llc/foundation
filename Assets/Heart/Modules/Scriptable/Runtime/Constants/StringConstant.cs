using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_string.asset", menuName = "Pancake/Scriptable/Constants/string")]
    [EditorIcon("so_blue_const")]
    [System.Serializable]
    public class StringConstant : ScriptableConstant<string>
    {
    }
}