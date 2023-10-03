using Pancake.LevelSystem;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    public class LevelInstantiate : GameComponent
    {
        [SerializeField] private ScriptableEventGetLevelCached loadLevelCachedEvent;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent;
        [SerializeField] private Transform root;

        public void Start()
        {
            var _ = Instantiate(loadLevelCachedEvent.Raise(), root, false);
        }

        protected override void OnEnabled() { reCreateLevelLoadedEvent.OnRaised += OnReCreateLevelLoaded; }

        protected override void OnDisabled() { reCreateLevelLoadedEvent.OnRaised -= OnReCreateLevelLoaded; }

        private void OnReCreateLevelLoaded()
        {
            root.RemoveAllChildren();
            var _ = Instantiate(loadLevelCachedEvent.Raise(), root, false);
        }
    }
}