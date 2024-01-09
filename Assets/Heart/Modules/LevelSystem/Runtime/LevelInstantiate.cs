using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    public class LevelInstantiate : GameComponent
    {
        [SerializeField] private ScriptableEventGetLevelCached loadLevelCachedEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private Transform root;

        public void Start() { Instantiate(loadLevelCachedEvent.Raise(), root, false); }

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
            Instantiate(levelComponent, root, false);
        }
    }
}