using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Pancake.Feedback
{
    public class DebugLogCollector : FeedbackInfoCollector
    {
        private readonly List<string> _dayDirectories = new List<string>();

        protected override void Awake()
        {
            base.Awake();

            popupFeedback.SubmitFeedbackCompletedAction -= OnSubmitFeedbackCompleted;
            popupFeedback.SubmitFeedbackCompletedAction += OnSubmitFeedbackCompleted;
        }

        private void OnSubmitFeedbackCompleted()
        {
            foreach (string directory in _dayDirectories)
            {
                if (Directory.Exists(directory)) directory.DeleteDirectory();
            }
            
            _dayDirectories.Clear();
        }

        public override void Collect()
        {
            //collect old log
            _dayDirectories.Clear();
            var oldLog = new StringBuilder();
            var path = $"{Application.persistentDataPath}/userlogs";
            if (path.DirectoryExists())
            {
                var info = new DirectoryInfo(path);
                
                foreach (var dayDirectoryInfo in info.GetDirectories())
                {
                    var dayPath = $"{path}/{dayDirectoryInfo.Name}";
                    var infoLog = new DirectoryInfo(dayPath);
                    _dayDirectories.Add(dayPath);
                    foreach (var timeDirectoryInfo in infoLog.GetDirectories())
                    {
                        var str = File.ReadAllText($"{dayPath}/{timeDirectoryInfo.Name}/logs.txt");
                        oldLog.AppendLine($"{dayDirectoryInfo.Name} - {timeDirectoryInfo.Name}");
                        oldLog.AppendLine(str);
                    }
                }
            }

            // attach log
            oldLog.AppendLine(RuntimeManager.sessionLogError.ToString());
            byte[] bytes = Encoding.ASCII.GetBytes(oldLog.ToString());
            popupFeedback.report.AttachFile("log.txt", bytes);
        }
    }
}