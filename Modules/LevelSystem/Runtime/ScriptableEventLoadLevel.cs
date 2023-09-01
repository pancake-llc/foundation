using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "event_load_level.asset", menuName = "Pancake/Misc/Level System/Event Load Level")]
    public class ScriptableEventLoadLevel : ScriptableEventFuncT_TResult<int, UniTask<LevelComponent>>
    {
    }
}