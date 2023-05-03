using System;
using System.Collections.Generic;
using System.Linq;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for Apex.
    /// </summary>
    public static class ApexUtility
    {
        internal static readonly HashSet<ExceptType> exceptTypes = new HashSet<ExceptType>();

        /// <summary>
        /// editor is enabled.
        /// </summary>
        public static bool Enabled { get; internal set; } = true;

        public static bool IsExceptType(Type type)
        {
            Func<ExceptType, bool> predicate = (exceptType) =>
            {
                Type _type = type;
                string name = exceptType.GetName();
                bool subClasses = exceptType.SubClasses();
                do
                {
                    if (_type.Name == name)
                    {
                        return true;
                    }

                    _type = _type.BaseType;
                } while (_type != null && subClasses);

                return false;
            };

            if (exceptTypes.Count > 0)
            {
                return exceptTypes.Any(predicate);
            }

            return false;
        }
    }
}