using Pancake.LevelSystem;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    /// <summary>
    /// Prewarm load level 
    /// </summary>
    public class LevelInitialization : Initialize
    {
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private ScriptableEventLoadLevel loadLevelEvent;
        [SerializeField] private BoolVariable loadLevelCompleted;

        public override async void Init()
        {
            loadLevelCompleted.Value = false;
            var prefab = await loadLevelEvent.Raise(currentLevelIndex.Value);
            loadLevelCompleted.Value = true;
        }
    }
}