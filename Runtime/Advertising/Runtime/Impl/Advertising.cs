using System;
using System.Collections;
using UnityEngine;

namespace Pancake.Monetization
{
    [AddComponentMenu("")]
    public class Advertising : MonoBehaviour
    {
        public static Advertising Instance { get; private set; }
        public static event Action<EInterstitialAdNetwork> InterstitialAdCompletedEvent;
        public static event Action<ERewardedAdNetwork> RewardedAdCompletedEvent;
        public static event Action<ERewardedAdNetwork> RewardedAdSkippedEvent;
        public static event Action<ERewardedInterstitialAdNetwork> RewardedInterstitialAdCompletedEvent;
        public static event Action<ERewardedInterstitialAdNetwork> RewardedInterstitialAdSkippedEvent;
        public static event Action<EAppOpenAdNetwork> AppOpenAdCompletedEvent;
        public static event Action RemoveAdsEvent;

        private static AdmobAdClient admobAdClient;
        private static ApplovinAdClient applovinAdClient;
        private static IronSourceAdClient ironSourceAdClient;
        private static bool isInitialized;
        private static EAutoLoadingAd autoLoadingAdMode = EAutoLoadingAd.None;
        private static bool flagAutoLoadingModeChange;
        private static IEnumerator autoLoadAdCoroutine;
        private static float lastTimeLoadInterstitialAdTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadRewardedInterstitialTimestamp = DEFAULT_TIMESTAMP;
        private static float lastTimeLoadAppOpenTimestamp = DEFAULT_TIMESTAMP;
        private const string REMOVE_ADS_KEY = "remove_ads";
        private const string APP_OPEN_ADS_KEY = "flag_app_open_ads";
        private const float DEFAULT_TIMESTAMP = -1000;


        public static EAutoLoadingAd AutoLoadingAdMode
        {
            get => autoLoadingAdMode;
            set
            {
                if (value == autoLoadingAdMode) return;

                flagAutoLoadingModeChange = true;
                Settings.AdSettings.AutoLoadingAd = value;
                autoLoadingAdMode = value;
                flagAutoLoadingModeChange = false;

                if (autoLoadAdCoroutine != null) Instance.StopCoroutine(autoLoadAdCoroutine);
                switch (value)
                {
                    case EAutoLoadingAd.None:
                        autoLoadAdCoroutine = null;
                        break;
                    case EAutoLoadingAd.All:
                        autoLoadAdCoroutine = IeAutoLoadAll();
                        Instance.StartCoroutine(autoLoadAdCoroutine);
                        break;
                    default:
                        autoLoadAdCoroutine = null;
                        break;
                }
            }
        }

        public static bool IsInitialized => isInitialized;

        public static AdmobAdClient AdmobAdClient
        {
            get
            {
                if (!InitializeCheck()) return null;
                if (admobAdClient == null) admobAdClient = SetupClient(EAdNetwork.Admob) as AdmobAdClient;
                return admobAdClient;
            }
        }

        public static ApplovinAdClient ApplovinAdClient
        {
            get
            {
                if (!InitializeCheck()) return null;
                if (applovinAdClient == null) applovinAdClient = SetupClient(EAdNetwork.Applovin) as ApplovinAdClient;
                return applovinAdClient;
            }
        }

        public static IronSourceAdClient IronSourceAdClient
        {
            get
            {
                if (!InitializeCheck()) return null;
                if (ironSourceAdClient == null) ironSourceAdClient = SetupClient(EAdNetwork.IronSource) as IronSourceAdClient;
                return ironSourceAdClient;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (Settings.AdSettings.AutoInit) Initialize();
        }

        private void Update()
        {
            if (!IsInitialized) return;

            if (!flagAutoLoadingModeChange && autoLoadingAdMode != Settings.AdSettings.AutoLoadingAd)
            {
                AutoLoadingAdMode = Settings.AdSettings.AutoLoadingAd;
            }
        }

        public static void Initialize()
        {
            isInitialized = true;
            AutoLoadingAdMode = Settings.AdSettings.AutoLoadingAd;
#if PANCAKE_ADMOB_ENABLE
            if (Settings.AdSettings.CurrentNetwork == EAdNetwork.Admob) RegisterAppStateChange();
#endif
        }

        private static bool InitializeCheck()
        {
            if (!IsInitialized)
            {
                Debug.LogError("You need to initialize the advertising to use");
                return false;
            }

            return true;
        }

        /// <summary>
        /// none
        /// admob
        /// applovin
        /// </summary>
        /// <param name="network"></param>
        public static void SetCurrentNetwork(string network)
        {
            switch (network.Trim().ToLower())
            {
                case "none":
                    Settings.AdSettings.CurrentNetwork = EAdNetwork.None;
                    break;
                case "admob":
                    Settings.AdSettings.CurrentNetwork = EAdNetwork.Admob;
                    break;
                case "applovin":
                    Settings.AdSettings.CurrentNetwork = EAdNetwork.Applovin;
                    break;
                case "ironsource":
                    Settings.AdSettings.CurrentNetwork = EAdNetwork.IronSource;
                    break;
                default:
                    Settings.AdSettings.CurrentNetwork = EAdNetwork.Admob;
                    break;
            }
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
                yield return new WaitForSeconds(Settings.AdSettings.AdCheckingInterval);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private static void AutoLoadInterstitialAd()
        {
            if (IsAdRemoved) return;
            if (IsInterstitialAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadInterstitialAdTimestamp < Settings.AdSettings.AdLoadingInterval) return;

            LoadInsterstitialAd();
            lastTimeLoadInterstitialAdTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadRewardedAd()
        {
            if (IsRewardedAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadRewardedTimestamp < Settings.AdSettings.AdLoadingInterval) return;

            LoadRewardedAd();
            lastTimeLoadRewardedTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadRewardedInterstitialAd()
        {
            if (IsRewardedInterstitialAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadRewardedInterstitialTimestamp < Settings.AdSettings.AdLoadingInterval) return;

            LoadRewardedInterstitialAd();
            lastTimeLoadRewardedInterstitialTimestamp = Time.realtimeSinceStartup;
        }

        private static void AutoLoadAppOpenAd()
        {
            if (IsAdRemoved || IsAppOpenRemoved) return;
            if (IsAppOpenAdReady()) return;

            if (Time.realtimeSinceStartup - lastTimeLoadAppOpenTimestamp < Settings.AdSettings.AdLoadingInterval) return;

            LoadAppOpenAd();
            lastTimeLoadAppOpenTimestamp = Time.realtimeSinceStartup;
        }

        private static void OnInterstitialAdCompleted(IAdClient client) { InterstitialAdCompletedEvent?.Invoke((EInterstitialAdNetwork) client.Network); }

        private static void OnRewardedAdCompleted(IAdClient client) { RewardedAdCompletedEvent?.Invoke((ERewardedAdNetwork) client.Network); }

        private static void OnRewardedAdSkipped(IAdClient client) { RewardedAdSkippedEvent?.Invoke((ERewardedAdNetwork) client.Network); }

        private static void OnRewardedInterstitialAdCompleted(IAdClient client)
        {
            RewardedInterstitialAdCompletedEvent?.Invoke((ERewardedInterstitialAdNetwork) client.Network);
        }

        private static void OnRewardedInterstitialAdSkipped(IAdClient client)
        {
            RewardedInterstitialAdSkippedEvent?.Invoke((ERewardedInterstitialAdNetwork) client.Network);
        }

        private static void OnAppOpenAdCompleted(IAdClient client) { AppOpenAdCompletedEvent?.Invoke((EAppOpenAdNetwork) client.Network); }

        private static AdClient GetClient(EAdNetwork network)
        {
            switch (network)
            {
                case EAdNetwork.None: return NoneAdClient.Instance;
                case EAdNetwork.Admob: return AdmobAdClient.Instance;
                case EAdNetwork.Applovin: return ApplovinAdClient.Instance;
                case EAdNetwork.IronSource: return IronSourceAdClient.Instance;
                default: return null;
            }
        }

        private static AdClient GetClientAlreadySetup(EAdNetwork network)
        {
            if (!InitializeCheck()) return NoneAdClient.Instance;
            switch (network)
            {
                case EAdNetwork.None: return NoneAdClient.Instance;
                case EAdNetwork.Admob: return AdmobAdClient;
                case EAdNetwork.Applovin: return ApplovinAdClient;
                case EAdNetwork.IronSource: return IronSourceAdClient;
                default: return null;
            }
        }

        private static void SetupEvent(IAdClient client)
        {
            if (client == null) return;

            client.OnInterstitialAdCompleted += OnInterstitialAdCompleted;
            client.OnRewardedAdCompleted += OnRewardedAdCompleted;
            client.OnRewardedAdSkipped += OnRewardedAdSkipped;
            client.OnRewardedInterstitialAdCompleted += OnRewardedInterstitialAdCompleted;
            client.OnRewardedInterstitialAdSkipped += OnRewardedInterstitialAdSkipped;
            client.OnAppOpenAdCompleted += OnAppOpenAdCompleted;
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

        public static bool IsAdRemoved => StorageUtil.GetBool(REMOVE_ADS_KEY, false);

        public static void RemoveAds()
        {
            StorageUtil.SetBool(REMOVE_ADS_KEY, true);
            StorageUtil.Save();

            RemoveAdsEvent?.Invoke();
        }

        public static void TurnOffAppOpenAds()
        {
            StorageUtil.SetBool(APP_OPEN_ADS_KEY, false);
            StorageUtil.Save();
        }

        public static void TurnOnAppOpenAds()
        {
            StorageUtil.SetBool(APP_OPEN_ADS_KEY, true);
            StorageUtil.Save();
        }

        public static bool IsAppOpenRemoved => !StorageUtil.GetBool(APP_OPEN_ADS_KEY, true) || IsAdRemoved;

        private static void ShowBannerAd(IAdClient client)
        {
            if (IsAdRemoved || !Application.isMobilePlatform) return;

            client.ShowBannerAd();
        }

        private static void HideBannerAd(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.HideBannerAd();
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

        private static void ShowInterstitialAd(IAdClient client)
        {
            if (IsAdRemoved || !Application.isMobilePlatform) return;
            client.ShowInterstitialAd();
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

        private static void ShowRewardedAd(IAdClient client) { client.ShowRewardedAd(); }

        private static void LoadRewardedInterstitialAd(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.LoadRewardedInterstitialAd();
        }

        private static bool IsRewardedInterstitialAdReady(IAdClient client)
        {
            if (!IsInitialized || !Application.isMobilePlatform) return false;
            return client.IsRewardedInterstitialAdReady();
        }

        private static void ShowRewardedInterstitialAd(IAdClient client) { client.ShowRewardedInterstitialAd(); }

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
            client.ShowAppOpenAd();
        }

        private static void ShowConsentForm(IAdClient client)
        {
            if (!Application.isMobilePlatform) return;
            client.ShowConsentForm();
        }

        public static void ShowBannerAd() { ShowBannerAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void HideBannerAd() { HideBannerAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void DestroyBannerAd() { DestroyBannerAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static float GetAdaptiveBannerHeight() { return GetClientAlreadySetup(Settings.CurrentNetwork).GetAdaptiveBannerHeight; }

        public static void LoadInsterstitialAd() { LoadInterstitialAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static bool IsInterstitialAdReady() { return IsInterstitialAdReady(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void ShowInterstitialAd() { ShowInterstitialAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void LoadRewardedAd() { LoadRewardedAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static bool IsRewardedAdReady() { return IsRewardedAdReady(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void ShowRewardedAd() { ShowRewardedAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void LoadRewardedInterstitialAd() { LoadRewardedInterstitialAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static bool IsRewardedInterstitialAdReady() { return IsRewardedInterstitialAdReady(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        public static void ShowRewardedInterstitialAd() { ShowRewardedInterstitialAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        private static void LoadAppOpenAd() { LoadAppOpenAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        private static bool IsAppOpenAdReady() { return IsAppOpenAdReady(GetClientAlreadySetup(Settings.CurrentNetwork)); }

        private static void ShowAppOpenAd() { ShowAppOpenAd(GetClientAlreadySetup(Settings.CurrentNetwork)); }

#if PANCAKE_ADMOB_ENABLE
        private static void RegisterAppStateChange() { GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged; }

        private static void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground)
            {
                ShowAppOpenAd();
            }
        }
#elif PANCAKE_MAX_ENABLE
        private void OnApplicationPause(bool pauseStatus)
        {
            if(!pauseStatus) ShowAppOpenAd();
        }
#endif


        public static void ShowConsentFrom() { ShowConsentForm(GetClientAlreadySetup(Settings.CurrentNetwork)); }
    }
}