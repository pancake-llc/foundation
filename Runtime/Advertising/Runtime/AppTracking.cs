#if PANCAKE_ANALYTIC
using Firebase.Analytics;
#endif

#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif

namespace Pancake.Monetization
{
    internal static class AppTracking
    {
#if PANCAKE_MAX_ENABLE
        internal static void TrackingRevenue(MaxSdkBase.AdInfo adInfo)
        {
#if PANCAKE_ADJUST
            var adRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAppLovinMAX);
            adRevenue.setRevenue(adInfo.Revenue, "USD");
            adRevenue.setAdRevenueNetwork(adInfo.NetworkName);
            adRevenue.setAdRevenuePlacement(adInfo.Placement);
            adRevenue.setAdRevenueUnit(adInfo.AdUnitIdentifier);
            Adjust.trackAdRevenue(adRevenue);
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
            var adRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, e.AdValue.CurrencyCode);
            adRevenue.setAdRevenueUnit(unitId);
            Adjust.trackAdRevenue(adRevenue);
#endif

            // google admob auto tracking ad_impression when using admob ad
        }
#endif
    }
}