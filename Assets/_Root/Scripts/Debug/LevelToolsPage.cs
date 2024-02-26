using System.Threading.Tasks;
using Pancake.Component;
using Pancake.DebugView;
using Pancake.LevelSystem;
using Pancake.Scriptable;
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
        private string _targetLevel;
        protected override string Title => "Level Tools";

        public void Setup(
            ScriptableEventVfxMagnet fxCoinSpawnEvent,
            IntVariable currentLevelIndex,
            ScriptableEventLoadLevel loadLevelEvent,
            ScriptableEventNoParam reCreateLevelLoadedEvent)
        {
            _fxCoinSpawnEvent = fxCoinSpawnEvent;
            _currentLevelIndex = currentLevelIndex;
            _loadLevelEvent = loadLevelEvent;
            _reCreateLevelLoadedEvent = reCreateLevelLoadedEvent;
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
            return Task.CompletedTask;
        }

        private async void GoToLevel()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!activeScene.Equals(Constant.GAMEPLAY_SCENE))
            {
                DebugEditor.LogWarning("You must be in a gameplay scene to use this feature!");
                return;
            }

            int target = (int.Parse(_targetLevel) - 1).Max(0);
            _currentLevelIndex.Value = target;
            var prefab = await _loadLevelEvent.Raise(target);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
        }

        private void OnLevelChanged(string level) { _targetLevel = level; }

        private async void NextLevel()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!activeScene.Equals(Constant.GAMEPLAY_SCENE))
            {
                DebugEditor.LogWarning("You must be in a gameplay scene to use this feature!");
                return;
            }

            _currentLevelIndex.Value += 1;
            var prefab = await _loadLevelEvent.Raise(_currentLevelIndex.Value);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
        }

        private async void PreviousLevel()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!activeScene.Equals(Constant.GAMEPLAY_SCENE))
            {
                DebugEditor.LogWarning("You must be in a gameplay scene to use this feature!");
                return;
            }

            _currentLevelIndex.Value = (_currentLevelIndex.Value - 1).Max(0);
            var prefab = await _loadLevelEvent.Raise(_currentLevelIndex.Value);
            if (prefab == null) return;
            _reCreateLevelLoadedEvent.Raise();
        }
    }
}