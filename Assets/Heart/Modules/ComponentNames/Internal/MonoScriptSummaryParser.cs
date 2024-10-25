#if UNITY_EDITOR
using JetBrains.Annotations;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Sisus.ComponentNames.Editor
{
    internal static class MonoScriptSummaryParser
    {
        private static readonly StringBuilder Sb = new();

        public static bool TryParseSummary([NotNull] MonoBehaviour monoBehaviour, out string summary)
        {
            if (!(MonoScript.FromMonoBehaviour(monoBehaviour) is MonoScript monoScript) || monoScript == null)
            {
                summary = "";
                return false;
            }

            string text = monoScript.text;
            int summaryStart = text.IndexOf("<summary>");
            if (summaryStart == -1)
            {
                summary = "";
                return false;
            }

            int baseClassBeforeSummary = text.LastIndexOf(':', summaryStart);
            if (baseClassBeforeSummary != -1)
            {
                summary = "";
                return false;
            }

            int summaryEnd = text.IndexOf("</summary>", summaryStart);
            if (summaryEnd == -1)
            {
                summary = "";
                return false;
            }

            summaryStart += "<summary>".Length;
            ParseSummary(text, summaryStart, summaryEnd);
            summary = Sb.ToString().TrimEnd();
            Sb.Clear();
            return true;
        }

        private static void ParseSummary(string code, int summaryStart, int summaryEnd)
        {
            int tagLength = -1;
            for (int i = summaryStart; i < summaryEnd; i++)
            {
                switch (code[i])
                {
                    case '<':
                        if (TryParseXMLTag(code, i, ref tagLength))
                        {
                            i += tagLength - 1;
                            break;
                        }

                        Sb.Append(code[i]);
                        break;
                    case '/':
                        if (SubstringEquals(code, i, "///"))
                        {
                            i += 2;
                            break;
                        }

                        Sb.Append(code[i]);
                        break;
                    case '\n':
                    case '\r':
                    case '\t':
                        break;
                    case ' ':
                        if (Sb.Length > 0)
                        {
                            char prev = Sb[Sb.Length - 1];
                            if (prev != ' ' && prev != '\n')
                            {
                                Sb.Append(' ');
                            }
                        }

                        break;
                    case '{':
                        Sb.Append('<');
                        break;
                    case '}':
                        Sb.Append('>');
                        break;
                    default:
                        Sb.Append(code[i]);
                        break;
                }
            }
        }

        private static bool TryParseXMLTag(string line, int tagStartIndex, ref int tagLength)
        {
            int firstLetterIndex = tagStartIndex + 1;
            if (line.Length <= firstLetterIndex)
            {
                return false;
            }

            switch (line[firstLetterIndex])
            {
                case 's':
                    // Examples:
                    // <see cref="Awake"/>
                    // <see cref="Awake">Awake function</see>
                    if (SubstringEquals(line, firstLetterIndex, "see cref=\""))
                    {
                        int refStartIndex = firstLetterIndex + "see cref=\"".Length;
                        int tagEndIndex = line.IndexOf('>', refStartIndex + 1);
                        if (tagEndIndex == -1)
                        {
                            return false;
                        }

                        // Example: <see cref="Awake"/>
                        if (line[tagEndIndex - 1] == '/')
                        {
                            int refEndIndex = line.IndexOf('"', refStartIndex + 1);
                            if (refEndIndex == -1)
                            {
                                return false;
                            }

                            Append(line.Substring(refStartIndex, refEndIndex - refStartIndex));
                            tagLength = tagEndIndex - tagStartIndex + 1;
                            return true;
                        }
                        // Example: <see cref="Awake">Awake function</see>
                        else
                        {
                            int contentStartIndex = tagEndIndex + 1;
                            int contentEndIndex = line.IndexOf('<', contentStartIndex + 1);
                            if (contentEndIndex == -1)
                            {
                                return false;
                            }

                            int closingTagEndIndex = line.IndexOf('>', contentEndIndex + 1);

                            Append(line.Substring(contentStartIndex, contentEndIndex - contentStartIndex));
                            tagLength = closingTagEndIndex - tagStartIndex + 1;
                            return true;
                        }
                    }

                    return false;
                case 't':
                    // Examples:
                    // <typeparamref name="TClient"/>
                    // <typeparamref name="TClient">client</typeparamref>
                    if (SubstringEquals(line, firstLetterIndex, "typeparamref name=\""))
                    {
                        int refStartIndex = firstLetterIndex + "typeparamref name=\"".Length;
                        int tagEndIndex = line.IndexOf('>', refStartIndex + 1);
                        if (tagEndIndex == -1)
                        {
                            return false;
                        }

                        // Example: <see cref="Awake"/>
                        if (line[tagEndIndex - 1] == '/')
                        {
                            int refEndIndex = line.IndexOf('"', refStartIndex + 1);
                            if (refEndIndex == -1)
                            {
                                return false;
                            }

                            Append(line.Substring(refStartIndex, refEndIndex - refStartIndex));
                            tagLength = tagEndIndex - tagStartIndex + 1;
                            return true;
                        }
                        // Example: <typeparamref name="TClient">client</typeparamref>
                        else
                        {
                            int contentStartIndex = tagEndIndex + 1;
                            int contentEndIndex = line.IndexOf('<', contentStartIndex + 1);
                            if (contentEndIndex == -1)
                            {
                                return false;
                            }

                            int closingTagEndIndex = line.IndexOf('>', contentEndIndex + 1);

                            Append(line.Substring(contentStartIndex, contentEndIndex - contentStartIndex));
                            tagLength = closingTagEndIndex - tagStartIndex + 1;
                            return true;
                        }
                    }

                    return false;
                case 'e':
                    if (SubstringEquals(line, firstLetterIndex, "example>"))
                    {
                        tagLength = "<example>".Length;
                        return true;
                    }

                    return false;
                case 'c':
                    if (SubstringEquals(line, firstLetterIndex, "code>"))
                    {
                        tagLength = "<code>".Length;
                        return true;
                    }

                    return false;
                case 'p':
                    if (SubstringEquals(line, firstLetterIndex, "para>"))
                    {
                        Sb.Append("\n\n");
                        tagLength = "<para>".Length;
                        return true;
                    }

                    return false;
                case '/':
                    if (SubstringEquals(line, firstLetterIndex, "/para>"))
                    {
                        tagLength = "</para>".Length;
                        return true;
                    }

                    if (SubstringEquals(line, firstLetterIndex, "/typeparamref>"))
                    {
                        tagLength = "</para>".Length;
                        return true;
                    }

                    if (SubstringEquals(line, firstLetterIndex, "/code>"))
                    {
                        tagLength = "</code>".Length;
                        return true;
                    }

                    if (SubstringEquals(line, firstLetterIndex, "/example>"))
                    {
                        tagLength = "</example>".Length;
                        return true;
                    }

                    return false;
            }

            int endIndex = line.IndexOf('>', tagStartIndex + 1);
            if (endIndex == -1)
            {
                return false;
            }

            tagLength = endIndex - tagStartIndex + 1;
            return true;
        }

        private static void Append(string substring) => Sb.Append(substring.Replace('{', '<').Replace('}', '>'));

        private static bool SubstringEquals(string line, int startIndex, string substring)
        {
            int charCount = substring.Length;
            int lastIndex = startIndex + charCount - 1;
            if (line.Length <= lastIndex)
            {
                return false;
            }

            for (int i = 0; i < charCount; i++)
            {
                if (line[startIndex + i] != substring[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif