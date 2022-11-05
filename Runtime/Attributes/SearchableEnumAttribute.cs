namespace Pancake
{
    public sealed class SearchableEnumAttribute : ViewAttribute
    {
        public SearchableEnumAttribute()
        {
            Height = 200.0f;
            OnSelectCallback = string.Empty;
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

        public string OnSelectCallback { get; set; }

        #endregion
    }
}