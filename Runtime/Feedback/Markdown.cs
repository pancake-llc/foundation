namespace Pancake.Feedback
{
    internal static class Markdown
    {
        // punctuation characters
        // headers
        private const string _H1 = "# ";
        private const string _H2 = "## ";
        private const string _H3 = "### ";
        private const string _H4 = "#### ";
        private const string _H5 = "##### ";
        private const string _H6 = "###### ";

        // emphasis
        private const string _EM = "*";
        private const string _STRONG = "**";
        private const string _STRIKE = "~~";

        // lists
        private const string _UL = "- ";
        private const string _OL = ". ";

        // links
        private const string _LINK_INLINE_PRE = "[";
        private const string _LINK_INLINE_MID = "](";

        private const string _LINK_INLINE_END = ")";
        // ef-TODO: support for reference links

        // images
        private const string _IMG_INLINE_PRE = "![";
        private const string _IMG_INLINE_MID = "](";
        private const string _IMG_INLINE_END = ")";

        // code blocks
        private const string _CODE_INLINE = "`";
        private const string _CODE_BLOCK = "```";

        // misc
        private const string _QUOTE = "> ";
        private const string _HR = "---";
        private const string _ENDL = "\n";
        private const string _LB = "\n\n";

        /// <summary>
        /// Creates a horizontal rule or line
        /// </summary>
        public const string HR = _HR;

        /// <summary>
        /// Creates a new paragraph
        /// </summary>
        public const string LINE_BREAK = _LB;

        // ef-TODO: support for tables

        /// <summary>
        /// Creates a header from the specified text, with the specified level
        /// </summary>
        /// <param name="text">The header text</param>
        /// <param name="level">The header level</param>
        public static string Header(string text, HeaderLevel level = HeaderLevel.H1)
        {
            switch (level)
            {
                case HeaderLevel.H1: return _H1 + text;
                case HeaderLevel.H2: return _H2 + text;
                case HeaderLevel.H3: return _H3 + text;
                case HeaderLevel.H4: return _H4 + text;
                case HeaderLevel.H5: return _H5 + text;
                case HeaderLevel.H6: return _H6 + text;
            }

            throw new System.ArgumentException("The header level value '" + level + "' is invalid.");
        }

        /// <summary>
        /// Creates a first-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H1(string text) => Header(text, HeaderLevel.H1);

        /// <summary>
        /// Creates a second-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H2(string text) => Header(text, HeaderLevel.H2);

        /// <summary>
        /// Creates a third-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H3(string text) => Header(text, HeaderLevel.H3);

        /// <summary>
        /// Creates a fourth-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H4(string text) => Header(text, HeaderLevel.H4);

        /// <summary>
        /// Creates a fifth-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H5(string text) => Header(text, HeaderLevel.H5);

        /// <summary>
        /// Creates a sixth-level header from the specified text
        /// </summary>
        /// <param name="text">The header text</param>
        public static string H6(string text) => Header(text, HeaderLevel.H6);

        /// <summary>
        /// Formats the text with emphasis/italics
        /// </summary>
        /// <param name="text">The text to be emphasized</param>
        public static string Em(string text) => _EM + text + _EM;

        /// <summary>
        /// Emboldens the text
        /// </summary>
        /// <param name="text">The text to be emboldened</param>
        public static string Strong(string text) => _STRONG + text + _STRONG;

        /// <summary>
        /// Strikes through the text
        /// </summary>
        /// <param name="text">The text</param>
        public static string Strike(string text) => _STRIKE + text + _STRIKE;

        /// <summary>
        /// Creates an unordered (bulleted) list from an array of items
        /// </summary>
        /// <param name="items">The items of the list</param>
        public static string UnorderedList(string[] items)
        {
            string list = string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                list += _UL + items[i] + _ENDL;
            }

            return list;
        }

        /// <summary>
        /// Creates an ordered (numbered) list from an array of items
        /// </summary>
        /// <param name="items">The items of the list</param>
        public static string OrderedList(string[] items)
        {
            string list = string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                list += (i + 1) + _OL + items[i] + _ENDL;
            }

            return list;
        }

        /// <summary>
        /// Creates an inline link
        /// </summary>
        /// <param name="text">The link text</param>
        /// <param name="url">The link url</param>
        public static string Hyperlink(string text, string url) => _LINK_INLINE_PRE + text + _LINK_INLINE_MID + url + _LINK_INLINE_END;

        /// <summary>
        /// Creates an inline image
        /// </summary>
        /// <param name="url">The url of the image</param>
        /// <param name="alt">The alt-text for the image</param>
        public static string Image(string url, string alt = "") => _IMG_INLINE_PRE + alt + _IMG_INLINE_MID + url + _IMG_INLINE_END;

        /// <summary>
        /// Creates an inline span of preformatted text
        /// </summary>
        /// <param name="text">The text</param>
        public static string Code(string text) => _CODE_INLINE + text + _CODE_INLINE;

        /// <summary>
        /// Creates a block of preformatted text
        /// </summary>
        /// <param name="text">The text</param>
        /// <param name="language">The language for syntax highlighting (where supported)</param>
        public static string CodeBlock(string text, string language = "") => _CODE_BLOCK + language + _ENDL + text + _ENDL + _CODE_BLOCK;

        /// <summary>
        /// Creates a block of quoted text
        /// </summary>
        /// <param name="text">The text</param>
        public static string Blockquote(string text) => _QUOTE + text;

        public enum HeaderLevel
        {
            H1,
            H2,
            H3,
            H4,
            H5,
            H6
        }
    }
}