using System;
namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HelpBoxAttribute : DecoratorAttribute
    {
        public readonly string text;

        public HelpBoxAttribute(string text)
        {
            this.text = text;
            Style = MessageStyle.Info;
        }

        public MessageStyle Style { get; set; }

        public float Height { get; set; }
    }
}
