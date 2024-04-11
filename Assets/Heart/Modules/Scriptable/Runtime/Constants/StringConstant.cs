using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "const_string.asset", menuName = "Pancake/Scriptable/Constants/string")]
    [EditorIcon("scriptable_const")]

    [System.Serializable]
    public class StringConstant : ScriptableConstant<string>
    {
    }
}