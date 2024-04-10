using Pancake.Apex;
using Pancake.Common;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [EditorIcon("csharp")]
    public class ConditionRateComponent : GameComponent
    {
        [SerializeField, Array] private int[] distanceLevels;
        [SerializeField] private IntVariable currentLevelIndex;
        [SerializeField] private IntVariable rateDisplayTimes;
        [SerializeField] private BoolVariable canShowRate;
        [SerializeField] private IntVariable lastLevelShowRate;
        [SerializeField] private ScriptableEventNoParam reCreateLevelLoadedEvent; // check rate for each new level
#if UNITY_ANDROID
        [SerializeField] private ScriptableEventNoParam initReviewEvent;
#endif

        protected void OnEnable() { reCreateLevelLoadedEvent.OnRaised += OnReCreateLevelLoaded; }

        private void OnReCreateLevelLoaded()
        {
            int indexLevelGoal = lastLevelShowRate.Value + distanceLevels[rateDisplayTimes.Value.Min(distanceLevels.Length - 1)];
            if (currentLevelIndex.Value == indexLevelGoal)
            {
#if UNITY_ANDROID
                initReviewEvent.Raise();
#endif
                canShowRate.Value = true;
            }
            else
            {
                canShowRate.Value = false;
            }

            if (currentLevelIndex.Value >= indexLevelGoal) lastLevelShowRate.Value = currentLevelIndex.Value;
        }

        protected void OnDisable() { reCreateLevelLoadedEvent.OnRaised -= OnReCreateLevelLoaded; }
    }
}