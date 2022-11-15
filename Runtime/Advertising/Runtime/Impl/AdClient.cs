using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public abstract class AdClient : IAdClient
    {
        protected bool isInitialized;

        public event Action<IAdClient> OnInterstitialAdCompleted;
        public event Action<IAdClient> OnRewardedAdSkipped;
        public event Action<IAdClient> OnRewardedAdCompleted;
        public event Action<IAdClient> OnRewardedInterstitialAdSkipped;
        public event Action<IAdClient> OnRewardedInterstitialAdCompleted;
        public event Action<IAdClient> OnAppOpenAdCompleted;
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
        protected abstract void InternalShowInterstitialAd();
        protected abstract bool InternalIsInterstitialAdReady();
        protected abstract void InternalLoadRewardedAd();
        protected abstract void InternalShowRewardedAd();
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

        protected virtual void InvokeInterstitialAdCompleted() { RuntimeHelper.RunOnMainThread(() => { OnInterstitialAdCompleted?.Invoke(this); }); }

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

        public void ShowInterstitialAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsInterstitialAdReady())
                {
                    Debug.Log($"Cannot show {Network} interstitial ad. Ad is not loaded.");
                    return;
                }
                
                InternalShowInterstitialAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public bool IsInterstitialAdReady() { return CheckInitialize(false) && InternalIsInterstitialAdReady(); }

        protected virtual void InvokeRewardedAdSkipped() { RuntimeHelper.RunOnMainThread(() => { OnRewardedAdSkipped?.Invoke(this); }); }
        protected virtual void InvokeRewardedAdCompleted() { RuntimeHelper.RunOnMainThread(() => { OnRewardedAdCompleted?.Invoke(this); }); }

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

        public void ShowRewardedAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsRewardedAdReady())
                {
                    Debug.LogFormat($"Cannot show {Network} rewarded ad : ad is not loaded.");
                    return;
                }
                
                InternalShowRewardedAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public bool IsRewardedAdReady() { return CheckInitialize(false) && InternalIsRewardedAdReady(); }

        protected virtual void InvokeRewardedInterstitialAdSkipped() { RuntimeHelper.RunOnMainThread(() => { OnRewardedInterstitialAdSkipped?.Invoke(this); }); }
        protected virtual void InvokeRewardedInterstitialAdCompleted() { RuntimeHelper.RunOnMainThread(() => { OnRewardedInterstitialAdCompleted?.Invoke(this); }); }

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

        protected virtual void InvokeAppOpenAdCompleted() { RuntimeHelper.RunOnMainThread(() => { OnAppOpenAdCompleted?.Invoke(this); }); }

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