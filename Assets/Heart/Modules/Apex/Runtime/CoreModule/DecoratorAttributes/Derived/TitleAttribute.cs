using UnityEngine;

namespace Pancake.Apex
{
    public sealed class TitleAttribute : DecoratorAttribute
    {
        public readonly string text;

        private TitleAttribute()
        {
            DrawSeparator = false;
            FontSize = 12;
            VerticalSpace = 8;
            Style = string.Empty;
            Anchor = TextAnchor.MiddleLeft;
        }

        public TitleAttribute(string text)
            : this()
        {
            this.text = text;
        }

        #region [Optional]

        /// <summary>
        /// Draw separator?
        /// </summary>
        public bool DrawSeparator { get; set; }

        /// <summary>
        /// Label font size.
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// Additional vertical space between fields.
        /// </summary>
        public float VerticalSpace { get; set; }

        /// <summary>
        /// Style of label.
        /// </summary>
        public string Style { get; set; }

        /// <summary>
        /// Label anchor.
        /// </summary>
        public TextAnchor Anchor { get; set; }

        #endregion
    }
}