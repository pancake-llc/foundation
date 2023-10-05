using Pancake.Apex;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("script_controller")]
    public class GameplayController : GameComponent
    {
        [SerializeField] private Transform canvasUI;
        [Header("BUTTON")] [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonSkipByAd;

        [Header("OTHER")] [SerializeField] private AudioComponent buttonAudio;
        [SerializeField] private ScriptableEventString changeSceneEvent;

        [Header("LEVEL")] [SerializeField] private RewardVariable rewardVariable;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventGetLevelCached getNextLevelCached;
        [SerializeField] private Transform levelRootHolder;
        [SerializeField] private IntVariable currentLevelIndex;

        private void Start()
        {
            buttonHome.onClick.AddListener(GoToMenu);
            buttonReplay.onClick.AddListener(OnButtonReplayClicked);
            buttonSkipByAd.onClick.AddListener(OnButtonSkipByAdClicked);
        }

        private void OnButtonSkipByAdClicked()
        {
            if (!Application.isMobilePlatform)
            {
                Execute();
            }
            else
            {
                rewardVariable.Context().Show().OnCompleted(Execute);
            }

            return;

            async void Execute()
            {
                currentLevelIndex.Value += 1;
                var prefabLevel = await loadLevelEvent.Raise(currentLevelIndex.Value);
                levelRootHolder.RemoveAllChildren();
                var instance = Instantiate(prefabLevel, levelRootHolder, false);
            }
        }

        private void OnButtonReplayClicked()
        {
            // Because the next level load has not yet been called, the cached next level is the current level being played
            var nextLevelPrefab = getNextLevelCached.Raise();
            // clear current instance
            levelRootHolder.RemoveAllChildren();
            var instance = Instantiate(nextLevelPrefab, levelRootHolder, false);
        }

        private void GoToMenu() { changeSceneEvent.Raise(Constant.MENU_SCENE); }
    }
}