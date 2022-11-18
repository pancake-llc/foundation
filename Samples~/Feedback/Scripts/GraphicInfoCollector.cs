using UnityEngine;

namespace Pancake.Feedback
{
    public class GraphicInfoCollector : FeedbackInfoCollector
    {
        public override void Collect()
        {
            // add section to report if it doesn't already exist
            if (!popupFeedback.report.HasSection(title)) popupFeedback.report.AddSection(title, sortOrder);

            // append graphics info to section
            popupFeedback.report["Additional Info"].AppendLine("Quality Level: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            popupFeedback.report["Additional Info"].AppendLine("Resolution: " + Screen.width + "x" + Screen.height);
            popupFeedback.report["Additional Info"].AppendLine("Full Screen: " + Screen.fullScreen);
        }
    }
}