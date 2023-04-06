#if PANCAKE_ADMOB
using GoogleMobileAds.Api;

namespace Pancake.Monetization
{
    public static class Admob
    {
        internal static void SetupDeviceTest()
        {
            var configuration = new RequestConfiguration.Builder().SetTestDeviceIds(AdSettings.AdmobSettings.DevicesTest).build();
            MobileAds.SetRequestConfiguration(configuration);
        }

        internal static AdRequest CreateRequest()
        {
            var builder = new AdRequest.Builder();
            // targetting setting
            // extra options
            // consent
            //if (AdSettings.AdCommonSettings.EnableGDPR) builder.AddExtra("npa", GDPRHelper.GetValueGDPR().ToString());

            return builder.Build();
        }
    }
}
#endif