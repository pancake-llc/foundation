using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public abstract class AdClient : IAdClient
    {
        protected bool isInitialized;
        public event Action<IAdClient> OnBannerAdDisplayed;
        public event Action<IAdClient> OnBannerAdCompleted;
        public event Action<double, string, string, string, EAdNetwork> OnBannerAdPaid;
        public event Action<IAdClient> OnInterstitialAdCompleted;
        public event Action<IAdClient> OnInterstitialAdDisplayed;
        public event Action<double, string, string, string, EAdNetwork> OnInterstitialAdPaid;
        public event Action<IAdClient> OnRewardedAdSkipped;
        public event Action<IAdClient> OnRewardedAdCompleted;
        public event Action<double, string, string, string, EAdNetwork> OnRewardedAdPaid;

        /// <summary>
        /// call when close rewarded ad
        /// it's more general than <see cref="OnRewardedAdSkipped"/> and <see cref="OnRewardedAdCompleted"/>
        /// </summary>
        public event Action<IAdClient> OnRewardedAdClosed;

        public event Action<IAdClient> OnRewardedAdDisplayed;
        public event Action<IAdClient> OnRewardedInterAdSkipped;
        public event Action<IAdClient> OnRewardedInterAdCompleted;
        public event Action<IAdClient> OnRewardedInterAdClosed;
        public event Action<IAdClient> OnRewardedInterAdDisplayed;
        public event Action<double, string, string, string, EAdNetwork> OnRewardedInterAdPaid;
        public event Action<IAdClient> OnAppOpenAdCompleted;
        public event Action<IAdClient> OnAppOpenAdDisplayed;
        public event Action<double, string, string, string, EAdNetwork> OnAppOpenAdPaid;

        public abstract EAdNetwork Network { get; }
        public abstract bool IsBannerAdSupported { get; }
        public abstract bool IsInsterstitialAdSupport { get; }
        public abstract bool IsRewardedAdSupport { get; }
        public abstract bool IsRewardedInterstitialAdSupport { get; }
        public abstract bool IsAppOpenAdSupport { get; }
        public abstract bool IsSdkAvaiable { get; }
        protected abstract string NoSdkMessage { get; }

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
        protected abstract void InternalDestroyBannerAd();
        protected abstract void InternalLoadInterstitialAd();
        protected abstract IInterstitial InternalShowInterstitialAd();
        protected abstract bool InternalIsInterstitialAdReady();
        protected abstract void InternalLoadRewardedAd();
        protected abstract IRewarded InternalShowRewardedAd();
        protected abstract bool InternalIsRewardedAdReady();
        protected abstract void InternalLoadRewardedInterstitialAd();
        protected abstract IRewardedInterstitial InternalShowRewardedInterstitialAd();
        protected abstract bool InternalIsRewardedInterstitialAdReady();
        protected abstract void InternalLoadAppOpenAd();
        protected abstract void InternalShowAppOpenAd();
        protected abstract bool InternalIsAppOpenAdReady();

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

        public void DestroyBannerAd()
        {
            if (CheckInitialize()) InternalDestroyBannerAd();
        }

        protected virtual void CallBannerAdDisplayed() { App.RunOnMainThread(() => { OnBannerAdDisplayed?.Invoke(this); }); }
        protected virtual void CallBannerAdCompleted() { App.RunOnMainThread(() => { OnBannerAdCompleted?.Invoke(this); }); }

        protected virtual void CallBannerAdPaid(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            App.RunOnMainThread(() =>
            {
                OnBannerAdPaid?.Invoke(value,
                    network,
                    unitId,
                    placement,
                    adNetwork);
            });
        }

        protected virtual void CallInterstitialAdCompleted() { App.RunOnMainThread(() => { OnInterstitialAdCompleted?.Invoke(this); }); }
        protected virtual void CallInterstitialAdDisplayed() { App.RunOnMainThread(() => { OnInterstitialAdDisplayed?.Invoke(this); }); }

        protected virtual void CallInterstitialAdPaid(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            App.RunOnMainThread(() =>
            {
                OnInterstitialAdPaid?.Invoke(value,
                    network,
                    unitId,
                    placement,
                    adNetwork);
            });
        }

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

        protected virtual void CallRewardedAdSkipped() { App.RunOnMainThread(() => { OnRewardedAdSkipped?.Invoke(this); }); }
        protected virtual void CallRewardedAdCompleted() { App.RunOnMainThread(() => { OnRewardedAdCompleted?.Invoke(this); }); }
        protected virtual void CallRewardedAdClosed() { App.RunOnMainThread(() => { OnRewardedAdClosed?.Invoke(this); }); }
        protected virtual void CallRewardedAdDisplayed() { App.RunOnMainThread(() => { OnRewardedAdDisplayed?.Invoke(this); }); }

        protected virtual void CallRewardedAdPaid(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            App.RunOnMainThread(() =>
            {
                OnRewardedAdPaid?.Invoke(value,
                    network,
                    unitId,
                    placement,
                    adNetwork);
            });
        }

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

        protected virtual void CallRewardedInterAdSkipped() { App.RunOnMainThread(() => { OnRewardedInterAdSkipped?.Invoke(this); }); }
        protected virtual void CallRewardedInterAdCompleted() { App.RunOnMainThread(() => { OnRewardedInterAdCompleted?.Invoke(this); }); }
        protected virtual void CallRewardedInterAdClosed() { App.RunOnMainThread(() => { OnRewardedInterAdClosed?.Invoke(this); }); }
        protected virtual void CallRewardedInterAdDisplayed() { App.RunOnMainThread(() => { OnRewardedInterAdDisplayed?.Invoke(this); }); }

        protected virtual void CallRewardedInterAdPaid(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            App.RunOnMainThread(() =>
            {
                OnRewardedInterAdPaid?.Invoke(value,
                    network,
                    unitId,
                    placement,
                    adNetwork);
            });
        }

        public void LoadRewardedInterAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return;
                if (!IsRewardedInterAdReady()) InternalLoadRewardedInterstitialAd();
            }
            else
            {
                Debug.Log(NoSdkMessage);
            }
        }

        public IRewardedInterstitial ShowRewardedInterAd()
        {
            if (IsSdkAvaiable)
            {
                if (!CheckInitialize()) return null;
                if (!IsRewardedInterAdReady())
                {
                    Debug.LogFormat($"Cannot show {Network} rewarded interstitial ad : ad is not loaded.");
                    return null;
                }

                return InternalShowRewardedInterstitialAd();
            }

            Debug.Log(NoSdkMessage);
            return null;
        }

        public bool IsRewardedInterAdReady() { return CheckInitialize(false) && InternalIsRewardedInterstitialAdReady(); }

        protected virtual void CallAppOpenAdCompleted() { App.RunOnMainThread(() => { OnAppOpenAdCompleted?.Invoke(this); }); }
        protected virtual void CallAppOpenAdDisplayed() { App.RunOnMainThread(() => { OnAppOpenAdDisplayed?.Invoke(this); }); }

        protected virtual void CallAppOpenAdPaid(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            App.RunOnMainThread(() =>
            {
                OnAppOpenAdPaid?.Invoke(value,
                    network,
                    unitId,
                    placement,
                    adNetwork);
            });
        }

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
    }
}