using Pancake.Common;
using Pancake.LevelSystem;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_default")]
    public class ConditionRateComponent : GameComponent
    {
        [SerializeField] private int[] distanceLevels;
        // [SerializeField] private IntVariable currentLevelIndex;
        // [SerializeField] private IntVariable rateDisplayTimes;
        // [SerializeField] private BoolVariable canShowRate;
        // [SerializeField] private IntVariable lastLevelShowRate;
        [SerializeField] private StringConstant levelType;

        protected void OnEnable() { LevelInstantiate.RegisterActionRecreateLevel(levelType.Value, OnRecreateLevelLoaded); }

        private void OnRecreateLevelLoaded()
        {
//             int indexLevelGoal = lastLevelShowRate.Value + distanceLevels[rateDisplayTimes.Value.Min(distanceLevels.Length - 1)];
//             if (currentLevelIndex.Value == indexLevelGoal)
//             {
// #if UNITY_ANDROID
//                 Rate.AppRatingComponent.InitReview();
// #endif
//                 canShowRate.Value = true;
//             }
//             else
//             {
//                 canShowRate.Value = false;
//             }
//
//             if (currentLevelIndex.Value >= indexLevelGoal) lastLevelShowRate.Value = currentLevelIndex.Value;
        }

        protected void OnDisable() { LevelInstantiate.RegisterActionRecreateLevel(levelType.Value, OnRecreateLevelLoaded); }
    }
}