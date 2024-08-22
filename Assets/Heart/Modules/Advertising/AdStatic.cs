using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public static class AdStatic
    {
        /**
         * For Applovin :  Fired when a rewarded ad is displayed (may not be received by Unity until the rewarded ad closes).
         * Admob still work correctly
         */
        public static AdUnit OnDisplayed(this AdUnit unit, Action onDisplayed)
        {
            unit.displayedCallback = onDisplayed;
            return unit;
        }

        public static AdUnit OnClosed(this AdUnit unit, Action onClosed)
        {
            unit.closedCallback = onClosed;
            return unit;
        }

        public static AdUnit OnLoaded(this AdUnit unit, Action onLoaded)
        {
            unit.loadedCallback = onLoaded;
            return unit;
        }

        public static AdUnit OnFailedToLoad(this AdUnit unit, Action onFailedToLoad)
        {
            unit.failedToLoadCallback = onFailedToLoad;
            return unit;
        }

        public static AdUnit OnFailedToDisplay(this AdUnit unit, Action onFailedToDisplay)
        {
            unit.failedToDisplayCallback = onFailedToDisplay;
            return unit;
        }

        /// <summary>
        /// Not affect to banner and app open ad
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static AdUnit OnCompleted(this AdUnit unit, Action onCompleted)
        {
            if (!Application.isMobilePlatform) onCompleted?.Invoke();

            switch (unit)
            {
                case AdmobInter inter:
                    inter.completedCallback = onCompleted;
                    return unit;
                case AdmobReward reward:
                    reward.completedCallback = onCompleted;
                    return unit;
                case AdmobRewardInter rewardInter:
                    rewardInter.completedCallback = onCompleted;
                    return unit;
                case ApplovinInter applovinInter:
                    applovinInter.completedCallback = onCompleted;
                    return unit;
                case ApplovinReward applovinReward:
                    applovinReward.completedCallback = onCompleted;
                    return unit;
                case ApplovinRewardInter applovinRewardInter:
                    applovinRewardInter.completedCallback = onCompleted;
                    return unit;
            }

            return unit;
        }

        /// <summary>
        /// Only affect to rewarded and rewarded interstitial
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="onSkipped"></param>
        /// <returns></returns>
        public static AdUnit OnSkipped(this AdUnit unit, Action onSkipped)
        {
            switch (unit)
            {
                case AdmobReward reward:
                    reward.skippedCallback = onSkipped;
                    return unit;
                case AdmobRewardInter rewardInter:
                    rewardInter.skippedCallback = onSkipped;
                    return unit;
                case ApplovinReward applovinReward:
                    applovinReward.skippedCallback = onSkipped;
                    return unit;
                case ApplovinRewardInter applovinRewardInter:
                    applovinRewardInter.skippedCallback = onSkipped;
                    return unit;
            }

            return unit;
        }
    }
}