using System.Collections;
using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Monetization
{
    [HideMonoScript]
    public class Advertising : GameComponent
    {
        [SerializeField] private AdSettings adSettings;
        [SerializeField] private ScriptableEventString changeNetworkEvent;
        [SerializeField] private ScriptableEventBool changePreventDisplayAppOpenEvent;

        private IEnumerator _autoLoadAdCoroutine;
        private float _lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private float _lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        private void Start()
        {
            if (adSettings.AdmobClient != null) adSettings.AdmobClient.Init();
            if (adSettings.ApplovinClient != null) adSettings.ApplovinClient.Init();

            if (changeNetworkEvent != null) changeNetworkEvent.OnRaised += OnChangeNetworkCallback;
            if (changePreventDisplayAppOpenEvent != null) changePreventDisplayAppOpenEvent.OnRaised += OnChangePreventDisplayOpenAd;

            if (_autoLoadAdCoroutine != null) StopCoroutine(_autoLoadAdCoroutine);
            _autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(_autoLoadAdCoroutine);
        }

        private void OnChangePreventDisplayOpenAd(bool state) { AdStatic.isShowingAd = state; }

        private void OnChangeNetworkCallback(string value)
        {
            adSettings.CurrentNetwork = value.Trim().ToLower() switch
            {
                "admob" => EAdNetwork.Admob,
                _ => EAdNetwork.Applovin
            };
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
            GetClient(adSettings.CurrentNetwork).LoadInterstitial();
            _lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadRewarded();
            _lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedInterstitialAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadRewardedInterstitialTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadRewardedInterstitial();
            _lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadAppOpenAd()
        {
            if (Time.realtimeSinceStartup - _lastTimeLoadAppOpenTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadAppOpen();
            _lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        private AdClient GetClient(EAdNetwork network)
        {
            return network switch
            {
                EAdNetwork.Admob => adSettings.AdmobClient,
                _ => adSettings.ApplovinClient,
            };
        }

#if PANCAKE_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) (GetClient(adSettings.CurrentNetwork) as ApplovinAdClient)?.ShowAppOpen();
        }
#endif
    }
}