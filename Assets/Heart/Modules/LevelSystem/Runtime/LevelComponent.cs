using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.LevelSystem
{
    [EditorIcon("icon_default")]
    public abstract class LevelComponent : GameComponent
    {
        [SerializeField] private StringConstant type;
        [SerializeField, HideInEditorMode, ReadOnly] protected int originLevelIndex;
        [SerializeField, HideInEditorMode, ReadOnly] protected int currentLevelIndex;

        private static event Action WinEvent;
        private static event Action LoseEvent;
        private static event Action ReplayEvent;
        private static event Action SkipEvent;

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
            WinEvent += OnWinLevel;
            LoseEvent += OnLoseLevel;
            ReplayEvent += OnReplayLevel;
            SkipEvent += OnSkipLevel;
        }

        protected virtual void OnDespawned()
        {
            WinEvent -= OnWinLevel;
            LoseEvent -= OnLoseLevel;
            ReplayEvent -= OnReplayLevel;
            SkipEvent -= OnSkipLevel;
        }

        protected void OnEnable() { OnSpawned(); }

        protected void OnDisable() { OnDespawned(); }

        public static void WinLevel() { WinEvent?.Invoke(); }
        public static void LoseLevel() { LoseEvent?.Invoke(); }
        public static void ReplayLevel() { ReplayEvent?.Invoke(); }
        public static void SkipLevel() { SkipEvent?.Invoke(); }


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
                LevelInstantiate.RecreateLevelLoaded(type.Value);
            }
        }
#endif

        private void Clear()
        {
            WinEvent = null;
            LoseEvent = null;
            ReplayEvent = null;
            SkipEvent = null;
        }

        protected override void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isUpdating && !UnityEditor.EditorApplication.isCompiling) return;
#endif
            Clear();
        }

        protected override void OnAfterDeserialize() { }
    }
}