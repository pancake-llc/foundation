#if PANCAKE_FIREBASE_ANALYTIC
using Firebase.Analytics;
#endif

namespace Pancake.Tracking
{
    public static class FirebaseTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string format, string clientType)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            switch (clientType.ToLower())
            {
                case "admob":
                    return;
                default:
                    Parameter[] parameters =
                    {
                        new("value", value),
                        new("ad_platform", "AppLovin"),
                        new("ad_format", format),
                        new("currency", "USD"),
                        new("ad_unit_name", unitId),
                        new("ad_source", network)
                    };

                    FirebaseAnalytics.LogEvent("ad_impression", parameters);
                    break;
            }
#endif
        }

        // ReSharper disable once InconsistentNaming
        public static void TrackATTResult(int status)
        {
#if PANCAKE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent("app_tracking_transparency", "status", status);
#endif
        }
    }
}