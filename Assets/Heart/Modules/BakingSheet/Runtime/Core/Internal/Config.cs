using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pancake.BakingSheet.Internal
{
    public static class Config
    {
        public const string COMMENT = "$";
        public const string INDEX_DELIMITER = ":";
        public const string SHEET_NAME_DELIMITER = ".";

        // TODO: in .net standard 2.1 this is not needed
        public static readonly string[] IndexDelimiterArray = {INDEX_DELIMITER};

        /// <summary>
        /// Split SheetName.SubName format.
        /// </summary>
        public static (string name, string subName) ParseSheetName(string name)
        {
            int idx = name.IndexOf(SHEET_NAME_DELIMITER, StringComparison.Ordinal);

            if (idx == -1) return (name, null);

            return (name.Substring(0, idx), name.Substring(idx + 1));
        }

        /// <summary>
        /// Iterate properties with both getter and setter.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetEligibleProperties(Type type)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

            while (type != null)
            {
                var properties = type.GetProperties(bindingFlags);

                foreach (var property in properties)
                {
                    if (property.IsDefined(typeof(NonSerializedAttribute))) continue;

                    if (property.GetMethod != null && property.SetMethod != null) yield return property;
                }

                type = type.BaseType;
            }
        }

        public static PropertyInfo GetRowArrayProperty(Type type)
        {
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SheetRowArray<,>))
                {
                    return type.GetProperty(nameof(ISheetRowArray.Arr));
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}