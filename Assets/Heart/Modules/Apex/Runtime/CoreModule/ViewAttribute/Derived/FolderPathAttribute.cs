using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FolderPathAttribute : ViewAttribute
    {
        public FolderPathAttribute()
        {
            this.Title = "Choose folder...";
            this.Folder = "";
            this.DefaultName = "";
        }

        #region [Parameters]

        /// <summary>
        /// Folder panel title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Start panel folder.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Default folder name.
        /// </summary>
        public string DefaultName { get; set; }

        /// <summary>
        /// Convert path to project relative.
        /// Only if selected folder inside Assets folder.
        /// </summary>
        public bool RelativePath { get; set; }

        #endregion
    }
}