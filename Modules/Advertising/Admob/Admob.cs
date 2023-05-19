#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;

namespace Pancake.Monetization
{
    public static class Admob
    {
        internal static void SetupDeviceTest()
        {
            var configuration = new RequestConfiguration {TestDeviceIds = AdSettings.AdmobDevicesTest};
            MobileAds.SetRequestConfiguration(configuration);
        }
    }
}
#endif