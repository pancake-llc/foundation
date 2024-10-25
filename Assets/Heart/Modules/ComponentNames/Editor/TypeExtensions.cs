using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Sisus.ComponentNames.Editor
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly Dictionary<char, Dictionary<Type, string>> toStringCache = new Dictionary<char, Dictionary<Type, string>>(3)
        {
            {
                '\0', new Dictionary<Type, string>(4096)
                {
                    {typeof(int), "Int"},
                    {typeof(uint), "UInt"},
                    {typeof(float), "Float"},
                    {typeof(double), "Double"},
                    {typeof(bool), "Boolean"},
                    {typeof(string), "String"},
                    {typeof(short), "Short"},
                    {typeof(ushort), "UShort"},
                    {typeof(byte), "Byte"},
                    {typeof(sbyte), "SByte"},
                    {typeof(long), "Long"},
                    {typeof(ulong), "ULong"},
                    {typeof(object), "object"},
                    {typeof(decimal), "Decimal"}
                }
            },
            {
                '/', new Dictionary<Type, string>(4096)
                {
                    {typeof(int), "Int"},
                    {typeof(uint), "UInteger"},
                    {typeof(float), "Float"},
                    {typeof(double), "Double"},
                    {typeof(bool), "Boolean"},
                    {typeof(string), "String"},
                    {typeof(short), "Short"},
                    {typeof(ushort), "UShort"},
                    {typeof(byte), "Byte"},
                    {typeof(sbyte), "SByte"},
                    {typeof(long), "Long"},
                    {typeof(ulong), "ULong"},
                    {typeof(object), "System/Object"},
                    {typeof(decimal), "Decimal"}
                }
            },
            {
                '.', new Dictionary<Type, string>(4096)
                {
                    {typeof(int), "Int"},
                    {typeof(uint), "UInt"},
                    {typeof(float), "Float"},
                    {typeof(double), "Double"},
                    {typeof(bool), "Boolean"},
                    {typeof(string), "String"},
                    {typeof(short), "Short"},
                    {typeof(ushort), "UShort"},
                    {typeof(byte), "Byte"},
                    {typeof(sbyte), "SByte"},
                    {typeof(long), "Long"},
                    {typeof(ulong), "ULong"},
                    {typeof(object), "System.Object"},
                    {typeof(decimal), "Decimal"}
                }
            }
        };

        [return: NotNull]
        public static string ToHumanReadableString([AllowNull] this Type type, char namespaceDelimiter = '\0')
        {
            return type is null ? "Null" : ToString(type, namespaceDelimiter, toStringCache);
        }

        [return: NotNull]
        internal static string ToString([DisallowNull] Type type, char namespaceDelimiter, Dictionary<char, Dictionary<Type, string>> cache)
        {
            if (cache[namespaceDelimiter].TryGetValue(type, out string cached))
            {
                return cached;
            }

            var builder = new StringBuilder();
            ToString(type, builder, namespaceDelimiter, cache);
            string result = builder.ToString();
            cache[namespaceDelimiter][type] = result;
            return result;
        }

        [return: NotNull]
        private static void ToString(
            [DisallowNull] Type type,
            [DisallowNull] StringBuilder builder,
            char namespaceDelimiter,
            Dictionary<char, Dictionary<Type, string>> cache,
            Type[] genericTypeArguments = null)
        {
            // E.g. List<T> generic parameter is T, in which case we just want to append "T".
            if (type.IsGenericParameter)
            {
                builder.Append(type.Name);
                return;
            }

            if (type.IsArray)
            {
                builder.Append(ToString(type.GetElementType(), namespaceDelimiter, cache));
                int rank = type.GetArrayRank();
                switch (rank)
                {
                    case 1:
                        builder.Append("[]");
                        break;
                    case 2:
                        builder.Append("[,]");
                        break;
                    case 3:
                        builder.Append("[,,]");
                        break;
                    default:
                        builder.Append('[');
                        for (int n = 1; n < rank; n++)
                        {
                            builder.Append(',');
                        }

                        builder.Append(']');
                        break;
                }

                return;
            }

            if (namespaceDelimiter != '\0' && type.Namespace != null)
            {
                var namespacePart = type.Namespace;
                if (namespaceDelimiter != '.')
                {
                    namespacePart = namespacePart.Replace('.', namespaceDelimiter);
                }

                builder.Append(namespacePart);
                builder.Append(namespaceDelimiter);
            }

#if CSHARP_7_3_OR_NEWER
            // You can create instances of a constructed generic type.
            // E.g. Dictionary<int, string> instead of Dictionary<TKey, TValue>.
            if (type.IsConstructedGenericType)
            {
                genericTypeArguments = type.GenericTypeArguments;
            }
#endif

            // If this is a nested class type then append containing type(s) before continuing.
            var containingClassType = type.DeclaringType;
            if (containingClassType != null && type != containingClassType)
            {
                // GenericTypeArguments can't be fetched from the containing class type
                // so need to pass them along to the ToString method and use them instead of
                // the results of GetGenericArguments.
                ToString(containingClassType,
                    builder,
                    '\0',
                    cache,
                    genericTypeArguments);
                builder.Append('.');
            }

            if (!type.IsGenericType)
            {
                builder.Append(type.Name);
                return;
            }

            var nullableUnderlyingType = Nullable.GetUnderlyingType(type);
            if (nullableUnderlyingType != null)
            {
                // "Int?" instead of "Nullable<Int>"
                builder.Append(ToString(nullableUnderlyingType, '\0', cache));
                builder.Append("?");
                return;
            }

            var name = type.Name;

            // If type name doesn't end with `1, `2 etc. then it's not a generic class type
            // but type of non-generic class nested inside a generic class.
            if(name[^2] is '`')
            {
                // E.g. TKey, TValue
                var genericTypes = type.GetGenericArguments();

                builder.Append(name.Substring(0, name.Length - 2));
                builder.Append('<');

                // Prefer using results of GenericTypeArguments over results of GetGenericArguments if available.
                int genericTypeArgumentsLength = genericTypeArguments != null ? genericTypeArguments.Length : 0;
                if (genericTypeArgumentsLength > 0)
                {
                    builder.Append(ToString(genericTypeArguments[0], '\0', cache));
                }
                else
                {
                    builder.Append(ToString(genericTypes[0], '\0', cache));
                }

                for (int n = 1, count = genericTypes.Length; n < count; n++)
                {
                    builder.Append(", ");

                    if (genericTypeArgumentsLength > n)
                    {
                        builder.Append(ToString(genericTypeArguments[n], '\0', cache));
                    }
                    else
                    {
                        builder.Append(ToString(genericTypes[n], '\0', cache));
                    }
                }

                builder.Append('>');
            }
            else
            {
                builder.Append(name);
            }
        }
    }
}