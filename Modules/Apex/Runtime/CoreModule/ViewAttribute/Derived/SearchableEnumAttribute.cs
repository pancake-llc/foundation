using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SearchableEnumAttribute : ViewAttribute
    {
        public SearchableEnumAttribute()
        {
            Height = 200.0f;
            OnSelect = string.Empty;
            HideValues = null;
        }

        #region [Parameters]

        /// <summary>
        /// Search menu max height.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Hide specific enum values.
        /// </summary>
        public string[] HideValues { get; set; }

        /// <summary>
        /// Called when item has been selected.
        /// </summary>
        public string OnSelect { get; set; }

        #endregion
    }
}