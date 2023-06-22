using System;
using System.IO;
using Pancake.Apex;
using UnityEngine;

namespace Pancake.Notification
{
    [Serializable]
    [Searchable]
    [EditorIcon("scriptable_notification")]
    [CreateAssetMenu(fileName = "notification_channel_data.asset", menuName = "Pancake/Misc/Notification Channel")]
    public class ScriptableNotificationVariable : ScriptableObject
    {
        [Serializable]
        public class NotificationData
        {
            public string title;
            public string message;

            public NotificationData(string title, string message)
            {
                this.title = title;
                this.message = message;
            }
        }

        [SerializeField, Guid] private string identifier;
        public int minute;
        [SerializeField] private bool repeat;
        [SerializeField] internal bool bigPicture;

        [ShowIf(nameof(bigPicture)), Message("File bigpicture must be place in folder StreamingAsset and contains file extension ex .jpg", Height = 40), Label("  Name Picture")] [SerializeField]
        internal string namePicture;

        [Array, SerializeField] private NotificationData[] datas;


        public void Send()
        {
            if (!Application.isMobilePlatform) return;
            var data = datas.PickRandom();
            string pathPicture = Path.Combine(Application.persistentDataPath, namePicture);
            NotificationConsole.Send(identifier,
                data.title,
                data.message,
                bigPicture: bigPicture,
                namePicture: pathPicture);
        }

        public void Schedule()
        {
            if (!Application.isMobilePlatform) return;
            var data = datas.PickRandom();

            string pathPicture = Path.Combine(Application.persistentDataPath, namePicture);

            NotificationConsole.Schedule(identifier,
                data.title,
                data.message,
                TimeSpan.FromMinutes(minute),
                bigPicture: bigPicture,
                namePicture: pathPicture,
                repeat: repeat);
        }
    }
}