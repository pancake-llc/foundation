using System;
using Pancake.Apex;
using Pancake.Scriptable;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pancake.LevelSystem
{
    [HideMonoScript]
    public class LevelComponent : GameComponent
    {
        [SerializeField, HideInEditorMode, ReadOnly] protected int originLevelIndex;
        [SerializeField, HideInEditorMode, ReadOnly] protected int currentLevelIndex;

        [SerializeField, Group("Event")] private ScriptableEventNoParam winEvent;
        [SerializeField, Group("Event")] private ScriptableEventNoParam loseEvent;
        [SerializeField, Group("Event")] private ScriptableEventNoParam replayEvent;
        [SerializeField, Group("Event")] private ScriptableEventNoParam skipEvent;

        internal void Init(int originLevelIndex, int currentLevelIndex)
        {
            this.originLevelIndex = originLevelIndex;
            this.currentLevelIndex = currentLevelIndex;
        }

        protected virtual void OnSkipLevel() { }

        protected virtual void OnReplayLevel() { }

        protected virtual void OnLoseLevel() { }

        protected virtual void OnWinLevel() { }

        protected virtual void OnSpawned()
        {
            if (winEvent != null) winEvent.OnRaised += OnWinLevel;
            if (loseEvent != null) loseEvent.OnRaised += OnLoseLevel;
            if (replayEvent != null) replayEvent.OnRaised += OnReplayLevel;
            if (skipEvent != null) skipEvent.OnRaised += OnSkipLevel;
        }

        protected virtual void OnDespawned()
        {
            if (winEvent != null) winEvent.OnRaised -= OnWinLevel;
            if (loseEvent != null) loseEvent.OnRaised -= OnLoseLevel;
            if (replayEvent != null) replayEvent.OnRaised -= OnReplayLevel;
            if (skipEvent != null) skipEvent.OnRaised -= OnSkipLevel;
        }

        protected override void OnEnabled() { OnSpawned(); }

        protected override void OnDisabled() { OnDespawned(); }


#if UNITY_EDITOR

        [Button]
        private void PlayThisLevel() { }
#endif
    }
}