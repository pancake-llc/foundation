using System;
using Pancake.Common;
using Pancake.Tracking;
using UnityEngine;
#if UNITY_IOS && PANCAKE_ATT
using Unity.Advertisement.IosSupport;
#endif

namespace Pancake.Game
{
    [EditorIcon("icon_default")]
    public class PrivacyInitialization : BaseInitialization
    {
        public override void Init()
        {
#if UNITY_IOS && PANCAKE_ATT
            SkAdNetworkBinding.SkAdNetworkRegisterAppForNetworkAttribution();
            SkAdNetworkBinding.SkAdNetworkUpdateConversionValue(HeartSettings.SkAdConversionValue);
#endif

            if (HeartSettings.EnablePrivacyFirstOpen && !Data.Load<bool>(Constant.User.AGREE_PRIVACY)) ShowPrivacy();
            else
            {
                RequestAuthorizationTracking();
            }
        }

        private static void RequestAuthorizationTracking()
        {
#if UNITY_IOS && PANCAKE_ATT
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking(CallbackAuthorizationTracking);
            }
            else
            {
                AppTracking.StartTrackingAdjust();
            }
#else
            AppTracking.StartTrackingAdjust();
#endif
        }

        // ReSharper disable once UnusedMember.Local
        private static void CallbackAuthorizationTracking(int status)
        {
            App.RunOnMainThread(() =>
            {
                AppTracking.StartTrackingAdjust();
                FirebaseTracking.TrackATTResult(status);
            });
        }

        private static void PrivacyPolicyValidate(bool status)
        {
            if (status)
            {
                if (!Data.Load<bool>(Constant.User.AGREE_PRIVACY)) ShowPrivacy();
            }
        }

        private static void ShowPrivacy()
        {
            if (Application.isEditor) return;
            App.RemoveFocusCallback(PrivacyPolicyValidate);

#if UNITY_ANDROID
            NativePopup.ShowNeutral(HeartSettings.PrivacyTitle,
                HeartSettings.PrivacyMessage,
                "Continue",
                "Privacy",
                "",
                () =>
                {
                    Time.timeScale = 1;
                    Data.Save(Constant.User.AGREE_PRIVACY, true);
                    App.RemoveFocusCallback(PrivacyPolicyValidate);

                    // Show ATT
                    RequestAuthorizationTracking();
                },
                () =>
                {
                    App.AddFocusCallback(PrivacyPolicyValidate);
                    Application.OpenURL(HeartSettings.PrivacyURL);
                });
#elif UNITY_IOS
            NativePopup.ShowQuestion(HeartSettings.PrivacyTitle,
                HeartSettings.PrivacyMessage,
                "Continue",
                "Privacy",
                () =>
                {
                    Time.timeScale = 1;
                    Data.Save(Constant.User.AGREE_PRIVACY, true);
                    App.RemoveFocusCallback(PrivacyPolicyValidate);

                    // Show ATT
                    RequestAuthorizationTracking();
                },
                () =>
                {
                    App.AddFocusCallback(PrivacyPolicyValidate);
                    Application.OpenURL(HeartSettings.PrivacyURL);
                });
#endif
            Time.timeScale = 0;
        }
    }
}