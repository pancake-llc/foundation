using System.Collections;
using System.Linq;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Monetization
{
    [AddComponentMenu("")]
    [HideMonoScript]
    public class Advertising : GameComponent
    {
        [SerializeField] private AdSettings adSettings;
        [SerializeField, Array] private AdClient[] clients;

        private void Start()
        {
            foreach (var client in clients)
            {
                client.Init();
            }

            if (autoLoadAdCoroutine != null) StopCoroutine(autoLoadAdCoroutine);
            autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(autoLoadAdCoroutine);
        }

        private static bool isInitialized;
        private static IEnumerator autoLoadAdCoroutine;
        private static float lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const float DEFAULT_TIMESTAMP = -1000;

        public static bool IsInitialized => isInitialized;

        public void Init() { isInitialized = true; }

        /// <summary>
        /// admob
        /// applovin
        /// </summary>
        /// <param name="network"></param>
        public void SetCurrentNetwork(string network)
        {
            adSettings.CurrentNetwork = network.Trim().ToLower() switch
            {
                "admob" => EAdNetwork.Admob,
                _ => EAdNetwork.Applovin
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        public void SetCurrentNetwork(EAdNetwork network) { SetCurrentNetwork(network.ToString()); }

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
            if (Time.realtimeSinceStartup - lastTimeLoadInterstitialAdTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadInterstitial();
            lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedAd()
        {
            if (Time.realtimeSinceStartup - lastTimeLoadRewardedTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadRewarded();
            lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadRewardedInterstitialAd()
        {
            if (Time.realtimeSinceStartup - lastTimeLoadRewardedInterstitialTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadRewardedInterstitial();
            lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private void AutoLoadAppOpenAd()
        {
            if (Time.realtimeSinceStartup - lastTimeLoadAppOpenTimestamp < adSettings.AdLoadingInterval) return;
            GetClient(adSettings.CurrentNetwork).LoadAppOpen();
            lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        private AdClient GetClient(EAdNetwork network) { return clients.First(_ => _.ClientType == network); }

#if PANCAKE_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            //if(!pauseStatus) ShowAppOpenAd();
        }
#endif
    }
}