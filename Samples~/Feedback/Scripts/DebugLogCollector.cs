using System;
using System.Text;
using UnityEngine;

namespace Pancake.Feedback
{
    public class DebugLogCollector : FeedbackInfoCollector
    {
        private StringBuilder _log;

        protected override void Awake()
        {
            base.Awake();
            _log = new StringBuilder();
            Application.logMessageReceived += HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType logType)
        {
            // enqueue the message
            if (logType != LogType.Exception)
            {
                _log.AppendFormat("{0}: {1}", logType.ToString(), logString);
            }
            else
            {
                // don't add log type to exceptions, as it's already in the string
                _log.AppendLine(logString);
            }

            // enqueue the stack trace
            _log.AppendLine(stackTrace);
        }


        public override void Collect()
        {
            // attach log
            byte[] bytes = Encoding.ASCII.GetBytes(_log.ToString());
            popupFeedback.report.AttachFile("log.txt", bytes);
        }
    }
}