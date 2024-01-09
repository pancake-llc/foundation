using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    public class LevelInstantiate : GameComponent
    {
        [SerializeField] private ScriptableEventGetLevelCached loadLevelCachedEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private ScriptableEventNoParam trackingStartLevelEvent;
        [SerializeField] private Transform root;

        public void Start()
        {
            if (trackingStartLevelEvent != null) trackingStartLevelEvent.Raise();
            Instantiate(loadLevelCachedEvent.Raise(), root, false);
        }

        protected override void OnEnabled() { reCreateLevelLoadedEvent.OnRaised += OnReCreateLevelLoaded; }

        protected override void OnDisabled() { reCreateLevelLoadedEvent.OnRaised -= OnReCreateLevelLoaded; }

        public void OnReCreateLevelLoaded()
        {
            root.RemoveAllChildren(true);
            LevelComponent levelComponent = null;
#if UNITY_EDITOR
            levelComponent = LevelDebug.IsTest ? LevelDebug.LevelPrefab : loadLevelCachedEvent.Raise();
#else
            levelComponent = loadLevelCachedEvent.Raise();
#endif
            if (trackingStartLevelEvent != null) trackingStartLevelEvent.Raise();
            Instantiate(levelComponent, root, false);
        }
    }
}