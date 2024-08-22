using System;
using Cysharp.Threading.Tasks;

namespace Pancake.LevelSystem
{
    public class LevelDimension
    {
#if PANCAKE_UNITASK
        internal event Func<int, UniTask<LevelComponent>> LoadLevelEvent;
#endif
        internal event Func<LevelComponent> GetNextLevelLoadedEvent;
        internal event Func<LevelComponent> GetPreviousLevelLoadedEvent;
        internal event Action<int> ChangeLevelIndexEvent;

        public UniTask<LevelComponent> LoadLevel(int index) { return LoadLevelEvent?.Invoke(index) ?? new UniTask<LevelComponent>(null); }
        public LevelComponent GetNextlevelLoaded() { return GetNextLevelLoadedEvent?.Invoke(); }
        public LevelComponent GetPreviousLevelLoaded() { return GetPreviousLevelLoadedEvent?.Invoke(); }
        public void ChangeLevelIndex(int index) { ChangeLevelIndexEvent?.Invoke(index); }
    }
}