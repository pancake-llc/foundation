using System;
#if PANCAKE_IAP
using Pancake.IAP;
#endif
using Pancake.Monetization;
using Pancake.Tracking;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;

namespace Pancake
{
    public static class Runtime
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (App.IsAppInitialized) return;

            if (Application.isPlaying)
            {
                var app = new GameObject("App");
                app.AddComponent<App.GlobalComponent>();
                UnityEngine.Object.DontDestroyOnLoad(app);

                Data.Init();
                var advertising = new GameObject("Advertising");
                advertising.AddComponent<Advertising>();
                advertising.transform.SetParent(app.transform);
                Advertising.OnBannerAdPaidEvent += AppTracking.TrackRevenue;
                Advertising.OnInterstitialAdPaidEvent += AppTracking.TrackRevenue;
                Advertising.OnRewardedAdPaidEvent += AppTracking.TrackRevenue;
                Advertising.OnRewardedInterAdPaidEvent += AppTracking.TrackRevenue;
                Advertising.OnAppOpenAdPaidEvent += AppTracking.TrackRevenue;
                Advertising.Init();

#if PANCAKE_IAP
                var pruchase = new GameObject("Purchase");
                pruchase.AddComponent<IAPManager>();
                pruchase.transform.SetParent(app.transform);
                IAPManager.Init();
                IAPManager.OnPurchaseEvent += Advertising.SwitchAdThread;
                IAPManager.OnPurchaseFailedEvent += Advertising.SwitchBackUnity;
                IAPManager.OnPurchaseSucceedEvent += Advertising.SwitchBackUnity;
#endif
                // Store the timestamp of the *first* init which can be used as a rough approximation of the installation time.
                if (!Data.HasKey(App.FIRST_INSTALL_TIMESTAMP_KEY)) Data.Save(App.FIRST_INSTALL_TIMESTAMP_KEY, DateTime.Now);

                App.IsAppInitialized = true;
                Debug.Log("<color=#52D5F2>Runtime has been initialized!</color>");

#if UNITY_IOS
                SkAdNetworkBinding.SkAdNetworkRegisterAppForNetworkAttribution();
                SkAdNetworkBinding.SkAdNetworkUpdateConversionValue(IOS14AdSupportSetting.SkAdConversionValue);
#endif
                if (HeartSettings.EnablePrivacyFirstOpen && !Data.Load<bool>(Invariant.USER_AGREE_PRIVACY_KEY)) ShowPrivacy();
                else
                {
                    RequestAuthorizationTracking();
                }

                var initProfile = Resources.Load<InitProfile>("InitProfile");
                if (initProfile != null)
                {
                    foreach (var i in initProfile.collection)
                    {
                        App.Attach(i);
                    }
                }
            }
        }

        private static void RequestAuthorizationTracking()
        {
#if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking(CallbackAuthorizationTracking);
            }
            else
            {
                AppTracking.CreateAdjustObject();
            }
#else
            AppTracking.StartTrackingAdjust();
#endif
        }

        private static void CallbackAuthorizationTracking(int status)
        {
            AppTracking.StartTrackingAdjust();
            FirebaseTracking.TrackATTResult(status); // todo: need confirm work?
        }

        private static void PrivacyPolicyValidate(bool status)
        {
            if (status)
            {
                if (!Data.Load<bool>(Invariant.USER_AGREE_PRIVACY_KEY)) ShowPrivacy();
            }
        }

        private static void ShowPrivacy()
        {
            if (Application.isEditor) return;
            App.RemoveFocusCallback(PrivacyPolicyValidate);
            NativePopup.ShowNeutral(HeartSettings.PrivacyTitle,
                HeartSettings.PrivacyMessage,
                "Continue",
                "Privacy",
                "",
                () =>
                {
                    Time.timeScale = 1;
                    Data.Save(Invariant.USER_AGREE_PRIVACY_KEY, true);
                    App.RemoveFocusCallback(PrivacyPolicyValidate);

                    // Show ATT
                    RequestAuthorizationTracking();
                },
                () =>
                {
                    App.AddFocusCallback(PrivacyPolicyValidate);
                    Application.OpenURL(HeartSettings.PrivacyUrl);
                });
            Time.timeScale = 0;
        }
    }
}