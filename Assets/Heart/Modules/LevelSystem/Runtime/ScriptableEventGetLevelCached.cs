﻿using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Searchable]
    [EditorIcon("so_blue_event")]
    [CreateAssetMenu(fileName = "event_get_level_cached.asset", menuName = "Pancake/Misc/Level System/Event Get Level Cached")]
    public class ScriptableEventGetLevelCached : ScriptableEventFunc<LevelComponent>
    {
    }
}