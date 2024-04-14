using System.Collections;
using System.Collections.Generic;
using Alchemy.Inspector;
#if PANCAKE_ADMOB
using GoogleMobileAds.Ump.Api;
#endif

using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Monetization
{

    public class Advertising : GameComponent
    {
        private AdClient _adClient;
        [SerializeField] private AdSettings adSettings;
        [SerializeField] private ScriptableEventString changeNetworkEvent;
        [SerializeField] private ScriptableEventBool changePreventDisplayAppOpenEvent;
#if PANCAKE_ADMOB
        [SerializeField, LabelText("Show GDPR Again Event")] private ScriptableEventNoParam showGdprAgainEvent;
        [SerializeField, LabelText("GDPR Reset Event")] private ScriptableEventNoParam gdprResetEvent;
#endif

        private IEnumerator _autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private void Start()
        {
            AdStatic.currentNetworkShared = adSettings.CurrentNetwork;
            if (adSettings.Gdpr)
            {
#if PANCAKE_ADMOB
                if (showGdprAgainEvent != null) showGdprAgainEvent.OnRaised += LoadAndShowConsentForm;
                if (gdprResetEvent != null) gdprResetEvent.OnRaised += GdprReset;
                var request = new ConsentRequestParameters {TagForUnderAgeOfConsent = false};
                if (adSettings.GdprTestMode)
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

            if (changeNetworkEvent != null) changeNetworkEvent.OnRaised += OnChangeNetworkCallback;
            if (changePreventDisplayAppOpenEvent != null) changePreventDisplayAppOpenEvent.OnRaised += OnChangePreventDisplayOpenAd;
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

        private void OnChangePreventDisplayOpenAd(bool state) { AdStatic.isShowingAd = state; }

        private void OnChangeNetworkCallback(string value)
        {
            adSettings.CurrentNetwork = value.Trim().ToLower() switch
            {
                "admob" => EAdNetwork.Admob,
                _ => EAdNetwork.Applovin
            };
            AdStatic.currentNetworkShared = adSettings.CurrentNetwork;
            AdStatic.waitAppOpenClosedAction = null;
            AdStatic.waitAppOpenDisplayedAction = null;
            InitClient();
        }

        private void InitClient()
        {
            _adClient = adSettings.CurrentNetwork switch
            {
                EAdNetwork.Applovin => new ApplovinAdClient(),
                EAdNetwork.Admob => new AdmobClient(),
                _ => _adClient
            };

            _adClient.SetupSetting(adSettings);
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
                yield return new WaitForSeconds(adSettings.AdCheckingInterval);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void AutoLoadInterstitialAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadInterstitialAdTimestamp < adSettings.AdLoadingInterval) return;
            _adClient.LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp < adSettings.AdLoadingInterval) return;
            _adClient.LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedInterstitialAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp < adSettings.AdLoadingInterval) return;
            _adClient.LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadAppOpenAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp < adSettings.AdLoadingInterval) return;
            _adClient.LoadAppOpen();
            _lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        private void OnDisable()
        {
#if PANCAKE_ADMOB
            if (showGdprAgainEvent != null) showGdprAgainEvent.OnRaised -= LoadAndShowConsentForm;
            if (gdprResetEvent != null) gdprResetEvent.OnRaised -= GdprReset;
#endif
        }

#if PANCAKE_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) (_adClient as ApplovinAdClient)?.ShowAppOpen();
        }
#endif
    }
}