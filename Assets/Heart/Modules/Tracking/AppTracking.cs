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
#if !UNITY_EDITOR && PANCAKE_ADJUST
            _ = new UnityEngine.GameObject("Adjust", typeof(AdjustSdk.Adjust));
            var adjustConfig = new AdjustSdk.AdjustConfig(AdjustConfig.AppToken, AdjustConfig.Environment, AdjustConfig.LogLevel == AdjustSdk.AdjustLogLevel.Suppress)
            {
                LogLevel = AdjustConfig.LogLevel, IsAdServicesEnabled = true, IsIdfaReadingEnabled = true
            };
            AdjustSdk.Adjust.InitSdk(adjustConfig);
#endif
        }
    }
}