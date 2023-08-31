using Pancake.Apex;
using UnityEngine;

namespace Pancake.LevelSystem
{
    public class LevelComponent : GameComponent
    {
        [SerializeField, ReadOnly] private int originLevelIndex;
        [SerializeField, ReadOnly] private int currentLevelIndex;

        public virtual void OnSpawned() { }

        public virtual void OnDespawned() { }

#if UNITY_EDITOR

        [Button]
        private void PlayThisLevel() { }
#endif
    }
}