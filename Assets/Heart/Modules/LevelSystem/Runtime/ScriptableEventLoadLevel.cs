#if PANCAKE_UNITASK
using Pancake.Scriptable;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Searchable]
    [EditorIcon("so_blue_event")]
    [CreateAssetMenu(fileName = "event_load_level.asset", menuName = "Pancake/Misc/Level System/Event Load Level")]
    public class ScriptableEventLoadLevel : ScriptableEventFuncT_TResult<int, UniTask<LevelComponent>>
    {
    }
}
#endif