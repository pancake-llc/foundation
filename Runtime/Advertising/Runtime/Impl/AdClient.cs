#if PANCAKE_ADS
using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public abstract class AdClient : IAdClient
    {
        protected bool isInitialized;

        public event Action<IAdClient> OnInterstitialAdCompleted;
        public event Action<IAdClient> OnInterstitialAdDisplayed;
        public event Action<IAdClient> OnRewardedAdSkipped;
        public event Action<IAdClient> OnRewardedAdCompleted;
        /// <summary>
        /// call when close rewarded ad
        /// it's more general than <see cref="OnRewardedAdSkipped"/> and <see cref="OnRewardedAdCompleted"/>
        /// </summary>
        public event Action<IAdClient> OnRewardedAdClosed;
        public event Action<IAdClient> OnRewardedAdDisplayed;
        public event Action<IAdClient> OnRewardedInterstitialAdSkipped;
        public event Action<IAdClient> OnRewardedInterstitialAdCompleted;
        public event Action<IAdClient> OnRewardedInterstitialAdClosed;
        public event Action<IAdClient> OnRewardedInterstitialAdDisplayed;
        public event Action<IAdClient> OnAppOpenAdCompleted;
        public event Action<IAdClient> OnAppOpenAdDisplayed;

        public abstract EAdNetwork Network { get; }
        public abstract bool IsBannerAdSupported { get; }
        public abstract bool IsInsterstitialAdSupport { get; }
        public abstract bool IsRewardedAdSupport { get; }
        public abstract bool IsRewardedInterstitialAdSupport { get; }
        public abstract bool IsAppOpenAdSupport { get; }
        public abstract bool IsSdkAvaiable { get; }
        protected abstract string NoSdkMessage { get; }
        public virtual float GetAdaptiveBannerHeight { get; }
        public virtual bool IsInitialized => isInitialized;

        public virtual void Initialize()
        {
            if (IsSdkAvaiable)
            {
                if (!IsInitialized) InternalInit();
                else Debug.Log($"{Network} client is already initialized. Ignoreing this call.");
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public virtual void RegisterAppStateChange() { }
        public event Action<IAdClient> OnBannerAdDisplayed;
        public event Action<IAdClient> OnBannerAdCompleted;

        protected virtual bool CheckInitialize(bool logMessage = true)
        {
            if (Network == EAdNetwork.None) return false;

            if (!IsInitialized && logMessage) Debug.Log($"Please initialize the {Network} client first.");
            return IsInitialized;
        }

        protected abstract void InternalInit();
        protected abstract void InternalShowBannerAd();
        protected abstract void InternalHideBannerAd();
        protected abstract void InternalDestroyBannerAd();
        protected abstract void InternalLoadInterstitialAd();
        protected abstract IInterstitial InternalShowInterstitialAd();
        protected abstract bool InternalIsInterstitialAdReady();
        protected abstract void InternalLoadRewardedAd();
        protected abstract IRewarded InternalShowRewardedAd();
        protected abstract bool InternalIsRewardedAdReady();
        protected virtual void InternalLoadRewardedInterstitialAd() { }
        protected virtual void InternalShowRewardedInterstitialAd() { }
        protected virtual bool InternalIsRewardedInterstitialAdReady() { return false; }
        protected virtual void InternalLoadAppOpenAd() { }
        protected virtual void InternalShowAppOpenAd() { }
        protected virtual bool InternalIsAppOpenAdReady() { return false; }

        public virtual void ShowBannerAd()
        {
            if (IsSdkAvaiable)
            {
                if (CheckInitialize()) InternalShowBannerAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public void HideBannerAd()
        {
            if (CheckInitialize()) InternalHideBannerAd();
        }

        public void DestroyBannerAd()
        {
            if (CheckInitialize()) InternalDestroyBannerAd();
        }
        protected virtual void CallBannerAdDisplayed() { Runtime.RunOnMainThread(() => { OnBannerAdDisplayed?.Invoke(this); }); }
        protected virtual void CallBannerAdCompleted() { Runtime.RunOnMainThread(() => { OnBannerAdCompleted?.Invoke(this); }); }
        protected virtual void CallInterstitialAdCompleted() { Runtime.RunOnMainThread(() => { OnInterstitialAdCompleted?.Invoke(this); }); }
        protected virtual void CallInterstitialAdDisplayed() { Runtime.RunOnMainThread(() => { OnInterstitialAdDisplayed?.Invoke(this); }); }

        public void LoadInterstitialAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;

                if (!IsInterstitialAdReady()) InternalLoadInterstitialAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public IInterstitial ShowInterstitialAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return null;
                if (!IsInterstitialAdReady())
                {
                    Debug.Log($"Cannot show {Network} interstitial ad. Ad is not loaded.");
                    return null;
                }

                return InternalShowInterstitialAd();
            }

            Debug.Log(NoSdkMessage);
            return null;
        }

        public bool IsInterstitialAdReady() { return CheckInitialize(false) && InternalIsInterstitialAdReady(); }

        protected virtual void CallRewardedAdSkipped() { Runtime.RunOnMainThread(() => { OnRewardedAdSkipped?.Invoke(this); }); }
        protected virtual void CallRewardedAdCompleted() { Runtime.RunOnMainThread(() => { OnRewardedAdCompleted?.Invoke(this); }); }
        protected virtual void CallRewardedAdClosed() { Runtime.RunOnMainThread(() => { OnRewardedAdClosed?.Invoke(this); }); }
        protected virtual void CallRewardedAdDisplayed() { Runtime.RunOnMainThread(() => { OnRewardedAdDisplayed?.Invoke(this); }); }

        public void LoadRewardedAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsRewardedAdReady()) InternalLoadRewardedAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public IRewarded ShowRewardedAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return null;
                if (!IsRewardedAdReady())
                {
                    Debug.LogFormat($"Cannot show {Network} rewarded ad : ad is not loaded.");
                    return null;
                }
                
                return InternalShowRewardedAd();
            }

            Debug.Log(NoSdkMessage);
            return null;
        }

        public bool IsRewardedAdReady() { return CheckInitialize(false) && InternalIsRewardedAdReady(); }

        protected virtual void CallRewardedInterstitialAdSkipped() { Runtime.RunOnMainThread(() => { OnRewardedInterstitialAdSkipped?.Invoke(this); }); }
        protected virtual void CallRewardedInterstitialAdCompleted() { Runtime.RunOnMainThread(() => { OnRewardedInterstitialAdCompleted?.Invoke(this); }); }
        protected virtual void CallRewardedInterstitialAdClosed() { Runtime.RunOnMainThread(() => { OnRewardedInterstitialAdClosed?.Invoke(this); }); }
        protected virtual void CallRewardedInterstitialAdDisplayed() { Runtime.RunOnMainThread(() => { OnRewardedInterstitialAdDisplayed?.Invoke(this); }); }

        public void LoadRewardedInterstitialAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsRewardedInterstitialAdReady()) InternalLoadRewardedInterstitialAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public void ShowRewardedInterstitialAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsRewardedInterstitialAdReady())
                {
                    Debug.LogFormat($"Cannot show {Network} rewarded interstitial ad : ad is not loaded.");
                    return;
                }
                
                InternalShowRewardedInterstitialAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public bool IsRewardedInterstitialAdReady() { return CheckInitialize(false) && InternalIsRewardedInterstitialAdReady(); }

        protected virtual void CallAppOpenAdCompleted() { Runtime.RunOnMainThread(() => { OnAppOpenAdCompleted?.Invoke(this); }); }
        protected virtual void CallAppOpenAdDisplayed() { Runtime.RunOnMainThread(() => { OnAppOpenAdDisplayed?.Invoke(this); }); }

        public void LoadAppOpenAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsAppOpenAdReady()) InternalLoadAppOpenAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public void ShowAppOpenAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsAppOpenAdReady())
                {
                    Debug.LogFormat($"Cannot show {Network} app open ad : ad is not loaded.");
                    return;
                }
                
                if (R.isShowingAd) return; // dose not show app open ad when interstitial or rewarded still displayed
                
                InternalShowAppOpenAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public bool IsAppOpenAdReady() { return CheckInitialize(false) && InternalIsAppOpenAdReady(); }


        public virtual void ShowConsentForm() { }
    }
}
#endif