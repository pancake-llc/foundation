using System;
using System.Collections.Generic;

namespace PancakeEditor.Attribute
{
    internal static class AttributeUtilities
    {
        public static bool TryGet<T>(this IReadOnlyList<System.Attribute> attributes, out T it) where T : System.Attribute
        {
            foreach (var attribute in attributes)
            {
                if (attribute is T typeAttribute)
                {
                    it = typeAttribute;
                    return true;
                }
            }

            it = null;
            return false;
        }
    }
}