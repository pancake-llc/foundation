using UnityEngine;

namespace Pancake
{
    public sealed class TitleAttribute : DecoratorAttribute
    {
        public readonly string text;

        private TitleAttribute()
        {
            Style = string.Empty;
            Anchor = TextAnchor.MiddleLeft;
            FontSize = 11;
            DrawSeparator = true;
            SeparatorHeight = 0.5f;
            SeparatorColor = string.Empty;
        }

        public TitleAttribute(string text)
            : this()
        {
            this.text = text;
        }

        /// <summary>
        /// Style of label.
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Label anchor.
        /// </summary>
        public TextAnchor Anchor { get; set; }

        /// <summary>
        /// Label font size.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Draw separator?
        /// </summary>
        public bool DrawSeparator { get; set; }

        /// <summary>
        /// Separator height.
        /// </summary>
        public float SeparatorHeight { get; set; }

        /// <summary>
        /// Draw separator?
        /// </summary>
        public string SeparatorColor { get; set; }
    }
}