using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_const_string.asset", menuName = "Pancake/Scriptable/ScriptableConstants/string")]
    [EditorIcon("scriptable_const")]
    public class StringConstant : ScriptableConstant<string>
    {
    }
}