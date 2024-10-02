#if PANCAKE_ADJUST
using AdjustSdk;
#endif

namespace Pancake.Tracking
{
    public static class AdjustTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string placement, string clientType)
        {
#if PANCAKE_ADJUST
            string source;
            switch (clientType.ToLower())
            {
                case "admob":
                    source = "admob_sdk";
                    break;
                default:
                    source = "applovin_max_sdk";
                    break;
            }

            var adRevenue = new AdjustAdRevenue(source);
            adRevenue.SetRevenue(value, "USD");
            adRevenue.AdRevenueNetwork = network;
            adRevenue.AdRevenuePlacement = placement;
            adRevenue.AdRevenueUnit = unitId;

            Adjust.TrackAdRevenue(adRevenue);
#endif
        }
    }
}