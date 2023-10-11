using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HelpBoxAttribute : DecoratorAttribute
    {
        public readonly string text;

        public HelpBoxAttribute(string text)
        {
            this.text = text;
            Style = MessageStyle.Info;
            Height = 20;
        }

        public MessageStyle Style { get; set; }

        public float Height { get; set; }
    }
}