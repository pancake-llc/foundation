using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_level.asset", menuName = "Pancake/Misc/Level System/Scriptable Event Level")]
    public class ScriptableEventLevel : ScriptableEventFunc<int, LevelComponent>
    {
    }
}