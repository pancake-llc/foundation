using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FilePathAttribute : ViewAttribute
    {
        public FilePathAttribute()
        {
            this.Title = "Choose file...";
            this.Directory = "";
            this.Extension = "";
        }

        #region [Parameters]

        /// <summary>
        /// File panel title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Start panel directory.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// File extension filter.
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Convert path to project relative.
        /// Only if selected file inside Assets folder.
        /// </summary>
        public bool RelativePath { get; set; }

        #endregion
    }
}