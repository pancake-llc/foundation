using System;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_list_action.asset", menuName = "Pancake/Scriptable/ScriptableLists/Action")]
    [EditorIcon("scriptable_list")]
    public class ScriptableListAction : ScriptableList<Action>
    {
    }
}