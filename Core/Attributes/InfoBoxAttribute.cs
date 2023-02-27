using System;
using System.Diagnostics;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class InfoBoxAttribute : Attribute
    {
        public string Text { get; }
        public EMessageType MessageType { get; }
        public string VisibleIf { get; }

        public InfoBoxAttribute(string text, EMessageType messageType = EMessageType.Info, string visibleIf = null)
        {
            Text = text;
            MessageType = messageType;
            VisibleIf = visibleIf;
        }
    }
}