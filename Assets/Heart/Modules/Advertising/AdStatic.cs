using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public static class AdStatic
    {
        /// <summary>
        /// prevent show app open ad, it will become true when interstitial or rewarded was showed
        /// </summary>
        internal static bool isShowingAd;
        internal static EAdNetwork currentNetworkShared;
        internal static Action waitAppOpenDisplayedAction;
        internal static Action waitAppOpenClosedAction;

        public static bool IsRemoveAd { get => Data.Load($"{Application.identifier}_removeads", false); set => Data.Save($"{Application.identifier}_removeads", value); }

        /**
         * For Applovin :  Fired when a rewarded ad is displayed (may not be received by Unity until the rewarded ad closes).
         * Admob still work correctly
         */
        public static AdUnitVariable OnDisplayed(this AdUnitVariable unit, Action onDisplayed)
        {
            unit.displayedCallback = onDisplayed;
            return unit;
        }

        public static AdUnitVariable OnClosed(this AdUnitVariable unit, Action onClosed)
        {
            unit.closedCallback = onClosed;
            return unit;
        }

        public static AdUnitVariable OnLoaded(this AdUnitVariable unit, Action onLoaded)
        {
            unit.loadedCallback = onLoaded;
            return unit;
        }

        public static AdUnitVariable OnFailedToLoad(this AdUnitVariable unit, Action onFailedToLoad)
        {
            unit.failedToLoadCallback = onFailedToLoad;
            return unit;
        }

        public static AdUnitVariable OnFailedToDisplay(this AdUnitVariable unit, Action onFailedToDisplay)
        {
            unit.failedToDisplayCallback = onFailedToDisplay;
            return unit;
        }

        /// <summary>
        /// Not affect for banner and app open ad
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static AdUnitVariable OnCompleted(this AdUnitVariable unit, Action onCompleted)
        {
            if (!Application.isMobilePlatform) onCompleted?.Invoke();

            switch (unit)
            {
                case AdmobInterVariable inter:
                    inter.completedCallback = onCompleted;
                    return unit;
                case AdmobRewardVariable reward:
                    reward.completedCallback = onCompleted;
                    return unit;
                case AdmobRewardInterVariable rewardInter:
                    rewardInter.completedCallback = onCompleted;
                    return unit;
                case ApplovinInterVariable applovinInter:
                    applovinInter.completedCallback = onCompleted;
                    return unit;
                case ApplovinRewardVariable applovinReward:
                    applovinReward.completedCallback = onCompleted;
                    return unit;
                case ApplovinRewardInterVariable applovinRewardInter:
                    applovinRewardInter.completedCallback = onCompleted;
                    return unit;
            }

            return unit;
        }

        /// <summary>
        /// Only affect for rewarded and rewarded interstitial
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="onSkipped"></param>
        /// <returns></returns>
        public static AdUnitVariable OnSkipped(this AdUnitVariable unit, Action onSkipped)
        {
            switch (unit)
            {
                case AdmobRewardVariable reward:
                    reward.skippedCallback = onSkipped;
                    return unit;
                case AdmobRewardInterVariable rewardInter:
                    rewardInter.skippedCallback = onSkipped;
                    return unit;
                case ApplovinRewardVariable applovinReward:
                    applovinReward.skippedCallback = onSkipped;
                    return unit;
                case ApplovinRewardInterVariable applovinRewardInter:
                    applovinRewardInter.skippedCallback = onSkipped;
                    return unit;
            }

            return unit;
        }
    }
}