using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SearchContent : ApexAttribute
    {
        public readonly string name;

        public SearchContent(string name)
        {
            this.name = name;
            Tooltip = string.Empty;
            Image = string.Empty;
            Hidden = false;
        }

        #region [Optional Parameters]

        /// <summary>
        /// Image of search item.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Tooltip of search item.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Hide this search item.
        /// </summary>
        public bool Hidden { get; set; }

        #endregion
    }
}