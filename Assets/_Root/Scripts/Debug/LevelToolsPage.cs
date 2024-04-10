using System.Threading.Tasks;
using Pancake.Common;
using Pancake.Component;
using Pancake.DebugView;
using Pancake.LevelSystem;
using Pancake.Scriptable;
using Pancake.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pancake.SceneFlow
{
    public class LevelToolsPage : DefaultDebugPageBase
    {
        private ScriptableEventVfxMagnet _fxCoinSpawnEvent;
        private IntVariable _currentLevelIndex;
        private ScriptableEventLoadLevel _loadLevelEvent;
        private ScriptableEventNoParam _reCreateLevelLoadedEvent;
        private ScriptableEventNoParam _hideUiGameplayEvent;

        private string _targetLevel;
        protected override string Title => "Level Tools";

        public void Setup(
            ScriptableEventVfxMagnet fxCoinSpawnEvent,
            IntVariable currentLevelIndex,
            ScriptableEventLoadLevel loadLevelEvent,
            ScriptableEventNoParam reCreateLevelLoadedEvent,
            ScriptableEventNoParam hideUiGameplayEvent)
        {
            _fxCoinSpawnEvent = fxCoinSpawnEvent;
            _currentLevelIndex = currentLevelIndex;
            _loadLevelEvent = loadLevelEvent;
            _reCreateLevelLoadedEvent = reCreateLevelLoadedEvent;
            _hideUiGameplayEvent = hideUiGameplayEvent;
        }

        public override Task Initialize()
        {
            AddButton("Add 10000 Coin",
                clicked: () =>
                {
                    UserData.AddCoin(10000);
                    _fxCoinSpawnEvent.Raise(Vector3.zero, 10000);
                });
            AddButton("Add 1M Coin",
                clicked: () =>
                {
                    UserData.AddCoin(1000000);
                    _fxCoinSpawnEvent.Raise(Vector3.zero, 1000000);
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
            _currentLevelIndex.Value = target;
            var prefab = await _loadLevelEvent.Raise(target);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
        }

        private void OnLevelChanged(string level) { _targetLevel = level; }

        private async void NextLevel()
        {
            if (!CheckInGamePlay()) return;
            _currentLevelIndex.Value += 1;
            var prefab = await _loadLevelEvent.Raise(_currentLevelIndex.Value);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
        }

        private async void PreviousLevel()
        {
            if (!CheckInGamePlay()) return;
            _currentLevelIndex.Value = (_currentLevelIndex.Value - 1).Max(0);
            var prefab = await _loadLevelEvent.Raise(_currentLevelIndex.Value);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
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