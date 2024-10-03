using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Pancake.Common;
using Pancake.Linq;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
#if PANCAKE_ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif

namespace Pancake.LevelSystem
{
    [EditorIcon("icon_default")]
    public class LevelCoordinator : GameComponent
    {
        [SerializeField] private StringConstant type;
        [SerializeField] private ELoopType loopType = ELoopType.Shuffle;
        [SerializeField, Space] private LevelSetting[] levelSettings;
        [ReadOnly] [SerializeField] private LevelComponent previousLevelLoaded;
        [ReadOnly] [SerializeField] private LevelComponent nextLevelLoaded;
        private bool _isReplay;
        private int _segmentLength;
        private int _totalLevel;
        private readonly List<string> _typeMappingOfSegment = new();
        private static readonly Dictionary<string, LevelDimension> Dimensions = new();

        private int NgNumber { get => Data.Load($"level_{type.Value}_ng_number", 1); set => Data.Save($"level_{type.Value}_ng_number", value); }
        public static int GetCurrentLevelIndex(string type) => Data.Load($"current_level_{type}_index", 0);

        public static void SetCurrentLevelIndex(string type, int value)
        {
            Data.Save($"current_level_{type}_index", value);
            Dimensions[type].ChangeLevelIndex(value);
        }

        public static void IncreaseLevelIndex(string type, int amount)
        {
            int value = GetCurrentLevelIndex(type) + amount;
            SetCurrentLevelIndex(type, value);
        }

        private void Awake()
        {
            // todo: change flow to avoid data not load before use
            Dimensions[type.Value] = new LevelDimension();
            _segmentLength = 0;
            _totalLevel = 0;
            _typeMappingOfSegment.Clear();

            _totalLevel = levelSettings.Filter(s => s).Distinct(s => s.LevelType.Value).Sum(s => s.TotalLevel);
            foreach (var levelSetting in levelSettings)
            {
                _segmentLength += levelSetting.NumberInSegment;
                // flat map
                for (var i = 0; i < levelSetting.NumberInSegment; i++)
                {
                    _typeMappingOfSegment.Add(levelSetting.LevelType.Value);
                }
            }

            CheckCacheLevel(GetCurrentLevelIndex(type.Value));
        }

        private void OnEnable()
        {
#if PANCAKE_ADDRESSABLE && PANCAKE_UNITASK
            var dimesion = Dimensions[type.Value];
            dimesion.LoadLevelEvent += OnLoadLevel;
            dimesion.GetNextLevelLoadedEvent += OnGetNextLevel;
            dimesion.GetPreviousLevelLoadedEvent += OnGetPreviousLevel;
#endif
        }

        private void OnDisable()
        {
#if PANCAKE_ADDRESSABLE && PANCAKE_UNITASK
            var dimesion = Dimensions[type.Value];
            dimesion.LoadLevelEvent -= OnLoadLevel;
            dimesion.GetNextLevelLoadedEvent -= OnGetNextLevel;
            dimesion.GetPreviousLevelLoadedEvent -= OnGetPreviousLevel;
            Dimensions.Remove(type.Value);
#endif
        }

#if PANCAKE_ADDRESSABLE && PANCAKE_UNITASK
        private async UniTask<LevelComponent> OnLoadLevel(int currentLevelIndex)
        {
            int indexInSegment = currentLevelIndex % _segmentLength;
            int indexSegment = currentLevelIndex / _segmentLength;
            string t = TypeOfIndex(indexInSegment);
            int countOfType = CountOfTypeInSegment(t);
            int countOfTypeBeforeIndex = CountOfTypeBeforeIndexInSegment(indexInSegment);

            int index = IndexInLevelContainer(indexSegment, countOfType, countOfTypeBeforeIndex);
            var setting = levelSettings.Filter(level => level.LevelType.Value == t).First();

            if (currentLevelIndex > _totalLevel - 1)
            {
                index = index % levelSettings.Filter(level => level.LevelType.Value == t).First().TotalLevel;

                if (loopType == ELoopType.Shuffle)
                {
                    if (currentLevelIndex >= _totalLevel * NgNumber)
                    {
                        foreach (var levelSetting in levelSettings)
                        {
                            MakeCacheLevel(levelSetting);
                        }

                        NgNumber++;
                    }

                    var result = await Addressables.LoadAssetAsync<GameObject>(string.Format(setting.Schema, setting.CacheLevels[index] + 1));
                    if (nextLevelLoaded != null) previousLevelLoaded = nextLevelLoaded;
                    nextLevelLoaded = result.GetComponent<LevelComponent>();
                    nextLevelLoaded.Init(setting.CacheLevels[index] + 1, currentLevelIndex); // write into prefab
                    return nextLevelLoaded;
                }
            }

            var obj = await Addressables.LoadAssetAsync<GameObject>(string.Format(setting.Schema, index + 1));
            if (nextLevelLoaded != null) previousLevelLoaded = nextLevelLoaded;
            nextLevelLoaded = obj.GetComponent<LevelComponent>();
            nextLevelLoaded.Init(index + 1, currentLevelIndex); // write into prefab
            return nextLevelLoaded;
        }

        private LevelComponent OnGetNextLevel() => nextLevelLoaded;

        private LevelComponent OnGetPreviousLevel() => previousLevelLoaded;
#endif

        private void CheckCacheLevel(int currentLevelIndex)
        {
            if (currentLevelIndex < _totalLevel * NgNumber)
            {
                foreach (var levelSetting in levelSettings)
                {
                    // try load cached data if exist
                    levelSetting.CacheLevels = Data.Load<List<int>>($"level_{type}_{levelSetting.LevelType}_cache");
                }

                return;
            }

            foreach (var levelSetting in levelSettings)
            {
                MakeCacheLevel(levelSetting);
            }

            NgNumber++;
        }

        private void MakeCacheLevel(LevelSetting levelSetting)
        {
            var tempList = new List<int>();
            for (int i = 0; i < levelSetting.TotalLevel; i++)
            {
                int tempIndex = i;
                var _ = levelSetting.LevelsLoopReplace.Map(level => level).Filter(level => level.source == tempIndex).FirstOrDefault();
                tempList.Add(_.source == i ? _.replace : i);
            }

            tempList.Shuffle();
            levelSetting.CacheLevels = tempList;
            Data.Save($"level_{type}_{levelSetting.LevelType}_cache", levelSetting.CacheLevels);
        }

        private int CountOfTypeBeforeIndexInSegment(int segmentIndex)
        {
            var sum = 0;
            for (var i = 0; i < segmentIndex; i++)
            {
                if (_typeMappingOfSegment[i] == _typeMappingOfSegment[segmentIndex]) sum++;
            }

            return sum;
        }

        private string TypeOfIndex(int segmentIndex) { return _typeMappingOfSegment[segmentIndex]; }

        private int CountOfTypeInSegment(string type)
        {
            var sum = 0;
            foreach (string t in _typeMappingOfSegment)
            {
                if (type == t) sum++;
            }

            return sum;
        }

        private int IndexInLevelContainer(int indexSegment, int countOfType, int countOfTypeBeforeIndex) { return countOfType * indexSegment + countOfTypeBeforeIndex; }

        public static void RegisterLevelIndexChanged(string id, Action<int> action) { Dimensions[id].ChangeLevelIndexEvent += action; }

        public static void UnRegisterLevelIndexChanged(string id, Action<int> action)
        {
            if (Dimensions.TryGetValue(id, out var dimension)) dimension.ChangeLevelIndexEvent -= action;
        }

#if PANCAKE_UNITASK
        public static UniTask<LevelComponent> LoadLevel(string id, int index) { return Dimensions[id].LoadLevel(index); }
#endif
        public static LevelComponent GetNextLevelLoaded(string id) { return Dimensions[id].GetNextlevelLoaded(); }
        public static LevelComponent GetPreviousLevelLoaded(string id) { return Dimensions[id].GetPreviousLevelLoaded(); }
    }
}