using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_string_pair.asset", menuName = "Pancake/Scriptable/ScriptableVariables/string pair")]
    [System.Serializable]
    public class ScritpableStringPairVariable : ScriptableVariable<StringPair>
    {
    }
}