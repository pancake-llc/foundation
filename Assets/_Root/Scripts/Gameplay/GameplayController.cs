using Pancake.Common;
using Pancake.LevelSystem;
using Pancake.Monetization;
using Pancake.Scriptable;
using Cysharp.Threading.Tasks;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_controller")]
    public class GameplayController : GameComponent
    {
        [Header("Button")] [SerializeField] private Button buttonHome;
        [SerializeField] private Button buttonReplay;
        [SerializeField] private Button buttonSkipByAd;

        [Header("Other")] [SerializeField] private ScriptableEventString changeSceneEvent;

        [Header("Level")] [SerializeField] private StringConstant levelType;
        [SerializeField] private Transform levelRootHolder;

        //private PopupContainer PersistentPopupContainer => PopupContainer.Find(Constant.PERSISTENT_POPUP_CONTAINER);

        private void Start()
        {
            buttonHome.onClick.AddListener(GoToMenu);
            buttonReplay.onClick.AddListener(OnButtonReplayClicked);
            buttonSkipByAd.onClick.AddListener(OnButtonSkipByAdClicked);
        }

        private void OnButtonSkipByAdClicked()
        {
            Advertising.Reward?.Show().OnCompleted(Execute);
            return;

            async void Execute()
            {
                LevelCoordinator.IncreaseLevelIndex(levelType.Value, 1);
                var prefabLevel = await LevelCoordinator.LoadLevel(levelType.Value, LevelCoordinator.GetCurrentLevelIndex(levelType.Value));
                levelRootHolder.RemoveAllChildren();
                var instance = Instantiate(prefabLevel, levelRootHolder, false);
            }
        }

        private void OnButtonReplayClicked()
        {
            // Because the next level load has not yet been called, the cached next level is the current level being played
            var nextLevelPrefab = LevelCoordinator.GetNextLevelLoaded(levelType.Value);
            // clear current instance
            levelRootHolder.RemoveAllChildren();
            var instance = Instantiate(nextLevelPrefab, levelRootHolder, false);
        }

        private async void GoToMenu()
        {
            PoolHelper.ReturnAllPool();
            // await PersistentPopupContainer.Push<SceneTransitionPopup>(nameof(SceneTransitionPopup),
            //     false,
            //     onLoad: t => { t.popup.view.Setup(); },
            //     popupId: nameof(SceneTransitionPopup)); // show transition
            changeSceneEvent.Raise(Constant.MENU_SCENE);
        }
    }
}