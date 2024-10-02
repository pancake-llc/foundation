namespace Pancake.Tracking
{
    public static class AppTracking
    {
        public static void TrackRevenue(double value, string network, string unitId, string format, string clientType)
        {
            AdjustTracking.TrackRevenue(value,
                network,
                unitId,
                format,
                clientType);
            FirebaseTracking.TrackRevenue(value,
                network,
                unitId,
                format,
                clientType);
        }

        public static void StartTrackingAdjust()
        {
#if PANCAKE_ADJUST
            _ = new UnityEngine.GameObject("Adjust", typeof(AdjustSdk.Adjust));
            AdjustSdk.Adjust.StartTracking(AdjustConfig.AppToken, AdjustConfig.Environment, AdjustConfig.LogLevel);
#endif
        }
    }
}