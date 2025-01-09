using UnityEngine;

namespace Pancake
{
    public static class DeviceInfo
    {
        private static string gpsAdIdInternal;
        public static string DeviceId => SystemInfo.deviceUniqueIdentifier;

        // ReSharper disable once InconsistentNaming
        public static string gpsAdId
        {
            get
            {
                if (string.IsNullOrEmpty(gpsAdIdInternal)) gpsAdIdInternal = GetGpsAdId();
                return gpsAdIdInternal;
            }
        }

        public static string idfa => UnityEngine.iOS.Device.advertisingIdentifier;

        private static string GetGpsAdId()
        {
            var id = string.Empty;

            if (Application.platform == RuntimePlatform.Android)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    using (var advertisingIdClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient"))
                    {
                        var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

                        var adInfo = advertisingIdClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", context);
                        id = adInfo.Call<string>("getId");
                    }
                }
            }

            return id;
        }
    }
}