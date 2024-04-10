using Pancake.Common;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Pancake.Sound;
using Cysharp.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("script_controller")]
    public class GameplayController : GameComponent
    {
        [Header("BUTTON")] [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonSkipByAd;

        [Header("OTHER")] [SerializeField] private ScriptableEventString changeSceneEvent;

        [Header("LEVEL")] [SerializeField] private RewardVariable rewardVariable;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private ScriptableEventGetLevelCached getNextLevelCached;
        [SerializeField] private ScriptableEventNoParam trackingStartLevelEvent;
        [SerializeField] private Transform levelRootHolder;
        [SerializeField] private IntVariable currentLevelIndex;

        private PopupContainer PersistentPopupContainer => PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER);

        private void Start()
        {
            buttonHome.onClick.AddListener(GoToMenu);
            buttonReplay.onClick.AddListener(OnButtonReplayClicked);
            buttonSkipByAd.onClick.AddListener(OnButtonSkipByAdClicked);
            trackingStartLevelEvent.OnRaised += OnTrackingStartLevel;
        }

        private void OnTrackingStartLevel()
        {
            // todo tracking with currentLevelIndex
        }

        private void OnButtonSkipByAdClicked()
        {
            rewardVariable.Context().Show().OnCompleted(Execute);
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

        private async void GoToMenu()
        {
            PoolHelper.ReturnAllPool();
            await PersistentPopupContainer.Push<SceneTransitionPopup>(nameof(SceneTransitionPopup),
                false,
                onLoad: t => { t.popup.view.Setup(); },
                popupId: nameof(SceneTransitionPopup)); // show transition
            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }

        protected void OnDisable() { trackingStartLevelEvent.OnRaised -= OnTrackingStartLevel; }
    }
}