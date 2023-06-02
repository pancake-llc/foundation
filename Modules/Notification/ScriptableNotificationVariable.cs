using System;
using System.Collections;
using System.IO;
using Pancake.Apex;
using Pancake.Scriptable;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace Pancake.Notification
{
    [Serializable]
    [EditorIcon("scriptable_notification")]
    [CreateAssetMenu(fileName = "notification_channel_data.asset", menuName = "Pancake/Misc/Notification Channel")]
    public class ScriptableNotificationVariable : ScriptableObject
    {
        public ETypeNotification type;
        public string channelId;
        public string groupId;
        public bool autoSchedule;
        [ShowIf(nameof(autoSchedule)), Label("  Minute")] public int minute;
        public bool bigPicture;

        [ShowIf(nameof(bigPicture)), Message("File bigpicture muest be place in folder StreamingAsset"), Label("  Name Picture")]
        public string namePicture;

        [Array] public NotificationData[] datas;

        private string _path;

        internal void Init()
        {
#if UNITY_ANDROID
            AndroidInit();
#endif
        }

        private void AndroidInit()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = channelId,
                Name = type == ETypeNotification.Repeat ? "Cygnus" : "Nova",
                Description = type == ETypeNotification.Repeat ? "Daily Newsletter" : "Newsletter Announcement",
                Importance = Importance.Default,
                Group = groupId,
                EnableLights = true,
                EnableVibration = true,
                CanShowBadge = true,
                LockScreenVisibility = LockScreenVisibility.Public
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            if (bigPicture)
            {
                _path = Path.Combine(Application.persistentDataPath, namePicture);
                App.RunCoroutine(PrepareImage(Application.persistentDataPath, namePicture));
            }
        }

        public void Schedule() { }

        public void Cancel() { }

        internal void AutoUpdateDeliveryTime()
        {
            if (!autoSchedule) return;

            var now = DateTime.Now.ToLocalTime();
            var data = datas.PickRandom();
            var time = now.AddMinutes(minute);
            var resultTime = new DateTime(time.Year,
                time.Month,
                time.Day,
                time.Hour,
                time.Minute,
                time.Second,
                DateTimeKind.Local);

            if (type == ETypeNotification.OnceTime)
            {
                SendNotification(data.title,
                    data.message,
                    resultTime,
                    channelId,
                    "icon_0",
                    "icon_1",
                    _path);
            }
            else
            {
                var timeSpan = new TimeSpan(0, 0, minute, 0);
                SendNotification(data.title,
                    data.message,
                    resultTime,
                    channelId,
                    "icon_0",
                    "icon_1",
                    _path,
                    timeSpan);
            }
        }

        public void TrySendManual(int customMinute, int specifyDataIndex = -1)
        {
            if (autoSchedule) return;
            var now = DateTime.Now.ToLocalTime();
            var data = specifyDataIndex == -1 ? datas.PickRandom() : datas[specifyDataIndex];
            var time = now.AddMinutes(customMinute);
            var resultTime = new DateTime(time.Year,
                time.Month,
                time.Day,
                time.Hour,
                time.Minute,
                time.Second,
                DateTimeKind.Local);

            SendNotification(data.title,
                data.message,
                resultTime,
                channelId,
                "icon_0",
                "icon_1",
                _path);
        }

        private IEnumerator PrepareImage(string des, string fileName)
        {
            string path = Path.Combine(des, fileName);
            if (File.Exists(path)) yield break;

            using (var uwr = new UnityWebRequest(Path.Combine(Application.streamingAssetsPath, fileName), UnityWebRequest.kHttpVerbGET))
            {
                uwr.downloadHandler = new DownloadHandlerFile(path);
                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success) Debug.LogError(uwr.error);
            }
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>
        /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
        /// the channel must be registered in <see cref="Initialize"/>.</param>
        /// <param name="smallIcon">Notification small icon.</param>
        /// <param name="largeIcon">Notification large icon.</param>
        /// <param name="bigPicturePath"></param>
        /// <param name="timeRepeatAt">time repeat fire notification</param>
        private void SendNotification(
            string title,
            string body,
            DateTime deliveryTime,
            string channelId,
            string smallIcon,
            string largeIcon,
            string bigPicturePath = null,
            TimeSpan? timeRepeatAt = null)
        {
#if UNITY_ANDROID
            var notification = new AndroidNotification {Title = title, Text = body, FireTime = deliveryTime, RepeatInterval = timeRepeatAt};
            if (string.IsNullOrEmpty(bigPicturePath))
            {
                notification.BigPicture = new BigPictureStyle {Picture = bigPicturePath};
            }

            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;
            AndroidNotificationCenter.SendNotification(notification, channelId);
#elif UNITY_IOS
            var notification = new iOSNotification {Title = title, Body = body, ShowInForeground = false, ForegroundPresentationOption =
                PresentationOption.Alert | PresentationOption.Sound | PresentationOption.Badge
            };
            iOSNotificationTrigger trigger;
            if (timeRepeatAt != null)
            {
                trigger = new iOSNotificationTimeIntervalTrigger() {TimeInterval = timeRepeatAt.Value, Repeats = true};
            }
            else
            {
                trigger = new iOSNotificationCalendarTrigger()
                {
                    Year = deliveryTime.Year,
                    Month = deliveryTime.Month,
                    Day = deliveryTime.Day,
                    Hour = deliveryTime.Hour,
                    Minute = deliveryTime.Minute,
                    Second = deliveryTime.Second
                };
            }

            notification.Trigger = trigger;
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }
    }
}