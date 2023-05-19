using System;
using System.Collections;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Monetization
{
    [AddComponentMenu("")]
    [HideMonoScript]
    public class Advertising : GameComponent
    {
        private void Start()
        {
            if (autoLoadAdCoroutine != null) StopCoroutine(autoLoadAdCoroutine);
            autoLoadAdCoroutine = IeAutoLoadAll();
            StartCoroutine(autoLoadAdCoroutine);
        }


        public static event Action RemoveAdsEvent;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private static AdmobAdClient admobAdClient;
#endif
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private static ApplovinAdClient applovinAdClient;
#endif
        private static bool isInitialized;
        private static IEnumerator autoLoadAdCoroutine;
        private static float lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private static bool flagStartupOpenAd;
        private const string REMOVE_ADS_KEY = "remove_ads";
        private const string APP_OPEN_ADS_KEY = "flag_app_open_ads";
        private const float DEFAULT_TIMESTAMP = -1000;

        public static bool IsInitialized => isInitialized;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private static AdmobAdClient AdmobAdClient
        {
            get
            {
                if (!IsInitialized) return null;
                if (admobAdClient == null) admobAdClient = SetupClient(EAdNetwork.Admob) as AdmobAdClient;
                return admobAdClient;
            }
        }
#endif

#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private static ApplovinAdClient ApplovinAdClient
        {
            get
            {
                if (!IsInitialized) return null;
                if (applovinAdClient == null) applovinAdClient = SetupClient(EAdNetwork.Applovin) as ApplovinAdClient;
                return applovinAdClient;
            }
        }
#endif

        private void Awake()
        {
            flagStartupOpenAd = true;
        }

        public static void Init()
        {
#if PANCAKE_ADVERTISING
            isInitialized = true;
#if PANCAKE_ADMOB
            if (AdSettings.CurrentNetwork == EAdNetwork.Admob) RegisterAppStateChange();
#endif
#endif
        }

        /// <summary>
        /// admob
        /// applovin
        /// </summary>
        /// <param name="network"></param>
        public static void SetCurrentNetwork(string network)
        {
            AdSettings.CurrentNetwork = network.Trim().ToLower() switch
            {
                "admob" => EAdNetwork.Admob,
                _ => EAdNetwork.Applovin
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="network"></param>
        public static void SetCurrentNetwork(EAdNetwork network) { SetCurrentNetwork(network.ToString()); }

        private static IEnumerator IeAutoLoadAll(float delay = 0)
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

        private static void AutoLoadInterstitialAd()
        {
            if (IsAdRemoved) return;
            if (IsInterstitialAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadInterstitialAdTimestamp < AdSettings.AdLoadingInterval) return;

            LoadInsterstitialAd();
            lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadRewardedAd()
        {
            if (IsRewardedAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadRewardedTimestamp < AdSettings.AdLoadingInterval) return;

            LoadRewardedAd();
            lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadRewardedInterstitialAd()
        {
            if (IsRewardedInterstitialAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadRewardedInterstitialTimestamp < AdSettings.AdLoadingInterval) return;

            LoadRewardedInterstitialAd();
            lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadAppOpenAd()
        {
            if (IsAdRemoved || IsAppOpenRemoved) return;
            if (IsAppOpenAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadAppOpenTimestamp < AdSettings.AdLoadingInterval) return;

            LoadAppOpenAd();
            lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }
        
        private static AdClient GetClient(EAdNetwork network)
        {
            switch (network)
            {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
                case EAdNetwork.Admob: return AdmobAdClient.Instance;
#endif
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
                case EAdNetwork.Applovin: return ApplovinAdClient.Instance;
#endif
                default: return NoneAdClient.Instance;
            }
        }

        private static AdClient GetClientAlreadySetup(EAdNetwork network)
        {
            if (!IsInitialized) return NoneAdClient.Instance;
            switch (network)
            {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
                case EAdNetwork.Admob: return AdmobAdClient;
#endif
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
                case EAdNetwork.Applovin: return ApplovinAdClient;
#endif
                default: return NoneAdClient.Instance;
            }
        }

        private static void SetupEvent(IAdClient client)
        {
            if (client == null) return;

            client.OnBannerAdCompleted += OnBannerAdCompleted;
            client.OnBannerAdDisplayed += OnBannerAdDisplayed;
            client.OnBannerAdPaid += OnBannerAdPaid;

            client.OnInterstitialAdCompleted += OnInterstitialAdCompleted;
            client.OnInterstitialAdDisplayed += OnInterstitialAdDisplayed;
            client.OnInterstitialAdPaid += OnInterstitialAdPaid;

            client.OnRewardedAdCompleted += OnRewardedAdCompleted;
            client.OnRewardedAdSkipped += OnRewardedAdSkipped;
            client.OnRewardedAdClosed += OnRewardedAdClosed;
            client.OnRewardedAdDisplayed += OnRewardedAdDisplayed;
            client.OnRewardedAdPaid += OnRewardedAdPaid;

            client.OnRewardedInterAdCompleted += OnRewardedInterAdCompleted;
            client.OnRewardedInterAdSkipped += OnRewardedInterAdSkipped;
            client.OnRewardedInterAdClosed += OnRewardedInterAdClosed;
            client.OnRewardedInterAdDisplayed += OnRewardedInterAdDisplayed;
            client.OnRewardedInterAdPaid += OnRewardedInterAdPaid;

            client.OnAppOpenAdCompleted += OnAppOpenAdCompleted;
            client.OnAppOpenAdDisplayed += OnAppOpenAdDisplayed;
            client.OnAppOpenAdPaid += OnAppOpenAdPaid;
        }

        private static AdClient SetupClient(EAdNetwork network)
        {
            var client = GetClient(network);
            if (client != null && client.Network != EAdNetwork.None)
            {
                SetupEvent(client);
                if (!client.IsInitialized) client.Initialize();
            }

            return client;
        }

        public static bool IsAdRemoved => Data.Load(REMOVE_ADS_KEY, false);

        public static void RemoveAds()
        {
            Data.Save(REMOVE_ADS_KEY, true);
            RemoveAdsEvent?.Invoke();
        }

        public static void TurnOffAppOpenAds() { Data.Save(APP_OPEN_ADS_KEY, false); }

        public static void TurnOnAppOpenAds() { Data.Save(APP_OPEN_ADS_KEY, true); }

        public static bool IsAppOpenRemoved => !Data.Load(APP_OPEN_ADS_KEY, true) || IsAdRemoved;

        private static void ShowBannerAd(IAdClient client)
        {
            if (IsAdRemoved || !Application.isMobilePlatform) return;
            client.ShowBannerAd();
        }

        private static void DestroyBannerAd(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.DestroyBannerAd();
        }

        private static void LoadInterstitialAd(IAdClient client)
        {
            if (IsAdRemoved || !Application.isMobilePlatform) return;
            client.LoadInterstitialAd();
        }

        private static bool IsInterstitialAdReady(IAdClient client)
        {
            if (!IsInitialized || IsAdRemoved || !Application.isMobilePlatform) return false;
            return client.IsInterstitialAdReady();
        }

        private static IInterstitial ShowInterstitialAd(IAdClient client)
        {
            if (IsAdRemoved || !Application.isMobilePlatform) return null;
            return client.ShowInterstitialAd();
        }

        private static void LoadRewardedAd(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.LoadRewardedAd();
        }

        private static bool IsRewardedAdReady(IAdClient client)
        {
            if (!IsInitialized || !Application.isMobilePlatform) return false;
            return client.IsRewardedAdReady();
        }

        private static IRewarded ShowRewardedAd(IAdClient client) { return client.ShowRewardedAd(); }

        private static void LoadRewardedInterstitialAd(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.LoadRewardedInterAd();
        }

        private static bool IsRewardedInterstitialAdReady(IAdClient client)
        {
            if (!IsInitialized || !Application.isMobilePlatform) return false;
            return client.IsRewardedInterAdReady();
        }

        private static IRewardedInterstitial ShowRewardedInterstitialAd(IAdClient client) { return client.ShowRewardedInterAd(); }

        private static void LoadAppOpenAd(IAdClient client)
        {
            if (IsAdRemoved || IsAppOpenRemoved || !Application.isMobilePlatform) return;
            client.LoadAppOpenAd();
        }

        private static bool IsAppOpenAdReady(IAdClient client)
        {
            if (!IsInitialized || IsAppOpenRemoved || IsAdRemoved || !Application.isMobilePlatform) return false;
            return client.IsAppOpenAdReady();
        }

        private static void ShowAppOpenAd(IAdClient client)
        {
            if (IsAdRemoved || IsAppOpenRemoved || !Application.isMobilePlatform) return;
            if (flagStartupOpenAd)
            {
                flagStartupOpenAd = false;
                return;
            }

            client.ShowAppOpenAd();
        }

        public static void ShowBannerAd() { ShowBannerAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static void DestroyBannerAd() { DestroyBannerAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static void LoadInsterstitialAd() { LoadInterstitialAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static bool IsInterstitialAdReady() { return IsInterstitialAdReady(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static IInterstitial ShowInterstitialAd() { return ShowInterstitialAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static void LoadRewardedAd() { LoadRewardedAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static bool IsRewardedAdReady() { return IsRewardedAdReady(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static IRewarded ShowRewardedAd() { return ShowRewardedAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static void LoadRewardedInterstitialAd() { LoadRewardedInterstitialAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static bool IsRewardedInterstitialAdReady() { return IsRewardedInterstitialAdReady(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        public static IRewardedInterstitial ShowRewardedInterstitialAd() { return ShowRewardedInterstitialAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        private static void LoadAppOpenAd() { LoadAppOpenAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        private static bool IsAppOpenAdReady() { return IsAppOpenAdReady(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

        private static void ShowAppOpenAd() { ShowAppOpenAd(GetClientAlreadySetup(AdSettings.CurrentNetwork)); }

#if PANCAKE_ADMOB
        private static void RegisterAppStateChange() { GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged; }

        internal static void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        }
#elif PANCAKE_APPLOVIN
        private void OnApplicationPause(bool pauseStatus)
        {
            if(!pauseStatus) ShowAppOpenAd();
        }
#endif

        /// <summary>
        /// Please only use this method when you are sure that your app switches to android activity and you don't want to show app-open-ad when you return to the game.
        /// <para>OTHERWISE DON'T USE IT</para>
        /// </summary>
        public static void SwitchAdThread() { R.isShowingAd = true; }

        public static void SwitchBackUnity(string _) { R.isShowingAd = false; }
    }
}