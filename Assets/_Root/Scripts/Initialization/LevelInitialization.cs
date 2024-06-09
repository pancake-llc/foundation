using Pancake.LevelSystem;
using UnityEngine;
using VitalRouter;

namespace Pancake.SceneFlow
{
    /// <summary>
    /// Prewarm load level 
    /// </summary>
    public class LevelInitialization : Initialize
    {
        [SerializeField] private StringConstant levelType;

        public override async void Init()
        {
            await LevelCoordinator.LoadLevel(levelType.Value, LevelCoordinator.GetCurrentLevelIndex(levelType.Value));
        }
    }
}