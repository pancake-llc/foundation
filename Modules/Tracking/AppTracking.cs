using Pancake.Monetization;

namespace Pancake.Tracking
{
    public static class AppTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string placement, EAdNetwork adNetwork)
        {
            AdjustTracking.TrackRevenue(value,
                network,
                unitId,
                placement,
                adNetwork);
            FirebaseTracking.TrackRevenue(value,
                network,
                unitId,
                placement,
                adNetwork);
        }

        public static void StartTrackingAdjust()
        {
#if PANCAKE_ADJUST
            var _ = new UnityEngine.GameObject("Adjust", typeof(com.adjust.sdk.Adjust));
            com.adjust.sdk.Adjust.StartTracking(AdjustConfig.AppToken, AdjustConfig.Environment, AdjustConfig.LogLevel);
#endif
        }
    }
}