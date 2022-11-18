using UnityEngine.Device;

namespace Pancake.Feedback
{
    public class SystemInfoCollector : FeedbackInfoCollector
    {
        public override void Collect()
        {
            // add section to report if it doesn't already exist
            if (!popupFeedback.report.HasSection(title)) popupFeedback.report.AddSection(title, sortOrder);

            // append system info to section
            popupFeedback.report["System Info"].AppendLine("OS: " + SystemInfo.operatingSystem);
            popupFeedback.report["System Info"].AppendLine("Processor: " + SystemInfo.processorType);
            popupFeedback.report["System Info"].AppendLine("Memory: " + SystemInfo.systemMemorySize);
            popupFeedback.report["System Info"].AppendLine("Graphics API: " + SystemInfo.graphicsDeviceType);
            popupFeedback.report["System Info"].AppendLine("Graphics Processor: " + SystemInfo.graphicsDeviceName);
            popupFeedback.report["System Info"].AppendLine("Graphics Memory: " + SystemInfo.graphicsMemorySize);
            popupFeedback.report["System Info"].AppendLine("Graphics Vendor: " + SystemInfo.graphicsDeviceVendor);
        }
    }
}