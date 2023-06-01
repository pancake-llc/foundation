using System;
using Pancake.Apex;
using Pancake.Scriptable;
using Unity.Notifications.Android;
using UnityEngine;

namespace Pancake.Notification
{
    [Serializable]
    [EditorIcon("scriptable_notification")]
    [CreateAssetMenu(fileName = "noti_channel_data.asset", menuName = "Pancake/Misc/Notification Channel")]
    public class ScriptableNotificationVariable : ScriptableObject
    {
        public ETypeNotification type;
        public string channelId;
        public string groupId;
        public int minute;
        public bool autoSchedule;
        public bool bigPicture;
        [Array, ShowIf(nameof(bigPicture))] public string[] pathsPicture;
        [Array] public NotificationData[] datas;

        public void Init()
        {
#if UNITY_ANDROID
            AndroidInit();
#elif UNITY_IOS
            IosInit();
#endif
        }

        private void AndroidInit()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = channelId,
                Name = type == ETypeNotification.OnceTime ? "Cygnus" : "Nova",
                Description = type == ETypeNotification.OnceTime ? "Newsletter Announcement" : "Daily Newsletter",
                Importance = Importance.Default,
                Group = groupId,
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }

        private void IosInit() { }
    }
}