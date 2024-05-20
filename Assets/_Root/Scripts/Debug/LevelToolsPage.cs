using System.Threading.Tasks;
using Pancake.Common;
using Pancake.Component;
using Pancake.DebugView;
using Pancake.LevelSystem;
using Pancake.Scriptable;
using Pancake.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VitalRouter;

namespace Pancake.SceneFlow
{
    public class LevelToolsPage : DefaultDebugPageBase
    {
        private StringConstant _currencyType;
        private StringConstant _levelType;
        private ScriptableEventNoParam _hideUiGameplayEvent;

        private string _targetLevel;
        protected override string Title => "Level Tools";

        public void Setup(StringConstant currencyType, StringConstant levelType, ScriptableEventNoParam hideUiGameplayEvent)
        {
            _currencyType = currencyType;
            _levelType = levelType;
            _hideUiGameplayEvent = hideUiGameplayEvent;
        }

        public override Task Initialize()
        {
            AddButton("Add 10000 Coin",
                clicked: () =>
                {
                    UserData.AddCoin(10000);
                    Router.Default.PublishAsync(new VfxMangnetCommand(_currencyType.Value, Vector3.zero, 10000));
                });
            AddButton("Add 1M Coin",
                clicked: () =>
                {
                    UserData.AddCoin(1000000);
                    Router.Default.PublishAsync(new VfxMangnetCommand(_currencyType.Value, Vector3.zero, 1000000));
                });

            AddButton("Next Level", clicked: NextLevel);
            AddButton("Previous Level", clicked: PreviousLevel);
            AddInputField("Target Level:", valueChanged: OnLevelChanged);
            AddButton("Go To Level", clicked: GoToLevel);
            AddButton("Win Level", clicked: WinLevel);
            AddButton("Lose Level", clicked: LoseLevel);
            return Task.CompletedTask;
        }

        private void LoseLevel()
        {
            if (!CheckInGamePlay()) return;
            _hideUiGameplayEvent.Raise();
            var popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            popupContainer.Popups.TryGetValue(nameof(WinPopup), out var win);
            if (win != null)
            {
                DebugEditor.Log("Popup Lose now cannot be displayed because Popup Win is showing!");
                return;
            }

            popupContainer.Push<LosePopup>(nameof(LosePopup), true, popupId: nameof(LosePopup));
        }

        private void WinLevel()
        {
            if (!CheckInGamePlay()) return;
            _hideUiGameplayEvent.Raise();
            var popupContainer = PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);
            popupContainer.Popups.TryGetValue(nameof(LosePopup), out var lose);
            if (lose != null)
            {
                DebugEditor.Log("Popup Win now cannot be displayed because Popup Lose is showing!");
                return;
            }


            popupContainer.Push<WinPopup>(nameof(WinPopup), true, popupId: nameof(WinPopup));
        }

        private async void GoToLevel()
        {
            if (!CheckInGamePlay()) return;
            int target = (int.Parse(_targetLevel) - 1).Max(0);
            LevelCoordinator.SetCurrentLevelIndex(_levelType.Value, target);
            var prefab = await LevelCoordinator.LoadLevel(_levelType.Value, target);
            if (prefab == null) return;
            LevelInstantiate.RecreateLevelLoaded(_levelType.Value);
        }

        private void OnLevelChanged(string level) { _targetLevel = level; }

        private async void NextLevel()
        {
            if (!CheckInGamePlay()) return;
            LevelCoordinator.IncreaseLevelIndex(_levelType.Value, 1);
            var prefab = await LevelCoordinator.LoadLevel(_levelType.Value, LevelCoordinator.GetCurrentLevelIndex(_levelType.Value));
            if (prefab == null) return;
            LevelInstantiate.RecreateLevelLoaded(_levelType.Value);
        }

        private async void PreviousLevel()
        {
            if (!CheckInGamePlay()) return;
            LevelCoordinator.SetCurrentLevelIndex(_levelType.Value, (LevelCoordinator.GetCurrentLevelIndex(_levelType.Value) - 1).Max(0));
            var prefab = await LevelCoordinator.LoadLevel(_levelType.Value, LevelCoordinator.GetCurrentLevelIndex(_levelType.Value));
            if (prefab == null) return;
            LevelInstantiate.RecreateLevelLoaded(_levelType.Value);
        }

        private bool CheckInGamePlay()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!activeScene.Equals(Constant.GAMEPLAY_SCENE))
            {
                DebugEditor.LogWarning("You must be in a gameplay scene to use this feature!");
                return false;
            }

            return true;
        }
    }
}