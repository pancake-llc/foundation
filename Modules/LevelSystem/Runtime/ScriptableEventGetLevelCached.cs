using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.LevelSystem
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "event_get_level_cached.asset", menuName = "Pancake/Misc/Level System/Event Get Level Cached")]
    public class ScriptableEventGetLevelCached : ScriptableEventFunc<LevelComponent>
    {
    }
}