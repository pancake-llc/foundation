using System;

namespace Pancake.Apex
{
/// <summary>
    /// Base attribute for enum field type representation, contains miscellaneous options for BaseEnumView.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class BaseEnumAttribute : ViewAttribute
    {
        public BaseEnumAttribute()
        {
            HideValues = null;
            AllowPaths = false;
            ShowPathAlways = false;
            FlagSeparator = ", ";
            UnityOrder = true;
            ShortcutBits = false;
            UseAsFlags = false;
            ShortNaming = false;
        }

        #region [Parameters]
        /// <summary>
        /// Hide specific enum values. 
        /// <br>Return type of member must be IEnumerable<Enum></br>
        /// </summary>
        public string HideValues { get; set; }

        /// <summary>
        /// Allow using a path as a name?
        /// </summary>
        public bool AllowPaths { get; set; }

        /// <summary>
        /// If you have allowed the use of paths, 
        /// set true if you want the path to the value to be displayed in the selected value. 
        /// </summary>
        public bool ShowPathAlways { get; set; }

        /// <summary>
        /// Custom separator symbol for enum flags.
        /// </summary>
        public string FlagSeparator { get; set; }

        /// <summary>
        /// Unity representation of enum shortcuts position.
        /// </summary>
        public bool UnityOrder { get; set; }

        /// <summary>
        /// Automatically add default shortcut bits to select/deselect all flags.
        /// </summary>
        public bool ShortcutBits { get; set; }

        /// <summary>
        /// Use this enum field as flags.
        /// </summary>
        public bool UseAsFlags { get; set; }

        /// <summary>
        /// Use the abbreviated name for the selected flags.
        /// <br><b>Keep in mind this option work only with enums which have [System.Flags] attribute.</b></br>
        /// </summary>
        public bool ShortNaming { get; set; }
        #endregion
    }
}