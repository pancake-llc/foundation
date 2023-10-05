using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DropdownReferenceAttribute : ViewAttribute
    {
        #region [Optional]
        /// <summary>
        /// Use default Unity popup style of dropdown controls.
        /// </summary>
        public bool PopupStyle { get; set; }
        #endregion
    }
}