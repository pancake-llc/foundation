using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Linq;
using Pancake.Scriptable;
using Pancake.Threading.Tasks;
using UnityEngine;
#if PANCAKE_ADDRESSABLE
using UnityEngine.AddressableAssets;
#endif

namespace Pancake.LevelSystem
{
    public class LevelCoordinator : GameComponent
    {
        [SerializeField] private string id = "normal";
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private ScriptableEventLevel eventGetLevel;
        [SerializeField] private ELoopType loopType = ELoopType.Shuffle;
        [HorizontalLine(Space = 10)] [SerializeField, Array] private LevelSetting[] levelSettings;

        [SerializeField, ReadOnly] private LevelComponent previousLevelLoaded;
        [SerializeField, ReadOnly] private LevelComponent nextLevelLoaded;


        private bool _isReplay;
        private int _segmentLength;
        private int _totalLevel;
        private readonly List<string> _typeMappingOfSegment = new List<string>();

        private int NgNumber { get => Data.Load($"level_{id}_ng_number", 1); set => Data.Save($"level_{id}_ng_number", value); }

        private void Start()
        {
            _segmentLength = 0;
            _totalLevel = 0;
            _typeMappingOfSegment.Clear();

            foreach (var levelSetting in levelSettings)
            {
                _segmentLength += levelSetting.NumberInSegment;
                _totalLevel += levelSetting.TotalLevel;
                // flat map
                for (var i = 0; i < levelSetting.NumberInSegment; i++)
                {
                    _typeMappingOfSegment.Add(levelSetting.LevelType.Value);
                }
            }

            CheckCacheLevel(currentLevelIndex.Value);
#if PANCAKE_ADDRESSABLE
            eventGetLevel.OnRaised += GetLevel;
#endif
        }

#if PANCAKE_ADDRESSABLE
        private async UniTask<LevelComponent> GetLevel(int currentLevelIndex)
        {
            int indexInSegment = currentLevelIndex % _segmentLength;
            int indexSegment = currentLevelIndex / _segmentLength;
            string type = TypeOfIndex(indexInSegment);
            int countOfType = CountOfTypeInSegment(type);
            int countOfTypeBeforeIndex = CountOfTypeBeforeIndexInSegment(indexInSegment);

            int index = IndexInLevelContainer(indexSegment, countOfType, countOfTypeBeforeIndex);
            var setting = levelSettings.Filter(level => level.LevelType.Value == type).First();

            if (currentLevelIndex > _totalLevel - 1)
            {
                index = index % levelSettings.Filter(level => level.LevelType.Value == type).First().TotalLevel;

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
                    return result.GetComponent<LevelComponent>();
                }
            }

            var obj = await Addressables.LoadAssetAsync<GameObject>(string.Format(setting.Schema, index + 1));
            return obj.GetComponent<LevelComponent>();
        }
#endif

        private void CheckCacheLevel(int currentLevelIndex)
        {
            if (currentLevelIndex < _totalLevel * NgNumber)
            {
                foreach (var levelSetting in levelSettings)
                {
                    // try load cached data if exist
                    levelSetting.CacheLevels = Data.Load<List<int>>($"level_{id}_{levelSetting.LevelType}_cache");
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
            Data.Save($"level_{id}_{levelSetting.LevelType}_cache", levelSetting.CacheLevels);
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
    }
}