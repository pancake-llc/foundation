#if PANCAKE_ADS
#if PANCAKE_ANALYTIC
using Firebase.Analytics;
#endif

#if PANCAKE_ADJUST
using AdjustSdk;
#endif

namespace Pancake.Monetization
{
    internal static class AppTracking
    {
#if PANCAKE_MAX_ENABLE
        internal static void TrackingRevenue(MaxSdkBase.AdInfo adInfo)
        {
#if PANCAKE_ADJUST
            
            var adRevenue = new AdjustAdRevenue("applovin_max_sdk");
            adRevenue.SetRevenue(adInfo.Revenue, "USD");
            adRevenue.AdRevenueNetwork = adInfo.NetworkName;
            adRevenue.AdRevenuePlacement = adInfo.Placement;
            adRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;

            Adjust.TrackAdRevenue(adRevenue);
#endif

#if PANCAKE_ANALYTIC

            // Log an event with ad value parameters
            Parameter[] parameters =
            {
                // Log ad value in micros.
                new Parameter("value", adInfo.Revenue), 
                new Parameter("ad_platform", "AppLovin"), 
                new Parameter("ad_format", adInfo.AdFormat),
                new Parameter("currency", "USD"),
                new Parameter("ad_unit_name", adInfo.AdUnitIdentifier), 
                new Parameter("ad_source", adInfo.NetworkName)
            };

            FirebaseAnalytics.LogEvent("ad_impression", parameters);
#endif
        }
#endif

#if PANCAKE_ADMOB_ENABLE
        internal static void TrackingRevenue(GoogleMobileAds.Api.AdValueEventArgs e, string unitId)
        {
            double revenue = e.AdValue.Value / 1000000f;
#if PANCAKE_ADJUST
            var adRevenue = new AdjustAdRevenue("admob_sdk");
            adRevenue.SetRevenue(revenue, "USD");
            adRevenue.AdRevenueUnit = unitId;

            Adjust.TrackAdRevenue(adRevenue);
#endif

            // google admob auto tracking ad_impression when using admob ad
        }
#endif
    }
}
#endif