using System;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [Serializable]
    public class LevelUnit
    {
        [Guid] public string id;
        public GameObject prefab;
    }


    [EditorIcon("scriptable_setting")]
    [Searchable]
    [CreateAssetMenu(fileName = "level_system_setting.asset", menuName = "Pancake/Misc/Level System Setting")]
    public class LevelSystemSetting : ScriptableObject
    {
        [SerializeField] private string fileName = "level_container";

        [Message("Please right click on Id field and do recreate id when adding element to units array")] [SerializeField, Range(1, 100)]
        private int levelPerFile = 30;

        [SerializeField, Array] private LevelUnit[] units;

        public string FileName => fileName;
        public int LevelPerFile => levelPerFile;
        public LevelUnit[] Units => units;
    }
}