using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Searchable]
    [HideMonoScript]
    [EditorIcon("scriptable_setting")]
    [CreateAssetMenu(fileName = "level_setting.asset", menuName = "Pancake/Misc/Level System/Setting")]
    public class LevelSetting : ScriptableObject
    {
        [field: SerializeField] public int TotalLevel { get; private set; }
        [field: SerializeField] public int NumberInSegment { get; private set; }
        [field: SerializeField] public string Schema { get; private set; } = "Level_{0}";
        [field: SerializeField] public StringConstant LevelType { get; private set; }
        [field: SerializeField, Array] public LevelLoopReplace[] LevelsLoopReplace { get; private set; }


        [HideInInspector] public List<int> CacheLevels { get; set; } = new List<int>();
    }
}