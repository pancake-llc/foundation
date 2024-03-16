using System;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "list_action.asset", menuName = "Pancake/Scriptable/Lists/action")]
    [EditorIcon("scriptable_list")]
    public class ListAction : ScriptableList<Action>
    {
    }
}