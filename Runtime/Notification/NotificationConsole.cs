using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Notification
{
    public enum TypeNoti
    {
        OnceTime = 0,
        Repeat = 1,
    }

    [Serializable]
    public struct NotificationData
    {
        public string title;
        public string message;

        public NotificationData(string title, string message)
        {
            this.title = title;
            this.message = message;
        }
    }

    [Serializable]
    public class NotificationStuctureData
    {
        public TypeNoti type;
        public string chanel;
        public int minute = 1;
        public bool autoSchedule;
        public NotificationData[] datas;
    }

    /// <summary>
    /// Manages the console on screen that displays information about notifications,
    /// and allows you to schedule more.
    /// </summary>
    [RequireComponent(typeof(GameNotificationsManager))]
    [HideMono]
    public class NotificationConsole : MonoBehaviour
    {
        // On iOS, this represents the notification's Category Identifier, and is optional
        // On Android, this represents the notification's channel, and is required (at least one).

        [SerializeField, Array] private NotificationStuctureData[] structures =
        {
            new NotificationStuctureData() {type = TypeNoti.Repeat, chanel = "channel_repeat", minute = 1440, autoSchedule = true},
            new NotificationStuctureData() {type = TypeNoti.OnceTime, chanel = "channel_event", minute = 120, autoSchedule = false},
            new NotificationStuctureData() {type = TypeNoti.OnceTime, chanel = "channel_noti", minute = 120, autoSchedule = false},
        };

        public UnityEvent onUpdateDeliveryTime;
        public GameNotificationsManager Manager => _manager != null ? _manager : _manager = GetComponent<GameNotificationsManager>();

        private GameNotificationChannel[] _channels;
        private GameNotificationsManager _manager;

        private IEnumerator Start()
        {
            // Set up channels (mostly for Android)
            // You need to have at least one of these

            _channels = new GameNotificationChannel[structures.Length];
            for (int i = 0; i < structures.Length; i++)
            {
                var chanelCache = structures[i];
                var chanelName = chanelCache.type == TypeNoti.OnceTime ? "Cygnus" : "Nova";
                var chanelDescription = chanelCache.type == TypeNoti.OnceTime ? "Newsletter Announcement" : "Daily Newsletter";
                _channels[i] = new GameNotificationChannel(structures[i].chanel, chanelName, chanelDescription);
            }

            yield return Manager.Initialize(_channels);
            Manager.CancelAllNotifications();
            Manager.DismissAllNotifications();
        }

        private void UpdateDeliveryTime()
        {
            var currentNow = DateTime.Now.ToLocalTime();

            for (int i = 0; i < structures.Length; i++)
            {
                if (!structures[i].autoSchedule) continue;

                var chanelCache = structures[i];
                var data = chanelCache.datas.PickRandom();

                if (chanelCache.type == TypeNoti.OnceTime)
                {
                    var deliveryTime = currentNow.AddMinutes(chanelCache.minute);
                    var resultTime = new DateTime(deliveryTime.Year,
                        deliveryTime.Month,
                        deliveryTime.Day,
                        deliveryTime.Hour,
                        deliveryTime.Minute,
                        deliveryTime.Second,
                        DateTimeKind.Local);
                    SendNotification(data.title,
                        data.message,
                        resultTime,
                        channelId: _channels[i].Id,
                        smallIcon: "icon_0",
                        largeIcon: "icon_1");
                }
                else
                {
                    var deliveryTime = currentNow.AddMinutes(chanelCache.minute);
                    var resultTime = new DateTime(deliveryTime.Year,
                        deliveryTime.Month,
                        deliveryTime.Day,
                        deliveryTime.Hour,
                        deliveryTime.Minute,
                        deliveryTime.Second,
                        DateTimeKind.Local);
                    var timeSpanResult = new TimeSpan(0, 0, chanelCache.minute, 0);
                    SendNotification(data.title,
                        data.message,
                        resultTime,
                        channelId: _channels[i].Id,
                        smallIcon: "icon_0",
                        largeIcon: "icon_1",
                        timeRepeatAt: timeSpanResult);
                }
            }
        }

        /// <summary>
        /// using for custom of not auto schedule notification
        /// </summary>
        /// <param name="id">id of chanel</param>
        /// <param name="customTimeSchedule"></param>
        public void UpdateDeliveryTimeBy(string id, int customTimeSchedule = -1)
        {
            int index = -1;

            for (int i = 0; i < structures.Length; i++)
            {
                if (structures[i].chanel.Equals(id))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Debug.LogWarning($"id: {id} not exist! please check again!");
                return;
            }

            UpdateDeliveryTimeBy(index, customTimeSchedule);
        }

        /// <summary>
        /// using for custom of not auto schedule notification
        /// </summary>
        /// <param name="index">index of id chanel</param>
        /// <param name="customTimeSchedule"></param>
        public void UpdateDeliveryTimeBy(int index, int customTimeSchedule = -1)
        {
            var currentNow = DateTime.Now.ToLocalTime();
            var structureData = structures[index];

            if (structureData.autoSchedule) return;

            var data = structureData.datas.PickRandom();

            if (structureData.type == TypeNoti.OnceTime)
            {
                var deliveryTime = currentNow.AddMinutes(customTimeSchedule == -1 ? structureData.minute : customTimeSchedule);
                var resultTime = new DateTime(deliveryTime.Year,
                    deliveryTime.Month,
                    deliveryTime.Day,
                    deliveryTime.Hour,
                    deliveryTime.Minute,
                    deliveryTime.Second,
                    DateTimeKind.Local);
                SendNotification(data.title,
                    data.message,
                    resultTime,
                    channelId: _channels[index].Id,
                    smallIcon: "icon_0",
                    largeIcon: "icon_1");
            }
            else
            {
                var deliveryTime = currentNow.AddMinutes(customTimeSchedule == -1 ? structureData.minute : customTimeSchedule);
                var resultTime = new DateTime(deliveryTime.Year,
                    deliveryTime.Month,
                    deliveryTime.Day,
                    deliveryTime.Hour,
                    deliveryTime.Minute,
                    deliveryTime.Second,
                    DateTimeKind.Local);
                var timeSpanResult = new TimeSpan(0, 0, customTimeSchedule == -1 ? structureData.minute : customTimeSchedule, 0);
                SendNotification(data.title,
                    data.message,
                    resultTime,
                    channelId: _channels[index].Id,
                    smallIcon: "icon_0",
                    largeIcon: "icon_1",
                    timeRepeatAt: timeSpanResult);
            }
        }

        /// <summary>
        /// using for custom of not auto schedule notification
        /// </summary>
        /// <param name="index">index of id chanel</param>
        /// <param name="indexData"></param>
        /// <param name="customTimeSchedule"></param>
        public void UpdateDeliveryTimeByIncremental(int index, int indexData, int customTimeSchedule = -1)
        {
            var currentNow = DateTime.Now.ToLocalTime();
            var structureData = structures[index];

            if (structureData.autoSchedule) return;
            if (structureData.type == TypeNoti.OnceTime)
            {
                var deliveryTime = currentNow.AddMinutes(customTimeSchedule == -1 ? structureData.minute : customTimeSchedule);
                var resultTime = new DateTime(deliveryTime.Year,
                    deliveryTime.Month,
                    deliveryTime.Day,
                    deliveryTime.Hour,
                    deliveryTime.Minute,
                    deliveryTime.Second,
                    DateTimeKind.Local);
                SendNotification(structureData.datas[indexData].title,
                    structureData.datas[indexData].message,
                    resultTime,
                    channelId: _channels[index].Id,
                    smallIcon: "icon_0",
                    largeIcon: "icon_1");
            }
            else
            {
                var deliveryTime = currentNow.AddMinutes(customTimeSchedule == -1 ? structureData.minute : customTimeSchedule);
                var resultTime = new DateTime(deliveryTime.Year,
                    deliveryTime.Month,
                    deliveryTime.Day,
                    deliveryTime.Hour,
                    deliveryTime.Minute,
                    deliveryTime.Second,
                    DateTimeKind.Local);
                var timeSpanResult = new TimeSpan(0, 0, customTimeSchedule == -1 ? structureData.minute : customTimeSchedule, 0);
                SendNotification(structureData.datas[indexData].title,
                    structureData.datas[indexData].message,
                    resultTime,
                    channelId: _channels[index].Id,
                    smallIcon: "icon_0",
                    largeIcon: "icon_1",
                    timeRepeatAt: timeSpanResult);
            }
        }

        /// <summary>
        /// using for custom of not auto schedule notification
        /// </summary>
        /// <param name="id">id of chanel</param>
        /// <param name="indexData"></param>
        /// <param name="customTimeSchedule"></param>
        public void UpdateDeliveryTimeByIncremental(string id, int indexData, int customTimeSchedule = -1)
        {
            int index = -1;

            for (int i = 0; i < structures.Length; i++)
            {
                if (structures[i].chanel.Equals(id))
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                Debug.LogWarning($"id: {id} not exist! please check again!");
                return;
            }

            UpdateDeliveryTimeByIncremental(index, indexData, customTimeSchedule);
        }

        private void OnEnable()
        {
            if (Manager != null)
            {
                Manager.LocalNotificationDelivered += OnDelivered;
                Manager.LocalNotificationExpired += OnExpired;
            }
        }

        private void OnDisable()
        {
            if (Manager != null)
            {
                Manager.LocalNotificationDelivered -= OnDelivered;
                Manager.LocalNotificationExpired -= OnExpired;
            }
        }

        /// <summary>
        /// Queue a notification with the given parameters.
        /// </summary>
        /// <param name="title">The title for the notification.</param>
        /// <param name="body">The body text for the notification.</param>
        /// <param name="deliveryTime">The time to deliver the notification.</param>
        /// <param name="badgeNumber">The optional badge number to display on the application icon.</param>
        /// <param name="reschedule">
        /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
        /// </param>
        /// <param name="channelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
        /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
        /// <param name="smallIcon">Notification small icon.</param>
        /// <param name="largeIcon">Notification large icon.</param>
        /// <param name="timeRepeatAt">time repeat fire notification</param>
        public void SendNotification(
            string title,
            string body,
            DateTime deliveryTime,
            int? badgeNumber = null,
            bool reschedule = false,
            string channelId = null,
            string smallIcon = null,
            string largeIcon = null,
            TimeSpan? timeRepeatAt = null)
        {
            IGameNotification notification = Manager.CreateNotification();

            if (notification == null) return;

            notification.Title = title;
            notification.Body = body;
            notification.Group = channelId;
#if UNITY_ANDROID
            if (timeRepeatAt != null)
            {
                if (notification is AndroidGameNotification notiAndroid)
                {
                    notiAndroid.RepeatInterval = timeRepeatAt;
                }
            }

#elif UNITY_IOS
            if (timeRepeatAt != null)
            {
                if (notification is iOSGameNotification notificationIOS)
                {
                    notificationIOS.TimeIntervalTriggerFlag = true;
                    notificationIOS.InternalNotification.Trigger = new iOSNotificationTimeIntervalTrigger {Repeats = true, TimeInterval = timeRepeatAt.Value,};
                }
            }
#endif
            notification.DeliveryTime = deliveryTime;
            notification.SmallIcon = smallIcon;
            notification.LargeIcon = largeIcon;

            if (badgeNumber != null) notification.BadgeNumber = badgeNumber;

            PendingNotification notificationToDisplay = Manager.ScheduleNotification(notification);
            notificationToDisplay.Reschedule = reschedule;
        }

        /// <summary>
        /// Cancel a given pending notification
        /// </summary>
        public void CancelPendingNotificationItem(PendingNotification itemToCancel) { Manager.CancelNotification(itemToCancel.Notification.Id.Value); }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                if (Manager.Initialized)
                {
                    Manager.CancelAllNotifications();
                    Manager.DismissAllNotifications();
                }

                UpdatePendingNotificationsNextFrame().RunCoroutine();
            }
            else
            {
                JobScheduleNotification();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                JobScheduleNotification();
            }
            else
            {
                if (Manager.Initialized)
                {
                    Manager.CancelAllNotifications();
                    Manager.DismissAllNotifications();
                }
            }
        }

        private void JobScheduleNotification()
        {
            if (!Manager.Initialized) return;

            Manager.CancelAllNotifications();
            Manager.DismissAllNotifications();
            UpdateDeliveryTime();
            onUpdateDeliveryTime?.Invoke();
        }

        private void OnDelivered(PendingNotification deliveredNotification)
        {
            // Schedule this to run on the next frame (can't create UI elements from a Java callback)
            ShowDeliveryNotificationCoroutine(deliveredNotification.Notification).RunCoroutine();
        }

        private void OnExpired(PendingNotification obj) { }

        private IEnumerator<float> ShowDeliveryNotificationCoroutine(IGameNotification deliveredNotification) { yield return Timing.WaitForOneFrame; }

        private IEnumerator<float> UpdatePendingNotificationsNextFrame() { yield return Timing.WaitForOneFrame; }
    }
}