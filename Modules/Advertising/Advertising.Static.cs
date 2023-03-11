using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public static class AdsUtil
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            var obj = new GameObject("Advertising") /*{hideFlags = HideFlags.HideInHierarchy}*/;
            obj.AddComponent<Advertising>();
            UnityEngine.Object.DontDestroyOnLoad(obj);
        }

        /**
         * For Applovin :  Fired when a rewarded ad is displayed (may not be received by Unity until the rewarded ad closes).
         * Admob still work correctly
         */
        public static IRewarded OnDisplayed(this IRewarded rewarded, Action onDisplayed)
        {
            rewarded?.Register(nameof(OnDisplayed), onDisplayed);
            return rewarded;
        }

        public static IRewarded OnCompleted(this IRewarded rewarded, Action onCompleted)
        {
            rewarded?.Register(nameof(OnCompleted), onCompleted);
            return rewarded;
        }

        public static IRewarded OnClosed(this IRewarded rewarded, Action onClosed)
        {
            rewarded?.Register(nameof(OnClosed), onClosed);
            return rewarded;
        }

        public static IRewarded OnSkipped(this IRewarded rewarded, Action onSkipped)
        {
            rewarded?.Register(nameof(OnSkipped), onSkipped);
            return rewarded;
        }

        /**
         * For Applovin : Fired when an interstitial ad is displayed (may not be received by Unity until the interstitial ad closes).
         * Admob still work correctly
         */
        public static IInterstitial OnDisplayed(this IInterstitial interstitial, Action onDisplayed)
        {
            interstitial?.Register(nameof(OnDisplayed), onDisplayed);
            return interstitial;
        }

        public static IInterstitial OnCompleted(this IInterstitial interstitial, Action onCompleted)
        {
            interstitial?.Register(nameof(OnCompleted), onCompleted);
            return interstitial;
        }

        /**
         * For Applovin :  Fired when a rewarded ad is displayed (may not be received by Unity until the rewarded ad closes).
         * Admob still work correctly
         */
        public static IRewardedInterstitial OnDisplayed(this IRewardedInterstitial rewarded, Action onDisplayed)
        {
            rewarded?.Register(nameof(OnDisplayed), onDisplayed);
            return rewarded;
        }

        public static IRewardedInterstitial OnCompleted(this IRewardedInterstitial rewarded, Action onCompleted)
        {
            rewarded?.Register(nameof(OnCompleted), onCompleted);
            return rewarded;
        }

        public static IRewardedInterstitial OnClosed(this IRewardedInterstitial rewarded, Action onClosed)
        {
            rewarded?.Register(nameof(OnClosed), onClosed);
            return rewarded;
        }

        public static IRewardedInterstitial OnSkipped(this IRewardedInterstitial rewarded, Action onSkipped)
        {
            rewarded?.Register(nameof(OnSkipped), onSkipped);
            return rewarded;
        }
    }
}