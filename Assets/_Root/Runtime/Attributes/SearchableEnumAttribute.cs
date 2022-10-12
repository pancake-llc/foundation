namespace Pancake
{
    public sealed class SearchableEnumAttribute : ViewAttribute
    {
        public SearchableEnumAttribute()
        {
            Sort = false;
            Height = 200.0f;
            DisableValues = null;
            HideValues = null;
        }

        #region [Parameters]

        /// <summary>
        /// Sort enum values.
        /// </summary>
        public bool Sort { get; set; }

        /// <summary>
        /// Search menu max height.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Disabled enum values.
        /// </summary>
        public string[] DisableValues { get; set; }

        /// <summary>
        /// Hide specific enum values.
        /// </summary>
        public string[] HideValues { get; set; }

        #endregion
    }
}