using Alchemy.Inspector;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.LevelSystem
{

    [EditorIcon("csharp")]
    public abstract class LevelComponent : GameComponent
    {
        [SerializeField, HideInEditMode, ReadOnly] protected int originLevelIndex;
        [SerializeField, HideInEditMode, ReadOnly] protected int currentLevelIndex;

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

        protected void OnEnable() { OnSpawned(); }

        protected void OnDisable() { OnDespawned(); }


#if UNITY_EDITOR

        [Button]
        public void PlayThisLevel()
        {
            LevelDebug.IsTest = true;
            LevelDebug.LevelPrefab = null;
            LevelDebug.PathLevelPrefab = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeGameObject);
            if (!Application.isPlaying)
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    string path = SceneUtility.GetScenePathByBuildIndex(0);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                    UnityEditor.EditorApplication.isPlaying = true;
                }
            }
            else
            {
                // recreate
                var levelInstantiate = FindAnyObjectByType<LevelInstantiate>();
                levelInstantiate.OnReCreateLevelLoaded();
            }
        }
#endif
    }
}