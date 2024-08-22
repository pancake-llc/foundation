using System;
using System.Collections;
using System.Collections.Generic;
#if PANCAKE_ADMOB
using GoogleMobileAds.Ump.Api;
#endif

using UnityEngine;
using Pancake.Common;

namespace Pancake.Monetization
{
    [EditorIcon("icon_manager")]
    public class Advertising : GameComponent
    {
        private static event Action<string> ChangeNetworkEvent;
        private static event Action<bool> ChangePreventDisplayAppOpenEvent;
        private static event Action ShowGdprAgainEvent;
        private static event Action GdprResetEvent;
        private static event Func<AdUnit> GetBannerAdEvent;
        private static event Func<AdUnit> GetInterAdEvent;
        private static event Func<AdUnit> GetRewardAdEvent;
        private static event Func<AdUnit> GetRewardInterAdEvent;
        private static event Func<AdUnit> GetAppOpenAdEvent;

        private AdClient _adClient;

        /// <summary>
        /// prevent show app open ad, it will become true when interstitial or rewarded was showed
        /// </summary>
        internal static bool isShowingAd;

        internal static Action waitAppOpenDisplayedAction;
        internal static Action waitAppOpenClosedAction;

        private IEnumerator _autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private void Start()
        {
            if (AdSettings.Gdpr)
            {
#if PANCAKE_ADMOB
                ShowGdprAgainEvent += LoadAndShowConsentForm;
                GdprResetEvent += GdprReset;
                var request = new ConsentRequestParameters {TagForUnderAgeOfConsent = false};
                if (AdSettings.GdprTestMode)
                {
                    string deviceID = SystemInfo.deviceUniqueIdentifier.ToUpper();
                    var consentDebugSettings = new ConsentDebugSettings {DebugGeography = DebugGeography.EEA, TestDeviceHashedIds = new List<string> {deviceID}};
                    request.ConsentDebugSettings = consentDebugSettings;
                }

                ConsentInformation.Update(request, OnConsentInfoUpdated);
#endif
            }
            else
            {
                InternalInitAd();
            }

            ChangeNetworkEvent += OnChangeNetworkCallback;
            ChangePreventDisplayAppOpenEvent += OnChangePreventDisplayOpenAd;
            GetBannerAdEvent += OnGetBannerAd;
            GetInterAdEvent += OnGetInterAd;
            GetRewardAdEvent += OnGetRewardAd;
            GetRewardInterAdEvent += OnGetRewardInterAd;
            GetAppOpenAdEvent += OnGetOpenAd;
        }

        private void InternalInitAd()
        {
            InitClient();
            if (_autoLoadAdCoroutine != null) StopCoroutine(_autoLoadAdCoroutine);
            _autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(_autoLoadAdCoroutine);
        }

#if PANCAKE_ADMOB
        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                Debug.Log("Error consentError = " + consentError);
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    Debug.Log("Error consentError = " + formError);
                    return;
                }

                if (ConsentInformation.CanRequestAds()) InternalInitAd();
            });
        }

        private void LoadAndShowConsentForm()
        {
            ConsentForm.Load((consentForm, loadError) =>
            {
                if (loadError != null)
                {
                    Debug.Log("Error loadError = " + loadError);
                    return;
                }

                consentForm.Show(showError =>
                {
                    if (showError != null)
                    {
                        Debug.Log("Error showError = " + showError);
                        return;
                    }

                    if (ConsentInformation.CanRequestAds()) InternalInitAd();
                });
            });
        }

        private void GdprReset() { ConsentInformation.Reset(); }

#endif

        private void OnChangePreventDisplayOpenAd(bool state) { isShowingAd = state; }

        private void OnChangeNetworkCallback(string value)
        {
            AdSettings.CurrentNetwork = value.Trim().ToLower() switch
            {
                "admob" => EAdNetwork.Admob,
                _ => EAdNetwork.Applovin
            };
            waitAppOpenClosedAction = null;
            waitAppOpenDisplayedAction = null;
            InitClient();
        }

        private void InitClient()
        {
            _adClient = AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => new ApplovinAdClient(),
                EAdNetwork.Admob => new AdmobClient(),
                _ => _adClient
            };

            _adClient.Init();
        }

        private IEnumerator IeAutoLoadAll(float delay = 0)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            while (true)
            {
                AutoLoadInterstitialAd();
                AutoLoadRewardedAd();
                AutoLoadRewardedInterstitialAd();
                AutoLoadAppOpenAd();
                yield return new WaitForSeconds(AdSettings.AdCheckingInterval);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void AutoLoadInterstitialAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp < AdSettings.AdLoadingInterval) return;
            _adClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp < AdSettings.AdLoadingInterval) return;
            _adClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedInterstitialAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp < AdSettings.AdLoadingInterval) return;
            _adClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadAppOpenAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp < AdSettings.AdLoadingInterval) return;
            _adClient.LoadAppOpen();
            _lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        private AdUnit OnGetOpenAd()
        {
            return AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => AdSettings.ApplovinAppOpen,
                _ => AdSettings.AdmobAppOpen,
            };
        }

        private AdUnit OnGetRewardInterAd()
        {
            return AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => AdSettings.ApplovinRewardInter,
                _ => AdSettings.AdmobRewardInter,
            };
        }

        private AdUnit OnGetRewardAd()
        {
            return AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => AdSettings.ApplovinReward,
                _ => AdSettings.AdmobReward,
            };
        }

        private AdUnit OnGetInterAd()
        {
            return AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => AdSettings.ApplovinInter,
                _ => AdSettings.AdmobInter,
            };
        }

        private AdUnit OnGetBannerAd()
        {
            return AdSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => AdSettings.ApplovinBanner,
                _ => AdSettings.AdmobBanner,
            };
        }

        private void OnDisable()
        {
            ChangeNetworkEvent -= OnChangeNetworkCallback;
            ChangePreventDisplayAppOpenEvent -= OnChangePreventDisplayOpenAd;
#if PANCAKE_ADMOB
            ShowGdprAgainEvent -= LoadAndShowConsentForm;
            GdprResetEvent -= GdprReset;
#endif
        }

#if PANCAKE_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) (_adClient as ApplovinAdClient)?.ShowAppOpen();
        }
#endif

        #region API

        public static AdUnit Banner => GetBannerAdEvent?.Invoke();
        public static AdUnit Inter => GetInterAdEvent?.Invoke();
        public static AdUnit Reward => GetRewardAdEvent?.Invoke();
        public static AdUnit RewardInter => GetRewardInterAdEvent?.Invoke();
        public static AdUnit AppOpen => GetAppOpenAdEvent?.Invoke();
        public static void ChangeNetwork(string network) { ChangeNetworkEvent?.Invoke(network); }
        public static void ChangePreventDisplayAppOpen(bool status) { ChangePreventDisplayAppOpenEvent?.Invoke(status); }
        public static void ShowGdprAgain() { ShowGdprAgainEvent?.Invoke(); }
        public static void ResetGdpr() { GdprResetEvent?.Invoke(); }

        public static bool IsRemoveAd { get => Data.Load($"{Application.identifier}_removeads", false); set => Data.Save($"{Application.identifier}_removeads", value); }

        #endregion
    }
}