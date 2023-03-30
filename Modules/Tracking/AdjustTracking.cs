#if PANCAKE_ADJUST
using com.adjust.sdk;
#endif
using Pancake.Monetization;

namespace Pancake.Tracking
{
    public static class AdjustTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
#if PANCAKE_ADJUST
            var source = "";
            double revenue = value;
            switch (adNetwork)
            {
                case EAdNetwork.Admob:
                    revenue = value / 1000000f;
                    source = com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob;
                    break;
                case EAdNetwork.Applovin:
                    source = com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAppLovinMAX;
                    break;
            }

            var adRevenue = new AdjustAdRevenue(source);
            adRevenue.setRevenue(revenue, "USD");
            adRevenue.setAdRevenueNetwork(network);
            adRevenue.setAdRevenuePlacement(placement);
            adRevenue.setAdRevenueUnit(unitId);

            Adjust.trackAdRevenue(adRevenue);
#endif
        }
    }
}