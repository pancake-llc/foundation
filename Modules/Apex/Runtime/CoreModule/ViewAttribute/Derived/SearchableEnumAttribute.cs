using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SearchableEnumAttribute : BaseEnumAttribute
    {
        public SearchableEnumAttribute() : base()
        {
            Height = 200.0f;
            ToggleIcons = false;
        }

        #region [Parameters]
        /// <summary>
        /// Search menu max height.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Use the built-in icons to mark the selected flags, when using flagged enum, keep in mind that icons from [SearchContent] attributes will be ignored.
        /// </summary>
        public bool ToggleIcons { get; set; }
        #endregion
    }
}