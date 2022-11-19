using System.Text;

namespace Pancake.Feedback
{
    public class DebugLogCollector : FeedbackInfoCollector
    {
        public override void Collect()
        {
            // attach log
            byte[] bytes = Encoding.ASCII.GetBytes(RuntimeManager.sessionLogError.ToString());
            popupFeedback.report.AttachFile("log.txt", bytes);
        }
    }
}