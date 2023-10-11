using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.LevelSystem
{
    [HideMonoScript]
    [EditorIcon("csharp")]
    public abstract class LevelComponent : GameComponent
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

        protected abstract void OnSkipLevel();
        protected abstract void OnReplayLevel();
        protected abstract void OnLoseLevel();
        protected abstract void OnWinLevel();

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
        private void PlayThisLevel()
        {
            LevelDebug.IsTest = true;
            LevelDebug.PathLevelPrefab =  UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeGameObject);
            if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(0);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                UnityEditor.EditorApplication.isPlaying = true;
            }
        }
#endif
    }
}