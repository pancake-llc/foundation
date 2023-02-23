using System;
using System.Collections.Generic;

namespace Pancake.AttributeDrawer
{
    internal static class AttributeUtilities
    {
        public static bool TryGet<T>(this IReadOnlyList<Attribute> attributes, out T it) where T : Attribute
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