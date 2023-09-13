#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif

namespace Pancake.Tracking
{
    public static class AdjustTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string placement, string clientType)
        {
#if PANCAKE_ADJUST
            var source = "";
            switch (clientType.ToLower())
            {
                case "admob":
                    source = com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob;
                    break;
                default:
                    source = com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAppLovinMAX;
                    break;
            }

            var adRevenue = new AdjustAdRevenue(source);
            adRevenue.setRevenue(value, "USD");
            adRevenue.setAdRevenueNetwork(network);
            adRevenue.setAdRevenuePlacement(placement);
            adRevenue.setAdRevenueUnit(unitId);

            Adjust.trackAdRevenue(adRevenue);
#endif
        }
    }
}