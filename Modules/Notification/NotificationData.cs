using System;

namespace Pancake.Notification
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
}